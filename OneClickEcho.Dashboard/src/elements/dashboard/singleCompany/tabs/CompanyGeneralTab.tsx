"use client";

import { Dispatch, SetStateAction } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";

import { Card } from "@/components/tremor/Card";
import { Divider } from "@/components/tremor/Divider";
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

import { CompanyDto, updateCompany, deleteCompany } from "../fetchData";
import { useToast } from "@/lib/useToast";

interface ICompanyGeneralTabProps {
    company: CompanyDto;
    setCompany: Dispatch<SetStateAction<CompanyDto>>;
}

export const CompanyGeneralTab = (props: ICompanyGeneralTabProps) => {
    const tCommon = useTranslations("Common");
    const router = useRouter();

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const handleChange = <K extends keyof CompanyDto>(name: K, value: CompanyDto[K] | undefined): void => {
        if (props.company[name] != null && typeof props.company[name] !== "undefined" && typeof props.company[name] !== typeof value) {
            console.error("Type mismatch");
        }

        props.setCompany((prevData: CompanyDto): CompanyDto => ({
            ...prevData,
            [name]: value
        }));
    };

    const handleSubmit = async () => {
        try {
            await updateCompany(props.company, authFetch);

            window.scrollTo({ top: 0, behavior: "smooth" });

            toast({
                title: tCommon("toasts.success"),
                description: tCommon("toasts.companySaved"),
                variant: "success",
                duration: 2000
            });
        } catch (e: any) {
            toast({
                title: tCommon("toasts.error"),
                description: e?.message ?? tCommon("errors.unexpected"),
                variant: "error",
                duration: 2000
            });
        }
    };

    const handleDelete = async () => {
        try {
            await deleteCompany(props.company.companyId, authFetch);

            toast({
                title: tCommon("toasts.success"),
                description: tCommon("toasts.companyDeleted"),
                variant: "success",
                duration: 2000
            });

            router.push(`/companies`);
        } catch (e: any) {
            toast({
                title: tCommon("toasts.error"),
                description: e?.message ?? tCommon("errors.unexpected"),
                variant: "error",
                duration: 2000
            });
        }
    };

    return (
        <div>
            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {tCommon("companyGeneral.mainInformationTitle")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{tCommon("companyGeneral.mainInformationDescription")}</p>
                </div>

                <div className="md:col-span-2">
                    <div className="mb-8">
                        <Label>{tCommon("companyGeneral.nameLabel")}</Label>
                        <Input value={props.company.name} onChange={(e) => handleChange("name", e.target.value)} />
                    </div>

                    <Divider />

                    <h2 className="mb-3 font-semibold text-gray-900 dark:text-gray-50">{tCommon("companyGeneral.pricesTitle")}</h2>

                    <div className="mb-5">
                        <Label>{tCommon("companyGeneral.viberPriceLabel")}</Label>
                        <Input
                            type="number"
                            value={props.company.viberPricePerMesssage}
                            min={0}
                            onChange={(e) => handleChange("viberPricePerMesssage", Number(e.target.value))}
                        />
                    </div>

                    <div className="mb-8">
                        <Label>{tCommon("companyGeneral.smsPriceLabel")}</Label>
                        <Input
                            type="number"
                            value={props.company.smsPricePerMesssage}
                            min={0}
                            onChange={(e) => handleChange("smsPricePerMesssage", Number(e.target.value))}
                        />
                    </div>

                    <Divider />

                    <h2 className="mb-3 font-semibold text-gray-900 dark:text-gray-50">{tCommon("companyGeneral.smsCredentialsTitle")}</h2>

                    <div className="mb-5">
                        <Label>{tCommon("companyGeneral.smsUsernameLabel")}</Label>
                        <Input value={props.company.smsUsername} onChange={(e) => handleChange("smsUsername", e.target.value)} />
                    </div>

                    <div className="mb-5">
                        <Label>{tCommon("companyGeneral.smsPasswordLabel")}</Label>
                        <Input
                            type="password"
                            placeholder="●●●●●●●●●●"
                            value={props.company.smsPassword}
                            onChange={(e) => handleChange("smsPassword", e.target.value)}
                        />
                    </div>

                    <Divider />

                    <div className="mb-5">
                        <Label>{tCommon("companyGeneral.apiPasswordLabel")}</Label>
                        <Input
                            type="password"
                            placeholder="●●●●●●●●●●"
                            value={props.company.apiPassword}
                            onChange={(e) => handleChange("apiPassword", e.target.value)}
                        />
                    </div>

                    <div className="flex justify-end">
                        <Button
                            className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            type="submit"
                            color="primary"
                            onClick={handleSubmit}
                        >
                            {tCommon("companyGeneral.saveCompany")}
                        </Button>
                    </div>
                </div>
            </section>

            <Divider />

            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {tCommon("companyGeneral.dangerZoneTitle")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{tCommon("companyGeneral.dangerZoneDescription")}</p>
                </div>

                <div className="md:col-span-2">
                    <Card className="overflow-hidden p-0">
                        <div className="text-gray-900 dark:text-gray-50 border-b border-tremor-border bg-tremor-background-muted px-4 py-3 dark:border-dark-tremor-border dark:bg-dark-tremor-background-muted">
                            <label className="font-medium text-tremor-content-strong dark:text-dark-tremor-content-strong">
                                {tCommon("companyGeneral.deleteCompanyTitle")}
                            </label>
                        </div>

                        <div className="flex items-center justify-between space-x-10 px-4 py-3 text-gray-500">
                            <p className="text-sm leading-6 text-tremor-content dark:text-dark-tremor-content">
                                {tCommon("companyGeneral.deleteCompanyWarning")}
                            </p>
                            <Button variant="destructive" className="py-1" onClick={handleDelete}>
                                {tCommon("common.delete")}
                            </Button>
                        </div>
                    </Card>
                </div>
            </section>
        </div>
    );
};
