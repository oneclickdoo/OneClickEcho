import { RiBarChartFill } from "@remixicon/react";

import { Card } from "./Card";

interface LoadingProps {
    text?: string;
}

const Loading = ({ text }: LoadingProps) => {
    return (
        <Card>
            <div className="mt-4 flex h-44 items-center justify-center rounded-tremor-small border border-dashed border-tremor-border p-4 dark:border-dark-tremor-border">
                <div className="text-center">
                    <RiBarChartFill
                        className="mx-auto h-8 w-8 text-tremor-content-subtle dark:text-dark-tremor-content-subtle"
                        aria-hidden="true"
                    />
                    <p className="mt-2 font-medium text-tremor-default text-tremor-content-strong dark:text-dark-tremor-content-strong">
                        No data to show
                    </p>
                    {text ? (
                        <p className="text-tremor-default text-tremor-content dark:text-dark-tremor-content">
                            {text}
                        </p>
                    ) : null}
                </div>
            </div>
        </Card>
    );
};

export default Loading;
