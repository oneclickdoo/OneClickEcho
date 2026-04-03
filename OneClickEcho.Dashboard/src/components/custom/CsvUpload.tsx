import { RefObject } from "react";

import { useDropzone, FileWithPath } from "react-dropzone";
import { RiFileTextLine } from "@remixicon/react";

import { Card } from "../tremor/Card";

interface ICsvUploadProps {
    file: FileWithPath | null;
    setFile: Function;
    refObject?: RefObject<HTMLInputElement>;
    disabled?: boolean;
}

export const CsvUpload = (props: ICsvUploadProps) => {
    const { getRootProps, getInputProps } = useDropzone({
        disabled: props.disabled,
        accept: {
            "text/csv": [".csv"],
            "text/plain": [".csv"],
            "application/vnd.ms-excel": [".csv"]
        },
        multiple: false,
        onDrop: (acceptedFiles: FileWithPath[]) => {
            const file = acceptedFiles[0];
            props.setFile(file);
        }
    });

    return (
        <Card>
            <div
                {...getRootProps({ className: `dropzone` })}
                className={`flex flex-col justify-center items-center p-8 rounded-md border border-dashed border-gray-300 transition-colors text-gray-750 cursor-pointer dark:text-gray-50 ${props.disabled ? "border-gray-300 bg-gray-100 text-gray-400 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-500" : ""}`}
            >
                {props.refObject ? <input {...getInputProps()} ref={props.refObject} /> : <input {...getInputProps()} />}
                <RiFileTextLine className={`size-[64px]${props.file ? " text-green-600" : ""}`} />
                <p className="mt-3">Drag & drop your CSV, or click to select it.</p>
            </div>
        </Card>
    );
};
