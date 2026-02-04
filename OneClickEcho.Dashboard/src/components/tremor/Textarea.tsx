// Tremor Textarea [v0.0.2]

import React from "react";

import { cx, focusInput, hasErrorInput } from "@/lib/utils";

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
    hasError?: boolean;
}

const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
    ({ className, hasError, value, ...props }, forwardedRef) => {
        // React warning fix: textarea value must not be null
        const safeValue = value === null ? "" : value;

        return (
            <textarea
                ref={forwardedRef}
                className={cx(
                    // base
                    "flex min-h-[4rem] w-full rounded-md border px-3 py-1.5 shadow-sm outline-none transition-colors sm:text-sm",
                    // margin
                    "mt-1",
                    // text color
                    "text-gray-900 dark:text-gray-50",
                    // border color
                    "border-gray-300 dark:border-gray-800",
                    // background color
                    "bg-white dark:bg-gray-950",
                    // placeholder color
                    "placeholder-gray-400 dark:placeholder-gray-500",
                    // disabled
                    "disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-300",
                    "disabled:dark:border-gray-700 disabled:dark:bg-gray-800 disabled:dark:text-gray-500",
                    // focus
                    focusInput,
                    // error
                    hasError ? hasErrorInput : "",
                    className
                )}
                tremor-id="tremor-raw"
                value={safeValue as any}
                {...props}
            />
        );
    }
);

Textarea.displayName = "Textarea";

export { Textarea, type TextareaProps };
