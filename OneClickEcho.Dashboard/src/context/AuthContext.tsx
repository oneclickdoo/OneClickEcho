"use client";

import { useState, useEffect, useContext, createContext, useCallback, useMemo } from "react";
import { useRouter } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";

import { useSlowOperationOverlay } from "@/hooks/useSlowOperationOverlay";

export const AuthContext = createContext<IAuthContext>({} as IAuthContext);

export interface ICompany {
    companyId: string | null;
    name: string;
    createdAt?: string;
}

export interface IUser {
    id: string;
    email: string;
    roles: [string];
    companies: Array<ICompany>;
}

export interface IDashboardManager {
    isAdministrator: boolean;
    companies: Array<ICompany>;
    currentCompany: ICompany | null;
}

export interface IAuthContext {
    authenticated: boolean;
    user: IUser | null;
    loading: boolean;
    login: (dataCredentials: { email: string; password: string }) => Promise<Response>;
    logout: () => Promise<void>;
    authFetch: (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>;
    dashboardManager: IDashboardManager | null;
    setCurrentCompany: (company: ICompany) => void;
}

export const AuthProvider = ({ children }: { children: JSX.Element | JSX.Element[] }) => {
    const tCommon = useTranslations("Common");

    const [user, setUser] = useState<any | null>(null);
    const [dashboardManager, setDashboardManager] = useState<IDashboardManager | null>(null);
    const [loading, setLoading] = useState(true);

    const { run: runWithSlowOverlay, wrapFetch, visible: slowNetworkVisible } = useSlowOperationOverlay(3000);

    const logout = useCallback(async () => {
        try {
            await fetch("/auth/logout", {
                method: "POST",
                credentials: "include"
            });
        } catch (e) {
            setUser(null);
            setDashboardManager(null);
            localStorage.removeItem("currentCompany");
            throw new Error("Failed to logout.");
        }
        setUser(null);
        setDashboardManager(null);
        localStorage.removeItem("currentCompany");
    }, []);

    /** Raw API fetch (session + 401). Used inside getUser to avoid nested slow-overlay ref counts. */
    const authFetchImpl = useCallback(
        async (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
            try {
                const response = await fetch(input, {
                    ...init,
                    credentials: "include"
                });

                if (response.status == 401 || response.status == 403) {
                    await logout();
                    throw new Error();
                }

                return response;
            } catch (e) {
                throw new Error("Authorization failed");
            }
        },
        [logout]
    );

    const authFetch = useMemo(() => wrapFetch(authFetchImpl), [wrapFetch, authFetchImpl]);

    /** Session probe: 401 without cookies is normal (e.g. login page). Do not use authFetchImpl here — it would logout + throw and spam the console. */
    const getUser: () => Promise<IUser | null> = async () => {
        setLoading(true);

        try {
            const response = await fetch("/api/User/CurrentUser", {
                method: "GET",
                credentials: "include",
                headers: {
                    Accept: "application/json",
                    "Content-Type": "application/json"
                }
            });

            if (response.status === 401 || response.status === 403) {
                try {
                    await fetch("/auth/logout", { method: "POST", credentials: "include" });
                } catch {
                    /* ignore */
                }
                setUser(null);
                setDashboardManager(null);
                setLoading(false);
                return null;
            }

            if (!response.ok) {
                setUser(null);
                setDashboardManager(null);
                setLoading(false);
                return null;
            }

            const data: IUser = await response.json();

            setDashboardManager(getDashboardManager(data));
            setUser(data);
            setLoading(false);

            return data;
        } catch {
            setUser(null);
            setDashboardManager(null);
            setLoading(false);
            return null;
        }
    };

    const setCurrentCompany = async (company: ICompany) => {
        let companies: Array<ICompany> = [];

        if (dashboardManager) {
            if (!dashboardManager.companies.find((c) => c.companyId === company.companyId)) {
                companies = [...dashboardManager.companies, company];
            } else {
                companies = dashboardManager.companies;
            }
        }

        localStorage.setItem("currentCompany", JSON.stringify(company));

        setDashboardManager({
            ...dashboardManager!,
            companies: companies,
            currentCompany: company
        });
    };

    const getDashboardManager = (user: IUser): IDashboardManager => {
        const isAdministrator = user.roles.includes("Administrator");

        let companies = user.companies;

        if (isAdministrator) {
            companies.unshift({
                companyId: null,
                name: tCommon("adminDashboard"),
                createdAt: new Date().toISOString()
            });
        }

        let currentCompany = dashboardManager?.currentCompany;

        if (!currentCompany) {
            const savedCompany = localStorage.getItem("currentCompany");

            if (savedCompany) {
                try {
                    currentCompany = JSON.parse(savedCompany) as ICompany;
                    if (
                        !isAdministrator &&
                        currentCompany &&
                        !companies.some((company) => company.companyId === currentCompany?.companyId)
                    ) {
                        currentCompany = null;
                    }
                    if (isAdministrator && !currentCompany) {
                        currentCompany = null;
                    }
                } catch (error) {
                    console.error("Failed to parse currentCompany from local storage", error);
                }
            } else {
                currentCompany = null;
            }
        }

        if (!currentCompany) {
            localStorage.removeItem("currentCompany");
            currentCompany = companies[0] ?? null;
        }

        return {
            companies: companies,
            isAdministrator: isAdministrator,
            currentCompany: currentCompany
        } as IDashboardManager;
    };

    const login = async (credentialsData: { email: string; password: string }) => {
        const body = new URLSearchParams();

        const email = credentialsData.email.trim();
        body.append("username", email);
        body.append("password", credentialsData.password);
        body.append("grant_type", "password");
        // Intersect with server grants (AuthorizationController); openid/profile/email improve token claims. Roles are still copied into the access token by GetDestinations.
        body.append("scope", "openid email profile offline_access");

        return runWithSlowOverlay(async () => {
            try {
                const response = await fetch("/auth/access_token", {
                    method: "POST",
                    headers: {
                        Accept: "application/json",
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString(),
                    credentials: "include"
                });

                if (response.ok) {
                    await getUser();
                }

                return response;
            } catch (e) {
                console.error(e);
                throw new Error("Failed to login.");
            }
        });
    };

    useEffect(() => {
        void getUser();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <>
            {slowNetworkVisible ? (
                <div
                    role="status"
                    aria-live="polite"
                    aria-busy="true"
                    className="fixed inset-0 z-[190] flex cursor-progress items-center justify-center bg-black/40 backdrop-blur-[1px]"
                >
                    <div className="flex flex-col items-center gap-3 rounded-lg bg-white px-8 py-6 shadow-xl dark:bg-gray-900 dark:text-gray-100">
                        <div
                            className="h-9 w-9 shrink-0 animate-spin rounded-full border-2 border-solid border-gray-200 border-t-indigo-600 dark:border-gray-700 dark:border-t-indigo-400"
                            aria-hidden
                        />
                        <p className="text-sm font-medium text-gray-800 dark:text-gray-200">{tCommon("loading")}</p>
                    </div>
                </div>
            ) : null}
            <AuthContext.Provider
                value={{ authenticated: !!user, user, login, logout, loading, authFetch, dashboardManager, setCurrentCompany }}
            >
                {children}
            </AuthContext.Provider>
        </>
    );
};

// @ts-ignore
export const ProtectedRoute = ({ children }) => {
    const { authenticated, loading } = useContext(AuthContext);
    const router = useRouter();
    const locale = useLocale();
    const tCommon = useTranslations("Common");

    useEffect(() => {
        if (!loading && !authenticated) {
            router.push(`/${locale}/login`);
        }
    }, [loading, authenticated, router, locale]);

    if (loading || !authenticated)
        return (
            <div className="fixed top-0 left-0 flex items-center justify-center w-full min-h-screen bg-gray-50">
                <div className="flex flex-col items-center">
                    <div className="w-16 h-16 border-t-4 border-solid border-black rounded-full animate-spin" />
                    <p className="mt-4 text-lg text-gray-700">{tCommon("appName")}</p>
                </div>
            </div>
        );

    return children;
};

export default AuthContext;

export const useAuth = () => useContext(AuthContext);
