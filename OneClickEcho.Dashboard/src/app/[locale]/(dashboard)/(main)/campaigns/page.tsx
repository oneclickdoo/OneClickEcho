"use client";
import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";

import { Dialog, DialogClose, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/tremor/Dialog";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

import { CampaignsTable } from "@/elements/dashboard/campaigns/tables/CampaignsTable";
import { createCampaign } from "@/elements/dashboard/campaigns/fetchData";

export default function Campaigns() {
    const t = useTranslations("Campaigns");

    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const [campaignName, setCampaignName] = useState<string>("");
    const [errorMessages, setErrorMessages] = useState<string>("");
    const [isLoading, setIsLoading] = useState<boolean>(false);

    const { dashboardManager, authFetch } = useAuth();
    const router = useRouter();

    const handleSubmit = async () => {
        if (!campaignName.trim()) return;

        setIsLoading(true);

        try {
            const result = await createCampaign(
                { name: campaignName, companyId: dashboardManager!.currentCompany!.companyId },
                authFetch
            );

            const response = await result.json();

            if (result.ok) {
                // ✅ zatvori modal odmah na success
                setIsDialogOpen(false);

                // opcionalno očisti state da sledeći put bude čist
                setCampaignName("");
                setErrorMessages("");

                router.push(`/${locale}/campaigns/${response.id}`);
                return;
            }

            setErrorMessages(
                response.errors?.["Name"]?.reduce((acc: string, item: string) => `${acc} ${item}`, "") ||
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
        <>
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <ShortcutHeader shortcut={makeShortcut(t("title"), "/campaigns")}>
                    <h1 className="ml-4 md:ml-0 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                        {t("title")}
                    </h1>
                </ShortcutHeader>

                <div className="flex items-center justify-between max-md:flex-col gap-3">
                    <Dialog
                        open={isDialogOpen}
                        onOpenChange={(open) => {
                            setIsDialogOpen(open);
                            // resetuj samo kada se otvara (ili kada se zatvara — kako želiš)
                            if (open) {
                                setCampaignName("");
                                setErrorMessages("");
                            }
                        }}
                    >
                        <DialogTrigger asChild>
                            <Button
                                className="h-8 text-sm border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                                type="button"
                                variant="primary"
                            >
                                {t("newCampaign")}
                            </Button>
                        </DialogTrigger>

                        <DialogContent className="sm:max-w-lg" aria-describedby="create new campaign dialog">
                            <DialogHeader>
                                <DialogTitle>{t("createCampaignTitle")}</DialogTitle>
                                <p className="text-sm text-gray-400 mt-1">{t("createCampaignDescription")}</p>

                                <div className="mt-5">
                                    <Input
                                        value={campaignName}
                                        onChange={(e) => setCampaignName(e.target.value)}
                                        className="mx-auto w-full"
                                        hasError={!!errorMessages}
                                        placeholder={t("campaignNamePlaceholder")}
                                        onKeyDown={(e) => {
                                            if (e.key === "Enter" && !isLoading) {
                                                e.preventDefault();
                                                handleSubmit();
                                            }
                                        }}
                                    />
                                    {errorMessages && <p className="mt-1 text-xs text-red-600">{errorMessages}</p>}
                                </div>
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
                                    disabled={isLoading || !campaignName.trim()}
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
                <CampaignsTable />
            </div>
        </>
    );
}
