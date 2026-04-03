"use client";

import { useState, useImperativeHandle, forwardRef, useCallback } from "react";

import DatePicker from "react-datepicker";
import { useLocale, useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";

import {
    Drawer,
    DrawerBody,
    DrawerClose,
    DrawerContent,
    DrawerDescription,
    DrawerFooter,
    DrawerHeader,
    DrawerTitle
} from "@/components/tremor/Drawer";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/tremor/Select";
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";
import { Checkbox } from "@/components/tremor/Checkbox";

import { updateLead, deleteLead } from "@/elements/dashboard/singleCampaign/fetchData";
import type { CampaignLeadDto } from "@/elements/dashboard/singleCampaign/fetchData";

import type { SelectOption } from "@/lib/selects";
import { getLeadGenderOptions } from "@/lib/selects";

import type { LeadGender } from "@/lib/enums";
import { convertStringToEnum } from "@/lib/enums";

import { useToast } from "@/lib/useToast";

type StateType = [boolean, () => void, () => void, () => void] & {
    state: boolean;
    open: () => void;
    close: () => void;
    toggle: () => void;
};

const useToggleState = (initial = false) => {
    const [state, setState] = useState<boolean>(initial);

    const close = () => setState(false);
    const open = () => setState(true);
    const toggle = () => setState((s) => !s);

    const hookData = [state, open, close, toggle] as StateType;
    hookData.state = state;
    hookData.open = open;
    hookData.close = close;
    hookData.toggle = toggle;

    return hookData;
};

export type CampaignLeadsTableModalHandle = {
    useDrawer: (campaignLeadDto: CampaignLeadDto) => void;
};

export const CampaignLeadsTableModal = forwardRef<CampaignLeadsTableModalHandle, { refreshData: () => void }>(
    (props, ref) => {
        const t = useTranslations("SingleCampaign.Modals.CampaignLeadsTable");
        const tCommon = useTranslations("Common");
        const locale = useLocale();

        const leadGenderOptions = getLeadGenderOptions(tCommon);

        const { toast } = useToast();

        const [data, setData] = useState<CampaignLeadDto>({
            leadId: "",
            phoneNumber: ""
        });

        const { authFetch } = useAuth();
        const [editOpen, showEdit, closeEdit] = useToggleState();

        const useDrawer = useCallback(
            (campaignLeadDto: CampaignLeadDto) => {
                setData(campaignLeadDto);
                showEdit();
            },
            [showEdit]
        );

        useImperativeHandle(
            ref,
            () => ({
                useDrawer
            }),
            [useDrawer]
        );

        const handleChange = <K extends keyof CampaignLeadDto>(name: K, value: CampaignLeadDto[K] | undefined): void => {
            if (data[name] != null && typeof data[name] !== "undefined" && typeof data[name] !== typeof value) {
                console.error("Type mismatch");
            }

            setData((prevData: CampaignLeadDto): CampaignLeadDto => ({
                ...prevData,
                [name]: value
            }));
        };

        const onSave = async () => {
            try {
                await updateLead(data, authFetch);

                toast({
                    title: t("toastSaveSuccessTitle"),
                    description: t("toastSaveSuccessDescription"),
                    variant: "success",
                    duration: 2000
                });

                closeEdit();
                props.refreshData();
            } catch (error) {
                console.error("Error updating lead:", error);

                toast({
                    title: t("toastSaveErrorTitle"),
                    description: t("toastSaveErrorDescription"),
                    variant: "error",
                    duration: 2500
                });
            }
        };

        const onDelete = async () => {
            try {
                await deleteLead(data.leadId, authFetch);

                toast({
                    title: t("toastDeleteSuccessTitle"),
                    description: t("toastDeleteSuccessDescription"),
                    variant: "success",
                    duration: 2000
                });

                closeEdit();
                props.refreshData();
            } catch (error) {
                console.error("Error deleting lead:", error);

                toast({
                    title: t("toastDeleteErrorTitle"),
                    description: t("toastDeleteErrorDescription"),
                    variant: "error",
                    duration: 2500
                });
            }
        };

        return (
            <div className="flex justify-center">
                <Drawer
                    open={editOpen}
                    onOpenChange={(modalOpened) => {
                        if (!modalOpened) closeEdit();
                    }}
                >
                    <DrawerContent className="sm:max-w-lg">
                        <DrawerHeader>
                            <DrawerTitle>{data?.phoneNumber}</DrawerTitle>
                            <DrawerDescription className="mt-1 text-sm">{t("subtitle")}</DrawerDescription>
                        </DrawerHeader>

                        <DrawerBody>
                            <div className="mb-4">
                                <Label>{t("firstName")}</Label>
                                <Input value={data.firstName} onChange={(e) => handleChange("firstName", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{t("lastName")}</Label>
                                <Input value={data.lastName} onChange={(e) => handleChange("lastName", e.target.value)} />
                            </div>

                            <div className="mb-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
                                <div>
                                    <Label>{t("gender")}</Label>
                                    <Select
                                        value={data.gender?.toString()}
                                        onValueChange={(gender) => {
                                            handleChange("gender", convertStringToEnum<LeadGender>(gender));
                                        }}
                                    >
                                        <SelectTrigger id="gender" className="mt-1 h-8 py-1">
                                            <SelectValue placeholder={t("selectPlaceholder")} />
                                        </SelectTrigger>

                                        <SelectContent>
                                            {leadGenderOptions.map((item: SelectOption<LeadGender>) => (
                                                <SelectItem key={item.value} value={item.value.toString()}>
                                                    {item.label}
                                                </SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
                                </div>

                                <div>
                                    <Label>{t("dateOfBirth")}</Label>
                                    <DatePicker
                                        locale={locale}
                                        selected={data.dateOfBirthObject}
                                        maxDate={new Date()}
                                        showMonthDropdown
                                        showYearDropdown
                                        dateFormat={locale === "sr" ? "dd.MM.yyyy." : "dd/MM/yyyy"}
                                        className="h-8 rounded-md border-gray-300 bg-white text-sm text-gray-900 dark:border-gray-800 dark:bg-gray-950 dark:text-gray-50 disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-400 disabled:dark:border-gray-700 disabled:dark:bg-gray-800 disabled:dark:text-gray-500"
                                        wrapperClassName="mt-1"
                                        onChange={(date) => {
                                            if (date) handleChange("dateOfBirthObject", date);
                                        }}
                                    />
                                </div>
                            </div>

                            <div className="mb-4">
                                <Label>{t("email")}</Label>
                                <Input value={data.email} onChange={(e) => handleChange("email", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{t("city")}</Label>
                                <Input value={data.city} onChange={(e) => handleChange("city", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{t("state")}</Label>
                                <Input value={data.state} onChange={(e) => handleChange("state", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{t("country")}</Label>
                                <Input value={data.country} onChange={(e) => handleChange("country", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <div className="flex gap-2">
                                    <Checkbox
                                        checked={data.isBlacklisted}
                                        onCheckedChange={() => handleChange("isBlacklisted", !data.isBlacklisted)}
                                    />
                                    <Label>{t("blacklisted")}</Label>
                                </div>
                            </div>
                        </DrawerBody>

                        <DrawerFooter className="mt-6">
                            <DrawerClose asChild>
                                <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary">
                                    {t("goBack")}
                                </Button>
                            </DrawerClose>

                            <Button
                                className="w-full bg-red-500 text-white hover:bg-red-600 dark:bg-red-500 dark:text-white hover:dark:bg-red-600 sm:w-fit"
                                onClick={onDelete}
                            >
                                {t("delete")}
                            </Button>

                            <Button className="w-full sm:w-fit" onClick={onSave}>
                                {t("save")}
                            </Button>
                        </DrawerFooter>
                    </DrawerContent>
                </Drawer>
            </div>
        );
    }
);

CampaignLeadsTableModal.displayName = "CampaignLeadsTableModal";
