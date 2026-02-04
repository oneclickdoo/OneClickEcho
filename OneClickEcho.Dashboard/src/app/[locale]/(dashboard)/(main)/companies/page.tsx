"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";
import { useAuth } from "@/context/AuthContext";

import { CompaniesTable } from "@/elements/dashboard/companies/tables/CompaniesTable";
import { createCompany } from "@/elements/dashboard/companies/fetchData";

import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger
} from "@/components/tremor/Dialog";
import { Button } from "@/components/tremor/Button";
import { Input } from "@/components/tremor/Input";

import { useToast } from "@/lib/useToast";

export default function CompaniesPage() {
    const t = useTranslations("Companies");

    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const [company, setCompany] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [errorMessages, setErrorMessages] = useState("");

    const { toast } = useToast();
    const { authFetch } = useAuth();
    const router = useRouter();

    const handleSubmit = async () => {
        setIsLoading(true);

        try {
            const result = await createCompany(company, authFetch);
            const response = await result.json();

            if (result.ok) {
                toast({
                    title: t("toasts.successTitle"),
                    description: t("toasts.created"),
                    variant: "success",
                    duration: 2000
                });

                router.push(`/${locale}/companies/${response.companyId}`);
                return;
            }

            setErrorMessages(
                response.errors?.["CompanyName"]?.reduce((acc: string, item: string) => `${acc} ${item}`, "") ||
                t("errors.unexpected")
            );
        } catch (e: any) {
            console.error(e);
            setErrorMessages(t("errors.unexpected"));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex flex-col">
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <ShortcutHeader shortcut={makeShortcut(t("title"), "/companies")}>
                    <h1 className="ml-4 md:ml-0 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                        {t("title")}
                    </h1>
                </ShortcutHeader>

                <div className="flex items-center justify-between max-md:flex-col gap-3">
                    <Dialog
                        onOpenChange={() => {
                            setCompany("");
                            setErrorMessages("");
                        }}
                    >
                        <DialogTrigger asChild>
                            <Button
                                className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                                type="button"
                                variant="primary"
                            >
                                {t("new")}
                            </Button>
                        </DialogTrigger>

                        <DialogContent className="sm:max-w-lg" aria-describedby="create new company dialog">
                            <DialogHeader>
                                <DialogTitle>{t("createTitle")}</DialogTitle>
                                <DialogDescription>
                                    <p className="text-sm text-gray-400 mt-1">{t("createDescription")}</p>

                                    <div className="mt-5">
                                        <Input
                                            value={company}
                                            onChange={(e) => setCompany(e.target.value)}
                                            className="mx-auto w-full"
                                            hasError={!!errorMessages}
                                            placeholder={t("namePlaceholder")}
                                        />
                                        {errorMessages && <p className="mt-1 text-xs text-red-600">{errorMessages}</p>}
                                    </div>
                                </DialogDescription>
                            </DialogHeader>

                            <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                                <DialogClose asChild>
                                    <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary">
                                        {t("goBack")}
                                    </Button>
                                </DialogClose>

                                <Button
                                    onClick={handleSubmit}
                                    isLoading={isLoading}
                                    className="w-full sm:w-fit border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                                >
                                    {t("create")}
                                </Button>
                            </DialogFooter>
                        </DialogContent>
                    </Dialog>
                </div>
            </div>

            <div className="mt-2 sm:mt-4 lg:mt-8">
                <CompaniesTable />
            </div>
        </div>
    );
}
