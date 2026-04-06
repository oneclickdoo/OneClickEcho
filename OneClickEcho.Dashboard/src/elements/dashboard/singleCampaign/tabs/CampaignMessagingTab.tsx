"use client";

import { Dispatch, SetStateAction, useEffect, useMemo, useRef, useState } from "react";
import DatePicker, { registerLocale } from "react-datepicker";
import { useLocale, useTranslations } from "next-intl";

import { enGB } from "date-fns/locale/en-GB";
import { srLatn } from "date-fns/locale/sr-Latn";

import { useAuth } from "@/context/AuthContext";

import { Label } from "@/components/tremor/Label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/tremor/Select";
import { Checkbox } from "@/components/tremor/Checkbox";
import { Divider } from "@/components/tremor/Divider";
import { Input } from "@/components/tremor/Input";
import { Textarea } from "@/components/tremor/Textarea";
import { MediaUpload, type FileExtended } from "@/components/custom/MediaUpload";
import { GptModal } from "@/components/custom/GptModal";
import { VariablesForm } from "@/components/custom/VariablesForm";
import { Tooltip } from "@/components/tremor/Tooltip";
import { Button } from "@/components/tremor/Button";

import { getCompanySenders, type SenderDto } from "../../singleCompany/fetchData";
import { type CampaignDto, updateCampaign, uploadCampaignViberMedia } from "@/elements/dashboard/singleCampaign/fetchData";

import { getCampaignSendingTypeOptions } from "@/lib/selects";
import { useToast } from "@/lib/useToast";
import { filterPassedTime, getMediaType, isOutOfWorkingHours } from "@/lib/utils";
import { migrateLegacyViberHtmlToMarkdown, viberMarkdownToPreviewHtml } from "@/lib/viberTextFormat";

import { CampaignMediaType, CampaignSendingType, CampaignStatus, convertStringToEnum, SenderType } from "@/lib/enums";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger
} from "@/components/tremor/Dialog";

// ✅ register datepicker locales ONCE (client module)
registerLocale("en", enGB);
registerLocale("sr", srLatn);

interface ICampaignMessagingTabProps {
    formData: CampaignDto;
    setFormData: Dispatch<SetStateAction<CampaignDto>>;
}

