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
| Postgres | **5432** (mapiran na 127.0.0.1 u compose primeru) |
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
6. **Disk:** `df -h /`, po potrebi `docker builder prune -af`.
7. **nginx:** ako su portovi isti, samo reload nije potreban; ako se menjao upstream, `nginx -t` i reload.

---

## 8. Rešavanje uobičajenih problema

| Simptom | Mogući uzrok | Korak |
|---------|----------------|-------|
| 502 na domen | Ništa na `127.0.0.1:3800` | `docker compose ... up -d dashboard`, `docker ps` |
| Conflict `oneclick_postgres` | Isti `container_name` već postoji | `up -d` samo za `api dashboard log-rotator` ili ukloniti duplikat kontejnera uz oprez za volume |
| API 500 na listi kampanja sa filterom | Stari filter sa zagradama / deploy | Noviji API sa ispravnim `CampaignTenantFilter` |
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

---

*Dokument odražava stanje repozitorijuma u vreme pisanja; proverite `docker-compose.yml` i `README.md` za eventualne izmene posle commit-a.*
