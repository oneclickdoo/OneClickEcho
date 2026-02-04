"use client";

import React from "react";
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
import { Badge } from "@/components/tremor/Badge";
import { DropdownMenuItem } from "@/components/tremor/Dropdown";
import { Input } from "@/components/tremor/Input";
import { Label } from "@/components/tremor/Label";
import { RadioCardGroup, RadioCardItem } from "@/components/tremor/RadioCard";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/tremor/Select";
import { Button } from "@/components/tremor/Button";

export const databases: {
    label: string;
    value: string;
    description: string;
    isRecommended: boolean;
}[] = [
        {
            label: "Base performance",
            value: "base-performance",
            description: "1/8 vCPU, 1 GB RAM",
            isRecommended: true
        },
        {
            label: "Advanced performance",
            value: "advanced-performance",
            description: "1/4 vCPU, 2 GB RAM",
            isRecommended: false
        },
        {
            label: "Turbo performance",
            value: "turbo-performance",
            description: "1/2 vCPU, 4 GB RAM",
            isRecommended: false
        }
    ];

export type ModalProps = {
    itemName: string;
};

export function ModalAddWorkspace({ itemName }: ModalProps) {
    const t = useTranslations("Common");
    const [open, setOpen] = React.useState(false);

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger className="w-full text-left" asChild>
                <DropdownMenuItem
                    onSelect={(event) => {
                        event.preventDefault();
                        setOpen(true);
                    }}
                >
                    {itemName}
                </DropdownMenuItem>
            </DialogTrigger>

            <DialogContent className="sm:max-w-2xl">
                <form>
                    <DialogHeader>
                        <DialogTitle>{t("addNewWorkspace")}</DialogTitle>
                        <DialogDescription className="mt-1 text-sm leading-6">{t("freePlanWorkspaceLimit")}</DialogDescription>

                        <div className="mt-4 grid grid-cols-2 gap-4">
                            <div>
                                <Label htmlFor="workspace-name" className="font-medium">
                                    {t("workspaceName")}
                                </Label>
                                <Input id="workspace-name" name="workspace-name" placeholder={t("workspaceNamePlaceholder")} className="mt-2" />
                            </div>

                            <div>
                                <Label htmlFor="starter-kit" className="font-medium">
                                    {t("starterKit")}
                                </Label>
                                <Select defaultValue="empty-workspace">
                                    <SelectTrigger id="starter-kit" name="starter-kit" className="mt-2">
                                        <SelectValue />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="empty-workspace">{t("starterKitEmpty")}</SelectItem>
                                        <SelectItem value="commerce-analytics">{t("starterKitCommerceAnalytics")}</SelectItem>
                                        <SelectItem value="product-analytics">{t("starterKitProductAnalytics")}</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="col-span-full">
                                <Label htmlFor="database-region" className="font-medium">
                                    {t("databaseRegion")}
                                </Label>
                                <Select defaultValue="europe-west-01">
                                    <SelectTrigger id="database-region" name="database-region" className="mt-2">
                                        <SelectValue />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="europe-west-01">europe-west-01</SelectItem>
                                        <SelectItem value="us-east-02">us-east-02</SelectItem>
                                        <SelectItem value="us-west-01">us-west-01</SelectItem>
                                    </SelectContent>
                                </Select>
                                <p className="mt-2 text-xs text-gray-500">{t("databaseRegionHint")}</p>
                            </div>
                        </div>

                        <div className="mt-4">
                            <Label htmlFor="database" className="font-medium">
                                {t("databaseConfiguration")}
                            </Label>

                            <RadioCardGroup defaultValue={databases[0].value} className="mt-2 grid grid-cols-1 gap-4 text-sm md:grid-cols-2">
                                {databases.map((database) => (
                                    <RadioCardItem key={database.value} value={database.value}>
                                        <div className="flex items-start gap-3">
                                            <div>
                                                {database.isRecommended ? (
                                                    <div className="flex items-center gap-2">
                                                        <span className="leading-5">{database.label}</span>
                                                        <Badge>{t("recommended")}</Badge>
                                                    </div>
                                                ) : (
                                                    <span>{database.label}</span>
                                                )}
                                                <p className="mt-1 text-xs text-gray-500">{database.description}</p>
                                            </div>
                                        </div>
                                    </RadioCardItem>
                                ))}
                            </RadioCardGroup>
                        </div>
                    </DialogHeader>

                    <DialogFooter className="mt-6">
                        <DialogClose asChild>
                            <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary" type="button">
                                {t("goBack")}
                            </Button>
                        </DialogClose>

                        <DialogClose asChild>
                            <Button type="submit" className="w-full sm:w-fit">
                                {t("addWorkspace")}
                            </Button>
                        </DialogClose>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
