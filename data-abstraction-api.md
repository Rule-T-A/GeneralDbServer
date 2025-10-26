# Data Abstraction REST API Specification

## Overview

A minimal REST API that provides a unified interface for interacting with data across different storage backends (CSV, SQL, NoSQL, etc.) without exposing underlying implementation details. Designed for token-efficient consumption by LLMs and other automated clients.

## Design Principles

- **Storage Agnostic**: Clients never need to know the underlying storage mechanism
- **Token Optimized**: Field projection, compact responses, and smart defaults minimize data transfer
- **Minimal Surface Area**: Small set of intuitive endpoints
- **Schema Flexibility**: Support for both data and structure modifications
- **Intelligent Defaults**: Automatic generation of sensible default values
- **Extensible**: Designed to accommodate future features (authentication, caching, etc.) without breaking changes

---

## Base URL

```
https://api.example.com/api
```

---

## API Endpoints

### Data Operations

Operations for querying and modifying records (rows).

#### 1. List Available Collections

```
GET /api/data
```

**Response:**
```json
["users", "orders", "products"]
```

Returns an array of collection names (without `.csv` extension) available in the data store.

**Example:**
```
GET /api/data
```

---

#### 2. List/Query Records

```
GET /data/{collection}
```

**Query Parameters:**
- `fields` (string): Comma-separated list of fields to return (e.g., `id,name,email`)
- `filter` (JSON string): Filter criteria (e.g., `{"status":"active"}`)
- `limit` (integer): Maximum records to return (default: 10, max: 100)
- `offset` (integer): Number of records to skip
- `cursor` (string): Pagination cursor (alternative to offset)
- `sort` (string): Sort order (e.g., `name:asc` or `created:desc`)
- `sample` (integer): Return N random records (useful for schema discovery)

**Response:**
```json
{
  "d": [
    {"id": 1, "name": "Alice"},
    {"id": 2, "name": "Bob"}
  ],
  "t": 150,
  "more": true,
  "cursor": "abc123"
}
```

**Response Fields:**
- `d`: Data array (compact key)
- `t`: Total count of matching records
- `more`: Boolean indicating more records exist
- `cursor`: Next page cursor (if applicable)

**Examples:**
```
GET /data/users?fields=id,name&limit=5
GET /data/orders?filter={"status":"pending"}&sort=created:desc
GET /data/products?sample=3
```

---

#### 2. Complex Query (Alternative to GET for complex filters)

```
POST /data/{collection}/query
```

Use this endpoint when filters are too complex for URL encoding or when you need to avoid URL length limitations.

**Request Body:**
```json
{
  "fields": ["id", "name", "email"],
  "filter": {
    "and": [
      {"status": "active"},
      {
        "or": [
          {"field": "age", "operator": "gte", "value": 18},
          {"role": "admin"}
        ]
      }
    ]
  },
  "limit": 10,
  "offset": 0,
  "sort": "name:asc"
}
```

**Response:** Same as GET query (ListResponse)

---

#### 3. Get Single Record

```
GET /data/{collection}/{id}
```

**Query Parameters:**
- `fields` (string): Comma-separated list of fields to return

**Response:**
```json
{
  "d": {"id": 1, "name": "Alice", "email": "alice@example.com"}
}
```

**Example:**
```
GET /data/users/123?fields=name,email,created
```

---

#### 4. Create Record

```
POST /data/{collection}
```

**Request Body:**
```json
{
  "name": "Charlie",
  "email": "charlie@example.com",
  "status": "active"
}
```

**Response:**
```json
{
  "d": {"id": 3, "name": "Charlie", "email": "charlie@example.com"},
  "id": 3
}
```

---

#### 6. Update Record

```
PATCH /data/{collection}/{id}
```

**Query Parameters:**
- `return` (string): Response format - `minimal` (default, only updated fields) or `full` (complete record)

**Request Body:**
```json
{
  "status": "inactive",
  "updated_at": "2025-10-26T10:30:00Z"
}
```

