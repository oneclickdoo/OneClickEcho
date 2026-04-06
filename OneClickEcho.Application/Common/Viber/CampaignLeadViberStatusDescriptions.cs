using OneClickEcho.Application.Common.Services.ViberService.Response.Enum;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Application.Common.Viber;

/// <summary>
/// Human-readable <c>viber_status_description</c> (Serbian) stored on campaign leads / API messages.
/// </summary>
public static class CampaignLeadViberStatusDescriptions
{
    /// <summary>Početno kada bulk send postavi <c>ViberStatus = Received</c> (1).</summary>
    public static string ForBulkSendSuccess() =>
        "Primljeno.";

    public static string ForBulkSendFailure(ViberMessageResponseStatus responseStatus) =>
        $"Odbijeno pri slanju ka provajderu: {FormatSendStatus(responseStatus)}.";

    /// <summary>
    /// Posle Comtrade delivery odgovora: osnovni tekst po statusu + opis <paramref name="subStatus"/> kada nije <see cref="DeliveryViberSubstatus.SRVC_SUCCESS"/>.
    /// </summary>
    public static string ForDelivery(CampaignLeadViberStatus status, DeliveryViberSubstatus subStatus, bool treatAsClicked)
    {
        CampaignLeadViberStatus effective = treatAsClicked ? CampaignLeadViberStatus.Clicked : status;
        string baseText = BaseForStatus(effective);

        if (subStatus != DeliveryViberSubstatus.SRVC_SUCCESS)
        {
            string sub = FormatSubStatus(subStatus);
            if (!string.IsNullOrWhiteSpace(sub))
            {
                return $"{baseText} {sub}".Trim();
            }
        }

        return baseText;
    }

    public static string ForExpired() =>
        "Poruka je istekla (validnost) pre konačne isporuke.";

    private static string BaseForStatus(CampaignLeadViberStatus status) =>
        status switch
        {
            CampaignLeadViberStatus.None => "Poruka još nije poslata ka provajderu.",
            CampaignLeadViberStatus.Received => "Primljeno.",
            CampaignLeadViberStatus.Pending => "Na čekanju kod Viber provajdera.",
            CampaignLeadViberStatus.Delivered => "Isporučena na uređaj primaoca.",
            CampaignLeadViberStatus.Seen => "Viđena od strane primaoca.",
            CampaignLeadViberStatus.Undelivered => "Nije isporučena na uređaj.",
            CampaignLeadViberStatus.Expired => "Istekla pre isporuke.",
            CampaignLeadViberStatus.Clicked => "Primaoc je kliknuo na sadržaj u poruci.",
            _ => "Nepoznat Viber status."
        };

    private static string FormatSendStatus(ViberMessageResponseStatus s) =>
        s switch
        {
            ViberMessageResponseStatus.MSG_SUCCESS => "uspeh",
            ViberMessageResponseStatus.MSG_GENERAL_ERROR => "opšta greška",
            ViberMessageResponseStatus.MSG_UNAUTHORIZED => "neovlašćeno",
            ViberMessageResponseStatus.MSG_NO_CREDIT => "nema kredita",
            ViberMessageResponseStatus.MSG_WRONG_DESTINATION => "pogrešna destinacija",
            ViberMessageResponseStatus.MSG_WRONG_DISPLAY =>
                "neispravan Viber pošiljalac (API polje Display): proverite „Viber sender” u kampanji — mora tačno odgovarati imenu odobrenom kod provajdera; ovo nije greška slike ili videa",
            _ => s.ToString()
        };

    private static string FormatSubStatus(DeliveryViberSubstatus sub) =>
        sub switch
        {
            DeliveryViberSubstatus.SRVC_SUCCESS => string.Empty,
            DeliveryViberSubstatus.SRVC_INTERNAL_FAILURE => "Razlog: interna greška servisa.",
            DeliveryViberSubstatus.SRVC_BAD_SERVICE_ID => "Razlog: neispravan servis.",
            DeliveryViberSubstatus.SRVC_BAD_DATA => "Razlog: neispravni podaci.",
            DeliveryViberSubstatus.SRVC_BLOCKED_MESSAGE_TYPE => "Razlog: blokiran tip poruke.",
            DeliveryViberSubstatus.SRVC_BAD_MESSAGE_TYPE => "Razlog: neispravan tip poruke.",
            DeliveryViberSubstatus.SRVC_BAD_PARAMETERS => "Razlog: neispravni parametri.",
            DeliveryViberSubstatus.SRVC_TIMEOUT => "Razlog: isteklo vreme.",
            DeliveryViberSubstatus.SRVC_USER_BLOCKED => "Razlog: korisnik je blokirao poruke.",
            DeliveryViberSubstatus.SRVC_NOT_VIBER_USER => "Razlog: broj nije Viber korisnik.",
            DeliveryViberSubstatus.SRVC_NO_SUITABLE_DEVICE => "Razlog: nema odgovarajućeg uređaja.",
            DeliveryViberSubstatus.SRVC_UNAUTHORIZED_IP => "Razlog: neovlašćena IP adresa.",
            DeliveryViberSubstatus.SRVC_ALREADY_SENT => "Razlog: poruka je već poslata.",
            DeliveryViberSubstatus.SRVC_NOT_PERMITTED => "Razlog: nije dozvoljeno.",
            DeliveryViberSubstatus.SRVC_BILLING_FAILURE => "Razlog: greška naplate.",
            DeliveryViberSubstatus.SRVC_NO_MORE_MESSAGES => "Razlog: nema više poruka u kvoti.",
            DeliveryViberSubstatus.SRVC_BAD_LABEL => "Razlog: neispravna oznaka (label).",
            DeliveryViberSubstatus.SRVC_INVALID_TTL => "Razlog: neispravno vreme važenja (TTL).",
            DeliveryViberSubstatus.SRVC_WAIT_FOR_USER_RESPONSE => "Razlog: čeka se odgovor korisnika.",
            DeliveryViberSubstatus.SRVC_INVALID_PHONE_NUMBER => "Razlog: neispravan broj telefona.",
            DeliveryViberSubstatus.SRVC_FILE_NOT_PERMITTED => "Razlog: tip fajla nije dozvoljen.",
            DeliveryViberSubstatus.SRVC_BAD_FILE_NAME_LENGTH => "Razlog: neispravna dužina imena fajla.",
            DeliveryViberSubstatus.SRVC_BAD_THUMBNAIL => "Razlog: neispravna minijatura (thumbnail).",
            DeliveryViberSubstatus.SRVC_BAD_FILE_SIZE => "Razlog: neispravna veličina fajla.",
            DeliveryViberSubstatus.SRVC_BAD_DURATION => "Razlog: neispravno trajanje videa.",
            DeliveryViberSubstatus.TEMPLATE_NOT_FOUND => "Razlog: šablon nije pronađen.",
            DeliveryViberSubstatus.TEMPLATE_VALIDATION_ERROR => "Razlog: greška validacije šablona.",
            DeliveryViberSubstatus.SRVC_SURVEY_VALIDATION_ERROR => "Razlog: greška validacije ankete/liste.",
            DeliveryViberSubstatus.SRVC_CAROUSEL_VALIDATION_ERROR => "Razlog: greška validacije karusela.",
            _ => $"Razlog: {sub}."
        };
}