export function CampaignMessagingTab({ formData, setFormData }: ICampaignMessagingTabProps) {
    const t = useTranslations("SingleCampaign.Tabs.CampaignMessaging");
    const tCommon = useTranslations("Common");
    const locale = useLocale(); // "en" | "sr"

    const [duration, setDuration] = useState<number | undefined>(undefined);
    const [media, setMedia] = useState<FileExtended | string | null>(null);
    const [thumbnail, setThumbnail] = useState<FileExtended | string | null>(null);
    const [senders, setSenders] = useState<SenderDto[]>([]);
    const [companyImages, setCompanyImages] = useState<string[]>([]);
    const [imageModalOpen, setImageModalOpen] = useState(false);

    const viberMessageInputRef = useRef<HTMLTextAreaElement>(null);
    const smsMessageInputRef = useRef<HTMLTextAreaElement>(null);

    const { dashboardManager, authFetch } = useAuth();
    const { toast } = useToast();

    const sendingTypeOptions = useMemo(() => getCampaignSendingTypeOptions(tCommon), [tCommon]);

    useEffect(() => {
        const effectFunction = async () => {
            const url = new URL(`/api/Company/${dashboardManager?.currentCompany?.companyId}/Images`, window.location.origin);

            const response = await authFetch(url, { headers: { Accept: "application/json" } });
            if (!response.ok) throw new Error("Network response was not ok");

            const data: { images: string[] } = await response.json();
            setCompanyImages(data.images.filter((img) => getMediaType(img) === CampaignMediaType.Image));
            return data;
        };

        void effectFunction();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        if (
            senders.length > 0 &&
            (formData.viberSender === "" || formData.viberSender === null || typeof formData.viberSender === "undefined")
        ) {
            formData.viberSender = senders.find((s) => s.type === SenderType.Viber)?.name ?? undefined;
        }

        if (
            senders.length > 0 &&
            (formData.smsSender === "" || formData.smsSender === null || typeof formData.smsSender === "undefined")
        ) {
            formData.smsSender = senders.find((s) => s.type === SenderType.SMS)?.name ?? undefined;
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [senders]);

    const handleChange = <K extends keyof CampaignDto>(name: K, value: CampaignDto[K] | undefined): void => {
        if (formData[name] != null && typeof formData[name] !== "undefined" && typeof formData[name] !== typeof value) {
            console.error("Type mismatch");
        }

        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const viberStickers = ["😀", "🔥", "🎉", "❤️", "👍", "👏", "📩", "⭐", "✅", "💥"];

    const updateTextareaSelection = (
        input: HTMLTextAreaElement | null,
        selectionStart: number,
        selectionEnd: number
    ) => {
        if (!input) return;

        window.requestAnimationFrame(() => {
            input.focus();
            input.setSelectionRange(selectionStart, selectionEnd);
        });
    };

    const wrapSelectedText = (
        input: HTMLTextAreaElement | null,
        value: string | undefined,
        wrapperStart: string,
        wrapperEnd: string
    ): string => {
        const text = value ?? "";
        if (!input) return `${wrapperStart}${text}${wrapperEnd}`;

        const start = input.selectionStart ?? 0;
        const end = input.selectionEnd ?? 0;
        const selectedText = text.slice(start, end);
        const nextValue = text.slice(0, start) + wrapperStart + selectedText + wrapperEnd + text.slice(end);
        const cursorStart = start + wrapperStart.length;
        const cursorEnd = cursorStart + selectedText.length;

        updateTextareaSelection(input, cursorStart, cursorEnd);
        return nextValue;
    };

    const insertAtCursor = (
        input: HTMLTextAreaElement | null,
        value: string | undefined,
        insertedText: string
    ): string => {
        const text = value ?? "";
        if (!input) return `${text}${insertedText}`;

        const start = input.selectionStart ?? 0;
        const end = input.selectionEnd ?? 0;
        const nextValue = text.slice(0, start) + insertedText + text.slice(end);
        const cursor = start + insertedText.length;

        updateTextareaSelection(input, cursor, cursor);
        return nextValue;
    };

    const applyViberBold = () => {
        const nextValue = wrapSelectedText(viberMessageInputRef.current, formData.viberMessage, "*", "*");
        handleChange("viberMessage", nextValue as any);
    };

    const applyViberItalic = () => {
        const nextValue = wrapSelectedText(viberMessageInputRef.current, formData.viberMessage, "_", "_");
        handleChange("viberMessage", nextValue as any);
    };

    const applyViberStrikethrough = () => {
        const nextValue = wrapSelectedText(viberMessageInputRef.current, formData.viberMessage, "~", "~");
        handleChange("viberMessage", nextValue as any);
    };

    const insertViberSticker = (sticker: string) => {
        const nextValue = insertAtCursor(viberMessageInputRef.current, formData.viberMessage, ` ${sticker} `);
        handleChange("viberMessage", nextValue as any);
    };

    const handleSubmit = async () => {
        try {
            const viberMessage = migrateLegacyViberHtmlToMarkdown(formData.viberMessage) ?? formData.viberMessage;
            const payload = { ...formData, viberMessage };
            if (viberMessage !== formData.viberMessage) {
                setFormData(payload);
            }

            await updateCampaign(payload, authFetch);

            if (formData.isViber && media && media instanceof File) {
                await uploadCampaignViberMedia(formData.campaignId, false, media, authFetch, duration);
            }

            if (formData.isViber && thumbnail && thumbnail instanceof File) {
                await uploadCampaignViberMedia(formData.campaignId, true, thumbnail, authFetch);
            }

            window.scrollTo({ top: 0, behavior: "smooth" });

            toast({
                variant: "success",
                title: tCommon("success"),
                description: t("toasts.saved"),
                duration: 2000
            });
        } catch (e) {
            console.error(e);
        }
    };

    const checkIsThumbnailVisible = (): boolean => {
        if (!media) return false;

        if (typeof media === "string") return getMediaType(media) === CampaignMediaType.Video;

        const isFileExtended = (obj: any): obj is FileExtended => obj && typeof obj === "object" && "preview" in obj;

        if (isFileExtended(media)) return ["video/mp4", "video/x-msvideo"].includes(media.type);

        return false;
    };

    useEffect(() => {
        (async () => {
            if (!dashboardManager?.currentCompany?.companyId) return;
            const data = await getCompanySenders(dashboardManager.currentCompany.companyId, authFetch);
            setSenders(data);
        })();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        if (formData.viberMedia) setMedia(formData.viberMedia);
        if (formData.viberVideoThumbnail) setThumbnail(formData.viberVideoThumbnail);
    }, [formData.viberMedia, formData.viberVideoThumbnail]);

    useEffect(() => {
        const runUseEffect = async () => {
            if (!formData?.campaignId || formData.status !== CampaignStatus.Draft) return;

            await updateCampaign(formData, authFetch);

            if (media && media instanceof File) {
                const data = await uploadCampaignViberMedia(formData.campaignId, false, media, authFetch, duration);
                if (data.path) handleChange("viberMedia", data.path as any);
            }

            if (thumbnail && thumbnail instanceof File) {
                const data = await uploadCampaignViberMedia(formData.campaignId, true, thumbnail, authFetch);
                if (data.path) handleChange("viberVideoThumbnail", data.path as any);
            }

            await updateCampaign(formData, authFetch);
        };

        void runUseEffect();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [media, thumbnail]);

    const scheduledDateFormat = locale === "sr" ? "dd.MM.yyyy. HH:mm" : "dd/MM/yyyy HH:mm";
    const datepickerLocale = locale === "sr" ? "sr" : "en";

    return (
        <div>
            <section className="grid grid-cols-1 gap-y-8 gap-x-14 md:grid-cols-3">
                <div>
                    <h2 id="campaign-general" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {t("general.title")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{t("general.subtitle")}</p>
                </div>

                <div className="md:col-span-2">
                    <div className="mb-4">
                        <Label>{t("fields.sendingType.label")}</Label>
                        <Select
                            disabled={formData.status !== CampaignStatus.Draft}
                            value={formData.sendingType?.toString()}
                            onValueChange={(sendingType) => {
                                const newType = convertStringToEnum<CampaignSendingType>(sendingType);
                                if (newType === CampaignSendingType.ScheduledDateTime) {
                                    const defaultSendAt = new Date();
                                    defaultSendAt.setMinutes(defaultSendAt.getMinutes() + 30);
                                    setFormData((prev) => ({
                                        ...prev,
                                        sendingType: newType,
                                        sendingDatetimeObject: defaultSendAt
                                    }));
                                } else {
                                    handleChange("sendingType", newType);
                                }
                            }}
                        >
                            <SelectTrigger id="sendingType" className="mt-2">
                                <SelectValue placeholder={tCommon("select")} />
                            </SelectTrigger>
                            <SelectContent>
                                {sendingTypeOptions.map((item) => (
                                    <SelectItem key={item.value} value={item.value.toString()}>
                                        {item.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {formData.sendingType === CampaignSendingType.ScheduledDateTime && (
                        <div className="mb-4">
                            <Label className="block">{t("fields.sendingDatetime.label")}</Label>
                            <DatePicker
                                locale={datepickerLocale}
                                disabled={formData.status !== CampaignStatus.Draft}
                                selected={formData.sendingDatetimeObject}
                                showTimeSelect
                                timeFormat="HH:mm"
                                timeIntervals={15}
                                minDate={new Date()}
                                filterTime={filterPassedTime}
                                dateFormat={scheduledDateFormat}
                                className="text-sm text-gray-900 dark:text-gray-50 bg-white dark:bg-gray-950 rounded-md border-gray-300 dark:border-gray-800 disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-400 disabled:dark:border-gray-700 disabled:dark:bg-gray-800 disabled:dark:text-gray-500"
                                wrapperClassName="mt-3"
                                onChange={(date) => {
                                    if (date) handleChange("sendingDatetimeObject", date as any);
                                }}
                            />
                        </div>
                    )}

                    <Label>{t("fields.channel.label")}</Label>

                    <div className="flex items-center gap-2 mt-2 mb-4">
                        <Checkbox
                            disabled={formData.status !== CampaignStatus.Draft}
                            checked={formData.isViber}
                            onCheckedChange={(checked) => {
                                handleChange("isViber", !!checked);
                                handleChange("isSms", false);
                            }}
                        />
                        <Label>{t("fields.channel.viber")}</Label>

                        <Checkbox
                            disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                            className="ml-4"
                            checked={formData.fallbackToSMS}
                            onCheckedChange={(checked) => handleChange("fallbackToSMS", !!checked)}
                        />
                        <Label>{t("fields.channel.fallbackToSms")}</Label>
                    </div>

                    <div className="flex items-center gap-2 mb-4">
                        <Checkbox
                            disabled={formData.status !== CampaignStatus.Draft}
                            checked={formData.isSms}
                            onCheckedChange={(checked) => {
                                handleChange("isSms", !!checked);
                                handleChange("isViber", false);
                            }}
                        />
                        <Label>{t("fields.channel.sms")}</Label>
                    </div>
                </div>
            </section>

            {formData.status === CampaignStatus.Draft && formData.isViber && isOutOfWorkingHours(new Date()) ? (
                <p className="mt-2 text-center text-white bg-orange-400 dark:bg-orange-500 p-2 rounded">
                    {t("warnings.viberWorkingHours")}
                </p>
            ) : null}

            {!formData.isSms && (
                <>
                    <Divider />

                    <section className="grid grid-cols-1 gap-y-8 gap-x-14 md:grid-cols-3">
                        <div>
                            <h2 id="campaign-viber" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                                {t("viber.title")}
                            </h2>
                            <p className="mt-1 text-sm leading-6 text-gray-500">{t("viber.subtitle")}</p>
                        </div>

                        <div className="md:col-span-2">
                            <div className="mb-4">
                                <Label>{t("viber.fields.sender")}</Label>
                                <Select
                                    disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                    value={formData.viberSender}
                                    onValueChange={(viberSender) => handleChange("viberSender", viberSender as any)}
                                >
                                    <SelectTrigger id="viberSender" className="mt-2">
                                        <SelectValue placeholder={tCommon("select")} />
                                    </SelectTrigger>

                                    <SelectContent>
                                        {senders.filter((s) => s.type === SenderType.Viber).length > 0 ? (
                                            senders
                                                .filter((s) => s.type === SenderType.Viber)
                                                .map((sender) => (
                                                    <SelectItem key={sender.id} value={sender.name}>
                                                        {sender.name}
                                                    </SelectItem>
                                                ))
                                        ) : (
                                            <p className="my-3 text-center text-sm">{tCommon("noData")}</p>
                                        )}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="mb-4">
                                <Label>{t("viber.fields.message")}</Label>

                                <p className="mb-2 max-w-3xl text-xs leading-relaxed text-gray-500 dark:text-gray-400">
                                    {t("viber.tooltips.formattingHint")}
                                </p>

                                <div className="mt-1 mb-2 flex flex-wrap items-center gap-2">
                                    <Button
                                        type="button"
                                        variant="secondary"
                                        title="*bold*"
                                        disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                        onClick={applyViberBold}
                                    >
                                        <strong>B</strong>
                                    </Button>

                                    <Button
                                        type="button"
                                        variant="secondary"
                                        title="_italic_"
                                        disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                        onClick={applyViberItalic}
                                    >
                                        <em>I</em>
                                    </Button>

                                    <Button
                                        type="button"
                                        variant="secondary"
                                        title={t("viber.formatting.strikethroughTitle")}
                                        disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                        onClick={applyViberStrikethrough}
                                    >
                                        <span className="line-through">S</span>
                                    </Button>

                                    <span className="inline-flex">
                                        <Tooltip side="top" content={t("viber.tooltips.underlineNotSupported")}>
                                            <span className="inline-flex cursor-default">
                                                <Button type="button" variant="secondary" disabled className="pointer-events-none opacity-60">
                                                    <span className="underline">U</span>
                                                </Button>
                                            </span>
                                        </Tooltip>
                                    </span>
                                </div>

                                <div className="mb-2 flex flex-wrap items-center gap-2">
                                    {viberStickers.map((sticker) => (
                                        <button
                                            key={sticker}
                                            type="button"
                                            disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                            className="rounded-md border border-gray-300 px-2 py-1 text-lg hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-700 dark:hover:bg-gray-800"
                                            onClick={() => insertViberSticker(sticker)}
                                        >
                                            {sticker}
                                        </button>
                                    ))}
                                </div>

                                <Textarea
                                    disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                    value={formData.viberMessage}
                                    ref={viberMessageInputRef}
                                    maxLength={1000}
                                    className="h-[200px] resize-none"
                                    onChange={(e) => handleChange("viberMessage", e.target.value as any)}
                                />

                                <small>
                                    {(formData.viberMessage?.length ?? 0)} / 1000 {t("viber.fields.characters")}
                                </small>

                                {formData.viberMessage && formData.viberMessage.trim().length > 0 && (
                                    <div className="mt-3 rounded-md border border-gray-200 bg-gray-50 p-3 text-sm dark:border-gray-700 dark:bg-gray-900/50">
                                        <div className="mb-1 text-xs font-medium text-gray-500 dark:text-gray-400">
                                            {t("viber.formatting.previewLabel")}
                                        </div>
                                        <div
                                            className="whitespace-pre-wrap break-words text-gray-900 dark:text-gray-100"
                                            // eslint-disable-next-line react/no-danger -- own campaign message preview only
                                            dangerouslySetInnerHTML={{
                                                __html: viberMarkdownToPreviewHtml(formData.viberMessage)
                                            }}
                                        />
                                    </div>
                                )}
                            </div>

                            <GptModal
                                disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                type="Viber"
                                campaignId={formData.campaignId}
                                message={formData.viberMessage}
                                setMessage={(message) => handleChange("viberMessage", message as any)}
                            />

                            <VariablesForm
                                disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                inputRef={viberMessageInputRef}
                                message={formData.viberMessage}
                                handleChange={(newMsg) => handleChange("viberMessage", newMsg as any)}
                            />

                            <div className="mb-4 flex items-center gap-2">
                                <Checkbox
                                    disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                    checked={formData.isTransactional as any}
                                    onCheckedChange={(checked) => {
                                        handleChange("isTransactional" as any, !!checked as any);
                                        setMedia(null);
                                        handleChange("viberMedia" as any, null as any);
                                        handleChange("viberButtonUrl" as any, null as any);
                                        handleChange("viberButtonUrlTitle" as any, null as any);
                                    }}
                                />
                                <Label>
                                    <Tooltip side="right" content={t("viber.tooltips.transactional")}>
                                        {t("viber.fields.transactional")}
                                    </Tooltip>
                                </Label>
                            </div>

                            <div className="mb-4">
                                <Label>{t("viber.fields.media")}</Label>

                                {formData.status === CampaignStatus.Draft && (
                                    <div className="flex items-center gap-4 mt-2 mb-4">
                                        <Dialog open={imageModalOpen} onOpenChange={setImageModalOpen}>
                                            <DialogTrigger asChild>
                                                <Button
                                                    disabled={formData.isTransactional as any}
                                                    className="h-8 text-sm border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                                                    type="button"
                                                    variant="primary"
                                                >
                                                    {t("viber.actions.selectExistingMedia")}
                                                </Button>
                                            </DialogTrigger>

                                            <DialogContent className="sm:max-w-lg" aria-describedby="select existing media dialog">
                                                <DialogHeader>
                                                    <DialogTitle>{t("viber.actions.selectImageTitle")}</DialogTitle>
                                                </DialogHeader>

                                                <div className="w-full max-h-[50vh] overflow-y-auto grid grid-cols-3 gap-2 pr-2 mt-2">
                                                    {companyImages.map((image, index) => (
                                                        <div
                                                            key={index}
                                                            className="p-1 border border-gray-300 dark:border-gray-800 rounded-md cursor-pointer"
                                                            onClick={() => {
                                                                setMedia(image);
                                                                handleChange("viberMedia" as any, image as any);
                                                                setImageModalOpen(false);
                                                            }}
                                                        >
                                                            <img
                                                                src={`${process.env.NEXT_PUBLIC_API_URL}/uploads/${image}`}
                                                                alt={typeof media === "string" ? media : t("viber.actions.imageAlt")}
                                                                width={100}
                                                                height={100}
                                                                className="w-full max-h-[150px] object-contain"
                                                                onLoad={() => {
                                                                    if (media && media instanceof File) {
                                                                        URL.revokeObjectURL((media as any).preview);
                                                                    }
                                                                }}
                                                            />
                                                        </div>
                                                    ))}
                                                </div>

                                                <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                                                    <DialogClose asChild>
                                                        <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary">
                                                            {tCommon("goBack")}
                                                        </Button>
                                                    </DialogClose>
                                                </DialogFooter>
                                            </DialogContent>
                                        </Dialog>
                                    </div>
                                )}

                                <MediaUpload
                                    file={media}
                                    setFile={setMedia}
                                    setDuration={setDuration}
                                    handleChange={handleChange as any}
                                    imageOnly={false}
                                    disabled={!formData.isViber || (formData.isTransactional as any) || formData.status !== CampaignStatus.Draft}
                                />
                            </div>

                            {checkIsThumbnailVisible() ? (
                                <div className="mb-4">
                                    <Label>{t("viber.fields.thumbnail")}</Label>
                                    <MediaUpload
                                        file={thumbnail}
                                        setFile={setThumbnail}
                                        imageOnly={true}
                                        disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                    />
                                </div>
                            ) : null}

                            <div className="mb-4">
                                <Label>{t("viber.fields.buttonUrl")}</Label>
                                <Input
                                    disabled={!formData.isViber || (formData.isTransactional as any) || formData.status !== CampaignStatus.Draft}
                                    value={formData.viberButtonUrl as any}
                                    onChange={(e) => handleChange("viberButtonUrl" as any, e.target.value as any)}
                                />
                            </div>

                            <div className="mb-4">
                                <Label>{t("viber.fields.buttonText")}</Label>
                                <Input
                                    disabled={!formData.isViber || (formData.isTransactional as any) || formData.status !== CampaignStatus.Draft}
                                    value={formData.viberButtonUrlTitle as any}
                                    onChange={(e) => handleChange("viberButtonUrlTitle" as any, e.target.value as any)}
                                />
                            </div>

                            <div className="mb-4 flex items-center gap-2">
                                <Checkbox
                                    disabled={!formData.isViber || (formData.isTransactional as any) || formData.status !== CampaignStatus.Draft}
                                    checked={formData.isViberReceivable}
                                    onCheckedChange={(checked) => handleChange("isViberReceivable", !!checked as any)}
                                />
                                <Label>
                                    <Tooltip side="right" content={t("viber.tooltips.enableReplies")}>
                                        {t("viber.fields.enableReplies")}
                                    </Tooltip>
                                </Label>
                            </div>

                            <div className="mb-4">
                                <Label>{t("viber.fields.validity")}</Label>
                                <Input
                                    type="number"
                                    disabled={!formData.isViber || formData.status !== CampaignStatus.Draft}
                                    value={formData.viberValidity as any}
                                    min={0}
                                    onChange={(e) => handleChange("viberValidity" as any, Number(e.target.value) as any)}
                                />
                            </div>
                        </div>
                    </section>
                </>
            )}

            {(!formData.isViber || formData.fallbackToSMS) && (
                <>
                    <Divider />

                    <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                        <div>
                            <h2 id="campaign-sms" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                                {t("sms.title")}
                            </h2>
                            <p className="mt-1 text-sm leading-6 text-gray-500">{t("sms.subtitle")}</p>
                        </div>

                        <div className="md:col-span-2">
                            <div className="mb-4">
                                <Label>{t("sms.fields.sender")}</Label>
                                <Select
                                    disabled={(!formData.isSms && !formData.fallbackToSMS) || formData.status !== CampaignStatus.Draft}
                                    value={formData.smsSender}
                                    onValueChange={(smsSender) => handleChange("smsSender", smsSender as any)}
                                >
                                    <SelectTrigger id="smsSender" className="mt-2">
                                        <SelectValue placeholder={tCommon("select")} />
                                    </SelectTrigger>

                                    <SelectContent>
                                        {senders.filter((s) => s.type === SenderType.SMS).length > 0 ? (
                                            senders
                                                .filter((s) => s.type === SenderType.SMS)
                                                .map((sender) => (
                                                    <SelectItem key={sender.id} value={sender.name}>
                                                        {sender.name}
                                                    </SelectItem>
                                                ))
                                        ) : (
                                            <p className="my-3 text-center text-sm">{tCommon("noData")}</p>
                                        )}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="mb-4">
                                <Label>{t("sms.fields.message")}</Label>
                                <Textarea
                                    disabled={(!formData.isSms && !formData.fallbackToSMS) || formData.status !== CampaignStatus.Draft}
                                    value={formData.smsMessage}
                                    ref={smsMessageInputRef}
                                    maxLength={160}
                                    className="h-[100px] resize-none"
                                    onChange={(e) => handleChange("smsMessage", e.target.value as any)}
                                />
                                <small>
                                    {(formData.smsMessage?.length ?? 0)} / 160 {t("sms.fields.characters")}
                                </small>
                            </div>

                            <GptModal
                                disabled={(!formData.isSms && !formData.fallbackToSMS) || formData.status !== CampaignStatus.Draft}
                                type="SMS"
                                campaignId={formData.campaignId}
                                message={formData.smsMessage}
                                setMessage={(message) => handleChange("smsMessage", message as any)}
                            />

                            <VariablesForm
                                disabled={(!formData.isSms && !formData.fallbackToSMS) || formData.status !== CampaignStatus.Draft}
                                inputRef={smsMessageInputRef}
                                message={formData.smsMessage}
                                handleChange={(newMsg) => handleChange("smsMessage", newMsg as any)}
                            />
                        </div>
                    </section>
                </>
            )}

            <Divider />

            <div className="flex justify-end">
                <Button
                    disabled={formData.status !== CampaignStatus.Draft}
                    type="button"
                    color="primary"
                    className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                    onClick={handleSubmit}
                >
                    {t("actions.save")}
                </Button>
            </div>
        </div>
    );
}
