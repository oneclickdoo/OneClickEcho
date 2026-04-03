import { RefObject } from "react";

import { Card } from "../tremor/Card";
import { Label } from "@/components/tremor/Label";
import { Button } from "@/components/tremor/Button";

interface IVariablesFormProps {
    disabled?: boolean;
    inputRef: RefObject<HTMLTextAreaElement>;
    message: string | undefined;
    handleChange: (newMessage: string) => void;
}

export const VariablesForm = (props: IVariablesFormProps) => {
    const injectVariable = (variable: string) => {
        if (!props.inputRef.current) return;

        const startPosition = props.inputRef.current.selectionStart;
        const endPosition = props.inputRef.current.selectionEnd;

        const currentMessage = props.message ? props.message : "";
        const newMessage = currentMessage.slice(0, startPosition) + variable + currentMessage.slice(endPosition);

        props.handleChange(newMessage);

        props.inputRef.current.focus();
    };

    return (
        <div className="flex flex-col gap-y-3 mt-3">
            <Label>Variables</Label>
            <Card className="flex gap-x-10 mb-4">
                <div>
                    <Label>Nominative:</Label>
                    <div className="flex gap-x-3 mt-3">
                        <Button
                            disabled={props.disabled}
                            type="submit"
                            color="primary"
                            className="border-transparent bg-emerald-600 text-white outline-emerald-500 hover:bg-emerald-500 disabled:bg-emerald-100 disabled:text-gray-400 dark:bg-emerald-500 dark:text-gray-900 dark:outline-emerald-500 dark:hover:bg-emerald-600 disabled:dark:bg-emerald-800 disabled:dark:text-emerald-400"
                            onClick={() => injectVariable("{firstName:nominative}")}
                        >
                            First name
                        </Button>
                        <Button
                            disabled={props.disabled}
                            type="submit"
                            color="primary"
                            className="border-transparent bg-emerald-600 text-white outline-emerald-500 hover:bg-emerald-500 disabled:bg-emerald-100 disabled:text-gray-400 dark:bg-emerald-500 dark:text-gray-900 dark:outline-emerald-500 dark:hover:bg-emerald-600 disabled:dark:bg-emerald-800 disabled:dark:text-emerald-400"
                            onClick={() => injectVariable("{lastName:nominative}")}
                        >
                            Last name
                        </Button>
                    </div>
                </div>
                <div>
                    <Label>Vocative:</Label>
                    <div className="flex gap-x-3 mt-3">
                        <Button
                            disabled={props.disabled}
                            type="submit"
                            color="primary"
                            className="border-transparent bg-emerald-600 text-white outline-emerald-500 hover:bg-emerald-500 disabled:bg-emerald-100 disabled:text-gray-400 dark:bg-emerald-500 dark:text-gray-900 dark:outline-emerald-500 dark:hover:bg-emerald-600 disabled:dark:bg-emerald-800 disabled:dark:text-emerald-400"
                            onClick={() => injectVariable("{firstName:vocative}")}
                        >
                            First name
                        </Button>
                        <Button
                            disabled={props.disabled}
                            type="submit"
                            color="primary"
                            className="border-transparent bg-emerald-600 text-white outline-emerald-500 hover:bg-emerald-500 disabled:bg-emerald-100 disabled:text-gray-400 dark:bg-emerald-500 dark:text-gray-900 dark:outline-emerald-500 dark:hover:bg-emerald-600 disabled:dark:bg-emerald-800 disabled:dark:text-emerald-400"
                            onClick={() => injectVariable("{lastName:vocative}")}
                        >
                            Last name
                        </Button>
                    </div>
                </div>
            </Card>
        </div>
    );
};
