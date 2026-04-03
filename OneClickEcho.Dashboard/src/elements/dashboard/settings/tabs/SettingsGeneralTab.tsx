import { useState } from "react";

import { useAuth } from "@/context/AuthContext";

import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

import { useToast } from "@/lib/useToast";

import { updateUserPassword } from "@/elements/dashboard/settings/fetchData";

export const SettingsGeneralTab = () => {
    const { authFetch } = useAuth();
    const { toast } = useToast();

    const [password, setPassword] = useState("");
    const [passwordError, setPasswordError] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [newPasswordError, setNewPasswordError] = useState("");

    const handleSubmit = async () => {
        try {
            const response = await updateUserPassword(
                {
                    password,
                    newPassword
                },
                authFetch
            );
            const data = await response.json();

            if (!response.ok) {
                setPasswordError(data.message ? "Wrong password." : "");
                setNewPasswordError(data.errors?.["NewPassword"]?.reduce((acc: string, item: string) => `${acc} ${item}`, "") || "");

                return;
            }

            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });

            toast({
                title: "Success",
                description: "The user password has been updated.",
                variant: "success",
                duration: 2000
            });

            setPassword("");
            setNewPassword("");
            setPasswordError("");
            setNewPasswordError("");
        } catch (e) {
            console.error(e);
        }
    };

    return (
        <div>
            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 id="personal-information" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        Password
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">Reset password using your current password.</p>
                </div>
                <div className="md:col-span-2">
                    <div className="mb-6">
                        <Label>Current Password</Label>
                        <Input
                            value={password}
                            type="password"
                            placeholder="●●●●●●●●●●"
                            hasError={!!passwordError}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        {passwordError && <p className="mt-1 text-xs text-red-600">{passwordError}</p>}
                    </div>
                    <div className="mb-6">
                        <Label>New Password</Label>
                        <Input
                            value={newPassword}
                            type="password"
                            placeholder="●●●●●●●●●●"
                            hasError={!!newPasswordError}
                            onChange={(e) => setNewPassword(e.target.value)}
                        />
                        {newPasswordError && <p className="mt-1 text-xs text-red-600">{newPasswordError}</p>}
                    </div>
                    <div className="flex justify-end">
                        <Button
                            className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            type="submit"
                            color="primary"
                            onClick={handleSubmit}
                        >
                            Save Company
                        </Button>
                    </div>
                </div>
            </section>
        </div>
    );
};
