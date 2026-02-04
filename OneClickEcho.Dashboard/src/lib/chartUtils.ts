// Tremor Raw chartColors [v0.0.0]

export type ColorUtility = "bg" | "stroke" | "fill" | "text";

export const chartColors = {
    indigo: {
        bg: "bg-indigo-600 dark:bg-indigo-500",
        stroke: "stroke-indigo-600 dark:stroke-indigo-500",
        fill: "fill-indigo-600 dark:fill-indigo-500",
        text: "text-indigo-600 dark:text-indigo-500"
    },
    blue: {
        bg: "bg-blue-500",
        stroke: "stroke-blue-500",
        fill: "fill-blue-500",
        text: "text-blue-500"
    },
    emerald: {
        bg: "bg-emerald-500",
        stroke: "stroke-emerald-500",
        fill: "fill-emerald-500",
        text: "text-emerald-500"
    },
    violet: {
        bg: "bg-violet-500",
        stroke: "stroke-violet-500",
        fill: "fill-violet-500",
        text: "text-violet-500"
    },
    amber: {
        bg: "bg-amber-500",
        stroke: "stroke-amber-500",
        fill: "fill-amber-500",
        text: "text-amber-500"
    },
    gray: {
        bg: "bg-gray-500",
        stroke: "stroke-gray-500",
        fill: "fill-gray-500",
        text: "text-gray-500"
    },
    cyan: {
        bg: "bg-cyan-500",
        stroke: "stroke-cyan-500",
        fill: "fill-cyan-500",
        text: "text-cyan-500"
    },
    pink: {
        bg: "bg-pink-500",
        stroke: "stroke-pink-500",
        fill: "fill-pink-500",
        text: "text-pink-500"
    }
} as const satisfies {
    [color: string]: {
        [key in ColorUtility]: string;
    };
};

export type AvailableChartColorsKeys = keyof typeof chartColors;

export const AvailableChartColors = Object.keys(chartColors) as AvailableChartColorsKeys[];

// --------------------------------------------------------
// Utilities
// --------------------------------------------------------

export const constructCategoryColors = (
    categories: string[],
    colors: AvailableChartColorsKeys[]
): Map<string, AvailableChartColorsKeys> => {
    const categoryColors = new Map<string, AvailableChartColorsKeys>();

    if (colors.length === 0) {
        return categoryColors;
    }

    categories.forEach((category, index) => {
        categoryColors.set(category, colors[index % colors.length]);
    });

    return categoryColors;
};

const FALLBACK_COLOR: Record<ColorUtility, string> = {
    bg: "bg-gray-500",
    stroke: "stroke-gray-500",
    fill: "fill-gray-500",
    text: "text-gray-500"
};

export const getColorClassName = (
    color: AvailableChartColorsKeys,
    type: ColorUtility
): string => {
    return chartColors[color]?.[type] ?? FALLBACK_COLOR[type];
};

// --------------------------------------------------------
// Tremor Raw getYAxisDomain [v0.0.0]
// --------------------------------------------------------

export const getYAxisDomain = (
    autoMinValue: boolean,
    minValue: number | undefined,
    maxValue: number | undefined
) => {
    const minDomain = autoMinValue ? "auto" : (minValue ?? 0);
    const maxDomain = maxValue ?? "auto";

    return [minDomain, maxDomain] as const;
};

// --------------------------------------------------------
// Tremor Raw hasOnlyOneValueForKey [v0.1.0]
// --------------------------------------------------------

export function hasOnlyOneValueForKey<
    T extends Record<string, any>,
    K extends keyof T
>(array: T[], keyToCheck: K): boolean {
    let hasFirst = false;
    let firstValue: T[K] | undefined = undefined;

    for (const obj of array) {
        if (Object.prototype.hasOwnProperty.call(obj, keyToCheck)) {
            const currentValue = obj[keyToCheck] as T[K];

            if (!hasFirst) {
                firstValue = currentValue;
                hasFirst = true;
            } else if (currentValue !== firstValue) {
                return false;
            }
        }
    }

    return true;
}

