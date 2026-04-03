// Tremor Toaster [v0.0.0]

import { useToast } from "@/lib/useToast";

import { Toast, ToastProvider, ToastViewport } from "./Toast";

const Toaster = () => {
    const { toasts } = useToast();

    return (
        <ToastProvider swipeDirection="right">
            {toasts.map(({ id, ...props }) => (
                <Toast key={id} {...props} />
            ))}
            <ToastViewport />
        </ToastProvider>
    );
};

export { Toaster };
