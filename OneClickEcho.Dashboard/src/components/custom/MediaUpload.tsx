"use client";

import { useState, useEffect } from "react";

import { useDropzone, FileWithPath } from "react-dropzone";
import { useLocale, useTranslations } from "next-intl";
import { RiDeleteBinLine } from "@remixicon/react";

import { Card } from "../tremor/Card";

import { publicUploadFileUrl } from "@/lib/publicMediaUrl";
import { getMediaType } from "@/lib/utils";

import { CampaignMediaType } from "@/lib/enums";

export interface FileExtended extends FileWithPath {
    preview: string;
}

export type SavedVideoMeta = {
    fileSizeBytes?: number | null;
    durationSeconds?: number | null;
};

interface IMediaUploadProps {
    file: FileExtended | string | null;
    setFile: Function;
    setDuration?: Function;
    imageOnly: boolean;
    disabled?: boolean;
    handleChange?: Function;
    /** When `file` is a server path (string), show size/duration from campaign. */
    savedVideoMeta?: SavedVideoMeta | null;
}

function formatFileSize(bytes: number, _locale: string): string {
    if (!Number.isFinite(bytes) || bytes < 0) {
        return "—";
    }
    if (bytes < 1024) {
        return `${bytes} B`;
    }
    const kb = bytes / 1024;
    if (kb < 1024) {
        return `${kb.toFixed(1)} KB`;
    }
    const mb = kb / 1024;
    return `${mb.toFixed(1)} MB`;
}

function formatDuration(seconds: number, locale: string): string {
    if (!Number.isFinite(seconds) || seconds < 0) {
        return "—";
    }
    const rounded = Math.round(seconds);
    const m = Math.floor(rounded / 60);
    const s = rounded % 60;
    if (m <= 0) {
        return new Intl.NumberFormat(locale, { maximumFractionDigits: 0 }).format(rounded) + "s";
    }
    return `${m}:${s.toString().padStart(2, "0")}`;
}

function isVideoMedia(file: FileExtended | string | null): boolean {
    if (!file) {
        return false;
    }
    if (file instanceof File) {
        return file.type.startsWith("video/");
    }
    try {
        return getMediaType(file) === CampaignMediaType.Video;
    } catch {
        return false;
    }
}

