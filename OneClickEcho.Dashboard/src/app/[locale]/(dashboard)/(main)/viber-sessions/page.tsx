"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
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
    campaignId?: string | null;
    campaignName?: string | null;
};

type SessionDetail = {
    id: string;
    sender: string;
    phoneNumber: string;
    isOpen: boolean;
    expiresAt: string;
    totalOutbound: number;
    consecutiveOutbound: number;
    messages: SessionMessage[];
};

type SessionListItem = {
    id: string;
    sender: string;
    phoneNumber: string;
    isOpen: boolean;
    expiresAt: string;
    lastActivityAt: string;
    lastPreview?: string | null;
    campaignId?: string | null;
    campaignName?: string | null;
};

export default function ViberSessionsPage() {
    const t = useTranslations("ViberSessions");
    const { authFetch, dashboardManager } = useAuth();
    const { toast } = useToast();
    const companyId = dashboardManager?.currentCompany?.companyId ?? "";
    const [rows, setRows] = useState<SessionListItem[]>([]);
    const [selected, setSelected] = useState<string | null>(null);
    const [current, setCurrent] = useState<SessionDetail | null>(null);
    const [phoneQuery, setPhoneQuery] = useState("");
    const [debouncedPhone, setDebouncedPhone] = useState("");
    const [senderFilter, setSenderFilter] = useState("");
    const [statusFilter, setStatusFilter] = useState<"all" | "open" | "closed">("all");
    const [campaignFilter, setCampaignFilter] = useState("");
    const [text, setText] = useState("");
    const [media, setMedia] = useState("");
    const [kind, setKind] = useState(0);

    useEffect(() => {
        const timer = window.setTimeout(() => setDebouncedPhone(phoneQuery.trim()), 300);
        return () => window.clearTimeout(timer);
    }, [phoneQuery]);

    const loadList = useCallback(async () => {
        if (!companyId) return;
        const url = new URL("/api/app/viber-sessions", window.location.origin);
        url.searchParams.set("companyId", companyId);
        if (debouncedPhone) url.searchParams.set("phone", debouncedPhone);
        const response = await authFetch(url.toString(), { headers: { Accept: "application/json" } });
        if (!response.ok) return;
        const data = (await response.json()) as SessionListItem[];
        setRows(data);
        setSelected((currentId) => (currentId && data.some((row) => row.id === currentId) ? currentId : null));
    }, [authFetch, companyId, debouncedPhone]);

    const loadDetail = useCallback(async (id: string) => {
        if (!companyId) return;
        const url = new URL(`/api/app/viber-sessions/${id}`, window.location.origin);
        url.searchParams.set("companyId", companyId);
        const response = await authFetch(url.toString(), { headers: { Accept: "application/json" } });
        if (!response.ok) return;
        setCurrent((await response.json()) as SessionDetail);
    }, [authFetch, companyId]);

    useEffect(() => {
        void loadList();
    }, [loadList]);

    useEffect(() => {
        if (!selected) {
            setCurrent(null);
            return;
        }
        void loadDetail(selected);
    }, [selected, loadDetail]);

    const senders = useMemo(
        () => Array.from(new Set(rows.map((row) => row.sender))).sort(),
        [rows]
    );
    const campaigns = useMemo(() => {
        const map = new Map<string, string>();
        for (const row of rows) {
            if (row.campaignId && row.campaignName) map.set(row.campaignId, row.campaignName);
        }
        return Array.from(map, ([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name));
    }, [rows]);

    const visibleRows = rows.filter((row) => {
        if (senderFilter && row.sender !== senderFilter) return false;
        if (statusFilter === "open" && !row.isOpen) return false;
        if (statusFilter === "closed" && row.isOpen) return false;
        if (campaignFilter && row.campaignId !== campaignFilter) return false;
        return true;
    });

    const opener = current?.messages.find((message) => message.campaignName)?.campaignName;

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
        setCurrent((await response.json()) as SessionDetail);
        await loadList();
    };

    let lastCampaign = "";

    return (
        <div className="flex flex-col gap-4">
            <ShortcutHeader shortcut={makeShortcut(t("title"), "/viber-sessions")}>
                <h1 className="text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">{t("title")}</h1>
            </ShortcutHeader>
            <p className="text-sm text-gray-500">{t("subtitle")}</p>
            <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
                <div className={`space-y-2 ${selected ? "hidden lg:block" : ""}`}>
                    <Input value={phoneQuery} onChange={(e) => setPhoneQuery(e.target.value)} placeholder={t("searchPhone")} />
                    <div className="grid grid-cols-1 gap-2">
                        <select
                            className="rounded-md border border-gray-200 bg-white px-2 py-1.5 text-sm dark:border-gray-800 dark:bg-gray-950"
                            value={senderFilter}
                            onChange={(e) => setSenderFilter(e.target.value)}
                        >
                            <option value="">{t("allSenders")}</option>
                            {senders.map((sender) => (
                                <option key={sender} value={sender}>{sender}</option>
                            ))}
                        </select>
                        <select
                            className="rounded-md border border-gray-200 bg-white px-2 py-1.5 text-sm dark:border-gray-800 dark:bg-gray-950"
                            value={statusFilter}
                            onChange={(e) => setStatusFilter(e.target.value as "all" | "open" | "closed")}
                        >
                            <option value="all">{t("allStatuses")}</option>
                            <option value="open">{t("open")}</option>
                            <option value="closed">{t("closed")}</option>
                        </select>
                        <select
                            className="rounded-md border border-gray-200 bg-white px-2 py-1.5 text-sm dark:border-gray-800 dark:bg-gray-950"
                            value={campaignFilter}
                            onChange={(e) => setCampaignFilter(e.target.value)}
                        >
                            <option value="">{t("allCampaigns")}</option>
                            {campaigns.map((campaign) => (
                                <option key={campaign.id} value={campaign.id}>{campaign.name}</option>
                            ))}
                        </select>
                    </div>
                    <div className="max-h-[70vh] space-y-2 overflow-y-auto">
                        {visibleRows.length === 0 ? (
                            <p className="text-sm text-gray-500">{rows.length === 0 ? t("empty") : t("noMatches")}</p>
                        ) : null}
                        {visibleRows.map((row) => (
                            <button
                                key={row.id}
                                type="button"
                                onClick={() => setSelected(row.id)}
                                className={`w-full rounded-md border px-3 py-2 text-left text-sm ${selected === row.id ? "border-indigo-500" : "border-gray-200 dark:border-gray-800"}`}
                            >
                                <span className="font-medium">{row.phoneNumber}</span>
                                <span className="block text-xs text-gray-500">
                                    {row.sender} · {row.isOpen ? t("open") : t("closed")}
                                </span>
                                {row.campaignName ? <span className="block truncate text-xs text-gray-600 dark:text-gray-300">{row.campaignName}</span> : null}
                                {row.lastPreview ? <span className="block truncate text-xs text-gray-400">{row.lastPreview}</span> : null}
                            </button>
                        ))}
                    </div>
                </div>
                <div className={selected ? "" : "hidden lg:block"}>
                    {current ? (
                        <div className="space-y-3">
                            <button type="button" className="text-sm text-indigo-600 lg:hidden" onClick={() => setSelected(null)}>
                                {t("backToList")}
                            </button>
                            <div>
                                <p className="font-medium text-gray-900 dark:text-gray-50">{current.phoneNumber}</p>
                                <p className="text-xs text-gray-500">
                                    {current.sender} · {current.isOpen ? t("open") : t("closed")} · {t("limits", { total: current.totalOutbound, consecutive: current.consecutiveOutbound })}
                                </p>
                                {opener ? <p className="text-xs text-gray-600 dark:text-gray-300">{t("openedBy", { campaign: opener })}</p> : null}
                            </div>
                            <div className="max-h-[420px] space-y-2 overflow-y-auto rounded-md border border-gray-200 p-3 dark:border-gray-800">
                                {current.messages.map((message) => {
                                    const showCampaign = !!message.campaignName && message.campaignName !== lastCampaign;
                                    if (message.campaignName) lastCampaign = message.campaignName;
                                    return (
                                        <div key={message.id}>
                                            {showCampaign ? (
                                                <p className="my-2 text-center text-[11px] text-gray-400">
                                                    {t("campaignDivider", { campaign: message.campaignName ?? "" })}
                                                </p>
                                            ) : null}
                                            <div className={message.direction === "In" ? "text-left" : "text-right"}>
                                                <p className="text-sm whitespace-pre-wrap">{message.body || message.media}</p>
                                                <p className="text-[11px] text-gray-400">
                                                    {message.direction === "In" ? t("fromUser") : t("fromApp")}
                                                    {message.billedAsPromotion ? ` · ${t("promo")}` : ""}
                                                </p>
                                            </div>
                                        </div>
                                    );
                                })}
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
                            <p className="text-xs text-gray-500">{t("composerNote")}</p>
                            <Button type="button" onClick={() => void send(false)}>{t("send")}</Button>
                        </div>
                    ) : (
                        <p className="text-sm text-gray-500">{t("pickConversation")}</p>
                    )}
                </div>
            </div>
        </div>
    );
}
