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

## 10. Docker — Postgres 17 + lozinka iz `.env` (2026-05-21)

- U `docker-compose.yml` slika **`postgres:17`** (ranije 16). `POSTGRES_PASSWORD` se čita iz **`.env`** (`${POSTGRES_PASSWORD:-changeme_set_in_dot_env}`) da `ConnectionStrings__Database` i Postgres budu usklađeni bez hardkoda u gitu.
- **Nadogradnja 16→17 na postojećem `pgdata`:** samo promena taga **ne radi** — potreban je `pg_dump` / restore ili novi volume. Za svež deploy prazan volume + migracije je OK.

---

## 12. Handbook — provera šeme vs migracije (2026-05-21)

- U `docs/PROJECT-HANDBOOK.md` dodat odeljak **2.8** (mapiranje ručnih SQL izmena na `migration_id`, SQL provere, upozorenje na lažan `__EFMigrationsHistory`), proširena checklista deploya i tabela u odeljku 8.

## 11. Dashboard — `/auth/access_token` log na API 500 (2026-05-21)

- API na grešci vraća **ProblemDetails** (`detail`), ne OAuth `error_description`. Rute `src/app/auth/access_token/route.ts` i `[locale]/auth/access_token/route.ts` sada čitaju i **`detail`**, da `docker compose logs dashboard` i JSON klijentu pokažu npr. „A database error occurred…“ umesto generičkog „Internal Server Error“.

---

## 13. Campaign lead report — HTTP 504 na velikim Viber filterima (2026-07-15)

- **Simptom:** filteri **Nije poslato (0)**, **Primljeno (1)**, **Isteklo (6)** → „Izveštaj nije mogao da se učita“ / **HTTP 504**; manji bucketi (npr. Viđeno, Kliknuto) rade. Najgore na **velikim današnjim** kampanjama (Expired).
- **Uzrok:** nije greška mapiranja filtera. Stari upit radio je `COUNT` + JOIN + `ORDER BY` telefon preko celog bucketa — duže od reverse-proxy timeouta.
- **Fix 1:** `COUNT` bez JOIN-a kad nema phone/unsubscribe filtera; `AsNoTracking`.
- **Fix 2:** status-only stranica — prvo `ORDER BY viber_message_id` + `LIMIT` na `campaign_leads`, pa JOIN samo za tu stranicu (ne sortira milione telefona).
- **Indeks:** `(campaign_id, viber_status, viber_message_id)` — `ix_campaign_leads_campaign_id_viber_status_message_id` (migracija `20260715140000_…`; stari 2-kolonski indeks se dropuje).
- **Fajlovi:** `CampaignLeadRepository.cs`, `CampaignLeadConfiguration.cs`, migracije + snapshot, i18n subtitle.

---

## 23. Kampanja — „Omogući odgovore“ samo za tekst, sliku i anketu uz dvosmernog pošiljaoca (2026-10-29)

- Checkbox je vidljiv samo za tekst, sliku i anketu, i samo ako je izabrani Viber pošiljalac označen kao dvosmeran (`senders.is_two_way`). Karusel, fajl, video i šablon ga nemaju; uključen flag se tada gasi.
- Na stranici kompanije, uz Viber pošiljaoca, stoji „Dvosmerno (odgovori)“. Kolona: `ALTER TABLE public.senders ADD COLUMN IF NOT EXISTS is_two_way boolean NOT NULL DEFAULT false;`
- Anketa i dalje sama uključuje odgovore, ali samo kad je pošiljalac dvosmeran.

## 22. Kampanja — checkbox „Transakciona poruka“ samo za tekst (2026-10-29)

- Checkbox je vidljiv samo kad je vrsta poruke „Tekst (bez medije)“. Za sliku, video, fajl, anketu, karusel i šablon se ne prikazuje, pa se ne može slučajno uključiti i vratiti formu na tekst.
- Prelazak sa teksta na drugu vrstu gasi `isTransactional`.

## 21. Kampanja — unos na tabu Poruke ostaje pri promeni taba (2026-10-29)

- Prelazak sa prvog taba više ne snima pa ponovo ne učitava kampanju sa servera, što je brisalo nečuvani tekst, pošiljaoca i vrstu poruke.
- Tab Poruke ostaje montiran, pa se lokalni unos (anketa, fajl) ne gubi dok se ne sačuva ili ne pokrene kampanja.

## 20. Viber sesija — nit prikazuje anketu i odgovor (2026-10-29)

- Dashboard sesije više ne prikazuje samo odlazne sesijske poruke. U istu nit ulaze i pitanje kampanje (anketa) i redovi iz `received_messages` za taj telefon i pošiljaoca.
- `ViberAnswersJob` ide na 1 minut. Odgovor sa telefona se upisuje tek kad taj posao povuče `AnswersById`, i samo ako API radi sa ispravkom datuma `/Date(...)/`.

