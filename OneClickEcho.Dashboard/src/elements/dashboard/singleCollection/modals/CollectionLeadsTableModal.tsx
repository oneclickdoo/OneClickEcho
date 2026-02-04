"use client";

import { useState, useImperativeHandle, forwardRef } from "react";
import DatePicker from "react-datepicker";
import { useTranslations } from "next-intl";

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

import { CampaignLeadDto, updateLead /*, deleteLead*/ } from "@/elements/dashboard/singleCampaign/fetchData";

import { getLeadGenderOptions } from "@/lib/selects";
import { LeadGender, convertStringToEnum } from "@/lib/enums";

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

export type CollectionLeadsTableModalHandle = {
    useDrawer: (campaignLeadDto: CampaignLeadDto) => void;
};

export const CollectionLeadsTableModal = forwardRef<CollectionLeadsTableModalHandle, { refreshData: () => void }>(
    (props, ref) => {
        const tCommon = useTranslations("Common");

        const [data, setData] = useState<CampaignLeadDto>({
            leadId: "",
            phoneNumber: ""
        });

        const { authFetch } = useAuth();

        const [editOpen, showEdit, closeEdit] = useToggleState();

        const useDrawer = (campaignLeadDto: CampaignLeadDto) => {
            setData(campaignLeadDto);
            showEdit();
        };

        useImperativeHandle(ref, () => ({
            useDrawer
        }));

        const handleChange = <K extends keyof CampaignLeadDto>(name: K, value: CampaignLeadDto[K] | undefined): void => {
            if (data[name] != null && typeof data[name] !== "undefined" && typeof data[name] !== typeof value) {
                console.error("Type mismatch");
            }

            setData((prevData) => ({
                ...prevData,
                [name]: value
            }));
        };

        const onSave = async () => {
            try {
                await updateLead(data, authFetch);

                closeEdit();
                props.refreshData();
            } catch (error) {
                console.error("Error updating lead:", error);
            }
        };

        const onDelete = async () => {
            console.log("Handle delete!");
            // try {
            //     await deleteLead(data.leadId, authFetch);
            //     closeEdit();
            //     props.refreshData();
            // } catch (error) {
            //     console.error("Error deleting lead:", error);
            // }
        };

        const genderOptions = getLeadGenderOptions(tCommon);

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
                            <DrawerDescription className="mt-1 text-sm">
                                {tCommon("lead.updateDetails")}
                            </DrawerDescription>
                        </DrawerHeader>

                        <DrawerBody>
                            <div className="mb-4">
                                <Label>{tCommon("lead.firstName")}</Label>
                                <Input value={data.firstName} onChange={(e) => handleChange("firstName", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{tCommon("lead.lastName")}</Label>
                                <Input value={data.lastName} onChange={(e) => handleChange("lastName", e.target.value)} />
                            </div>

                            <div className="grid grid-cols-1 gap-4 mb-4 sm:grid-cols-2">
                                <div>
                                    <Label>{tCommon("lead.gender")}</Label>
                                    <Select
                                        value={data.gender?.toString()}
                                        onValueChange={(val) => {
                                            handleChange("gender", convertStringToEnum<LeadGender>(val));
                                        }}
                                    >
                                        <SelectTrigger id="gender" className="h-8 mt-1 py-1">
                                            <SelectValue placeholder={tCommon("common.select")} />
                                        </SelectTrigger>

                                        <SelectContent>
                                            {genderOptions.map((item) => (
                                                <SelectItem key={item.value} value={item.value.toString()}>
                                                    {item.label}
                                                </SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
                                </div>

                                <div>
                                    <Label>{tCommon("lead.dateOfBirth")}</Label>
                                    <DatePicker
                                        selected={data.dateOfBirthObject}
                                        maxDate={new Date()}
                                        showMonthDropdown
                                        showYearDropdown
                                        className="h-8 text-sm text-gray-900 dark:text-gray-50 bg-white dark:bg-gray-950 rounded-md border-gray-300 dark:border-gray-800 disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-400 disabled:dark:border-gray-700 disabled:dark:bg-gray-800 disabled:dark:text-gray-500"
                                        wrapperClassName="mt-1"
                                        onChange={(date) => {
                                            if (date) handleChange("dateOfBirthObject", date);
                                        }}
                                    />
                                </div>
                            </div>

                            <div className="mb-4">
                                <Label>{tCommon("lead.email")}</Label>
                                <Input value={data.email} onChange={(e) => handleChange("email", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{tCommon("lead.city")}</Label>
                                <Input value={data.city} onChange={(e) => handleChange("city", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{tCommon("lead.state")}</Label>
                                <Input value={data.state} onChange={(e) => handleChange("state", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <Label>{tCommon("lead.country")}</Label>
                                <Input value={data.country} onChange={(e) => handleChange("country", e.target.value)} />
                            </div>

                            <div className="mb-4">
                                <div className="flex gap-2">
                                    <Checkbox
                                        checked={data.isBlacklisted}
                                        onCheckedChange={() => handleChange("isBlacklisted", !data.isBlacklisted)}
                                    />
                                    <Label>{tCommon("lead.blacklisted")}</Label>
                                </div>
                            </div>
                        </DrawerBody>

                        <DrawerFooter className="mt-6">
                            <DrawerClose asChild>
                                <Button className="w-full mt-2 sm:w-fit sm:mt-0" variant="secondary">
                                    {tCommon("common.goBack")}
                                </Button>
                            </DrawerClose>

                            <Button
                                className="w-full text-white bg-red-500 dark:text-white dark:bg-red-500 hover:bg-red-600 hover:dark:bg-red-600 sm:w-fit"
                                onClick={onDelete}
                            >
                                {tCommon("common.delete")}
                            </Button>

                            <Button className="w-full sm:w-fit" onClick={onSave}>
                                {tCommon("common.save")}
                            </Button>
                        </DrawerFooter>
                    </DrawerContent>
                </Drawer>
            </div>
        );
    }
);

CollectionLeadsTableModal.displayName = "CollectionLeadsTableModal";
