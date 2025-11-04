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

**Query Parameters:**
- `limit` (optional): Maximum number of records to return (default: 100)

**Example:**
```bash
curl http://localhost:5012/api/data/users
curl http://localhost:5012/api/data/users?limit=10
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

### POST /api/data/upload
Upload a CSV file to create or replace a collection

**Content-Type**: `multipart/form-data`

**Form Fields:**
- `collection` (string): The name of the collection (CSV file name without extension)
- `file` (file): The CSV file to upload (.csv files only)

**Example using Swagger UI:**
1. Navigate to `http://localhost:5012/swagger`
2. Find `POST /api/data/upload`
3. Click "Try it out"
4. Enter collection name (e.g., "products")
5. Click "Choose File" and select your CSV file
6. Click "Execute"

**Example using PowerShell:**
```powershell
$formData = @{
    collection = "products"
    file = Get-Item "C:\path\to\products.csv"
}

Invoke-RestMethod -Uri http://localhost:5012/api/data/upload -Method POST -Form $formData
```

**Example using curl:**
```bash
curl -X POST http://localhost:5012/api/data/upload \
  -F "collection=products" \
  -F "file=@/path/to/products.csv"
```

**Response:**
```json
{
  "message": "CSV file uploaded successfully as collection 'products'",
  "collection": "products",
  "filePath": "C:\\path\\to\\testdata\\products.csv"
}
```

**Note**: 
- The uploaded CSV file will be saved to the `testdata` directory
- If a collection with the same name already exists, it will be replaced
- Only `.csv` files are accepted
- Collection names must be valid (no path traversal characters like `..` or `/`)

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
✅ **CSV file upload** - Upload CSV files to create or replace collections  
✅ Pagination (limit parameter)  
✅ Swagger documentation  
✅ Works with existing testdata/users.csv  
✅ File upload via Swagger UI with file picker  

**Note**: The underlying adapter supports filtering, sorting, field selection, and offset pagination, but these are not yet exposed via REST API query parameters. They can be added in future updates.  

## Next Steps

The API is currently a basic implementation. To add more features:
- **Query Parameters**: Expose filtering, sorting, field selection, and offset pagination from the adapter
- **Authentication**: Add API key authentication
- **Request Validation**: Add validation middleware
- **DTOs**: Create DTOs for request/response (currently using models directly)
- **Error Handling**: Add global error handling middleware
- **Bulk Operations**: Add bulk create/update/delete endpoints
- **Aggregations**: Add summary and aggregation endpoints

See `IMPLEMENTATION_PLAN.md` and `data-abstraction-api.md` for the full specification.

