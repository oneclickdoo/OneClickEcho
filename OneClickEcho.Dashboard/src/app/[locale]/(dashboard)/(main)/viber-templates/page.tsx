"use client";

import { useCallback, useEffect, useState } from "react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";
import { Button } from "@/components/tremor/Button";
import { Input } from "@/components/tremor/Input";
import { Label } from "@/components/tremor/Label";
import { Textarea } from "@/components/tremor/Textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/tremor/Select";
import { getCompanySenders, type SenderDto } from "@/elements/dashboard/singleCompany/fetchData";
import { SenderType } from "@/lib/enums";
import { useToast } from "@/lib/useToast";

type TemplateRow = {
    id: string;
    name: string;
    language: string;
    body: string;
    status: string;
    canSend: boolean;
    display: string;
    providerTemplateId?: string | null;
};

const LANGUAGES = ["sr", "en", "hr", "bs", "de", "ru"];

export default function ViberTemplatesPage() {
    const t = useTranslations("ViberTemplates");
    const { authFetch, dashboardManager } = useAuth();
    const { toast } = useToast();
    const companyId = dashboardManager?.currentCompany?.companyId ?? "";
    const [rows, setRows] = useState<TemplateRow[]>([]);
    const [senders, setSenders] = useState<SenderDto[]>([]);
    const [name, setName] = useState("");
    const [display, setDisplay] = useState("");
    const [language, setLanguage] = useState("sr");
    const [body, setBody] = useState("Hello {{name}}, your code is ready.");
    const [tag, setTag] = useState("");

    const load = useCallback(async () => {
        if (!companyId) return;
        const url = new URL("/api/app/viber-templates", window.location.origin);
        url.searchParams.set("companyId", companyId);
        const response = await authFetch(url.toString(), { headers: { Accept: "application/json" } });
        if (!response.ok) return;
        setRows(await response.json());
    }, [authFetch, companyId]);

    useEffect(() => {
        void load();
        if (!companyId) return;
        void getCompanySenders(companyId, authFetch).then((data) => {
            setSenders(data.filter((s) => s.type === SenderType.Viber));
            setDisplay((prev) => prev || data.find((s) => s.type === SenderType.Viber)?.name || "");
        });
    }, [authFetch, companyId, load]);

    const create = async () => {
        const response = await authFetch(new URL("/api/app/viber-templates", window.location.origin).toString(), {
            method: "POST",
            headers: { "Content-Type": "application/json", Accept: "application/json" },
            body: JSON.stringify({ companyId, display, name, language, body, tag: tag || null })
        });
        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            toast({ variant: "error", title: t("error"), description: err.message || t("saveFailed") });
            return;
        }
        setName("");
        await load();
    };

    const refresh = async (id: string) => {
        const url = new URL(`/api/app/viber-templates/${id}/refresh`, window.location.origin);
        url.searchParams.set("companyId", companyId);
        await authFetch(url.toString(), { method: "POST" });
        await load();
    };

    const remove = async (id: string) => {
        const url = new URL(`/api/app/viber-templates/${id}`, window.location.origin);
        url.searchParams.set("companyId", companyId);
        await authFetch(url.toString(), { method: "DELETE" });
        await load();
    };

    return (
        <div className="flex flex-col gap-6">
            <ShortcutHeader shortcut={makeShortcut(t("title"), "/viber-templates")}>
                <h1 className="text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">{t("title")}</h1>
            </ShortcutHeader>
            <p className="text-sm text-gray-500">{t("subtitle")}</p>
            <div className="grid gap-3 max-w-xl">
                <Label>{t("sender")}</Label>
                <Select value={display} onValueChange={setDisplay}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                        {senders.map((s) => (
                            <SelectItem key={s.id} value={s.name}>{s.name}</SelectItem>
                        ))}
                    </SelectContent>
                </Select>
                <Input placeholder={t("name")} value={name} onChange={(e) => setName(e.target.value)} />
                <Select value={language} onValueChange={setLanguage}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                        {LANGUAGES.map((code) => (
                            <SelectItem key={code} value={code}>{code}</SelectItem>
                        ))}
                    </SelectContent>
                </Select>
                <Textarea value={body} onChange={(e) => setBody(e.target.value)} rows={5} />
                <Input placeholder={t("tag")} value={tag} onChange={(e) => setTag(e.target.value)} />
                <Button type="button" onClick={() => void create()} disabled={!companyId || !name || !display || !body}>
                    {t("create")}
                </Button>
            </div>
            <div className="space-y-3">
                {rows.map((row) => (
                    <div key={row.id} className="rounded-md border border-gray-200 p-3 dark:border-gray-800">
                        <div className="flex items-center justify-between gap-3">
                            <div>
                                <p className="font-medium">{row.name}</p>
                                <p className="text-xs text-gray-500">{row.display} · {row.language} · {row.status}{row.canSend ? "" : ` · ${t("pending")}`}</p>
                            </div>
                            <div className="flex gap-2">
                                <Button type="button" variant="secondary" onClick={() => void refresh(row.id)}>{t("refresh")}</Button>
                                <Button type="button" variant="secondary" onClick={() => void remove(row.id)}>{t("delete")}</Button>
                            </div>
                        </div>
                        <p className="mt-2 text-sm whitespace-pre-wrap">{row.body}</p>
                    </div>
                ))}
            </div>
        </div>
    );
}