**Response (default/minimal):**
```json
{
  "d": {"status": "inactive", "updated_at": "2025-10-26T10:30:00Z"},
  "success": true
}
```

**Response (with `?return=full`):**
```json
{
  "d": {"id": 1, "name": "Alice", "status": "inactive", "email": "alice@example.com", "updated_at": "2025-10-26T10:30:00Z"},
  "success": true
}
```

---

#### 7. Delete Record

```
DELETE /data/{collection}/{id}
```

**Response:**
```json
{
  "success": true,
  "id": 1
}
```

---

#### 8. Bulk Operations

```
POST /data/{collection}/bulk
```

**Request Body:**
```json
{
  "action": "create",
  "atomic": false,
  "records": [
    {"name": "Alice", "email": "alice@example.com"},
    {"name": "Bob", "email": "bob@example.com"}
  ]
}
```

**Request Fields:**
- `action` (string): One of `create`, `update`, `delete`
- `atomic` (boolean): If `true`, all operations succeed or all fail (default: `false`)
- `records` (array): Array of records to process

**Response (Best-Effort Mode, atomic=false):**
```json
{
  "success": true,
  "succeeded": 98,
  "failed": 2,
  "results": [
    {"index": 0, "id": 1, "success": true},
    {"index": 1, "success": false, "error": "Validation failed: email required"},
    {"index": 2, "id": 2, "success": true}
  ]
}
```

**Response (Atomic Mode, atomic=true, all succeed):**
```json
{
  "success": true,
  "created": 100,
  "ids": [1, 2, 3, ...]
}
```

**Response (Atomic Mode, atomic=true, one fails):**
```json
{
  "success": false,
  "error": "Transaction rolled back: validation error on record 42",
  "failed_index": 42,
  "failed_error": "Email already exists"
}
```

---

#### 9. Summary/Aggregation

```
GET /data/{collection}/summary
```

**Query Parameters:**
- `field` (string): Field to aggregate on

**Response:**
```json
{
  "active": 45,
  "inactive": 12,
  "pending": 8
}
```

**Example:**
```
GET /data/users/summary?field=status
```

---

#### 10. Complex Aggregation

```
POST /data/{collection}/aggregate
```

For complex aggregations requiring grouping by multiple fields or multiple aggregate functions.

**Request Body:**
```json
{
  "group_by": ["category", "status"],
  "aggregates": [
    {"field": "price", "function": "avg", "alias": "avg_price"},
    {"field": "quantity", "function": "sum", "alias": "total_qty"},
    {"field": "id", "function": "count", "alias": "count"}
  ],
  "filter": {"status": "active"}
}
```

**Supported Functions:** `count`, `sum`, `avg`, `min`, `max`

**Response:**
```json
{
  "d": [
    {
      "category": "Electronics",
      "status": "active",
      "avg_price": 299.99,
      "total_qty": 150,
      "count": 25
    },
    {
      "category": "Books",
      "status": "active",
      "avg_price": 19.99,
      "total_qty": 500,
      "count": 100
    }
  ]
}
```

---

### Schema Operations

Operations for modifying data structure (columns, tables).

#### 1. List Collections

```
GET /schema
```

**Alternative:** Collections can also be listed via the Data Operations endpoint:
```
GET /api/data
```

**Response (via /schema):**
```json
{
  "collections": ["users", "orders", "products"]
}
```

**Response (via /api/data):**
```json
["users", "orders", "products"]
```

---

#### 2. Get Collection Schema

```
GET /schema/{collection}
```

**Response:**
```json
{
  "name": "users",
  "fields": [
    {
      "name": "id",
      "type": "integer",
      "nullable": false,
      "default": null
    },
    {
      "name": "email",
      "type": "string",
      "nullable": true,
      "default": ""
    },
    {
      "name": "status",
      "type": "string",
      "nullable": false,
      "default": "active"
    }
  ]
}
```

**Field Types:**
- `string`, `integer`, `float`, `boolean`, `date`, `datetime`, `array`, `object`

---

#### 3. Create Collection

```
POST /schema
```

