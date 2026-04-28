import { cx } from "@/lib/utils";

/** Small “live” dot for campaign status PreparingLaunch (server building lead snapshot). */
export function CampaignPreparingLaunchIndicator({ className }: { className?: string }) {
    return (
        <span className={cx("relative inline-flex h-2 w-2 shrink-0", className)} aria-hidden>
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-violet-400 opacity-60" />
            <span className="relative inline-flex h-2 w-2 rounded-full bg-violet-500 dark:bg-violet-400" />
        </span>
    );
}
