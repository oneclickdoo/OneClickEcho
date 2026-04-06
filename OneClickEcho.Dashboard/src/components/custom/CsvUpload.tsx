import type { MutableRefObject, Ref, RefObject } from "react";
import { useCallback, useMemo } from "react";

import { useDropzone, FileWithPath } from "react-dropzone";
import { RiFileTextLine } from "@remixicon/react";

import { cx } from "@/lib/utils";
import { Card } from "../tremor/Card";

interface ICsvUploadProps {
    file: FileWithPath | null;
    setFile: (file: FileWithPath | null) => void;
    refObject?: RefObject<HTMLInputElement>;
    disabled?: boolean;
}

function assignRef<T>(ref: Ref<T> | undefined, value: T | null) {
    if (!ref) return;
    if (typeof ref === "function") {
        ref(value);
    } else {
        (ref as MutableRefObject<T | null>).current = value;
    }
}

export const CsvUpload = (props: ICsvUploadProps) => {
    const onDrop = useCallback(
        (acceptedFiles: FileWithPath[]) => {
            const next = acceptedFiles[0];
            if (next) props.setFile(next);
        },
        [props]
    );

    const { getRootProps, getInputProps, isDragActive } = useDropzone({
        disabled: props.disabled,
        accept: {
            "text/csv": [".csv"],
            "application/csv": [".csv"],
            "text/plain": [".csv"],
            "text/x-csv": [".csv"],
            "application/vnd.ms-excel": [".csv"],
            // Windows / browsers often report .csv as octet-stream
            "application/octet-stream": [".csv"]
        },
        multiple: false,
        onDrop
    });

    const rootClassName = useMemo(
        () =>
            cx(
                "dropzone flex flex-col justify-center items-center p-8 rounded-md border border-dashed border-gray-300 transition-colors text-gray-750 cursor-pointer select-none dark:text-gray-50",
                isDragActive && "border-indigo-500 bg-indigo-50/50 dark:border-indigo-400 dark:bg-indigo-950/30",
                props.disabled &&
                    "cursor-not-allowed border-gray-300 bg-gray-100 text-gray-400 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-500",
                props.file && "border-green-400 dark:border-green-600"
            ),
        [isDragActive, props.disabled, props.file]
    );

    const rawInputProps = getInputProps();
    const { ref: dropzoneInputRef, ...inputPropsRest } = rawInputProps as typeof rawInputProps & {
        ref?: Ref<HTMLInputElement>;
    };

    const mergeInputRef = useCallback(
        (node: HTMLInputElement | null) => {
            assignRef(dropzoneInputRef, node);
            if (props.refObject) {
                (props.refObject as MutableRefObject<HTMLInputElement | null>).current = node;
            }
        },
        [dropzoneInputRef, props.refObject]
    );

    return (
        <Card className="p-0">
            <div {...getRootProps({ className: rootClassName })}>
                <input {...inputPropsRest} ref={mergeInputRef} />
                <RiFileTextLine
                    className={cx("pointer-events-none size-[64px] shrink-0", props.file && "text-green-600")}
                    aria-hidden
                />
                <p className="pointer-events-none mt-3 text-center">Drag & drop your CSV, or click to select it.</p>
                {props.file ? (
                    <p className="mt-2 max-w-full truncate px-2 text-center text-sm font-medium text-green-700 dark:text-green-400">
                        {props.file.name}
                    </p>
                ) : null}
            </div>
        </Card>
    );
};