**Request Body:**
```json
{
  "name": "products",
  "fields": [
    {"name": "id", "type": "integer"},
    {"name": "name", "type": "string"},
    {"name": "price", "type": "float"}
  ]
}
```

**Response:**
```json
{
  "success": true,
  "collection": "products",
  "fields": 3
}
```

---

#### 4. Rename Collection

```
PATCH /schema/{collection}
```

**Request Body:**
```json
{
  "name": "customers"
}
```

**Response:**
```json
{
  "success": true,
  "old_name": "users",
  "new_name": "customers"
}
```

---

#### 5. Delete Collection

```
DELETE /schema/{collection}
```

**Query Parameters:**
- `dry_run` (boolean): Preview operation without executing (default: false)

**Response:**
```json
{
  "success": true,
  "collection": "users",
  "records_deleted": 150
}
```

**Dry Run Response:**
```json
{
  "dry_run": true,
  "records_affected": 150,
  "warnings": ["All data will be permanently deleted"]
}
```

---

#### 6. Add Column

```
POST /schema/{collection}/fields
```

**Request Body:**
```json
{
  "name": "phone",
  "type": "string",
  "nullable": true,
  "default": "000-000-0000"
}
```

**Request Body (with intelligent defaults):**
```json
{
  "name": "is_verified",
  "type": "boolean"
}
```

**Response:**
```json
{
  "success": true,
  "field": "phone",
  "type": "string",
  "default": "000-000-0000",
  "default_generated": false,
  "applied_to_records": 150
}
```

**Response (with intelligent defaults):**
```json
{
  "success": true,
  "field": "is_verified",
  "type": "boolean",
  "default": false,
  "default_generated": true,
  "default_strategy": "pattern_match",
  "applied_to_records": 150
}
```

---

#### 7. Modify Column

```
PATCH /schema/{collection}/fields/{field_name}
```

Allows renaming, type changes, nullability changes, and default value modifications.

**Request Body (rename only):**
```json
{
  "name": "email_address"
}
```

**Request Body (change type):**
```json
{
  "type": "integer",
  "conversion_strategy": "cast"
}
```

**Request Body (multiple changes):**
```json
{
  "name": "user_email",
  "type": "string",
  "nullable": false,
  "default": "user@example.com"
}
```

**Conversion Strategies:**
- `cast`: Attempt automatic type conversion (default)
- `truncate`: Truncate values that don't fit (for string length changes)
- `fail_on_error`: Fail entire operation if any conversion fails
- `set_null`: Set unconvertible values to null (requires nullable=true)

**Response:**
```json
{
  "success": true,
  "field": "user_email",
  "old_name": "email",
  "new_name": "user_email",
  "old_type": "string",
  "new_type": "string",
  "conversion_errors": 0,
  "records_affected": 150
}
```

**Response (with conversion errors in best-effort mode):**
```json
{
  "success": true,
  "field": "age",
  "old_type": "string",
  "new_type": "integer",
  "conversion_errors": 3,
  "errors": [
    {"record_id": 5, "value": "unknown", "error": "Cannot cast to integer"},
    {"record_id": 12, "value": "N/A", "error": "Cannot cast to integer"}
  ],
  "records_affected": 147
}
```

---

#### 8. Delete Column

```
DELETE /schema/{collection}/fields/{field_name}
```

**Query Parameters:**
- `dry_run` (boolean): Preview operation without executing

**Response:**
```json
{
  "success": true,
  "field": "phone",
  "records_affected": 150
}
```

---

## Intelligent Default Generation

When adding a new column without specifying a `default` value, the system generates intelligent defaults using a multi-strategy approach:

### Strategy Hierarchy

1. **User-Specified**: Use the explicitly provided default value
2. **Pattern Matching**: Detect common naming patterns
3. **Context Analysis**: Analyze existing data in the collection
4. **Type-Based**: Fall back to safe type defaults

### Pattern-Based Defaults

