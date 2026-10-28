"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";

import { Input } from "@/components/tremor/Input";
import { Label } from "@/components/tremor/Label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/tremor/Select";
import { Button } from "@/components/tremor/Button";

import { useAuth } from "@/context/AuthContext";
import type { CampaignDto } from "@/elements/dashboard/singleCampaign/fetchData";
import { CampaignStatus, CampaignViberContentKind } from "@/lib/enums";

type Card = {
    imageUrl: string;
    title: string;
    primaryLabel: string;
    primaryUrl: string;
    secondaryLabel: string;
    secondaryUrl: string;
};

type TemplateRow = {
    id: string;
    name: string;
    language: string;
    body: string;
    status: string;
    canSend: boolean;
    display: string;
};

const emptyCard = (): Card => ({
    imageUrl: "",
    title: "",
    primaryLabel: "",
    primaryUrl: "",
    secondaryLabel: "",
    secondaryUrl: ""
});

function parseCards(json?: string | null): Card[] {
    if (!json) return [emptyCard(), emptyCard()];
    try {
        const parsed = JSON.parse(json) as { items?: Array<Record<string, unknown>> };
        const items = parsed.items ?? [];
        const cards = items.map((item) => {
            const primary = (item.primaryButton ?? {}) as Record<string, string>;
            const secondary = (item.secondaryButton ?? {}) as Record<string, string>;
            return {
                imageUrl: String(item.imageUrl ?? ""),
                title: String(item.title ?? ""),
                primaryLabel: String(primary.label ?? ""),
                primaryUrl: String(primary.actionUrl ?? ""),
                secondaryLabel: String(secondary.label ?? ""),
                secondaryUrl: String(secondary.actionUrl ?? "")
            };
        });
        return cards.length >= 2 ? cards : [emptyCard(), emptyCard()];
    } catch {
        return [emptyCard(), emptyCard()];
    }
}

function toJson(cards: Card[]): string {
    return JSON.stringify({
        items: cards.map((card) => ({
            imageUrl: card.imageUrl.trim(),
            title: card.title.trim(),
            primaryButton: { label: card.primaryLabel.trim(), actionUrl: card.primaryUrl.trim() },
            secondaryButton:
                card.secondaryLabel.trim() || card.secondaryUrl.trim()
                    ? { label: card.secondaryLabel.trim(), actionUrl: card.secondaryUrl.trim() }
                    : null
        }))
    });
}