## 19. Viber sesija — slika bez teksta (2026-10-29)

- Comtrade tip **307** je samo slika, **221** samo fajl. Tekst uz njih se ne šalje u istom payload-u.
- Ako korisnik unese i tekst i sliku/fajl, prvo ide posebna poruka **306**, pa medij. Svaka dobija svoj `viber_message_id` i broji se u limit 10/60.
- `8000000000` je namerni početak identity za sesijske poruke.

## 18. Viber odgovori — datum Comtrade `/Date(...)/` (2026-10-29)

- **Simptom:** `AnswersById` vrati odgovor, `received_messages` ostane prazan, ponovni poziv da `ViberAnswers: []`.
- **Uzrok:** `ViberAnswer.Received` je `DateTime`, a Comtrade šalje `/Date(milisekunde+offset)/`. `System.Text.Json` baci izuzetak, posao ne sačuva red, a Comtrade odgovor više ne vraća.
- **Fix:** `ComtradeDateTimeJsonConverter` na `Received`. Odgovori kampanje se snimaju pre poll-a sesije.
- **Fajlovi:** `ViberAnswer.cs`, `ComtradeDateTimeJsonConverter.cs`, `ViberDeliveryService.cs`.

## 17. Comtrade Viber — GIF, karusel, šabloni, sesija (2026-10-22)

- **GIF / odnos slike / anketa:** `ImageFormat` (`gif` do 24 MB, tipovi 107 i 108) i `ImageRatio` (`square` / `portrait` / `landscape`). Anketa 2–10 opcija, `ListMessageUiType` 1 radio / 2 dugmad. Kampanja + `POST /api/Message/Send`.
- **Karusel 901** i **transakcioni šablon 1702** na kampanji i na Send. Lokalni CRUD šablona: dashboard `api/app/viber-templates`, javni `api/viber/templates` (ista auth kao Send). Slanje samo kad je status `APPROVED`.
- **Viber sesija** nije Message/Send. Odgovor na kampanju sa „Omogući odgovore“ otvara prozor 24h (60 poruka, 10 zaredom). Slanje 306/307/221/802: dashboard `api/app/viber-sessions`, javni `POST /api/viber/session/send`. Van prozora HTTP 409 `ViberSession.Closed`; `allowOutsideSession` šalje kao promociju.
- **Migracija:** `20261022140000_AddComtradeViberGifCarouselTemplateSession`. Identity sesijskih poruka od `8000000000`.
- **Fajlovi:** domen/slanje/kontroleri, dashboard stranice `viber-templates` i `viber-sessions`, `doc.md`, `ApiMessage.http`.

## 16. API Message — blokada duplikata istog dana (2026-08-04)

- `POST /api/Message/Send`: odbija identičnu poruku (ista kompanija, telefon, tip, tekst, sender, media, button) u istom kalendarskom danu (Europe/Belgrade) sa **HTTP 409** `ApiMessage.DuplicateSameDay` (+ `ExistingId` u poruci).
- **Fajlovi:** `SendApiMessageHandler`, `IApiMessageRepository.FindIdenticalSameDayAsync`, `ApiMessageRepository`, `ApiMessageController`, `doc.md`.

## 15. API Message delivery poll — ne stati na Seen (2026-08-03)

- `GetSentApiMessages` ranije isključivao `ViberStatus == Seen`, pa klik posle otvaranja poruke nije mogao da se upiše kao `Clicked`.
- Sada se polluje dok status nije **Clicked** ili **Expired** (kao kod campaign leadova).
- Napomena: ako Comtrade `DeliveryById` vrati `ClickCount: 0`, mi i dalje ne možemo da postavimo Kliknuto — to je na provajderu.

## 14. API Message — status u Send response + GET poll (2026-08-03)

- **Send** (`POST /api/Message/Send`) više ne vraća samo `{ id }` — dodati `isSent`, `viberStatus`, `viberStatusDescription`, `smsStatus`, `smsStatusDescription`, `phoneNumber`, `createdAt` (početno: status 0 / „još nije poslata“).
- **Novi endpoint:** `GET /api/Message/{id}/status?companyId=&apiPassword=` — ista auth šema kao Send; čita ažurirani status iz `api_messages` (Quartz delivery ~1 min).
- **Fajlovi:** `SendApiMessageResponse` / Handler, `GetApiMessageStatus*`, `ApiMessageController`, `IApiMessageRepository.GetByIdAsync`, `doc.md`, `ApiMessage.http`.

---

**Cursor:** pravilo [`.cursor/rules/workspace-session-context.mdc`](../.cursor/rules/workspace-session-context.mdc) (`alwaysApply: true`) podsjeće agenta da pročita ovaj fajl pri većim zadacima.

*Ažuriraj ovaj fajl ili dodaj novu sekciju kada uradiš veće izmene van git commit poruka.*
