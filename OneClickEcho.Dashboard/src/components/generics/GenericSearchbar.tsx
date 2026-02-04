import { useState, useImperativeHandle, forwardRef } from "react";
import { useTranslations } from "next-intl";

import { useDebouncedCallback } from "use-debounce";

import { Searchbar } from "@/components/tremor/Searchbar";

export const GenericSearchbar = forwardRef(function GenericSearchbar(
    {
        onSearchChange,
        placeholder
    }: {
        // eslint-disable-next-line no-unused-vars
        onSearchChange: (value: string) => void;
        placeholder: string;
    },
    ref
) {
    const t = useTranslations("Common");

    const [searchTerm, setSearchTerm] = useState<string>("");

    const debouncedSetFilterValue = useDebouncedCallback((value: string) => {
        onSearchChange(value);
    }, 300);

    const handleSearchChange = (event: any) => {
        const value = event.target.value;

        setSearchTerm(value);

        debouncedSetFilterValue(value);
    };

    useImperativeHandle(ref, () => ({
        resetInput: () => {
            setSearchTerm("");
        }
    }));

    return (
        <Searchbar
            type="search"
            placeholder={t("searchBy", { field: placeholder })}
            value={searchTerm}
            onChange={handleSearchChange}
            className="w-full sm:max-w-[250px] sm:[&>input]:h-[30px]"
        />
    );
});