| Pattern | Type | Default | Example |
|---------|------|---------|---------|
| `*_id` | integer | `null` | `user_id` |
| `id` | integer | `auto_increment` | `id` |
| `is_*` | boolean | `false` | `is_verified` |
| `has_*` | boolean | `false` | `has_premium` |
| `*_at` | datetime | `current_timestamp` | `created_at` |
| `created*` | datetime | `current_timestamp` | `created_date` |
| `updated*` | datetime | `current_timestamp` | `updated_at` |
| `email` | string | `user@example.com` | `email` |
| `phone` | string | `000-000-0000` | `phone` |
| `status` | string | `active` | `status` |
| `enabled` | boolean | `true` | `enabled` |
| `active` | boolean | `true` | `active` |
| `count` | integer | `0` | `view_count` |
| `price` | float | `0.0` | `price` |
| `url` | string | `https://` | `website_url` |

### Type-Based Defaults

| Type | Default |
|------|---------|
| `string` | `""` |
| `integer` | `0` |
| `float` | `0.0` |
| `boolean` | `false` |
| `date` | `current_date` |
| `datetime` | `current_timestamp` |
| `array` | `[]` |
| `object` | `{}` |

### Context-Aware Defaults

When pattern matching fails, the system analyzes existing records:

```
Example: Adding "department" field to "employees" collection
→ System finds 70% of records have related data suggesting "Sales"
→ Default generated: "Sales"
→ Strategy: "context_aware"
```

### Special Default Types

**Computed Defaults:**
```json
{
  "name": "full_name",
  "type": "string",
  "computed": "concat(first_name, ' ', last_name)"
}
```

**Enum Defaults:**
```json
{
  "name": "size",
  "type": "string",
  "allowed_values": ["S", "M", "L", "XL"],
  "default": "M"
}
```

---

## Filter Syntax

### Simple Filters

```json
{"status": "active"}
{"age": 25}
```

### Operator-Based Filters

```json
{
  "field": "age",
  "operator": "gte",
  "value": 18
}
```

**Supported Operators:**
- `eq` (equal), `ne` (not equal)
- `gt` (greater than), `gte` (greater than or equal)
- `lt` (less than), `lte` (less than or equal)
- `in` (in array), `nin` (not in array)
- `contains` (substring match)
- `startswith`, `endswith`

### Compound Filters

```json
{
  "and": [
    {"status": "active"},
    {"field": "age", "operator": "gte", "value": 18}
  ]
}
```

```json
{
  "or": [
    {"status": "active"},
    {"status": "pending"}
  ]
}
```

---

## Response Formats

### Success Response

```json
{
  "d": {...},
  "success": true
}
```

### Error Response

```json
{
  "error": {
    "code": "INVALID_FIELD",
    "message": "Field 'email' does not exist in collection 'users'",
    "details": {...}
  }
}
```

**Common Error Codes:**
- `INVALID_FIELD`: Requested field doesn't exist
- `INVALID_FILTER`: Malformed filter syntax
- `COLLECTION_NOT_FOUND`: Collection doesn't exist
- `RECORD_NOT_FOUND`: Record with specified ID not found
- `VALIDATION_ERROR`: Data validation failed
- `SCHEMA_CONFLICT`: Schema operation conflicts with existing data
- `CONVERSION_ERROR`: Type conversion failed
- `ATOMIC_TRANSACTION_FAILED`: Atomic bulk operation failed and was rolled back

---

## Security and Extension Points

This specification focuses on the data contract and API functionality. The following aspects are intentionally left as extension points for implementation:

### Authentication & Authorization (Extension Point)

The API is designed to work with or without authentication. When implementing authentication:

- Add an optional `Authorization` header (e.g., `Bearer <token>` or `API-Key <key>`)
- Implement collection-level permissions (read/write/admin)
- Implement record-level access control as needed
- All endpoints remain the same; authentication is handled at the middleware/gateway layer

