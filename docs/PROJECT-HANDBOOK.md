# OneClickEcho — priručnik za projekat, Docker, Next.js, nginx, Git i server

Ovaj dokument sumira arhitekturu repozitorijuma, kritične tačke ponašanja sistema i operativne korake za razvoj, kontejnerizaciju i produkciju. Za kratki uvod u lokalno pokretanje API-ja i migracije vidi i [README.md](../README.md) u korenu repoa.

---

## 1. Pregled repozitorijuma

| Sloj / projekat | Uloga |
|-----------------|--------|
| **OneClickEcho.Api** | ASP.NET Core host: REST API, autentikacija (OpenIddict), statički `/uploads` gde je podešeno. |
| **OneClickEcho.Application** | CQRS komande/upiti, validacija poslovnih pravila. |
| **OneClickEcho.Domain** | Agregati (kampanja, lead, company…), enumi, value objekti. |
| **OneClickEcho.Infrastructure** | Spoljni servisi: Viber/SMS slanje, Quartz jobovi, integracije. |
| **OneClickEcho.Persistence** | EF Core, repozitorijumi, OData-style filteri za listu (`Filtering<T>`), migracije. |
| **OneClickEcho.Dashboard** | Next.js (App Router), admin UI, `next-intl` (lokalizacija). |
| **docker/** | `Dockerfile_Api`, `Dockerfile_Dashboard`, log rotacija. |
| **docker-compose.yml** | `api`, `dashboard`, `postgres`, `redis`, Seq, `log-rotator`; profil **`full`** za API/dashboard/log-rotator. |

**Tok u produkciji (tipično):** browser → **nginx** (HTTPS) → **Next.js** na portu **3800**; API pozivi idu na isti host (npr. `/api`) ili na poseban upstream ka **API** na portu **3901** (zavisi od nginx konfiguracije i `NEXT_PUBLIC_API_URL`).

---

## 2. Važne tačke u kodu i ponašanju

### 2.1 Autentikacija i okruženje

- **OpenIddict** u Dockeru zahteva promenljive iz `docker-compose.yml` (RSA PKCS#8 base64, issuer, signing/encryption). Issuer mora da se poklapa sa javnim URL-om sajta (inace greške tipa ID2088).
- **PublicUploads:BaseUrl** — apsolutan HTTPS URL do `/uploads` (npr. `https://domen/uploads`), ne interni `http://api:3901`. Bitno za Viber medija (Comtrade substatus 28 ako je pogrešno).

### 2.2 `viber_message_id`

- Dodeljuje se pri slanju kampanje; PostgreSQL identitet + opcioni **floor** (`Messaging:CampaignLeadViberMessageId:Floor` / `VIBER_CAMPAIGN_MESSAGE_ID_FLOOR`) da se ne preklapaju sa starim sistemima posle migracije baze.
- Delivery job uparuje odgovor Comtrade-a sa `CampaignLead.ViberMessageId`.

### 2.3 OData filter i lista kampanja

- Backend **ne podržava zagrade** u filter stringu na nivou lexer-a. `CampaignTenantFilter.BuildCampaignsListFilter` spaja tenant filter sa klijentskim filterom **bez** `(...)` oko klauzula — inace 500 pri listi kampanja sa godišnjim filterom.

### 2.4 Viber delivery i SMS fallback

- **ViberDeliveryJob** (npr. svakih 1 min) pita Comtrade za statuse; za **Undelivered** na Viberu i uključenom **SMS fallback**-u šalje SMS.
- Duplikat SMS-a sprečava se time što se **`SMSStatus`** rezerviše (**Pending**) pre HTTP poziva ka SMS gateway-u i time što se u listu za fallback ne ulaze leadovi koji već nemaju `SMSStatus == None`. Detalji u `SmsSendingService` / `ViberDeliveryService`.

### 2.5 Docker i Postgres

- U `docker-compose.yml` servis **postgres** ima fiksno **`container_name: oneclick_postgres`**. Ako taj kontejner već postoji izvan trenutnog Compose projekta, `docker compose up` može javiti **Conflict**. Rešenje: podići samo app servise (`api`, `dashboard`, `log-rotator`) ili uskladiti jedan izvor istine za Postgres.

### 2.6 Disk i Docker build cache

- `docker builder prune` brzo oslobađa desetine GB ako se često radi `docker compose build` na serveru.

### 2.7 Viber — sprečavanje duplog slanja i evidencija duplih delivery redova (Comtrade)

Ovaj projekat ima **dva odvojena cilja**:

- **Sprečiti duplo slanje** iste kampanje ka istom telefonu (naš bug / race condition).
- Ako Comtrade u delivery odgovoru vrati **duple redove**, to treba evidentirati u bazi radi audita (provajder/transport duplikati).

#### 2.7.1 Sprečavanje duplog slanja (outbound)

- Slanje kampanje (Viber) radi kroz `MessageSendingService` → `ViberSendingService.SendViberMessagesToLeads`.
- Da bi se sprečilo duplo slanje pri paralelnom izvršavanju job-ova, outbound ima **atomsku DB rezervaciju**:
  - `ICampaignLeadRepository.TryMarkViberPendingIfNoneAsync(...)`
  - radi `UPDATE ... WHERE viber_status = None` i postavlja `Pending` pre HTTP poziva ka Comtrade-u.
- `ViberDeliveryJob` je označen sa `[DisallowConcurrentExecution]` da Quartz ne pokreće isti job paralelno u istom scheduleru.

#### 2.7.2 `viber_delivery_events` — tabela za duple delivery redove

Tabela `viber_delivery_events` služi **samo** za evidentiranje duplih delivery redova koje Comtrade vrati u `DeliveryById` JSON-u.
Ne služi za kompletan “history” svih delivery polling odgovora.

- Popunjava se u `ViberDeliveryService.GetViberDeliveryForLast49Hours`.
- Upis je **serijalizovan** u bazi (repo koristi `pg_advisory_xact_lock` u transakciji) da bi se izbegli dupli upisi pri concurrency/race.

#### 2.7.3 Pravilo šta se upisuje (ključ “isti izraz”)

Posmatra se “isti izraz” iz Comtrade JSON-a po ključu:

- `MessageId` (naš `CampaignLead.ViberMessageId`)
- `Status`
- `SubStatus`
- `ClickCount`

Ignoriše se `Delivered` timestamp (može da se razlikuje a da je poruka realno ista).

Upis u `viber_delivery_events` se radi **samo ako u istom JSON-u** postoji više identičnih redova po tom ključu:

- Ako je identičan red prisutan \(N\) puta u `ViberMessageResponses`, upisuje se \(N - 1\) redova u `viber_delivery_events`.
- Ako se bilo šta od ključa razlikuje (npr. `Status` 3 pa 4, ili `ClickCount` 0 pa 1), to je progresija i **ne upisuje se**.

#### 2.7.4 Operativa — SQL provere i čišćenje

Broj duplikata po `message_id/status/substatus/click_count`:

```sql
SELECT viber_message_id, status, sub_status, click_count, COUNT(*) AS cnt
FROM public.viber_delivery_events
GROUP BY viber_message_id, status, sub_status, click_count
HAVING COUNT(*) > 1
ORDER BY cnt DESC;
```

Brisanje viškova (ostavi po 1 red po ključu):

```sql
WITH ranked AS (
    SELECT
        id,
        ROW_NUMBER() OVER (
            PARTITION BY campaign_lead_id, viber_message_id, status, sub_status, click_count
            ORDER BY created_at ASC, id ASC
        ) AS rn
    FROM public.viber_delivery_events
)
DELETE FROM public.viber_delivery_events v
USING ranked r
WHERE v.id = r.id
  AND r.rn > 1;
```

#### 2.7.5 Troubleshooting

- Ako `viber_delivery_events` ostaje prazna iako očekujete duplikate:
  - proveriti API logove za `ViberDeliveryJob` exception (Quartz job može da “pada” i tada nema upisa),
  - proveriti da li API kontejner zaista radi na poslednjem commitu (rebuild bez keša po potrebi).

### 2.8 Migracije EF i provera šeme baze (produkcija)

Izvor istine za šemu je **`OneClickEcho.Persistence/Migrations/`** + **`ApplicationDbContextModelSnapshot.cs`**. Pri startu API-ja `SeederRunner` poziva `Database.MigrateAsync()` i primenjuje sve migracije koje **nedostaju** u `__EFMigrationsHistory`.

**Ne raditi** samo ručni `INSERT` u `__EFMigrationsHistory` bez odgovarajućeg `Up()` — baza ostaje bez kolona, a EF misli da je migracija primenjena (greške tipa `42703 column ... does not exist`).

#### 2.8.1 Ručne izmene na serveru → migracija u repou

Sve kolone/tabele koje su na produkciji dodavane SQL-om tokom deploya **već postoje** u migracijama:

| Šema (kolona / indeks / tabela) | Tabela | `migration_id` |
|--------------------------------|--------|----------------|
| `viber_file_size`, `viber_video_thumbnail`, `viber_video_duration` | `api_messages` | `20260402120100_ApiMessageViberVideoMetadata` |
| `sms_message`, `sms_sender`, `viber_validity` | `api_messages` | `20251102011826_AddSmsMessageSenderAndViberValidity` |
| `viber_content_kind`, `viber_survey_options_json` | `campaigns` | `20260402140000_AddCampaignViberContentKindSurvey` |
| unique index `ix_campaign_leads_campaign_id_lead_id_unique` | `campaign_leads` | `20260402120000_UniqueCampaignLeadCampaignAndLead` |
| tabela `viber_delivery_events` | nova tabela | `20260504115724_AddViberDeliveryEvents` |

**Napomena:** `CampaignStatus.PreparingLaunch = 5` u kodu **ne zahteva** novu migraciju — koristi postojeću kolonu `campaigns.status` (`smallint`).

#### 2.8.2 Provera istorije migracija

```bash
docker exec -it oneclick_postgres psql -U oneclickecho_admin -d oneclickecho -c \
  "SELECT migration_id FROM \"__EFMigrationsHistory\" ORDER BY migration_id;"
```

Za poslednje izmene (april–maj 2026) moraju postojati bar:

- `20251102011826_AddSmsMessageSenderAndViberValidity`
- `20260402120000_UniqueCampaignLeadCampaignAndLead`
- `20260402120100_ApiMessageViberVideoMetadata`
- `20260402140000_AddCampaignViberContentKindSurvey`
- `20260504115724_AddViberDeliveryEvents`

#### 2.8.3 Provera kolona i tabele (jedan upit)

```bash
docker exec -it oneclick_postgres psql -U oneclickecho_admin -d oneclickecho -c "
SELECT
  EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='api_messages' AND column_name='viber_file_size') AS api_viber_file_size,
  EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='api_messages' AND column_name='sms_message') AS api_sms_message,
  EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='campaigns' AND column_name='viber_content_kind') AS camp_content_kind,
  EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='campaigns' AND column_name='viber_survey_options_json') AS camp_survey_json,
  EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='viber_delivery_events') AS viber_delivery_events_tbl,
  EXISTS (SELECT 1 FROM pg_indexes WHERE indexname='ix_campaign_leads_campaign_id_lead_id_unique') AS cl_unique_idx;
"
```

Sve kolone `t` → šema se slaže sa repoom. Ako nešto `f` → pokrenuti `dotnet ef database update` lokalno protiv kopije baze, ili na serveru **restart API** posle deploya poslednjeg image-a i proveriti log (`Migration started` / greška migrate), ili primeniti odgovarajući `Up()` iz fajla migracije u `Migrations/`.

#### 2.8.4 Novi server (preporuka)

1. Prazan volume za Postgres (ili svesno restore iz `pg_dump` iste verzije šeme).
2. `docker compose --profile full up -d api` — migracije pri startu.
3. Provera iz **2.8.2** i **2.8.3** pre puštanja saobraćaja.

Primer `.env` za OpenIddict u Dockeru: vidi [docs/.env.txt](.env.txt) (`OpenIddict__Issuer`, `OpenIddict__RsaPrivateKeyPkcs8` — učitavaju se preko `env_file`; ne pregaziti praznim `${OPENIDDICT_*:-}` u compose-u).

#### 2.8.5 Selektivni restore jedne kompanije (npr. Biosvet)

Kad je baza prazna / seedovana, a treba samo **jedna kompanija** iz starog dump-a (kampanje, leadovi, `sms_username` / `sms_password` / `api_password`), **ne** raditi pun `dropdb` + restore celog dump-a.

Skripta: [`scripts/restore-company-from-dump.sh`](../scripts/restore-company-from-dump.sh) (server) ili [`scripts/restore-company-from-dump.ps1`](../scripts/restore-company-from-dump.ps1) (Windows).

Server (`/root/oneclickecho.dump`):

```bash
cd /var/www/OneClickEcho
docker compose --profile full stop api dashboard
COMPANY_ID=075fe381-fda7-4d94-aaf1-5d72ec07a2eb DUMP_PATH=/root/oneclickecho.dump ./scripts/restore-company-from-dump.sh
docker compose --profile full up -d api dashboard
```

- Ostale kompanije i globalni login **`itocs@oneclick.rs`** se ne diraju.
- Kredencijali kompanije dolaze iz dump-a (`companies`); OpenIddict / `Viber__*` u `.env` ostaju.

---

## 3. Git

```bash
git clone https://github.com/oneclickdoo/OneClickEcho.git
cd OneClickEcho
```

- Grane i PR proces: po dogovoru tima; za produkciju deployujte samo tagovane / reviewovane commit-e.
- **Ne commitovati** tajne (connection stringovi, OpenIddict ključevi, Viber/SMS lozinke). Koristiti `.env` na serveru (van repoa), User Secrets lokalno za API, ili menadžer tajni u CI/CD.
- U repou postoji `.gitlab-ci.yml` — ako koristite samo GitHub, ili podesite runnere ili uklonite/zamenite pipeline.

---

## 4. Lokalni razvoj (bez Docker celog steka)

### 4.1 API (.NET 8)

```bash
dotnet run --project OneClickEcho.Api
```

Migracije:

```bash
dotnet ef migrations add <ImeMigracije> --project OneClickEcho.Persistence --startup-project OneClickEcho.Api
dotnet ef database update --project OneClickEcho.Persistence --startup-project OneClickEcho.Api
```

HTTPS dev sertifikat (iz README):

```bash
mkdir certificates && cd certificates
dotnet dev-certs https -ep ./certificate.crt --trust --format PEM
```

### 4.2 Dashboard (Next.js)

```bash
cd OneClickEcho.Dashboard
npm install
npm run dev
```

Produkcijski build lokalno:

```bash
npm run build
npm run start
# start skripta već koristi port 3800 i 0.0.0.0 (vidi package.json)
```

Promenljive okruženja za build često uključuju **`NEXT_PUBLIC_API_URL`** (javni URL API-ja koji browser koristi).

---

## 5. Docker

Radni direktorijum: **koren repozitorijuma** (gde je `docker-compose.yml`).

### 5.1 Profil `full`

Servisi **`api`**, **`dashboard`** i **`log-rotator`** su pod profilom **`full`**. Bez profila se ne podižu automatski zajedno sa ostalim servisima koji nemaju profil.

Tipično na serveru (Postgres **već** radi kao `oneclick_postgres`):

```bash
docker compose --profile full up -d api dashboard log-rotator
```

Ako želite i Compose-ov Postgres (samo ako **nema** konflikta imena):

```bash
docker compose --profile full up -d
```

### 5.2 Build slika

```bash
docker compose build api dashboard
docker compose --profile full up -d api dashboard log-rotator
```

Kontekst builda je koren repoa (bitno za zajedničke fajlove / analitiku u dashboardu).

### 5.3 Važne promenljive (`.env` + `environment` u compose)

- `OPENIDDICT_*`, `PUBLIC_UPLOADS_BASE_URL`, `Messaging__CampaignLeadViberMessageId__Floor` / `VIBER_CAMPAIGN_MESSAGE_ID_FLOOR`
- Za dashboard build args: `NEXT_PUBLIC_API_URL`, `API_INTERNAL_URL` (unutrašnji URL API-ja iz kontejnera, npr. `http://api:3901`)

### 5.4 Portovi (podrazumevano u compose/Dockerfile)

| Servis   | Port |
|----------|------|
| API      | **3901** |
| Dashboard | **3800** |
| Postgres | **17**, port **5432** (u compose primeru mapiran na **127.0.0.1** — ne na `0.0.0.0`) |
| Redis    | **6379** |
| Seq      | **5341**, UI **8081** |

### 5.5 Čišćenje prostora

```bash
docker system df
docker builder prune -af   # keš buildova — veliki dobitak na disk
df -h /
```

---

## 6. nginx (produkcija)

- Za javni sajt dashboarda često postoji `server` blok za domen sa **`proxy_pass http://127.0.0.1:3800;`** (Next mora da sluša na hostu).
- **502 Bad Gateway** + `connect() failed (111: Connection refused)` u `error.log` znači da **nema procesa** na tom portu (npr. dashboard kontejner nije podignut posle reboot-a).
- API može biti na istom serveru (`proxy_pass` na `127.0.0.1:3901`) ili na drugom hostu — uskladiti sa `NEXT_PUBLIC_API_URL` u buildu fronta.

Posle izmene konfiguracije:

```bash
sudo nginx -t && sudo systemctl reload nginx
```

---

## 7. Podizanje / ažuriranje na serveru (checklista)

1. **Git:** `git pull` na grani koja se deployuje.
2. **.env** na serveru ažuriran (bez commitovanja u git).
3. **Build:** `docker compose build api dashboard` (ili samo onaj servis koji se menja).
4. **Up:** `docker compose --profile full up -d api dashboard log-rotator` (+ `docker start oneclick_postgres` ako je baza van compose-a i stoji Exited).
5. **Provera:** `curl -I http://127.0.0.1:3800/`, `curl -I http://127.0.0.1:3901/` (ili health endpoint ako postoji), `docker compose --profile full ps`.
6. **Šema baze:** provera iz odeljka **2.8** (`__EFMigrationsHistory` + SQL za ključne kolone).
7. **Disk:** `df -h /`, po potrebi `docker builder prune -af`.
8. **nginx:** ako su portovi isti, samo reload nije potreban; ako se menjao upstream, `nginx -t` i reload.

---

## 8. Rešavanje uobičajenih problema

| Simptom | Mogući uzrok | Korak |
|---------|----------------|-------|
| 502 na domen | Ništa na `127.0.0.1:3800` | `docker compose ... up -d dashboard`, `docker ps` |
| Conflict `oneclick_postgres` | Isti `container_name` već postoji | `up -d` samo za `api dashboard log-rotator` ili ukloniti duplikat kontejnera uz oprez za volume |
| API 500 na listi kampanja sa filterom | Stari filter sa zagradama / deploy | Noviji API sa ispravnim `CampaignTenantFilter` |
| API 500 `column ... does not exist` (42703) | Migracije nisu primenjene; samo red u `__EFMigrationsHistory` | Odeljak **2.8** — `MigrateAsync` / `Up()` iz migracije, ne samo INSERT u history |
| Overview `/api/Admin/Analytics` 500 | Npr. nedostaje `campaigns.viber_content_kind` | Migracija `20260402140000_AddCampaignViberContentKindSurvey` |
| Disk pun | Docker build cache / stare slike | `docker builder prune -af`, zatim `docker image prune` po potrebi |
| CPU alarm | Kratak šilj ili konstantan opterećenje | `docker stats`, `top`; prag alarma ili više vCPU |

---

## 9. Gde tražiti dalje u kodu

- **Rute API:** `OneClickEcho.Api/Controllers/`
- **Viber slanje / delivery:** `OneClickEcho.Infrastructure/Services/MessageHandling/Viber/`
- **SMS:** `OneClickEcho.Infrastructure/Services/MessageHandling/Sms/`
- **Quartz raspored:** `OneClickEcho.Infrastructure/Services/Scheduling/`
- **Tenant filter kampanja:** `OneClickEcho.Api/Infrastructure/Utils/CampaignTenantFilter.cs`
- **Dashboard stranice:** `OneClickEcho.Dashboard/src/app/`
- **i18n poruke:** `OneClickEcho.Dashboard/messages/*.json`
- **Zabeleške iz Cursor sesija (šta je menjano van kratkog git opisa):** [WORKSPACE-SESSION-CHANGELOG.md](WORKSPACE-SESSION-CHANGELOG.md)

---

*Dokument odražava stanje repozitorijuma u vreme pisanja; proverite `docker-compose.yml` i `README.md` za eventualne izmene posle commit-a.*
