"use client";

import { useState, useEffect, FormEvent, useCallback } from "react";
import { useTranslations } from "next-intl";

import { RiDeleteBinLine } from "@remixicon/react";

import { useAuth } from "@/context/AuthContext";

import { Card } from "@/components/tremor/Card";
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

import { SenderDto, CreateSenderDto, getCompanySenders, createSender, deleteSender } from "../fetchData";

import { useToast } from "@/lib/useToast";
import { SenderType } from "@/lib/enums";

interface ICompanySendersTabProps {
    companyId: string;
}

export const CompanySendersTab = ({ companyId }: ICompanySendersTabProps) => {
    const tCommon = useTranslations("Common");

    const [viberSenderName, setViberSenderName] = useState<string>("");
    const [smsSenderName, setSmsSenderName] = useState<string>("");
    const [senders, setSenders] = useState<SenderDto[]>([]);

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const getSenders = useCallback(async () => {
        const data = await getCompanySenders(companyId, authFetch);
        setSenders(data);
    }, [companyId, authFetch]);

    const handleSubmit = async (event: FormEvent<HTMLFormElement>, type: SenderType) => {
        event.preventDefault();

        try {
            const payload: CreateSenderDto = {
                companyId,
                name: type === SenderType.Viber ? viberSenderName : smsSenderName,
                type
            };

            await createSender(payload, authFetch);

            if (type === SenderType.Viber) {
                setViberSenderName("");
            } else {
                setSmsSenderName("");
            }

            await getSenders();

            toast({
                title: tCommon("toasts.success"),
                description: tCommon("toasts.senderSaved"),
                variant: "success",
                duration: 2000
            });
        } catch (e: any) {
            toast({
                title: tCommon("toasts.error"),
                description: e?.message ?? tCommon("errors.unexpected"),
                variant: "error",
                duration: 2000
            });
        }
    };

    const handleDelete = async (id: string) => {
        try {
            await deleteSender(id, authFetch);

            await getSenders();

            toast({
                title: tCommon("toasts.success"),
                description: tCommon("toasts.senderDeleted"),
                variant: "success",
                duration: 2000
            });
        } catch (e: any) {
            toast({
                title: tCommon("toasts.error"),
                description: e?.message ?? tCommon("errors.unexpected"),
                variant: "error",
                duration: 2000
            });
        }
    };

    useEffect(() => {
        getSenders();
    }, [getSenders]);

    return (
        <div>
            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-2">
                {/* Viber */}
                <div>
                    <h2 className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {tCommon("companySenders.viberTitle")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{tCommon("companySenders.viberDescription")}</p>

                    <form className="my-5" onSubmit={(e) => handleSubmit(e, SenderType.Viber)}>
                        <Label>{tCommon("companySenders.nameLabel")}</Label>
                        <div className="flex items-center gap-x-5">
                            <Input value={viberSenderName} onChange={(e) => setViberSenderName(e.target.value)} />
                            <Button
                                type="submit"
                                color="primary"
                                className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            >
                                {tCommon("common.add")}
                            </Button>
                        </div>
                    </form>

                    {senders
                        .filter((sender) => sender.type === SenderType.Viber)
                        .map((sender) => (
                            <Card key={sender.id} className="flex justify-between items-center mb-2 py-2 px-4 overflow-hidden">
                                <p className="text-sm">{sender.name}</p>
                                <RiDeleteBinLine
                                    className="size-5 text-red-500 cursor-pointer hover:text-red-600"
                                    onClick={() => handleDelete(sender.id)}
                                />
                            </Card>
                        ))}
                </div>

                {/* SMS */}
                <div>
                    <h2 className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {tCommon("companySenders.smsTitle")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{tCommon("companySenders.smsDescription")}</p>

                    <form className="my-5" onSubmit={(e) => handleSubmit(e, SenderType.SMS)}>
                        <Label>{tCommon("companySenders.nameLabel")}</Label>
                        <div className="flex items-center gap-x-5">
                            <Input value={smsSenderName} onChange={(e) => setSmsSenderName(e.target.value)} />
                            <Button
                                type="submit"
                                color="primary"
                                className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            >
                                {tCommon("common.add")}
                            </Button>
                        </div>
                    </form>

                    {senders
                        .filter((sender) => sender.type === SenderType.SMS)
                        .map((sender) => (
                            <Card key={sender.id} className="flex justify-between items-center mb-2 py-2 px-4 overflow-hidden">
                                <p className="text-sm">{sender.name}</p>
                                <RiDeleteBinLine
                                    className="size-5 text-red-500 cursor-pointer hover:text-red-600"
                                    onClick={() => handleDelete(sender.id)}
                                />
                            </Card>
                        ))}
                </div>
            </section>
        </div>
    );
};
