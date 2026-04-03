import { IFetch } from "@/lib/networking";

export type CurrentUserUpdatePasswordDto = {
    id: string;
    password: string;
    newPassword: string;
};

export const updateUserPassword = async (updatedData: Partial<CurrentUserUpdatePasswordDto>, authFetch: IFetch) => {
    const url = new URL("/api/User/PasswordUpdate", window.location.origin);

    const payload = {
        ...updatedData
    };

    return await authFetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(payload),
        credentials: "include"
    });
};
