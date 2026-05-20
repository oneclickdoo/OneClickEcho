# Zabeleške iz razvojne sesije (Cursor workspace)

**Datum:** 2026-04-02  
**Svrha:** Kratak zapis šta je menjano u repou tokom sesije, da pri sledećem ulasku u workspace bude jasan kontekst. Nije zamena za `git log`.

---

## 1. Dashboard — analitika kampanje (Viber)

- **Kartica „Viber primaoci“:** broj za liniju dostave prikazuje **`funnelDelivered`** (dostavljeno + viđeno + klik), ne samo strogi `delivered`.
- **i18n:** `cards.messagesSent.deliveredDevice` u `en.json` / `sr.json` usklađen sa tim značenjem.
- **Funnel grafikon:** fiksni redosled redova: ukupno poruka → ukupno isporučeno (`funnelDelivered`) → isporučeno (status 3) → viđeno → kliknuto → na čekanju → neisporučeno → neposlate (samo ako `notSent > 0`).
- **Tipovi:** `IFunnelChartData` proširen sa `funnelDelivered`; uklonjen `received` iz funela (i dalje u pie/KPI).
- **Fajlovi:** `CampaignAnalyticsTab.tsx`, `messages/en.json`, `messages/sr.json`.

---

## 2. Pokretanje kampanje — brži HTTP, priprema u pozadini

- **Novi status:** `CampaignStatus.PreparingLaunch = 5` u domenu (`OneClickEcho.Domain`).
- **`LaunchCampaignHandler`:** samo validacija (Draft, kanal, kolekcije, zakazano vreme), postavljanje `PreparingLaunch`, `SaveChanges`, zatim zakazivanje Quartz posla — **bez** masovnog brisanja/dodavanja leadova u tom requestu.
- **`PrepareCampaignLaunchCommand` / `PrepareCampaignLaunchHandler`:** prebačena stara logika pripreme leadova; na kraju `Queued`; za **Immediate** odmah zakazuje slanje.
- **`PrepareCampaignLaunchJob`** (Quartz) + **`ICampaignLaunchScheduler`** / **`QuartzCampaignLaunchScheduler`**.
- **`PauseCampaignHandler`:** dozvoljena pauza i iz `PreparingLaunch` (pored `Queued`).
- **Registracija:** `InfrastructureServiceRegistration` — `AddSingleton<ICampaignLaunchScheduler, QuartzCampaignLaunchScheduler>()`, Scrutor isključuje duplikat po imenu klase.

---

## 3. Dashboard — UX za launch i status

- **Enum:** `CampaignStatus.PreparingLaunch` u `src/lib/enums.ts`.
- **Badge / lista:** `selects.ts`, `CampaignsTable.tsx`, `Enums` + `Common` u `messages`.
- **Stranica kampanje:** toast sa `launchSuccess` + `launchSuccessBackground`, duže trajanje; `useQuery` `refetchInterval` 2s dok je `PreparingLaunch`; dugme Pauza i za `PreparingLaunch`.
- **Vizuel:** `CampaignPreparingLaunchIndicator.tsx` (ping tačka); bedž sa `Tooltip` (Radix/Tremor) umesto `title`; `getStatusVariant` za preparing → `default` (indigo) u tabeli.
- **`CampaignAnalyticsTab`:** `isNotLaunched` uključuje `PreparingLaunch`; analytics query isključen tokom `PreparingLaunch`.

---

## 4. Infrastruktura — Viber delivery (Comtrade)

- **`ViberDeliveryService.GetViberDeliveryForLast49Hours`:**
  - **`Dictionary<long, CampaignLead>`** po `ViberMessageId` + `TryGetValue` umesto `FirstOrDefault` u unutrašnjoj petlji (manje CPU na velikim listama).
  - **`SaveChangesAsync` posle svakog Comtrade chunk-a** (posle obrade jednog `DeliveryById` odgovora), zatim i dalje završni `SaveChanges` posle SMS fallback-a i odjava.
- **Konfiguracija Quartz:** `Scheduling:ViberDeliveryPollIntervalMinutes` (podrazumevano **1**, clamp **1–120** min) za interval **`ViberDeliveryJob`** — čita se u `AddSchedulingService(this IServiceCollection services, IConfiguration configuration)`.
- **Fajlovi:** `ViberDeliveryService.cs`, `SchedulingServiceConfiguration.cs`, `InfrastructureServiceRegistration.cs`, `OneClickEcho.Api/appsettings.json`.

---

## 5. Napomene za sledeći rad

- Proveriti **Comtrade rate limit** pre smanjenja `ViberDeliveryPollIntervalMinutes` ispod 1 (ako ikada pređe na sekunde, treba poseo dizajn triggera).
- **`GetLast49HoursViberCampaigns`** i dalje filtrira `InProgress | Done` (delivery i za završene kampanje u prozoru) — nije menjano u ovoj sesiji osim ako nije druga grana.

---

## 6. Viber outbound anti-dup zaštita (2026-05-04)

- **`ViberSendingService.SendViberMessagesToLeads`:** pre outbound HTTP poziva, svaki `CampaignLead` u batch-u sada ide u `ViberStatus = Pending` + status opis i odmah se radi `SaveChangesAsync`.
- Time `RetryPendingViberCampaignSendsJob` više ne može da pokupi iste redove kao `None` dok je prvi send u toku (sprečava duplo slanje iste kampanje ka istom telefonu pri paralelnom/proklizalom izvršavanju jobova).
- Posle odgovora od Comtrade-a zadržana je postojeća logika prelaza na `Received`/`Undelivered`.

---

## 7. Evidencija duplih Comtrade delivery redova (2026-05-04)

- Dodata nova tabela **`viber_delivery_events`** (EF migracija `AddViberDeliveryEvents`) za sirove Viber delivery događaje po `CampaignLead`.
- **`ViberDeliveryService.GetViberDeliveryForLast49Hours`:** pre deduplikacije za status update, svaki red iz Comtrade JSON odgovora se upisuje kao poseban event.
- Time i potpuno isti duplikati (isti `message_id`, status i substatus) ostaju vidljivi u našoj bazi i mogu da se analiziraju kroz audit/izveštaj.
- Deduplikacija (`DeliveryViberResponseDeduplicator`) ostaje aktivna samo za izračun trenutnog “finalnog” statusa na `CampaignLead` redu.

---

## 8. Ograničenje unosa u `viber_delivery_events` (2026-05-05)

- U `ViberDeliveryService.GetViberDeliveryForLast49Hours` tabela `viber_delivery_events` sada čuva samo **višak duplikata** po istom `message_id` (drugi, treći… red), ne i prvi/originalni delivery red.
- Time se broj redova u tabeli približava realnom broju duplih delivery zapisa umesto da raste sa svim regularnim polling rezultatima.

---

## 9. Docker — API port na hostu (2026-05-20)

- U `docker-compose.yml` za servis `api` dodato mapiranje **`127.0.0.1:3901:3901`** da `curl`/nginx na hostu mogu da gađaju API; bez toga API sluša samo unutar Docker mreže (`dashboard` → `http://api:3901` i dalje radi).

---

**Cursor:** pravilo [`.cursor/rules/workspace-session-context.mdc`](../.cursor/rules/workspace-session-context.mdc) (`alwaysApply: true`) podsjeće agenta da pročita ovaj fajl pri većim zadacima.

*Ažuriraj ovaj fajl ili dodaj novu sekciju kada uradiš veće izmene van git commit poruka.*
