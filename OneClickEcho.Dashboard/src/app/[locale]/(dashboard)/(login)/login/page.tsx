"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { useRouter, useParams } from "next/navigation";
import { useTranslations } from "next-intl";
import { RiGoogleFill } from "@remixicon/react";

import { useAuth } from "@/context/AuthContext";
import { Divider } from "@/components/tremor/Divider";
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Toaster } from "@/components/tremor/Toaster";
import { Button } from "@/components/tremor/Button";
import { useToast } from "@/lib/useToast";

export default function Login() {
    const [isLoading, setIsLoading] = useState(false);

    const router = useRouter();
    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const t = useTranslations("Login");

    const { login, loading, user } = useAuth();
    const { toast } = useToast();

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setIsLoading(true);

        const form = event.currentTarget as HTMLFormElement;
        const formData = new FormData(form);

        const data: { email: string; password: string } = {
            email: (formData.get("email") as string) ?? "",
            password: (formData.get("password") as string) ?? ""
        };

        try {
            const response = await login(data);

            if (response.ok) {
                router.push(`/${locale}/overview`);
            } else {
                const tToast = useTranslations("Toasts");
                const tErr = useTranslations("Errors");

                let description = tErr("unknown");
                try {
                    const errJson = (await response.json()) as { error?: string };
                    if (errJson.error && typeof errJson.error === "string") {
                        description = errJson.error;
                    }
                } catch {
                    /* ignore */
                }

                toast({
                    title: tToast("errorTitle"),
                    description,
                    variant: "error",
                    duration: 6000
                });
            }
        } catch (error) {
            console.error("Error:", error);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex min-h-screen flex-1 flex-col justify-center px-4 lg:px-6">
            <Toaster />

            <div className="sm:w-full sm:max-w-sm sm:mx-auto">
                <Image
                    className="mx-auto h-24 w-auto text-center mb-4 dark:hidden"
                    width={100}
                    height={100}
                    src="/oneclick_echo_novi.svg"
                    alt="OneClick Echo Logo"
                />
                <Image
                    className="mx-auto h-24 w-auto text-center mb-4 hidden dark:block"
                    width={100}
                    height={100}
                    src="/oneclick_echo_novi_dark_theme.svg"
                    alt="OneClick Echo Logo"
                />

                {/* Language switch (login route is /{locale}/login) */}
                <div className="flex justify-center gap-3 mb-3 text-sm">
                    <Link
                        className={locale === "en" ? "font-semibold underline" : "opacity-70"}
                        href="/en/login"
                    >
                        EN
                    </Link>
                    <Link
                        className={locale === "sr" ? "font-semibold underline" : "opacity-70"}
                        href="/sr/login"
                    >
                        SR
                    </Link>
                </div>

                <h3 className="text-center text-lg font-semibold text-gray-900 dark:text-gray-50">
                    {t("title")}
                </h3>

                <form className="mt-6" onSubmit={handleSubmit}>
                    <div>
                        <Label htmlFor="email" className="font-medium">
                            {t("email")}
                        </Label>
                        <Input
                            type="email"
                            id="email"
                            name="email"
                            autoComplete="email"
                            placeholder="john@company.com"
                            className="mt-2"
                        />
                    </div>

                    <div className="mt-3">
                        <Label htmlFor="password" className="font-medium">
                            {t("password")}
                        </Label>
                        <Input
                            type="password"
                            id="password"
                            name="password"
                            autoComplete="current-password"
                            placeholder={t("password")}
                            className="mt-2"
                        />
                    </div>

                    <Button
                        type="submit"
                        className="w-full mt-6"
                        isLoading={isLoading || loading || user !== null}
                    >
                        {t("signIn")}
                    </Button>
                </form>

                <Divider>{t("orWith")}</Divider>

                <Button disabled variant="secondary" className="w-full">
                    <div className="inline-flex items-center gap-2">
                        <RiGoogleFill className="size-5" aria-hidden />
                        {t("google")}
                    </div>
                </Button>

                <p className="mt-4 text-xs text-gray-500 dark:text-gray-500">
                    {t("tosPrefix")}{" "}
                    <a
                        href="https://oneclicksolutions.com/terms-and-conditions"
                        target="_blank"
                        className="underline underline-offset-4"
                    >
                        {t("terms")}
                    </a>{" "}
                    {t("and")}{" "}
                    <a
                        href="https://oneclicksolutions.com/privacy-policy"
                        target="_blank"
                        className="underline underline-offset-4"
                    >
                        {t("privacy")}
                    </a>
                    .
                </p>
            </div>
        </div>
    );
}
