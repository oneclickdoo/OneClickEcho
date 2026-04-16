import moment from "moment";

export class FilterManager {
    filters: any = {};
    currentCompany: string | null;

    constructor(currentCompany: string | null) {
        this.currentCompany = currentCompany;
    }

    setFilter(key: string, filterValue: any) {
        this.filters[key] = filterValue;
    }

    removeFilter(key: string) {
        delete this.filters[key];
    }

    clearFilters() {
        this.filters = {};
    }

    private escapeODataString(value: string) {
        // escape single quote for OData-like string literals
        return value.replace(/'/g, "''");
    }

    private formatValue(value: any) {
        if (value === null || value === undefined) return "null";
        if (typeof value === "number") return String(value);
        if (typeof value === "boolean") return value ? "true" : "false";
        return `'${this.escapeODataString(String(value))}'`;
    }

    generate = () => {
        const dotnetFindArray: string[] = [];

        if (this.currentCompany) {
            // ako je CompanyId numeric, ovo i dalje radi (String -> '123' može biti OK ili ne)
            // ako ti mora bez navodnika, reci pa ću vratiti na: CompanyId eq ${this.currentCompany}
            dotnetFindArray.push(`CompanyId eq ${this.formatValue(this.currentCompany)}`);
        }

        for (let filterKey in this.filters) {
            const filterObject = this.filters[filterKey];

            const field = filterKey.charAt(0).toUpperCase() + filterKey.slice(1);

            switch (filterObject?.type) {
                case "search": {
                    const v = filterObject?.value ?? "";
                    dotnetFindArray.push(`${field} co ${this.formatValue(v)}`);
                    break;
                }

                case "select": {
                    const v = filterObject?.value;
                    const keyLower = filterKey.toLowerCase();
                    // Backend lexer maps unquoted 0/1 to int; quoted '1' was string and broke bool (500).
                    if (
                        (keyLower === "isblacklisted" || keyLower === "isunsubscribed") &&
                        (v === "0" || v === "1")
                    ) {
                        dotnetFindArray.push(`${field} eq ${v}`);
                    } else {
                        dotnetFindArray.push(`${field} eq ${this.formatValue(v)}`);
                    }
                    break;
                }

                case "checkbox": {
                    const arr: any[] = Array.isArray(filterObject?.value) ? filterObject.value : [];
                    dotnetFindArray.push(`${field} in ${arr.map((v) => this.formatValue(v)).join(",")}`);
                    break;
                }

                case "date": {
                    const from: Date | undefined = filterObject?.value?.from;
                    if (!from) break;

                    const to: Date = filterObject?.value?.to ? filterObject.value.to : from;

                    dotnetFindArray.push(`${field} ge ${from.toISOString()}`);
                    dotnetFindArray.push(`${field} le ${moment(to).add(1, "days").toISOString()}`);
                    break;
                }

                default:
                    break;
            }
        }

        return dotnetFindArray.join(" and ");
    };
}
