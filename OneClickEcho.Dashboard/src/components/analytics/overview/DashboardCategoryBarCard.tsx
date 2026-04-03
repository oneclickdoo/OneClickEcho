"use client";

import { useTranslations } from "next-intl";

import { Badge } from "@/components/tremor/Badge";
import { cx } from "@/lib/utils";
import type { KpiEntryExtended } from "@/data/schema";

export type CardProps = {
    title: string;
    change?: string;
    value: string;
    valueDescription: string;
    subtitle: string;
    ctaDescription?: string;
    ctaText?: string;
    ctaLink?: string;
    data: KpiEntryExtended[];
};

export function CategoryBarCard({
    title,
    change,
    value,
    valueDescription,
    subtitle,
    ctaDescription,
    ctaText,
    ctaLink,
    data
}: CardProps) {
    const tKpi = useTranslations("Kpi");

    return (
        <div className="flex flex-col justify-between">
            <div>
                <div className="flex items-center gap-2">
                    <h3 className="font-bold text-gray-900 dark:text-gray-50 sm:text-sm">{title}</h3>
                    {change ? <Badge variant="neutral">{change}</Badge> : null}
                </div>

                <p className="flex items-baseline gap-2 mt-2">
                    <span className="text-xl text-gray-900 dark:text-gray-50">{value}</span>
                    <span className="text-sm text-gray-500">{valueDescription}</span>
                </p>

                <div className="mt-4">
                    <p className="font-medium text-sm text-gray-900 dark:text-gray-50">{subtitle}</p>

                    <div className="flex items-center gap-0.5 mt-2">
                        {data
                            .filter((item) => item.key !== "unsubscribed")
                            .map((item) => (
                                <div
                                    key={item.key}
                                    className={cx(item.color, "h-1.5 rounded-full")}
                                    style={{ width: `${item.percentage}%` }}
                                />
                            ))}
                    </div>
                </div>

                <ul role="list" className="mt-5 space-y-2">
                    {data.map((item) => (
                        <li key={item.key} className="flex items-center gap-2 text-xs">
                            <span className={cx(item.color, "size-2.5 rounded-sm")} aria-hidden />
                            <span className="text-gray-900 dark:text-gray-50">{tKpi(item.key)}</span>
                            <span className="text-gray-600 dark:text-gray-400">
                                ({item.value} / {item.percentage}%)
                            </span>
                        </li>
                    ))}
                </ul>
            </div>

            {ctaDescription && ctaText && ctaLink ? (
                <p className="mt-6 text-xs text-gray-500">
                    {ctaDescription}{" "}
                    <a href={ctaLink} className="text-indigo-600 dark:text-indigo-400">
                        {ctaText}
                    </a>
                </p>
            ) : null}
        </div>
    );
}
