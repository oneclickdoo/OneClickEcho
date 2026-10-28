# Send Message API

Sends a message through the API with support for SMS and Viber messaging.

## Endpoint

**Base URL**: `https://viber.oneclick.rs`  
**Path**: `/api/message/send`  
**Method**: POST  
**Content-Type**: application/json

## Request Body

| Field | Type | Required | Description                                                 |
|-------|------|----------|-------------------------------------------------------------|
| `companyId` | UUID | Yes | The unique identifier of the company                        |
| `apiPassword` | string | Yes | Authentication password for the API                         |
| `sender` | string | Yes | The sender's identifier/name                                |
| `apiMessageType` | number | Yes | Type of the message to be sent (1 for Viber, 2 for SMS)     |
| `phoneNumber` | string | Yes | Recipient's phone number (with country code, e.g., +381...) |
| `message` | string | Yes | The message content                                         |
| `hasSmsFallback` | boolean | No | If true, falls back to SMS when Viber delivery fails        |
| `viberMedia` | string | No | URL to media content for Viber message                      |
| `viberButtonUrl` | string | No | URL for the Viber message button                            |
| `viberButtonUrlTitle` | string | No | Text to display on the Viber button                         |
| `viberValidity` | number | No | Message validity in seconds (default often 86400)           |
| `viberImageFormat` | string | No | `gif` or `img`. GIF is also inferred from a `.gif` URL (types 107 and 108, max 24 MB) |
| `viberImageRatio` | string | No | `square` (1:1), `portrait` (9:16), or `landscape` (16:9) for image messages |
| `viberSurveyOptionsJson` | string | No | JSON array of 2–10 unique options, max 50 characters. Sends Comtrade type **801** |
| `viberSurveyUiType` | number | No | `1` radio buttons (default), `2` buttons |
| `viberCarouselJson` | string | No | JSON with `items` (2–5 cards). Sends Comtrade type **901** |
| `viberTemplateId` | UUID | No | Local id of an **approved** transactional template. Sends type **1702**. `message` is not required |
| `viberTemplateParamsJson` | string | No | JSON object of template variable values, e.g. `{"name":"Ana"}` |

### Example Request

```json
{
    "companyId": "b25cfadc-8a0f-4306-be83-821116ca6e2e",
    "apiPassword": "TestPassword",
    "sender": "OneClickTest",
    "apiMessageType": 1,
    "phoneNumber": "+381621671771",
    "message": "Test message.",
    "viberMedia": "https://example.com/image.jpg",
    "viberButtonUrl": "https://example.com",
    "viberButtonUrlTitle": "Test button"
}
```

### Example Response (immediate ack)

Sending is **asynchronous** (queued, then background job → Comtrade → delivery poll).  
On `Send` you get the message id and the **initial** status (usually `viberStatus: 0`).

```json
{
    "id": "687787a1-d8ee-4625-8a02-7f4d63197e56",
    "isSent": false,
    "viberStatus": 0,
    "viberStatusDescription": "Poruka još nije poslata ka provajderu.",
    "smsStatus": 0,
    "smsStatusDescription": null,
    "phoneNumber": "+381621671771",
    "createdAt": "2026-08-03T10:00:00"
}
```

---

## Get message status (poll)

**Path**: `/api/message/{id}/status`  
**Method**: GET  

| Query | Type | Required | Description |
|-------|------|----------|-------------|
| `companyId` | UUID | Yes | Same company as on Send |
| `apiPassword` | string | Yes | Same API password as on Send |

### Example

`GET /api/message/687787a1-d8ee-4625-8a02-7f4d63197e56/status?companyId=...&apiPassword=...`

### Example Response

```json
{
    "id": "687787a1-d8ee-4625-8a02-7f4d63197e56",
    "isSent": true,
    "viberStatus": 4,
    "viberStatusDescription": "Viđena od strane primaoca.",
    "smsStatus": 0,
    "smsStatusDescription": null,
    "phoneNumber": "+381621671771",
    "viberMessageId": 5000000123,
    "createdAt": "2026-08-03T10:00:00"
}
```