export function ViberExtraContentFields({
    formData,
    handleChange
}: {
    formData: CampaignDto;
    handleChange: (name: keyof CampaignDto, value: CampaignDto[keyof CampaignDto]) => void;
}) {
    const t = useTranslations("SingleCampaign.Tabs.CampaignMessaging");
    const { authFetch, dashboardManager } = useAuth();
    const kind = formData.viberContentKind ?? CampaignViberContentKind.Text;
    const draft = formData.status === CampaignStatus.Draft;
    const [cards, setCards] = useState<Card[]>(() => parseCards(formData.viberCarouselJson));
    const [templates, setTemplates] = useState<TemplateRow[]>([]);

    useEffect(() => {
        setCards(parseCards(formData.viberCarouselJson));
    }, [formData.campaignId]);

    useEffect(() => {
        const companyId = dashboardManager?.currentCompany?.companyId;
        if (!companyId || kind !== CampaignViberContentKind.TransactionalTemplate) return;
        const url = new URL("/api/app/viber-templates", window.location.origin);
        url.searchParams.set("companyId", companyId);
        void authFetch(url.toString(), { headers: { Accept: "application/json" } })
            .then((r) => (r.ok ? r.json() : []))
            .then((rows: TemplateRow[]) => setTemplates(Array.isArray(rows) ? rows : []))
            .catch(() => setTemplates([]));
    }, [dashboardManager?.currentCompany?.companyId, kind, authFetch]);

    if (kind === CampaignViberContentKind.Image) {
        return (
            <div className="mb-4">
                <Label>{t("viber.imageRatio.label")}</Label>
                <Select
                    disabled={!draft}
                    value={formData.viberImageRatio || "square"}
                    onValueChange={(value) => handleChange("viberImageRatio", value)}
                >
                    <SelectTrigger className="mt-2">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="square">{t("viber.imageRatio.square")}</SelectItem>
                        <SelectItem value="portrait">{t("viber.imageRatio.portrait")}</SelectItem>
                        <SelectItem value="landscape">{t("viber.imageRatio.landscape")}</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        );
    }

    if (kind === CampaignViberContentKind.Survey) {
        return (
            <div className="mb-4">
                <Label>{t("viber.survey.uiLabel")}</Label>
                <Select
                    disabled={!draft}
                    value={String(formData.viberSurveyUiType ?? 1)}
                    onValueChange={(value) => handleChange("viberSurveyUiType", Number(value))}
                >
                    <SelectTrigger className="mt-2">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="1">{t("viber.survey.uiRadio")}</SelectItem>
                        <SelectItem value="2">{t("viber.survey.uiButtons")}</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        );
    }

    if (kind === CampaignViberContentKind.Carousel) {
        const commit = (next: Card[]) => {
            setCards(next);
            handleChange("viberCarouselJson", toJson(next));
        };
        return (
            <div className="mb-4 space-y-3">
                <p className="text-xs text-gray-500">{t("viber.carousel.hint")}</p>
                {cards.map((card, index) => (
                    <div key={index} className="space-y-2 rounded-md border border-gray-200 p-3 dark:border-gray-700">
                        <Label>{t("viber.carousel.card", { n: index + 1 })}</Label>
                        <Input disabled={!draft} placeholder={t("viber.carousel.image")} value={card.imageUrl} onChange={(e) => {
                            const next = [...cards];
                            next[index] = { ...card, imageUrl: e.target.value };
                            commit(next);
                        }} />
                        <Input disabled={!draft} maxLength={38} placeholder={t("viber.carousel.title")} value={card.title} onChange={(e) => {
                            const next = [...cards];
                            next[index] = { ...card, title: e.target.value };
                            commit(next);
                        }} />
                        <Input disabled={!draft} maxLength={10} placeholder={t("viber.carousel.primaryLabel")} value={card.primaryLabel} onChange={(e) => {
                            const next = [...cards];
                            next[index] = { ...card, primaryLabel: e.target.value };
                            commit(next);
                        }} />
                        <Input disabled={!draft} placeholder={t("viber.carousel.primaryUrl")} value={card.primaryUrl} onChange={(e) => {
                            const next = [...cards];
                            next[index] = { ...card, primaryUrl: e.target.value };
                            commit(next);
                        }} />
                        <Input disabled={!draft} maxLength={12} placeholder={t("viber.carousel.secondaryLabel")} value={card.secondaryLabel} onChange={(e) => {
                            const next = [...cards];
                            next[index] = { ...card, secondaryLabel: e.target.value };
                            commit(next);
                        }} />
                        <Input disabled={!draft} placeholder={t("viber.carousel.secondaryUrl")} value={card.secondaryUrl} onChange={(e) => {
                            const next = [...cards];
                            next[index] = { ...card, secondaryUrl: e.target.value };
                            commit(next);
                        }} />
                    </div>
                ))}
                <div className="flex gap-2">
                    <Button type="button" variant="secondary" disabled={!draft || cards.length >= 5} onClick={() => commit([...cards, emptyCard()])}>
                        {t("viber.carousel.add")}
                    </Button>
                    <Button type="button" variant="secondary" disabled={!draft || cards.length <= 2} onClick={() => commit(cards.slice(0, -1))}>
                        {t("viber.carousel.remove")}
                    </Button>
                </div>
            </div>
        );
    }

    if (kind === CampaignViberContentKind.TransactionalTemplate) {
        const selected = templates.find((row) => row.id === formData.viberTemplateId);
        const names = selected ? Array.from(selected.body.matchAll(/\{\{([A-Za-z]+)\}\}/g)).map((m) => m[1]) : [];
        const unique = Array.from(new Set(names));
        let params: Record<string, string> = {};
        try {
            params = formData.viberTemplateParamsJson ? JSON.parse(formData.viberTemplateParamsJson) : {};
        } catch {
            params = {};
        }
        return (
            <div className="mb-4 space-y-2">
                <Label>{t("viber.template.pick")}</Label>
                <Select
                    disabled={!draft}
                    value={formData.viberTemplateId || ""}
                    onValueChange={(value) => handleChange("viberTemplateId", value)}
                >
                    <SelectTrigger className="mt-2">
                        <SelectValue placeholder={t("viber.template.pick")} />
                    </SelectTrigger>
                    <SelectContent>
                        {templates.filter((row) => row.canSend).map((row) => (
                            <SelectItem key={row.id} value={row.id}>
                                {row.name} ({row.language})
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
                <p className="text-xs text-gray-500">{t("viber.template.hint")}</p>
                {unique.map((name) => (
                    <Input
                        key={name}
                        disabled={!draft}
                        maxLength={125}
                        placeholder={name}
                        value={params[name] ?? ""}
                        onChange={(e) => {
                            const next = { ...params, [name]: e.target.value };
                            handleChange("viberTemplateParamsJson", JSON.stringify(next));
                        }}
                    />
                ))}
            </div>
        );
    }

    return null;
}
