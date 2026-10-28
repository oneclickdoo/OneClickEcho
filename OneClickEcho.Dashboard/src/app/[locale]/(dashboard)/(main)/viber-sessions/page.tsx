"use client";

import { useCallback, useEffect, useState } from "react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";
import { Button } from "@/components/tremor/Button";
import { Input } from "@/components/tremor/Input";
import { Textarea } from "@/components/tremor/Textarea";
import { useToast } from "@/lib/useToast";

type SessionMessage = {
    id: string;
    direction: string;
    body?: string | null;
    media?: string | null;
    isSent: boolean;
    billedAsPromotion: boolean;
    createdAt: string;
};

type SessionRow = {
    id: string;
    sender: string;
    phoneNumber: string;
    isOpen: boolean;
    expiresAt: string;
    totalOutbound: number;
    consecutiveOutbound: number;
    messages: SessionMessage[];
};

export default function ViberSessionsPage() {
    const t = useTranslations("ViberSessions");
    const { authFetch, dashboardManager } = useAuth();
    const { toast } = useToast();
    const companyId = dashboardManager?.currentCompany?.companyId ?? "";
    const [rows, setRows] = useState<SessionRow[]>([]);
    const [selected, setSelected] = useState<string | null>(null);
    const [text, setText] = useState("");
    const [media, setMedia] = useState("");
    const [kind, setKind] = useState(0);

    const load = useCallback(async () => {
        if (!companyId) return;
        const url = new URL("/api/app/viber-sessions", window.location.origin);
        url.searchParams.set("companyId", companyId);
        const response = await authFetch(url.toString(), { headers: { Accept: "application/json" } });
        if (!response.ok) return;
        const data = (await response.json()) as SessionRow[];
        setRows(data);
        setSelected((current) => current ?? data[0]?.id ?? null);
    }, [authFetch, companyId]);

    useEffect(() => {
        void load();
    }, [load]);

    const current = rows.find((row) => row.id === selected) ?? null;

    const send = async (allowOutsideSession: boolean) => {
        if (!current) return;
        const lines = text.split("\n").map((line) => line.trim()).filter(Boolean);
        const response = await authFetch(new URL("/api/app/viber-sessions/send", window.location.origin).toString(), {
            method: "POST",
            headers: { "Content-Type": "application/json", Accept: "application/json" },
            body: JSON.stringify({
                companyId,
                sender: current.sender,
                phoneNumber: current.phoneNumber,
                kind,
                text: kind === 3 ? lines[0] ?? null : text || null,
                media: media || null,
                quickRepliesJson: kind === 3 ? JSON.stringify(lines.slice(1)) : null,
                surveyUiType: 2,
                allowOutsideSession
            })
        });
        if (response.status === 409 && !allowOutsideSession) {
            if (window.confirm(t("outsideConfirm"))) {
                await send(true);
            }
            return;
        }
        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            toast({ variant: "error", title: t("error"), description: err.message || t("sendFailed") });
            return;
        }
        setText("");
        setMedia("");
        await load();
    };

    return (
        <div className="flex flex-col gap-4">
            <ShortcutHeader shortcut={makeShortcut(t("title"), "/viber-sessions")}>
                <h1 className="text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">{t("title")}</h1>
            </ShortcutHeader>
            <p className="text-sm text-gray-500">{t("subtitle")}</p>
            <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
                <div className="space-y-2">
                    {rows.length === 0 ? <p className="text-sm text-gray-500">{t("empty")}</p> : null}
                    {rows.map((row) => (
                        <button
                            key={row.id}
                            type="button"
                            onClick={() => setSelected(row.id)}
                            className={`w-full rounded-md border px-3 py-2 text-left text-sm ${selected === row.id ? "border-indigo-500" : "border-gray-200 dark:border-gray-800"}`}
                        >
                            <span className="font-medium">{row.phoneNumber}</span>
                            <span className="block text-xs text-gray-500">{row.sender} · {row.isOpen ? t("open") : t("closed")}</span>
                        </button>
                    ))}
                </div>
                <div>
                    {current ? (
                        <div className="space-y-3">
                            <p className="text-xs text-gray-500">
                                {t("limits", { total: current.totalOutbound, consecutive: current.consecutiveOutbound })}
                            </p>
                            <div className="max-h-[420px] space-y-2 overflow-y-auto rounded-md border border-gray-200 p-3 dark:border-gray-800">
                                {current.messages.map((message) => (
                                    <div key={message.id} className={message.direction === "In" ? "text-left" : "text-right"}>
                                        <p className="text-sm">{message.body || message.media}</p>
                                        <p className="text-[11px] text-gray-400">
                                            {message.direction}{message.billedAsPromotion ? ` · ${t("promo")}` : ""}
                                        </p>
                                    </div>
                                ))}
                            </div>
                            <div className="flex gap-2 text-sm">
                                {[0, 1, 2, 3].map((value) => (
                                    <button key={value} type="button" className={kind === value ? "font-semibold" : ""} onClick={() => setKind(value)}>
                                        {t(`kind.${value}`)}
                                    </button>
                                ))}
                            </div>
                            <Textarea value={text} onChange={(e) => setText(e.target.value)} placeholder={kind === 3 ? t("repliesPlaceholder") : t("textPlaceholder")} rows={3} />
                            {kind === 1 || kind === 2 ? (
                                <Input value={media} onChange={(e) => setMedia(e.target.value)} placeholder={t("mediaPlaceholder")} />
                            ) : null}
                            <Button type="button" onClick={() => void send(false)}>{t("send")}</Button>
                        </div>
                    ) : null}
                </div>
            </div>
        </div>
    );
}
