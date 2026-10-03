# MediCart Suggestions API Specification

## Endpoint: GET /Medicines/Suggestions

### Request Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `search`  | string | Yes    | Search term (minimum 2 characters recommended) |

### Response Payload Format
Returns an array of JSON objects matching the schema:
```json
[
  {
    "id": 12,
    "name": "Napa Extra",
    "genericName": "Paracetamol + Caffeine",
    "dosage": "500mg+65mg",
    "manufacturer": "Beximco Pharmaceuticals Ltd.",
    "productType": "Tablet",
    "price": 3.00,
    "inStock": true,
    "stockQuantity": 150,
    "requiresRx": false,
    "imageUrl": "/images/medicines/napa-extra.webp"
  }
]
```

### Error Handling & Edge Cases
- Empty or whitespace query returns empty array `[]` with status `200 OK`.
- Unmatched query returns empty array `[]` with status `200 OK`.
- Database failure returns standard ASP.NET Core 500 error handled gracefully on client.

