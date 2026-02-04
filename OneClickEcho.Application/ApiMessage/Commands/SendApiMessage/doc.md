# Send Message API

Sends a message through the API with support for SMS and Viber messaging.

## Endpoint

**Base URL**: `https://echo.oneclick.rs`  
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