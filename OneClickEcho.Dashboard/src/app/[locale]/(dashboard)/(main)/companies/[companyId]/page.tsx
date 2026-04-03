"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { RiEyeFill } from "@remixicon/react";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/tremor/Tabs";
import { Button } from "@/components/tremor/Button";

import { CompanyGeneralTab } from "@/elements/dashboard/singleCompany/tabs/CompanyGeneralTab";
import { CompanyUsersTab } from "@/elements/dashboard/singleCompany/tabs/CompanyUsersTab";
import { CompanySendersTab } from "@/elements/dashboard/singleCompany/tabs/CompanySendersTab";
import { CompanyDto, getCompanyById } from "@/elements/dashboard/companies/fetchData";

export default function SingleCompany({
    params
}: {
    params: { locale: string; companyId: string };
}) {
    const t = useTranslations("CompanyPage");

    const locale = params.locale || "en";

    const [company, setCompany] = useState<CompanyDto>({
        companyId: "",
        name: "",
        viberPricePerMesssage: 0,
        smsPricePerMesssage: 0,
        createdAt: new Date()
    });

    const router = useRouter();
    const { authFetch, setCurrentCompany } = useAuth();

    useEffect(() => {
        let cancelled = false;

        (async () => {
            const data = await getCompanyById(params.companyId, authFetch);
            if (!cancelled) setCompany(data);
        })();

        return () => {
            cancelled = true;
        };
    }, [params.companyId, authFetch]);

    const companyName = company?.name ?? "";

    return (
        <>
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <div className="md:mr-20">
                    <ShortcutHeader
                        shortcut={makeShortcut(t("shortcutTitle", { name: companyName }), `/companies/${company.companyId}`)}
                    >
                        <div className="text-sm text-gray-400">
                            <h1 className="text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                                {t("title", { name: companyName })}
                            </h1>
                            <div>
                                {t("companyIdLabel")}: <span className="font-semibold">{company.companyId}</span>
                            </div>
                        </div>
                    </ShortcutHeader>
                </div>

                <div className="flex items-center justify-between max-md:flex-col gap-3">
                    <Button
                        className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                        type="button"
                        variant="primary"
                        onClick={() => {
                            if (!company) return;

                            setCurrentCompany({
                                companyId: company.companyId,
                                name: company.name
                            });

                            router.push(`/${locale}/overview`);
                        }}
                    >
                        <div className="flex items-center gap-2">
                            <div>{t("loginAsCompany")}</div>
                            <RiEyeFill className="size-4" />
                        </div>
                    </Button>
                </div>
            </div>

            <Tabs defaultValue="tab1" className="mt-4 sm:mt-6 lg:mt-10">
                <TabsList variant="line">
                    <TabsTrigger value="tab1">{t("tabs.general")}</TabsTrigger>
                    <TabsTrigger value="tab2">{t("tabs.users")}</TabsTrigger>
                    <TabsTrigger value="tab3">{t("tabs.senders")}</TabsTrigger>
                </TabsList>

                <div className="pt-6">
                    <TabsContent value="tab1">
                        <CompanyGeneralTab company={company} setCompany={setCompany} />
                    </TabsContent>
                    <TabsContent value="tab2">
                        <CompanyUsersTab companyId={params.companyId} />
                    </TabsContent>
                    <TabsContent value="tab3">
                        <CompanySendersTab companyId={params.companyId} />
                    </TabsContent>
                </div>
            </Tabs>
        </>
    );
}
