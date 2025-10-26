# How to Use the REST API

## Starting the Server

### Option 1: Use the PowerShell Script
```powershell
.\START_API.ps1
```

### Option 2: Manual Start
```powershell
cd DataAbstractionAPI.API
dotnet run
```

The server will start on: **http://localhost:5012** (HTTP) or **https://localhost:7128** (HTTPS)

---

## Using the API

Once the server is running, you have **5 ways** to use it:

### 1. **Swagger UI** (Easiest! ✨)
Open your browser to:
```
http://localhost:5012/swagger
```
- Click "Try it out" on any endpoint
- Enter parameters
- Click "Execute"
- See the response right there!

### 2. **Browser** (For GET requests)
Just open:
```
http://localhost:5012/api/data
http://localhost:5012/api/data/users
http://localhost:5012/api/data/users/schema
```

### 3. **PowerShell**
```powershell
# List all available collections
Invoke-RestMethod -Uri http://localhost:5012/api/data

# Get all users
Invoke-RestMethod -Uri http://localhost:5012/api/data/users

# Get a specific user (replace {id} with actual ID from first response)
Invoke-RestMethod -Uri http://localhost:5012/api/data/users/{id}

# Get schema
Invoke-RestMethod -Uri http://localhost:5012/api/data/users/schema
```

### 4. **curl** (Command Line)
```bash
# List all available collections
curl http://localhost:5012/api/data

# Get all users
curl http://localhost:5012/api/data/users

# Create a new user
curl -X POST http://localhost:5012/api/data/users -H "Content-Type: application/json" -d "{\"name\":\"Jane Doe\",\"email\":\"jane@example.com\",\"age\":\"28\",\"active\":\"true\"}"

# Update a user (replace {id})
curl -X PUT http://localhost:5012/api/data/users/{id} -H "Content-Type: application/json" -d "{\"age\":\"30\"}"

# Delete a user
curl -X DELETE http://localhost:5012/api/data/users/{id}
```

### 5. **Postman or Insomnia**
Create requests to:
- `GET http://localhost:5012/api/data`
- `GET http://localhost:5012/api/data/users`
- `POST http://localhost:5012/api/data/users`
- `PUT http://localhost:5012/api/data/users/{id}`
- `DELETE http://localhost:5012/api/data/users/{id}`

---

## Available Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/data` | List all available collections |
| GET | `/api/data/users` | List all users |
| GET | `/api/data/users/{id}` | Get specific user |
| GET | `/api/data/users/schema` | Get user schema |
| POST | `/api/data/users` | Create new user |
| PUT | `/api/data/users/{id}` | Update user |
| DELETE | `/api/data/users/{id}` | Delete user |

---

## Example: Complete Workflow

### 1. Start the Server
```powershell
.\START_API.ps1
```

### 2. Get All Users
```powershell
Invoke-RestMethod -Uri http://localhost:5012/api/data/users
```

**Response:**
```json
{
  "data": [
    { "id": "1", "Data": { "id": "1", "name": "Alice", "email": "alice@example.com" } },
    { "id": "2", "Data": { "id": "2", "name": "Bob", "email": "bob@example.com" } }
  ],
  "total": 2,
  "more": false
}
```

### 3. Create a New User
```powershell
$body = @{
    name = "Charlie"
    email = "charlie@example.com"
    age = "35"
    active = "true"
} | ConvertTo-Json

Invoke-RestMethod -Uri http://localhost:5012/api/data/users -Method POST -Body $body -ContentType "application/json"
```

### 4. Update the User
```powershell
$body = '{"age":"36"}' 
Invoke-RestMethod -Uri http://localhost:5012/api/data/users/{id} -Method PUT -Body $body -ContentType "application/json"
```

### 5. Delete the User
```powershell
Invoke-RestMethod -Uri http://localhost:5012/api/data/users/{id} -Method DELETE
```

---

## Quick Test Commands

Copy and paste these into PowerShell:

```powershell
# 1. List all available collections
Invoke-RestMethod http://localhost:5012/api/data

# 2. Get all users
Invoke-RestMethod http://localhost:5012/api/data/users

# 3. Get schema
Invoke-RestMethod http://localhost:5012/api/data/users/schema

# 4. Create a user
$newUser = '{"name":"Test User","email":"test@example.com","age":"25"}'
Invoke-RestMethod http://localhost:5012/api/data/users -Method POST -Body $newUser -ContentType "application/json"
```

---

## Troubleshooting

### Server won't start?
- Make sure port 5000 is available
- Check if another application is using that port
- Try: `dotnet run --project DataAbstractionAPI.API --urls "http://localhost:5013"`

### Can't connect?
- Make sure the server is actually running
- Look for "Now listening on: http://localhost:5012" in the console
- Wait a few seconds for it to fully start

### Want to see logs?
- Check the console output where you started the server
- The server logs all requests

---

## Try It Now!

1. **Start the server:**
   ```powershell
   cd DataAbstractionAPI.API
   dotnet run
   ```

2. **Open Swagger UI in your browser:**
   ```
   http://localhost:5012/swagger
   ```

3. **Or test in PowerShell:**
   ```powershell
   # List all collections
   Invoke-RestMethod http://localhost:5012/api/data
   
   # Get all users
   Invoke-RestMethod http://localhost:5012/api/data/users
   ```

Enjoy! 🚀