### Viber status codes (`viberStatus`)

| Code | Meaning |
|------|---------|
| 0 | Not sent / queued |
| 1 | Received by provider |
| 2 | Pending |
| 3 | Delivered to device |
| 4 | Seen |
| 5 | Undelivered |
| 6 | Expired |
| 7 | Clicked |

`viberStatusDescription` is a Serbian human-readable text (includes provider sub-reasons when available, e.g. blocked / not a Viber user).

Poll every ~15–60s until status is terminal for your use case (e.g. Seen, Clicked, Undelivered, Expired). Delivery updates come from Comtrade polling (~1 min).

### Comtrade `MessageType` (not `apiMessageType`)

`apiMessageType: 1` only means **Viber channel**. The Comtrade outbound type is chosen in code (`DetermineApiMessageType`):

| Your fields | Comtrade `MessageType` |
|-------------|------------------------|
| text only | **106** |
| text + button | **109** |
| text + image + button | **108** |
| image only | **107** |
| video variants | **230–233** |
| `viberSurveyOptionsJson` | **801** |
| `viberCarouselJson` | **901** |
| `viberTemplateId` (approved) | **1702** |

`message` is required unless `viberTemplateId` is set. GIF media is only valid for image types 107 and 108.

“Identical” for the same-day duplicate check also includes image format, ratio, survey JSON, survey UI, carousel JSON, template id, and template params.

---

## Transactional templates

Same auth as Send (`companyId` + `apiPassword`). Approval happens at Comtrade and can take up to 24 hours. Send with `viberTemplateId` only when `canSend` is true.

| Method | Path |
|--------|------|
| POST | `/api/viber/templates` |
| GET | `/api/viber/templates?companyId=&apiPassword=` |
| DELETE | `/api/viber/templates/{id}?companyId=&apiPassword=` |
| POST | `/api/viber/templates/{id}/refresh?companyId=&apiPassword=` |

Create body: `companyId`, `apiPassword`, `display` (Viber sender), `name`, `language` (must match Comtrade’s list, e.g. `sr`, `en`, `fr_FR`), `body` (max 1000, must start and end with static text, max 8 `{{Variables}}` of English letters), optional `tag`.

---

## Viber session

This is **not** `POST /api/Message/Send`. A session opens when a person replies to a campaign that has replies enabled. Window is 24 hours from the first reply (not extended). Limits: 60 outbound messages in the window, 10 in a row without a new reply.

| Method | Path |
|--------|------|
| POST | `/api/viber/session/send` |
| GET | `/api/viber/session?companyId=&apiPassword=&sender=&phoneNumber=` |

Send body: `companyId`, `apiPassword`, `sender`, `phoneNumber`, `kind` (`0` text / 306, `1` image or GIF / 307, `2` file / 221, `3` quick replies / 802), `text`, `media`, `quickRepliesJson` (JSON array, 2–10), `surveyUiType`, `allowOutsideSession`.

If the window is closed the API returns **HTTP 409** `ViberSession.Closed`. Sending again with `allowOutsideSession: true` delivers the message billed as promotional.

Example body with `viberMedia` (`.jpg`/`.png`) + `viberButtonUrl` + title → payload includes `"MessageType": 108`, `ImageUrl`, `ButtonUrl`, `ButtonCaption`.

Check Docker API logs after send:

```text
[API→Comtrade] MessageId=... ComtradeMessageType=108 (OneWayTextImageButton) ...
[ComTrade Send] id=... type=108 btn=y img=y
```


Identical messages to the **same phone** for the **same company** are rejected within the **same calendar day** (Europe/Belgrade).

“Identical” means same: `phoneNumber`, `apiMessageType`, `message`, `sender`, `viberMedia`, `viberButtonUrl`, `viberButtonUrlTitle`, image format, ratio, survey, carousel, and template fields.

**HTTP 409 Conflict** example:

```json
{
  "code": "ApiMessage.DuplicateSameDay",
  "message": "An identical message to this phone number was already accepted today for this company. ExistingId:5eceb554-9f64-48c8-ab45-eca56751677f"
}
```
