"use client";

import React from "react";
import type { Column } from "@tanstack/react-table";
import { RiAddLine, RiArrowDownSLine, RiCornerDownRightLine } from "@remixicon/react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/tremor/Button";
import { Checkbox } from "@/components/tremor/Checkbox";
import { Input } from "@/components/tremor/Input";
import { Label } from "@/components/tremor/Label";
import { Popover, PopoverClose, PopoverContent, PopoverTrigger } from "@/components/tremor/Popover";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/tremor/Select";
import { DateRange, DateRangePicker } from "@/components/tremor/DatePicker";

import { cx, focusRing } from "@/lib/utils";

export type ConditionFilter = {
    condition: string;
    value: [number | string, number | string];
};

type DateFilter = DateRange | undefined;
type FilterType = "select" | "checkbox" | "number" | "date";

interface DataTableFilterProps<TData, TValue> {
    // Next.js warning (serializable props) - this component is used inside client tree.
    // @ts-ignore
    column: Column<TData, TValue> | undefined;

    title?: string;

    options?: {
        label: string;
        value: string;
    }[];

    type?: FilterType;

    // eslint-disable-next-line no-unused-vars
    // @ts-ignore
    onFilter?: (filter: any, column: string, type: string) => void;

    // eslint-disable-next-line no-unused-vars
    // @ts-ignore
    formatter?: (value: any) => string;
}

const ColumnFiltersLabel = ({
    columnFilterLabels,
    className,
    andText,
    moreText
}: {
    columnFilterLabels: string[] | undefined;
    className?: string;
    andText: string;
    moreText: (count: number) => string;
}) => {
    if (!columnFilterLabels) return null;

    if (columnFilterLabels.length < 3) {
        return (
            <span className={cx("truncate", className)}>
                {columnFilterLabels.map((value, index) => (
                    <span key={value} className={cx("font-semibold text-indigo-600 dark:text-indigo-400")}>
                        {value}
                        {index < columnFilterLabels.length - 1 && ", "}
                    </span>
                ))}
            </span>
        );
    }

    return (
        <span className={cx("font-semibold text-indigo-600 dark:text-indigo-400", className)}>
            {columnFilterLabels[0]} {andText} {moreText(columnFilterLabels.length - 1)}
        </span>
    );
};

type FilterValues = string | string[] | ConditionFilter | undefined | DateRange;

