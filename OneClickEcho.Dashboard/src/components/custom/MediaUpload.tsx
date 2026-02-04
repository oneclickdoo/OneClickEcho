import { useState, useEffect } from "react";

import { useDropzone, FileWithPath } from "react-dropzone";
import { RiDeleteBinLine } from "@remixicon/react";

import { Card } from "../tremor/Card";

import { getMediaType } from "@/lib/utils";

import { CampaignMediaType } from "@/lib/enums";

export interface FileExtended extends FileWithPath {
    preview: string;
}

interface IMediaUploadProps {
    file: FileExtended | string | null;
    setFile: Function;
    setDuration?: Function;
    imageOnly: boolean;
    disabled?: boolean;
    handleChange?: Function;
}

export const MediaUpload = (props: IMediaUploadProps) => {
    const [mediaType, setMediaType] = useState<CampaignMediaType>(CampaignMediaType.Image);

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

            props.setFile(
                Object.assign(file, {
                    preview: URL.createObjectURL(file)
                })
            );
        }
    });

    useEffect(() => {
        if (props.file && typeof props.file === "string") {
            setMediaType(getMediaType(props.file));
        }

        // make sure to revoke the data uris to avoid memory leaks, will run on unmount
        return () => {
            if (props.file && props.file instanceof File) {
                URL.revokeObjectURL(props.file.preview);
            }
        };
    }, [props.file]);

    return (
        <Card className="mt-1">
            {props.file ? (
                <aside className="mt-4 flex items-center gap-x-3">
                    <div className="flex justify-center w-[200px] h-[200px] p-1 box-border border border-gray-200 rounded-md">
                        <div className="flex min-w-0 overflow-hidden">
                            {mediaType === CampaignMediaType.Image ? (
                                <img
                                    src={
                                        props.file instanceof File
                                            ? props.file.preview
                                            : `${process.env.NEXT_PUBLIC_API_URL}/uploads/${props.file}`
                                    }
                                    alt={props.file instanceof File ? props.file.name : "Campaign Viber Image"}
                                    width={100}
                                    height={100}
                                    className="block w-auto h-full"
                                    onLoad={() => {
                                        if (props.file && props.file instanceof File) {
                                            URL.revokeObjectURL(props.file.preview);
                                        }
                                    }}
                                />
                            ) : (
                                <video
                                    src={
                                        props.file instanceof File
                                            ? props.file.preview
                                            : `${process.env.NEXT_PUBLIC_API_URL}/uploads/${props.file}`
                                    }
                                    width={100}
                                    height={100}
                                    controls
                                    className="block w-auto h-full"
                                    onLoad={() => {
                                        if (props.file && props.file instanceof File) {
                                            URL.revokeObjectURL(props.file.preview);
                                        }
                                    }}
                                    onLoadedMetadata={(e) => {
                                        if ("duration" in e.target && props.setDuration)
                                            props.setDuration(Math.round(e.target.duration as number));
                                    }}
                                >
                                    Your browser does not support the video tag.
                                </video>
                            )}
                        </div>
                    </div>
                    <RiDeleteBinLine
                        className="text-red-500 cursor-pointer hover:text-red-600"
                        size={24}
                        onClick={() => {
                            props.setFile(null);
                            if (props.handleChange) props.handleChange("viberMedia", null);
                        }}
                    />
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
                    <p className="text-center">
                        Drag & drop your {props.imageOnly ? "image" : "media"}, or click to select it. Recommended image dimensions are
                        500x500.
                    </p>
                </div>
            )}
        </Card>
    );
};
