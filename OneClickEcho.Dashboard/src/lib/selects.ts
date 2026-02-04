import { badgeVariants } from "@/components/tremor/Badge";
import { CampaignSendingType, CampaignStatus, LeadGender } from "@/lib/enums";

export interface SelectOption<TValue> {
    value: TValue;
    label: string;
}

type BadgeVariant =
    (typeof badgeVariants)["variants"]["variant"][keyof (typeof badgeVariants)["variants"]["variant"]];

export interface CampaignStatusOption extends SelectOption<CampaignStatus> {
    variant: BadgeVariant;
}

/**
 * next-intl usage:
 * const tCommon = useTranslations("Common");
 * const sendingTypeOptions = getCampaignSendingTypeOptions(tCommon);
 */
export const getCampaignSendingTypeOptions = (t: (key: string) => string): Array<SelectOption<CampaignSendingType>> => [
    {
        value: CampaignSendingType.ScheduledDateTime,
        label: t("campaignSendingType.scheduledDateTime")
    },
    {
        value: CampaignSendingType.ByDateOfBirth,
        label: t("campaignSendingType.byDateOfBirth")
    },
    {
        value: CampaignSendingType.Immediate,
        label: t("campaignSendingType.immediate")
    }
];

/**
 * next-intl usage:
 * const tCommon = useTranslations("Common");
 * const statusOptions = getCampaignStatusOptions(tCommon);
 */
export const getCampaignStatusOptions = (t: (key: string) => string): Array<CampaignStatusOption> => [
    {
        value: CampaignStatus.Draft,
        label: t("campaignStatus.draft"),
        variant: badgeVariants.variants.variant.neutral
    },
    {
        value: CampaignStatus.Queued,
        label: t("campaignStatus.queued"),
        variant: badgeVariants.variants.variant.warning
    },
    {
        value: CampaignStatus.InProgress,
        label: t("campaignStatus.inProgress"),
        variant: badgeVariants.variants.variant.warning
    },
    {
        value: CampaignStatus.Done,
        label: t("campaignStatus.done"),
        variant: badgeVariants.variants.variant.success
    }
];

export const getLeadGenderOptions = (t: (key: string) => string): Array<SelectOption<LeadGender>> => [
    {
        value: LeadGender.Male,
        label: t("leadGender.male")
    },
    {
        value: LeadGender.Female,
        label: t("leadGender.female")
    }
];