export const MediaUpload = (props: IMediaUploadProps) => {
    const t = useTranslations("SingleCampaign.Tabs.CampaignMessaging.viber.mediaUpload");
    const locale = useLocale();
    const [mediaType, setMediaType] = useState<CampaignMediaType>(CampaignMediaType.Image);
    const [detectedDurationSec, setDetectedDurationSec] = useState<number | null>(null);

    const { getRootProps, getInputProps } = useDropzone({
        disabled: props.disabled,
        accept: {
            "image/jpeg": [".jpg", ".jpeg"],
            "image/png": [".png"],
            "video/mp4": [".mp4"]
        },
        multiple: false,
        onDrop: (acceptedFiles: FileWithPath[]) => {
            const file = acceptedFiles[0];

            if (file.type.startsWith("video/")) {
                setMediaType(CampaignMediaType.Video);
            }

            props.setDuration?.(undefined);

            props.setFile(
                Object.assign(file, {
                    preview: URL.createObjectURL(file)
                })
            );
        }
    });

    useEffect(() => {
        setDetectedDurationSec(null);
    }, [props.file]);

    useEffect(() => {
        if (props.file && typeof props.file === "string") {
            try {
                setMediaType(getMediaType(props.file));
            } catch {
                const s = props.file.toLowerCase();
                setMediaType(/\.(mp4|avi)(\?|#|$)/i.test(s) ? CampaignMediaType.Video : CampaignMediaType.Image);
            }
        }

        return () => {
            if (props.file && props.file instanceof File) {
                URL.revokeObjectURL(props.file.preview);
            }
        };
    }, [props.file]);

    const showVideoMeta = !props.imageOnly && isVideoMedia(props.file);
    let fileSizeLabel: string | null = null;
    let durationLabel: string | null = null;

    if (showVideoMeta) {
        if (props.file instanceof File) {
            fileSizeLabel = formatFileSize(props.file.size, locale);
            durationLabel =
                detectedDurationSec != null ? formatDuration(detectedDurationSec, locale) : t("durationPending");
        } else if (typeof props.file === "string" && props.savedVideoMeta) {
            const sz = props.savedVideoMeta.fileSizeBytes;
            const dur = props.savedVideoMeta.durationSeconds;
            fileSizeLabel = sz != null && sz > 0 ? formatFileSize(sz, locale) : "—";
            durationLabel = dur != null && dur > 0 ? formatDuration(dur, locale) : "—";
        }
    }

    const dropzoneText = props.imageOnly ? t("dropzoneImage") : t("dropzoneMedia");

    return (
        <Card className="mt-1">
            {props.file ? (
                <aside className="mt-4 flex flex-col gap-2">
                    <div className="flex items-center gap-x-3">
                        <div className="flex justify-center w-[200px] h-[200px] p-1 box-border border border-gray-200 rounded-md dark:border-gray-700">
                            <div className="flex min-w-0 overflow-hidden">
                                {mediaType === CampaignMediaType.Image ? (
                                    <img
                                        src={
                                            props.file instanceof File
                                                ? props.file.preview
                                                : publicUploadFileUrl(props.file)
                                        }
                                        alt={props.file instanceof File ? props.file.name : "Campaign Viber Image"}
                                        width={100}
                                        height={100}
                                        className="block w-auto h-full"
                                    />
                                ) : (
                                    <video
                                        src={
                                            props.file instanceof File
                                                ? props.file.preview
                                                : publicUploadFileUrl(props.file)
                                        }
                                        width={100}
                                        height={100}
                                        controls
                                        className="block w-auto h-full"
                                        onLoadedMetadata={(e) => {
                                            const el = e.target as HTMLVideoElement;
                                            const raw = el.duration;
                                            if (!Number.isFinite(raw) || raw <= 0) {
                                                return;
                                            }
                                            const sec = Math.max(1, Math.round(raw));
                                            setDetectedDurationSec(sec);
                                            props.setDuration?.(sec);
                                        }}
                                    >
                                        {t("videoUnsupported")}
                                    </video>
                                )}
                            </div>
                        </div>
                        <RiDeleteBinLine
                            className="text-red-500 cursor-pointer hover:text-red-600 shrink-0"
                            size={24}
                            onClick={() => {
                                props.setFile(null);
                                if (!props.handleChange) {
                                    return;
                                }
                                if (props.imageOnly) {
                                    props.handleChange("viberVideoThumbnail", null);
                                } else {
                                    props.handleChange("viberMedia", null);
                                    props.handleChange("viberFileSize", null);
                                    props.handleChange("viberVideoDuration", null);
                                }
                            }}
                        />
                    </div>
                    {showVideoMeta && fileSizeLabel && durationLabel ? (
                        <p className="text-xs text-gray-600 dark:text-gray-400 pl-0.5">
                            {t("videoMeta", { fileSize: fileSizeLabel, duration: durationLabel })}
                        </p>
                    ) : null}
                </aside>
            ) : (
                <div
                    {...getRootProps({ className: `dropzone` })}
                    className={[
                        "cursor-pointer flex flex-col items-center justify-center rounded-md border border-dashed border-gray-300 p-8 transition-colors",
                        "text-gray-750 dark:text-gray-50",
                        props.disabled
                            ? [
                                  "border-gray-300 bg-gray-100 text-gray-400",
                                  "dark:border-gray-700 dark:bg-gray-800 dark:text-gray-500"
                              ].join(" ")
                            : ""
                    ].join(" ")}
                >
                    <input {...getInputProps()} />
                    <p className="text-center text-sm">{dropzoneText}</p>
                </div>
            )}
        </Card>
    );
};