**Example with authentication:**
```
GET /data/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Other Extension Points

The API is designed to accommodate future features without breaking changes:

- **Rate Limiting**: Add `X-RateLimit-*` headers to responses
- **Caching**: Add `ETag` and `Cache-Control` headers
- **Versioning**: Use `Accept: application/vnd.api.v2+json` headers or URL versioning
- **Webhooks**: Add webhook subscription endpoints for data change notifications
- **Audit Logging**: Track all data and schema modifications
- **Multi-tenancy**: Add tenant identifier to all operations

---

## Token Optimization for LLMs

### Best Practices

1. **Always use field projection**
   ```
   ✅ GET /data/users?fields=id,name
   ❌ GET /data/users (returns all 50 columns)
   ```

2. **Set reasonable limits**
   ```
   GET /data/users?limit=5
   ```

3. **Use summary endpoints for counts**
   ```
   GET /data/users/summary?field=status
   ```

4. **Sample for schema discovery**
   ```
   GET /data/users?sample=1
   ```

5. **Cache schema information**
   ```
   Call GET /schema/users once, store field names
   ```

6. **Use minimal return mode for updates**
   ```
   PATCH /data/users/1 (returns only updated fields)
   ```

### Typical LLM Workflow

```
1. Discover collections:
   GET /schema → ["users", "orders"]

2. Get schema (cache this):
   GET /schema/users → field names and types

3. Query efficiently:
   GET /data/users?fields=id,name&limit=5

4. Get details on demand:
   GET /data/users/123?fields=name,email

5. Aggregate data:
   GET /data/users/summary?field=status

6. Update records:
   PATCH /data/users/123 {"status": "inactive"}
   (returns only {"d": {"status": "inactive"}, "success": true})
```

---

## Architecture

```
Client (LLM/Application)
    ↓
REST API Endpoints
    ↓
[Optional: Auth/Rate Limit Middleware]
    ↓
Adapter Interface (storage-agnostic)
    ↓
┌─────────┬─────────┬─────────┐
│   CSV   │   SQL   │ MongoDB │  ... (storage backends)
└─────────┴─────────┴─────────┘
```

### Adapter Interface

Each storage backend implements:

```
interface DataAdapter {
  // Data operations
  list(collection, filters, pagination)
  get(collection, id)
  create(collection, data)
  update(collection, id, data)
  delete(collection, id)
  bulkOperation(collection, action, records, atomic)
  aggregate(collection, groupBy, aggregates, filter)
  
  // Schema operations
  getSchema(collection)
  addField(collection, name, type, options)
  modifyField(collection, name, changes, conversionStrategy)
  deleteField(collection, name)
  createCollection(name, fields)
  renameCollection(oldName, newName)
  deleteCollection(collection)
  
  // Utility
  generateDefault(fieldName, fieldType, context)
  convertType(value, fromType, toType, strategy)
}
```

---

## Configuration Example

**System Prompt for LLM:**

```
You have access to a data API at https://api.example.com/api

Available collections: users, orders, products

For each collection:
1. Call GET /schema/{collection} to discover fields (cache this)
2. Use ?fields=field1,field2 to minimize token usage
3. Use ?limit=5 for exploration, increase as needed
4. Use summary endpoints for counts and aggregations
5. PATCH returns only updated fields by default

Always request only the fields you need.
```

---

## Version

API Version: 1.0
Last Updated: October 26, 2025

---

## OpenAPI/Swagger Specification

Below is the complete OpenAPI 3.0 specification in YAML format:

```yaml
openapi: 3.0.3
info:
  title: Data Abstraction REST API
  description: A minimal REST API that provides a unified interface for interacting with data across different storage backends (CSV, SQL, NoSQL, etc.)
  version: 1.0.0
  contact:
    name: API Support
    url: https://api.example.com/support

servers:
  - url: https://api.example.com/api
    description: Production server
  - url: http://localhost:8080/api
    description: Development server

tags:
  - name: Data Operations
    description: CRUD operations on records (rows)
  - name: Schema Operations
    description: Operations for modifying data structure (columns, tables)