export function DataTableFilter<TData, TValue>({
    column,
    title,
    options,
    onFilter,
    type = "select",
    formatter = (value) => value?.toString?.() ?? ""
}: DataTableFilterProps<TData, TValue>) {
    const t = useTranslations("Common");

    const columnFilters = column?.getFilterValue() as FilterValues;
    const [selectedValues, setSelectedValues] = React.useState<FilterValues>(columnFilters);

    const columnFilterLabels = React.useMemo(() => {
        if (!selectedValues) return undefined;

        if (Array.isArray(selectedValues)) {
            return selectedValues.map((value) => formatter(value));
        }

        if (typeof selectedValues === "string") {
            return [formatter(selectedValues)];
        }

        if (typeof selectedValues === "object" && "condition" in selectedValues) {
            const condition = options?.find((option) => option.value === selectedValues.condition)?.label;
            if (!condition) return undefined;

            if (!selectedValues.value?.[0] && !selectedValues.value?.[1]) return [`${condition}`];
            if (!selectedValues.value?.[1]) return [`${condition} ${formatter(selectedValues.value?.[0])}`];

            return [`${condition} ${formatter(selectedValues.value?.[0])} ${t("and")} ${formatter(selectedValues.value?.[1])}`];
        }

        return undefined;
    }, [selectedValues, options, formatter, t]);

    const getDisplayedFilter = () => {
        switch (type) {
            case "select":
                return (
                    <Select
                        value={selectedValues as string}
                        onValueChange={(value) => {
                            setSelectedValues(value);
                        }}
                    >
                        <SelectTrigger className="mt-2 sm:py-1">
                            <SelectValue placeholder={t("select")} />
                        </SelectTrigger>
                        <SelectContent>
                            {options?.map((item) => (
                                <SelectItem key={item.value} value={item.value}>
                                    {item.label}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                );

            case "checkbox":
                return (
                    <div className="mt-2 space-y-2 overflow-y-auto sm:max-h-36">
                        {options?.map((option) => (
                            <div key={option.label} className="flex items-center gap-2">
                                <Checkbox
                                    id={option.value}
                                    checked={(selectedValues as string[])?.includes(option.value)}
                                    onCheckedChange={(checked) => {
                                        setSelectedValues((prev) => {
                                            if (checked) {
                                                return prev ? [...(prev as string[]), option.value] : [option.value];
                                            }
                                            return (prev as string[]).filter((value) => value !== option.value);
                                        });
                                    }}
                                />
                                <Label htmlFor={option.value} className="text-base sm:text-sm">
                                    {option.label}
                                </Label>
                            </div>
                        ))}
                    </div>
                );

            case "number": {
                const isBetween = (selectedValues as ConditionFilter)?.condition === "is-between";

                return (
                    <div className="space-y-2">
                        <Select
                            value={(selectedValues as ConditionFilter)?.condition}
                            onValueChange={(value) => {
                                setSelectedValues((prev) => {
                                    return {
                                        condition: value,
                                        value: [value !== "" ? (prev as ConditionFilter)?.value?.[0] : "", ""]
                                    };
                                });
                            }}
                        >
                            <SelectTrigger className="mt-2 sm:py-1">
                                <SelectValue placeholder={t("selectCondition")} />
                            </SelectTrigger>
                            <SelectContent>
                                {options?.map((item) => (
                                    <SelectItem key={item.value} value={item.value}>
                                        {item.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>

                        <div className="mt-2 flex w-full items-center gap-2">
                            <RiCornerDownRightLine className="size-4 shrink-0 text-gray-500" aria-hidden="true" />
                            <Input
                                disabled={!(selectedValues as ConditionFilter)?.condition}
                                type="number"
                                placeholder={t("amountPlaceholder")}
                                className="sm:[&>input]:py-1"
                                value={(selectedValues as ConditionFilter)?.value?.[0]}
                                onChange={(e) => {
                                    setSelectedValues((prev) => {
                                        return {
                                            condition: (prev as ConditionFilter)?.condition,
                                            value: [e.target.value, isBetween ? (prev as ConditionFilter)?.value?.[1] : ""]
                                        };
                                    });
                                }}
                            />

                            {(selectedValues as ConditionFilter)?.condition === "is-between" && (
                                <>
                                    <span className="text-xs font-medium text-gray-500">{t("and")}</span>
                                    <Input
                                        disabled={!(selectedValues as ConditionFilter)?.condition}
                                        type="number"
                                        placeholder={t("amountPlaceholder")}
                                        className="sm:[&>input]:py-1"
                                        value={(selectedValues as ConditionFilter)?.value?.[1]}
                                        onChange={(e) => {
                                            setSelectedValues((prev) => {
                                                return {
                                                    condition: (prev as ConditionFilter)?.condition,
                                                    value: [(prev as ConditionFilter)?.value?.[0], e.target.value]
                                                };
                                            });
                                        }}
                                    />
                                </>
                            )}
                        </div>
                    </div>
                );
            }

            case "date":
                return (
                    <div className="space-y-2">
                        <div className="mt-2 flex w-full items-center gap-2">
                            <DateRangePicker value={selectedValues as DateFilter} onChange={setSelectedValues} />
                        </div>
                    </div>
                );
        }
    };

    React.useEffect(() => {
        setSelectedValues(columnFilters);
        if (onFilter && column?.id) {
            onFilter(columnFilters ? columnFilters : null, column.id, type);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [columnFilters]);

    return (
        <Popover>
            <PopoverTrigger asChild>
                <button
                    type="button"
                    className={cx(
                        "flex w-full items-center gap-x-1.5 whitespace-nowrap rounded-md border border-gray-300 px-2 py-1.5 font-medium text-gray-600 hover:bg-gray-50 sm:w-fit sm:text-xs dark:border-gray-700 dark:text-gray-400 hover:dark:bg-gray-900",
                        selectedValues &&
                            ((typeof selectedValues === "object" && "condition" in selectedValues && selectedValues.condition !== "") ||
                                (typeof selectedValues === "string" && selectedValues !== "") ||
                                (Array.isArray(selectedValues) && selectedValues.length > 0))
                            ? ""
                            : "border-dashed",
                        focusRing
                    )}
                >
                    <span
                        aria-hidden="true"
                        onClick={(e) => {
                            if (selectedValues) {
                                e.stopPropagation();
                                column?.setFilterValue("");
                                setSelectedValues("");
                            }
                        }}
                    >
                        <RiAddLine
                            className={cx("-ml-px size-5 shrink-0 transition sm:size-4", selectedValues && "rotate-45 hover:text-red-500")}
                            aria-hidden="true"
                        />
                    </span>

                    {columnFilterLabels && columnFilterLabels.length > 0 ? (
                        <span>{title}</span>
                    ) : (
                        <span className="w-full text-left sm:w-fit">{title}</span>
                    )}

                    {columnFilterLabels && columnFilterLabels.length > 0 && (
                        <span className="h-4 w-px bg-gray-300 dark:bg-gray-700" aria-hidden="true" />
                    )}

                    <ColumnFiltersLabel
                        columnFilterLabels={columnFilterLabels}
                        className="w-full text-left sm:w-fit"
                        andText={t("and")}
                        moreText={(count) => t("moreCount", { count })}
                    />

                    <RiArrowDownSLine className="size-5 shrink-0 text-gray-500 sm:size-4" aria-hidden="true" />
                </button>
            </PopoverTrigger>

            <PopoverContent
                align="start"
                sideOffset={7}
                className="min-w-[calc(var(--radix-popover-trigger-width))] max-w-[calc(var(--radix-popover-trigger-width))] sm:min-w-56 sm:max-w-56"
                onInteractOutside={() => {
                    if (
                        !columnFilters ||
                        (typeof columnFilters === "string" && columnFilters === "") ||
                        (Array.isArray(columnFilters) && columnFilters.length === 0) ||
                        (typeof columnFilters === "object" && "condition" in columnFilters && columnFilters.condition === "")
                    ) {
                        column?.setFilterValue("");
                        setSelectedValues("");
                    }
                }}
            >
                <form
                    onSubmit={(e) => {
                        e.preventDefault();
                        column?.setFilterValue(selectedValues);
                    }}
                >
                    <div className="space-y-2">
                        <div>
                            <Label className="text-base font-medium sm:text-sm">
                                {t("filterBy", { title: title ?? "" })}
                            </Label>
                            {getDisplayedFilter()}
                        </div>

                        <PopoverClose className="w-full" asChild>
                            <Button type="submit" className="w-full sm:py-1">
                                {t("apply")}
                            </Button>
                        </PopoverClose>

                        {columnFilterLabels && columnFilterLabels.length > 0 && (
                            <Button
                                variant="secondary"
                                className="w-full sm:py-1"
                                type="button"
                                onClick={() => {
                                    column?.setFilterValue("");
                                    setSelectedValues(type === "checkbox" ? [] : type === "number" ? { condition: "", value: ["", ""] } : "");
                                }}
                            >
                                {t("reset")}
                            </Button>
                        )}
                    </div>
                </form>
            </PopoverContent>
        </Popover>
    );
}
