"use client";

import { useState, useImperativeHandle, forwardRef } from "react";
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
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Switch } from "@/components/tremor/Switch";
import { Button } from "@/components/tremor/Button";

import { deleteUser, updateUserPassword, UserDto } from "@/elements/dashboard/singleCompany/fetchData";

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

export const CompanyUsersTableModal = forwardRef((props: { refreshData: () => void }, ref) => {
    const tCommon = useTranslations("Common");

    const [data, setData] = useState<UserDto>({ id: "", email: "" });
    const [checked, setChecked] = useState(false);
    const [deleteChecked, setDeleteChecked] = useState(false);
    const [newPassword, setNewPassword] = useState("");
    const [newPasswordError, setNewPasswordError] = useState("");

    const { authFetch } = useAuth();
    const [editOpen, showEdit, closeEdit] = useToggleState();

    const useDrawer = (userDto: UserDto) => {
        setData(userDto);
        showEdit();
    };

    useImperativeHandle(ref, () => ({ useDrawer }));

    const onSave = async () => {
        try {
            const request = await updateUserPassword(
                {
                    id: data.id,
                    newPassword: newPassword
                },
                authFetch
            );

            const dataJson = await request.json();

            if (!request.ok) {
                setNewPasswordError(
                    dataJson.errors?.["NewPassword"]?.reduce((acc: string, item: string) => `${acc} ${item}`, "") ||
                    tCommon("errors.unexpected")
                );
                return;
            }

            closeEdit();
            props.refreshData();
        } catch (error) {
            console.error("Error updating user password:", error);
            setNewPasswordError(tCommon("errors.unexpected"));
        }
    };

    const onDelete = async () => {
        try {
            await deleteUser({ id: data.id }, authFetch);

            closeEdit();
            props.refreshData();
        } catch (error) {
            console.error("Error deleting user:", error);
        }
    };

    return (
        <div className="flex justify-center">
            <Drawer
                open={editOpen}
                onOpenChange={(modalOpened) => {
                    if (!modalOpened) closeEdit();

                    setChecked(false);
                    setDeleteChecked(false);
                    setNewPassword("");
                    setNewPasswordError("");
                }}
            >
                <DrawerContent className="sm:max-w-lg">
                    <DrawerHeader>
                        <DrawerTitle>{data?.email}</DrawerTitle>
                        <DrawerDescription className="mt-1 text-sm">
                            {tCommon("companyUsersModal.updateUser")}
                        </DrawerDescription>
                    </DrawerHeader>

                    <DrawerBody>
                        <div className="mb-4">
                            <Label>{tCommon("companyUsersModal.newPassword")}</Label>
                            <Input
                                type="password"
                                placeholder="●●●●●●●●●●"
                                disabled={!checked}
                                value={newPassword}
                                hasError={!!newPasswordError}
                                onChange={(e) => setNewPassword(e.target.value)}
                            />
                            {newPasswordError && <p className="mt-1 text-xs text-red-600">{newPasswordError}</p>}
                        </div>

                        <div className="flex items-center justify-start gap-2">
                            <Switch
                                id="password-edit-toggle"
                                checked={checked}
                                onCheckedChange={setChecked}
                            />
                            <Label htmlFor="password-edit-toggle">
                                {tCommon("companyUsersModal.enablePasswordEditing")}
                            </Label>
                        </div>

                        <div className="mt-10 flex justify-between">
                            <Button
                                variant="destructive"
                                disabled={!deleteChecked}
                                className="w-full sm:w-fit px-10"
                                onClick={onDelete}
                            >
                                {tCommon("common.delete")}
                            </Button>

                            <div className="flex items-center">
                                <div className="flex items-center justify-start gap-2">
                                    <Label htmlFor="user-delete-toggle">
                                        {tCommon("companyUsersModal.enableUserDelete")}
                                    </Label>
                                    <Switch
                                        id="user-delete-toggle"
                                        checked={deleteChecked}
                                        onCheckedChange={setDeleteChecked}
                                    />
                                </div>
                            </div>
                        </div>
                    </DrawerBody>

                    <DrawerFooter className="mt-6">
                        <DrawerClose asChild>
                            <Button className="w-full mt-2 sm:w-fit sm:mt-0" variant="secondary">
                                {tCommon("common.goBack")}
                            </Button>
                        </DrawerClose>
                        <Button className="w-full sm:w-fit" onClick={onSave}>
                            {tCommon("common.save")}
                        </Button>
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
        </div>
    );
});
