# Installation Guide

> **Tooltip Summary:** "Install aiMate using Docker (recommended) or build from source. Get running in 10 minutes with our quick start guide."

---

## 🚀 Quick Start with Docker (Recommended)

The fastest way to get aiMate running is with Docker.

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) 20.10+ and Docker Compose
- 4GB+ RAM
- 10GB+ disk space

### Option 1: Quick Start (InMemory Database)

Perfect for testing. Data is stored in memory and lost on restart.

```bash
# Clone the repository
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# Copy environment template
cp .env.example .env

# Edit .env with your API keys
nano .env

# Start all services
docker-compose -f docker-compose.production.yml up -d

# Open in browser
open http://localhost:5000
```

###Option 2: Production Setup (PostgreSQL)

For production use with persistent storage.

```bash
# Clone and enter directory
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# Copy and configure environment
cp .env.example .env
nano .env  # Set DATABASE_PROVIDER=PostgreSQL

# Start with PostgreSQL
docker-compose -f docker-compose.production.yml --profile with-postgres up -d

# Check status
docker-compose -f docker-compose.production.yml ps

# Open in browser
open http://localhost:5000
```

**First Login:**
- Default admin user is created on first run
- Check logs for credentials: `docker-compose logs aimate-web`

---

## 💻 Local Development Setup

For contributors or advanced users who want to run from source.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/) with pgvector (optional - can use InMemory)
- [Node.js 20+](https://nodejs.org/) (for frontend build)

### Installation Steps

```bash
# Clone repository
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# Restore dependencies
dotnet restore

# Option 1: InMemory database (quick start)
dotnet run --project AiMate.Web

# Option 2: PostgreSQL
# Update connection string in appsettings.json
# Run migrations
dotnet ef database update --project AiMate.Infrastructure

# Start application
dotnet run --project AiMate.Web

# Open browser
open https://localhost:5001
```

---

## 🔧 Environment Configuration

### Required Environment Variables

Edit `.env` file:

```bash
# Database
DATABASE_PROVIDER=PostgreSQL  # or InMemory
DATABASE_CONNECTION_STRING="Host=localhost;Database=aimate;Username=postgres;Password=yourpassword"

# Authentication
JWT_SECRET_KEY="your-256-bit-secret-key-change-this-in-production"
JWT_ISSUER="https://api.aimate.co.nz"
JWT_AUDIENCE="https://api.aimate.co.nz"

# AI Provider (LiteLLM)
LITELLM_BASE_URL="http://localhost:4000"
LITELLM_API_KEY="your-litellm-key"

# Optional: AI Provider API Keys (for BYOK)
OPENAI_API_KEY="sk-..."
ANTHROPIC_API_KEY="sk-ant-..."
GOOGLE_API_KEY="..."
```

### LiteLLM Setup

aiMate requires LiteLLM as an AI gateway.

**Option 1: Docker (easiest)**

```bash
# Already included in docker-compose.production.yml
docker-compose up litellm
```

**Option 2: Python Install**

```bash
# Install LiteLLM
pip install litellm[proxy]

# Create config
cat > litellm_config.yaml <<EOF
model_list:
  - model_name: gpt-4
    litellm_params:
      model: gpt-4
      api_key: ${OPENAI_API_KEY}
  - model_name: claude-3-opus
    litellm_params:
      model: claude-3-opus-20240229
      api_key: ${ANTHROPIC_API_KEY}
EOF

# Start proxy
litellm --config litellm_config.yaml --port 4000
```

---

## 🌐 Cloud Deployment

### Deploy to Azure

```bash
# Login to Azure
az login

# Create resource group
az group create --name aiMate-rg --location australiaeast

# Create container instance
az container create \
  --resource-group aiMate-rg \
  --name aiMate \
  --image ghcr.io/choonforge/aimate:latest \
  --dns-name-label aiMate-demo \
  --ports 80 443 \
  --environment-variables \
    DATABASE_PROVIDER=PostgreSQL \
    JWT_SECRET_KEY=$JWT_SECRET
```

### Deploy to AWS ECS

See [Deployment Guide](../docs/DEPLOYMENT.md) for AWS, Google Cloud, and other platforms.

---

## 📦 What Gets Installed

When you install aiMate, you get:

| Component | Description | Port |
|-----------|-------------|------|
| **aiMate Web** | Blazor Server UI + REST API | 5000/5001 |
| **PostgreSQL** | Database with pgvector | 5432 |
| **LiteLLM** | AI gateway and proxy | 4000 |
| **Redis** (optional) | Caching and sessions | 6379 |
| **Nginx** (optional) | Reverse proxy and SSL | 80/443 |

---

## ✅ Verify Installation

### Check Services

```bash
# Docker
docker-compose ps

# All services should show "Up"
```

### Test Web Interface

1. Open http://localhost:5000
2. You should see the aiMate login page
3. Register a new account or use default admin credentials

### Test API

```bash
# Health check
curl http://localhost:5000/health

# Expected response:
# {"status": "Healthy"}

# API documentation
open http://localhost:5000/swagger
```

---

## 🔄 Updating aiMate

### Docker Update

```bash
# Pull latest images
docker-compose pull

# Restart services
docker-compose down
docker-compose up -d

# Check logs
docker-compose logs -f
```

### Source Update

```bash
# Pull latest code
git pull origin main

# Update dependencies
dotnet restore

# Run migrations
dotnet ef database update --project AiMate.Infrastructure

# Rebuild and restart
dotnet build
dotnet run --project AiMate.Web
```

---

## 🗑️ Uninstall

### Docker Uninstall

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (WARNING: deletes all data)
docker-compose down -v

# Remove images
docker rmi aimate-web aimate-litellm postgres:16
```

### Local Uninstall

```bash
# Delete database
dropdb aimate

# Remove files
rm -rf aiMate/
```

---

## 🆘 Troubleshooting Installation

### Port Already in Use

If port 5000 is in use:

```bash
# Edit docker-compose.yml
ports:
  - "5001:5000"  # Use port 5001 instead
```

### Database Connection Errors

```bash
# Check PostgreSQL is running
docker-compose ps postgres

# View logs
docker-compose logs postgres

# Test connection
psql -h localhost -U postgres -d aimate
```

### LiteLLM Not Starting

```bash
# Check LiteLLM logs
docker-compose logs litellm

# Verify config
cat litellm_config.yaml

# Test direct connection
curl http://localhost:4000/health
```

### Docker Permission Denied

```bash
# Add user to docker group
sudo usermod -aG docker $USER

# Re-login or run
newgrp docker
```

---

## 📚 Next Steps

Now that aiMate is installed:

1. **[First Steps](FirstSteps.md)** - Create your first workspace and chat
2. **[Settings](../UserGuide/Settings.md)** - Configure your preferences
3. **[Model Configuration](../AdminGuide/ModelConfiguration.md)** - Add AI models

---

## 🔗 Related Documentation

- [System Requirements](../docs/SYSTEM_SPECIFICATION_2025-11-22.md#deployment-architecture)
- [Docker Deployment Guide](../src/DOCKER_DEPLOYMENT.md)
- [Security Configuration](../SECURITY.md)
- [Environment Variables Reference](../docs/ENVIRONMENT_VARIABLES.md)

---

**Need Help?** Join our [GitHub Discussions](https://github.com/ChoonForge/aiMate/discussions) or email hello@aimate.co.nz
