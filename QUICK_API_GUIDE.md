# Quick API Guide - REST Server

**Status**: API is ready to run!

## Quick Start

### 1. Start the API Server

```bash
cd DataAbstractionAPI.API
dotnet run
```

The API will start on: `http://localhost:5012` (HTTP) or `https://localhost:7128` (HTTPS)

### 2. Access Swagger UI

Open your browser and visit:
- http://localhost:5012/swagger

This gives you an interactive API explorer!

## API Endpoints

### GET /api/data
List all available collections

**Example:**
```bash
curl http://localhost:5012/api/data
```

### GET /api/data/{collection}
List all records in a collection

**Example:**
```bash
curl http://localhost:5012/api/data/users
```

### GET /api/data/{collection}/{id}
Get a specific record by ID

**Example:**
```bash
curl http://localhost:5012/api/data/users/{id-from-first-request}
```

### GET /api/data/{collection}/schema
Get the schema (field definitions) for a collection

**Example:**
```bash
curl http://localhost:5012/api/data/users/schema
```

### POST /api/data/{collection}
Create a new record

**Example:**
```bash
curl -X POST http://localhost:5012/api/data/users \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"John Doe\",\"email\":\"john@example.com\",\"age\":\"30\"}"
```

### PUT /api/data/{collection}/{id}
Update an existing record

**Example:**
```bash
curl -X PUT http://localhost:5012/api/data/users/{id} \
  -H "Content-Type: application/json" \
  -d "{\"age\":\"31\"}"
```

### DELETE /api/data/{collection}/{id}
Delete a record

**Example:**
```bash
curl -X DELETE http://localhost:5012/api/data/users/{id}
```

## Test Data

The API uses the existing test data:
- `testdata/users.csv` - Sample user data with 3 records

## Try It Out!

1. **Start the server:**
   ```bash
   cd DataAbstractionAPI.API
   dotnet run
   ```

2. **In another terminal, test the endpoints:**
   ```bash
   # List all available collections
   curl http://localhost:5012/api/data
   
   # Get all users
   curl http://localhost:5012/api/data/users
   
   # Get the first user by ID (get an ID from the first response)
   curl http://localhost:5012/api/data/users/{first-id}
   
   # Get the schema
   curl http://localhost:5012/api/data/users/schema
   ```

3. **Or use Swagger UI:**
   - Open http://localhost:5012/swagger
   - Click "Try it out" on any endpoint
   - Enter parameters and click "Execute"

## Features Available

✅ Full CRUD operations (Create, Read, Update, Delete)  
✅ Get collection schema  
✅ List all collections  
✅ Swagger documentation  
✅ Works with existing testdata/users.csv  

## Next Steps

This is a minimal API. To add more features, implement:
- Authentication (API keys)
- Request validation
- Advanced filtering (query parameters)
- Pagination with offset/limit
- DTOs for request/response
- Error handling middleware

See `IMPLEMENTATION_PLAN.md` Phase 3 for the full implementation plan.

