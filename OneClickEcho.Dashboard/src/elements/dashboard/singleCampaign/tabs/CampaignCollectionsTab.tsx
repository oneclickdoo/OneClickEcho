"use client";

import { useMemo, useRef, useState } from "react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import type { PaginationState, SortingState } from "@tanstack/react-table";

import { FilterManager } from "@/components/filtering/FilterManager";

import { CampaignCollectionsTable } from "@/elements/dashboard/singleCampaign/tables/CampaignCollectionsTable";
import {
    assignCampaignLeadCollection,
    type CampaignDto,
    deleteCampaignLeadCollection,
    fetchCampaignLeadCollectionsData,
    type LeadCollectionDto
} from "@/elements/dashboard/singleCampaign/fetchData";

import {
    CampaignLeadsTableAssign,
    type CampaignLeadsTableAssignHandle
} from "@/elements/dashboard/singleCampaign/tables/CampaignLeadsTableAssign";

import { Card } from "@/components/tremor/Card";
import { CampaignStatus } from "@/lib/enums";

interface ICampaignCollectionsTab {
    campaignId: string;
    campaign: CampaignDto;
}

type LeadCollectionRow = LeadCollectionDto & { count?: number };

export function CampaignCollectionsTab(props: ICampaignCollectionsTab) {
    const t = useTranslations("SingleCampaign.Tabs.CampaignCollections");

    const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }]);
    const [pagination, setPagination] = useState<PaginationState>({
        pageIndex: 0,
        pageSize: 20
    });

    const { dashboardManager, authFetch } = useAuth();

    const refCampaignLeadsTableAssign = useRef<CampaignLeadsTableAssignHandle>(null);

    const [filterManager] = useState(() => new FilterManager(dashboardManager!.currentCompany!.companyId));

    const dataQuery = useQuery({
        queryKey: ["campaign-lead-collections", props.campaignId, pagination, sorting, filterManager.filters],
        queryFn: () =>
            fetchCampaignLeadCollectionsData(props.campaignId, pagination, sorting, filterManager.generate(), authFetch),
        placeholderData: keepPreviousData
    });

    // ✅ make rows mutable copy (fix readonly rows from API typings)
    const leadsDataMutable = useMemo(() => {
        if (!dataQuery.data) return undefined;

        return {
            ...dataQuery.data,
            rows: [...dataQuery.data.rows] as LeadCollectionRow[]
        };
    }, [dataQuery.data]);

    // ✅ this matches CampaignLeadsTableAssign: LeadCollectionDto[] (mutable)
    const assignedItemsForAssign = useMemo(() => {
        if (!leadsDataMutable) return undefined;

        return {
            ...leadsDataMutable,
            rows: leadsDataMutable.rows as LeadCollectionDto[]
        };
    }, [leadsDataMutable]);

    const totalLeads = useMemo(() => {
        return leadsDataMutable?.rows.reduce((acc, item) => acc + (item.count ?? 0), 0) ?? 0;
    }, [leadsDataMutable]);

    const assignCollectionToCampaign = async (collectionId: string) => {
        try {
            const response = await assignCampaignLeadCollection(props.campaignId, collectionId, authFetch);

            await dataQuery.refetch();

            // ✅ refresh assign table via correct handle
            refCampaignLeadsTableAssign.current?.refetchDataQuery();

            if (!response.ok) {
                // opcionalno: toast error
            }
        } catch (e) {
            console.error(e);
        }
    };

    const unassignCollectionFromCampaign = async (leadCollectionId: string) => {
        try {
            await deleteCampaignLeadCollection(props.campaignId, leadCollectionId, authFetch);

            await dataQuery.refetch();

            // ✅ refresh assign table so buttons re-enable
            refCampaignLeadsTableAssign.current?.refetchDataQuery();
        } catch (e) {
            console.error(e);
        }
    };

    return (
        <>
            {props.campaign.status === CampaignStatus.Draft && (
                <CampaignLeadsTableAssign
                    assignedItems={assignedItemsForAssign}
                    campaignId={props.campaignId}
                    onAssign={assignCollectionToCampaign}
                    ref={refCampaignLeadsTableAssign}
                />
            )}

            <br />

            <Card className="mb-2 w-full p-2 text-center">
                {t("totalLeads")}: {totalLeads}
            </Card>

            <CampaignCollectionsTable
                campaign={props.campaign}
                leadsData={leadsDataMutable}
                sorting={sorting}
                setSorting={setSorting}
                pagination={pagination}
                setPagination={setPagination}
                handleDelete={unassignCollectionFromCampaign}
            />
        </>
    );
}