paths:
  /data/{collection}:
    get:
      tags:
        - Data Operations
      summary: List/Query Records
      description: Retrieve a list of records from a collection with optional filtering, pagination, and field projection
      operationId: listRecords
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
          example: users
        - name: fields
          in: query
          schema:
            type: string
          description: Comma-separated list of fields to return
          example: id,name,email
        - name: filter
          in: query
          schema:
            type: string
          description: JSON filter criteria
          example: '{"status":"active"}'
        - name: limit
          in: query
          schema:
            type: integer
            default: 10
            maximum: 100
          description: Maximum number of records to return
        - name: offset
          in: query
          schema:
            type: integer
            default: 0
          description: Number of records to skip
        - name: cursor
          in: query
          schema:
            type: string
          description: Pagination cursor
        - name: sort
          in: query
          schema:
            type: string
          description: Sort order
          example: name:asc
        - name: sample
          in: query
          schema:
            type: integer
          description: Return N random records
      responses:
        '200':
          description: Successful response
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ListResponse'
        '400':
          description: Bad request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'
    post:
      tags:
        - Data Operations
      summary: Create Record
      description: Create a new record in the collection
      operationId: createRecord
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              additionalProperties: true
            example:
              name: Charlie
              email: charlie@example.com
              status: active
      responses:
        '201':
          description: Record created successfully
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/CreateResponse'
        '400':
          description: Bad request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

  /data/{collection}/query:
    post:
      tags:
        - Data Operations
      summary: Complex Query
      description: Query records with complex filters (alternative to GET for complex queries)
      operationId: complexQuery
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ComplexQueryRequest'
      responses:
        '200':
          description: Successful response
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ListResponse'
        '400':
          description: Bad request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

  /data/{collection}/{id}:
    get:
      tags:
        - Data Operations
      summary: Get Single Record
      description: Retrieve a specific record by ID
      operationId: getRecord
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
        - name: id
          in: path
          required: true
          schema:
            type: string
          description: Record ID
        - name: fields
          in: query
          schema:
            type: string
          description: Comma-separated list of fields to return
      responses:
        '200':
          description: Successful response
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/GetResponse'
        '404':
          description: Record not found
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'
    patch:
      tags:
        - Data Operations
      summary: Update Record
      description: Update specific fields of a record
      operationId: updateRecord
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
        - name: id
          in: path
          required: true
          schema:
            type: string
          description: Record ID
        - name: return
          in: query
          schema:
            type: string
            enum: [minimal, full]
            default: minimal
          description: Response format - minimal (only updated fields) or full (complete record)
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              additionalProperties: true
            example:
              status: inactive
              updated_at: '2025-10-26T10:30:00Z'
      responses:
        '200':
          description: Record updated successfully
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/UpdateResponse'
        '404':
          description: Record not found
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'
    delete:
      tags:
        - Data Operations
      summary: Delete Record
      description: Delete a specific record by ID
      operationId: deleteRecord
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
        - name: id
          in: path
          required: true
          schema:
            type: string
          description: Record ID
      responses:
        '200':
          description: Record deleted successfully
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/DeleteResponse'
        '404':
          description: Record not found
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

  /data/{collection}/bulk:
    post:
      tags:
        - Data Operations
      summary: Bulk Operations
      description: Perform bulk create, update, or delete operations
      operationId: bulkOperation
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/BulkRequest'
      responses:
        '200':
          description: Bulk operation completed
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/BulkResponse'
        '400':
          description: Bad request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

  /data/{collection}/summary:
    get:
      tags:
        - Data Operations
      summary: Summary/Aggregation
      description: Get aggregated summary of a field
      operationId: getSummary
      parameters:
        - name: collection
          in: path
          required: true
          schema:
            type: string
          description: Name of the collection
        - name: field
          in: query
          required: true
          schema:
            type: string
          description: Field to aggregate on
      responses:
        '200':
          description: Summary data
          content:
            application/json:
              schema:
                type: object
                additionalProperties:
                  type: integer
              example:
                active: 45
                inactive: 12
                pending: 8

  /data/{collection}/aggregate:
    post:
      tags:
        - Data Operations
      summary: Complex Aggregation
      description: Perform complex aggregations with grouping and multiple aggregate functions
      operationId: complexAggregate
      parameters:
        - name: collection
          in: path