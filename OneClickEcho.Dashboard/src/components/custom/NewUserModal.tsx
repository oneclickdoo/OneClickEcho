"use client";

import { Dispatch, SetStateAction } from "react";
import { useTranslations } from "next-intl";

import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger
} from "@/components/tremor/Dialog";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

interface INewUserModalProps {
    email: string;
    setEmail: Dispatch<SetStateAction<string>>;
    password: string;
    setPassword: Dispatch<SetStateAction<string>>;
    errorMessages: string;
    setErrorMessages: Dispatch<SetStateAction<string>>;
    isLoading: boolean;
    setIsLoading: Dispatch<SetStateAction<boolean>>;
    handleSubmit: () => void;
}

export const NewUserModal = (props: INewUserModalProps) => {
    const tCommon = useTranslations("Common");

    return (
        <Dialog
            onOpenChange={() => {
                props.setEmail("");
                props.setPassword("");
                props.setErrorMessages("");
                props.setIsLoading(false);
            }}
        >
            <DialogTrigger asChild>
                <Button
                    className="border-transparent bg-green-600 text-white outline-green-500 hover:bg-green-500 disabled:bg-green-100 disabled:text-gray-400 dark:bg-green-500 dark:text-gray-900 dark:outline-green-500 dark:hover:bg-green-600 disabled:dark:bg-green-800 disabled:dark:text-green-400"
                    type="submit"
                    variant="primary"
                >
                    {tCommon("companyUsers.newUserButton")}
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-lg" aria-describedby="create-new-user-dialog">
                <DialogHeader>
                    <DialogTitle>{tCommon("companyUsers.newUserTitle")}</DialogTitle>
                    <DialogDescription>
                        <p className="text-sm text-gray-400 mt-1">{tCommon("companyUsers.newUserHint")}</p>

                        <div className="mt-5">
                            <Input
                                value={props.email}
                                onChange={(e) => props.setEmail(e.target.value)}
                                className="w-full mx-auto"
                                hasError={!!props.errorMessages}
                                placeholder={tCommon("companyUsers.emailPlaceholder")}
                            />
                            {props.errorMessages && <p className="mt-1 text-xs text-red-600">{props.errorMessages}</p>}
                        </div>

                        {props.password && (
                            <div className="flex justify-end items-center mt-2">
                                <div className="flex items-center gap-2">
                                    <span className="text-sm text-gray-400">
                                        {tCommon("companyUsers.generatedPassword")}{" "}
                                        "<span className="font-semibold">{props.password}</span>"
                                    </span>
                                </div>
                            </div>
                        )}
                    </DialogDescription>
                </DialogHeader>

                <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                    <DialogClose asChild>
                        <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary">
                            {tCommon("common.goBack")}
                        </Button>
                    </DialogClose>

                    {props.password ? (
                        <DialogClose asChild>
                            <Button
                                isLoading={props.isLoading}
                                className="w-full sm:w-fit border-transparent"
                                onClick={() => {
                                    navigator.clipboard
                                        .writeText(`Email:"${props.email}"\nPassword:"${props.password}"`)
                                        .then(() => {
                                            props.setEmail("");
                                        });
                                }}
                            >
                                {tCommon("companyUsers.copyAndClose")}
                            </Button>
                        </DialogClose>
                    ) : (
                        <Button
                            isLoading={props.isLoading}
                            className="w-full sm:w-fit border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            onClick={props.handleSubmit}
                        >
                            {tCommon("common.create")}
                        </Button>
                    )}
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
