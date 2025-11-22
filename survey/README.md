# aiMate Alpha Testing Survey

A standalone ASP.NET Core web application for collecting survey responses from potential alpha testers for the aiMate platform.

## 🎯 Purpose

This survey helps us understand:
- Current AI tool usage patterns
- User demographics and technical comfort levels
- Pain points with existing AI solutions
- Feature requests and ideal AI assistant characteristics
- Interest in alpha testing aiMate

## 🏗️ Architecture

- **Framework**: ASP.NET Core 8.0 with Razor Pages
- **Database**: SQL Server (for better write concurrency)
- **ORM**: Entity Framework Core 8.0
- **Frontend**: Vanilla JavaScript with CSS
- **Export**: CSV via CsvHelper library

## 📁 Project Structure

```
survey/
├── Controllers/
│   └── SurveyController.cs       # API endpoints for submission and admin
├── Data/
│   └── SurveyDbContext.cs        # Entity Framework context
├── Migrations/                    # EF Core database migrations
├── Models/
│   └── SurveyResponse.cs         # Survey data model
├── Pages/
│   ├── Admin/
│   │   ├── Results.cshtml        # Admin dashboard
│   │   └── Results.cshtml.cs
│   ├── Index.cshtml              # Public survey form
│   └── Index.cshtml.cs
├── wwwroot/
│   ├── css/
│   │   └── survey.css            # Styling
│   └── js/
│       └── survey.js             # Form handling and submission
├── Program.cs                     # Application startup
└── appsettings.json              # Configuration
```

## 🚀 Setup Instructions

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (2019+ or SQL Server Express)
- Visual Studio 2022, VS Code, or JetBrains Rider (optional)

### Local Development

1. **Clone and navigate to the survey directory**:
   ```bash
   cd /path/to/aiMate/survey
   ```

2. **Configure the database connection**:

   Edit `appsettings.Development.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=AlphaSurvey;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

   For SQL Server authentication (instead of Windows auth):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=AlphaSurvey;User Id=your_username;Password=your_password;TrustServerCertificate=True;"
     }
   }
   ```

3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

   The application will:
   - Start on `https://localhost:5001` (or the next available port)
   - Automatically create and migrate the database on first run
   - Display the survey form at the root URL

5. **Access the survey**:
   - Public survey: `https://localhost:5001/`
   - Admin dashboard: `https://localhost:5001/Admin/Results`

## 🔐 Admin Access

The admin dashboard is protected by a simple key-based authentication.

### Setting the Admin Key

**Option 1: Environment Variable (Recommended)**
```bash
# Linux/Mac
export SURVEY_ADMIN_KEY="your-secure-admin-key-here"

# Windows CMD
set SURVEY_ADMIN_KEY=your-secure-admin-key-here

# Windows PowerShell
$env:SURVEY_ADMIN_KEY="your-secure-admin-key-here"
```

**Option 2: Use Default Key**
- Default key: `your-secret-admin-key-here`
- **IMPORTANT**: Change this before deploying to production!

**Option 3: Configuration File**
Update the controller to read from appsettings.json if preferred.

### Accessing Admin Features

1. Navigate to `/Admin/Results`
2. Enter the admin key
3. View:
   - Real-time statistics
   - Top AI tools mentioned
   - All survey responses
   - Export data to CSV

## 📊 API Endpoints

### Public Endpoints

**POST /api/survey/submit**
- Submits a new survey response
- Request body: JSON with survey data
- Returns: Success message with response ID

### Admin Endpoints (require `?adminKey=xxx`)

**GET /api/survey/stats**
- Returns aggregated statistics
- Response includes: total responses, AI user count, alpha tester interest, top tools

**GET /api/survey/responses**
- Returns all survey responses
- Response: Array of full survey response objects

**GET /api/survey/export**
- Downloads all responses as CSV
- Filename: `survey_responses_YYYYMMDD_HHMMSS.csv`

## 🎨 Customization

### Tracking Referral Sources

Add a query parameter to your survey links to track which Facebook group (or other source) respondents came from:

```
https://your-domain.com/?source=seattle-tech-group
https://your-domain.com/?source=ai-enthusiasts-pdx
https://your-domain.com/?ref=reddit-r-machinelearning
```

The referral source will be automatically captured and displayed in the admin dashboard.

### Styling

All styles are in `/wwwroot/css/survey.css`. The color scheme uses:
- Primary: Purple gradient (`#667eea` to `#764ba2`)
- Success: Green (`#48bb78`)
- Background: White with gradient background

Modify the CSS variables and gradient stops to match your branding.

### Adding Questions

To add new questions:

1. **Update the model** (`Models/SurveyResponse.cs`):
   ```csharp
   public string? NewQuestion { get; set; }
   ```

2. **Create a migration**:
   ```bash
   dotnet ef migrations add AddNewQuestion
   ```

3. **Update the form** (`Pages/Index.cshtml`):
   ```html
   <div class="form-group">
       <label for="newQuestion">Your Question?</label>
       <input type="text" id="newQuestion" name="newQuestion">
   </div>
   ```

4. **Update JavaScript** (`wwwroot/js/survey.js`):
   ```javascript
   newQuestion: document.getElementById('newQuestion').value || null,
   ```

## 🚢 Deployment

### Azure App Service

1. **Create an Azure App Service** (Windows or Linux)

2. **Create an Azure SQL Database**:
   - Get the connection string from Azure Portal
   - Update `appsettings.json`

3. **Set environment variables** in App Service Configuration:
   ```
   SURVEY_ADMIN_KEY = your-secure-production-key
   ```

4. **Deploy**:
   ```bash
   dotnet publish -c Release
   ```

   Then use one of:
   - Visual Studio Publish
   - Azure CLI: `az webapp deploy`
   - GitHub Actions
   - Azure DevOps Pipeline

### Docker (Optional)

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AlphaSurvey.csproj", "./"]
RUN dotnet restore "AlphaSurvey.csproj"
COPY . .
RUN dotnet build "AlphaSurvey.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlphaSurvey.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlphaSurvey.dll"]
```

Build and run:
```bash
docker build -t alpha-survey .
docker run -p 8080:80 -e SURVEY_ADMIN_KEY="your-key" alpha-survey
```

### IIS (Windows Server)

1. Install .NET 8.0 Hosting Bundle
2. Create a new IIS site
3. Point to published output folder
4. Configure application pool (No Managed Code)
5. Set environment variable in `web.config` or IIS

## 📈 Data Analysis

### Exporting Data

Use the admin dashboard's "Export CSV" button to download all responses. The CSV includes:
- All demographics fields
- AI usage patterns
- Barriers to adoption
- Open-ended feedback
- Contact information (for alpha testers)
- Metadata (submission time, IP, referral source)

### Sample Analysis Queries

**Find all alpha testers**:
```sql
SELECT Email, AgeRange, OccupationCategory, TechComfortLevel, AIToolsUsed
FROM SurveyResponses
WHERE InterestedInAlphaTesting = 1
ORDER BY SubmittedAt DESC
```

**Most mentioned AI tools**:
```sql
SELECT AIToolsUsed, COUNT(*) as Count
FROM SurveyResponses
WHERE AIToolsUsed IS NOT NULL
GROUP BY AIToolsUsed
ORDER BY Count DESC
```

**Barriers by demographics**:
```sql
SELECT
    TechComfortLevel,
    SUM(CASE WHEN BarrierCost = 1 THEN 1 ELSE 0 END) as CostBarrier,
    SUM(CASE WHEN BarrierComplexity = 1 THEN 1 ELSE 0 END) as ComplexityBarrier,
    SUM(CASE WHEN BarrierPrivacyConcerns = 1 THEN 1 ELSE 0 END) as PrivacyBarrier
FROM SurveyResponses
GROUP BY TechComfortLevel
```

## 🔒 Security Considerations

1. **Admin Key**: Change the default admin key before production deployment
2. **HTTPS**: Always use HTTPS in production (configured in `Program.cs`)
3. **Rate Limiting**: Consider adding rate limiting to prevent spam
4. **Email Validation**: Email addresses are validated client-side and with `[EmailAddress]` attribute
5. **SQL Injection**: Protected by Entity Framework parameterized queries
6. **CORS**: Currently allows all origins for API access - restrict in production if needed

## 🐛 Troubleshooting

### Database Connection Errors

**Error**: "A network-related or instance-specific error occurred"
- **Fix**: Verify SQL Server is running and connection string is correct
- Check: SQL Server Configuration Manager → SQL Server Network Configuration → TCP/IP enabled

**Error**: "Login failed for user"
- **Fix**: Verify credentials or use Trusted_Connection=True for Windows auth

### Migration Issues

**Error**: "No migrations configuration type was found"
- **Fix**: Run `dotnet ef database update` manually

**Error**: "The database already exists"
- **Fix**: This is fine - migrations will run automatically on startup

### Admin Access Issues

**Error**: "Unauthorized" when accessing admin endpoints
- **Fix**: Verify admin key matches environment variable or default key
- Check browser console for API errors

## 📝 License

This survey application is part of the aiMate project.

## 🤝 Support

For issues or questions:
- Check the main aiMate repository issues
- Contact the development team

---

**Happy surveying! 🚀**
