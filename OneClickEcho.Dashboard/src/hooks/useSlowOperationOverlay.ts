import { useCallback, useRef, useState } from "react";

import type { IFetch } from "@/lib/networking";

const DEFAULT_DELAY_MS = 3000;

/**
 * Full-screen overlay only if some async work lasts longer than `delayMs`.
 * Supports parallel operations (ref-count) and resets the delay when one finishes but others remain.
 */
export function useSlowOperationOverlay(delayMs: number = DEFAULT_DELAY_MS) {
    const [visible, setVisible] = useState(false);
    const pendingRef = useRef(0);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    const clearTimer = useCallback(() => {
        if (timerRef.current !== null) {
            clearTimeout(timerRef.current);
            timerRef.current = null;
        }
    }, []);

    const armTimer = useCallback(() => {
        clearTimer();
        timerRef.current = setTimeout(() => {
            timerRef.current = null;
            if (pendingRef.current > 0) {
                setVisible(true);
            }
        }, delayMs);
    }, [clearTimer, delayMs]);

    const run = useCallback(
        async <T,>(fn: () => Promise<T>): Promise<T> => {
            pendingRef.current += 1;
            if (pendingRef.current === 1) {
                armTimer();
            }
            try {
                return await fn();
            } finally {
                pendingRef.current -= 1;
                if (pendingRef.current === 0) {
                    clearTimer();
                    setVisible(false);
                } else {
                    armTimer();
                }
            }
        },
        [armTimer, clearTimer]
    );

    const wrapFetch = useCallback(
        (inner: IFetch): IFetch => {
            return (input, init) => run(() => inner(input, init));
        },
        [run]
    );

    return { run, wrapFetch, visible };
}
