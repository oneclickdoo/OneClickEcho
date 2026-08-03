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
