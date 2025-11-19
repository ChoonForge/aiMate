# aiMate Docker Deployment Guide

This guide explains how to deploy aiMate using Docker Compose, pulling directly from the GitHub repository.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Database Options](#database-options)
3. [Public vs Private Repository](#public-vs-private-repository)
4. [Configuration](#configuration)
5. [Deployment Scenarios](#deployment-scenarios)
6. [Troubleshooting](#troubleshooting)

## Quick Start

### Prerequisites

- Docker Engine 20.10+
- Docker Compose v2.0+
- (For private repos) GitHub Personal Access Token

### Default Setup (InMemory Database)

This is the fastest way to get started. Data is stored in memory and **will be lost on restart**.

```bash
# 1. Copy the example environment file
cp .env.example .env

# 2. Edit .env and configure your API keys
nano .env

# 3. Start the services
docker-compose -f docker-compose.production.yml up -d

# 4. Access aiMate
# Open your browser to http://localhost:5000
```

## Database Options

aiMate supports two database providers:

### 1. InMemory Database (Default)

**Pros:**
- ✅ Quick setup - no database installation required
- ✅ Minimal resource requirements
- ✅ Perfect for testing and development

**Cons:**
- ❌ No data persistence - data lost on restart
- ❌ Not suitable for production

**Configuration:**
```env
DATABASE_PROVIDER=InMemory
```

### 2. PostgreSQL Database

**Pros:**
- ✅ Full data persistence
- ✅ Production-ready
- ✅ Supports vector embeddings (pgvector)

**Cons:**
- ❌ Requires more resources
- ❌ Additional configuration needed

**Configuration:**
```env
DATABASE_PROVIDER=PostgreSQL
POSTGRES_PASSWORD=your-secure-password
POSTGRES_CONNECTION_STRING=Host=postgres;Database=aimate;Username=aimate;Password=your-secure-password
```

**Start with PostgreSQL:**
```bash
docker-compose -f docker-compose.production.yml --profile with-postgres up -d
```

## Public vs Private Repository

### Option 1: Public Repository (Recommended for General Use)

If the repository is public, Docker can pull it directly without any credentials.

**Advantages:**
- ✅ No authentication required
- ✅ Simplest setup
- ✅ Works out of the box

The `docker-compose.production.yml` is already configured to pull from:
```yaml
context: https://github.com/ChoonForge/aiMate.git#main:src
```

### Option 2: Private Repository

For private repositories, you have several options:

#### Method 1: Clone Locally (Recommended)

Clone the repository first, then build locally:

```bash
# 1. Clone the repository with authentication
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# 2. Modify docker-compose.production.yml to use local build
# Change the context line from:
#   context: https://github.com/ChoonForge/aiMate.git#main:src
# To:
#   context: .

# 3. Start services
docker-compose -f docker-compose.production.yml up -d
```

#### Method 2: Use GitHub Personal Access Token

If you want to pull directly from a private repo:

```bash
# 1. Create a GitHub Personal Access Token
# Go to: https://github.com/settings/tokens
# Scopes needed: repo (Full control of private repositories)

# 2. Modify docker-compose.production.yml
# Change the context to include your token:
#   context: https://YOUR_TOKEN@github.com/ChoonForge/aiMate.git#main:src

# 3. Start services
docker-compose -f docker-compose.production.yml up -d
```

⚠️ **Security Warning:** Never commit your token to git! Use environment variables:

```yaml
# In docker-compose.production.yml
context: https://${GITHUB_TOKEN}@github.com/ChoonForge/aiMate.git#main:src
```

```bash
# In .env
GITHUB_TOKEN=ghp_your_token_here
```

#### Method 3: SSH Authentication

For local development with SSH keys:

```bash
# 1. Ensure your SSH key is loaded
ssh-add ~/.ssh/id_rsa

# 2. Modify docker-compose.production.yml to use SSH
# Change context to:
#   context: git@github.com:ChoonForge/aiMate.git#main:src

# 3. Enable BuildKit (required for SSH)
export DOCKER_BUILDKIT=1

# 4. Update docker-compose.production.yml to forward SSH
# Add under the build section:
#   ssh:
#     - default

# 5. Start services
docker-compose -f docker-compose.production.yml up -d
```

## Configuration

### Environment Variables

All configuration is done via the `.env` file. See `.env.example` for all available options.

**Required Settings:**
```env
JWT_SECRET=your-secret-key-change-in-production
LITELLM_MASTER_KEY=sk-1234
```

**Optional AI Provider Keys:**
```env
OPENAI_API_KEY=sk-...
ANTHROPIC_API_KEY=sk-ant-...
GOOGLE_API_KEY=...
```

### LiteLLM Configuration

If you need to customize LiteLLM (the AI gateway), edit `litellm-config.yaml`:

```yaml
model_list:
  - model_name: gpt-4
    litellm_params:
      model: openai/gpt-4
      api_key: ${OPENAI_API_KEY}
```

## Deployment Scenarios

### Scenario 1: Testing/Development (InMemory)

Perfect for quick testing and development:

```bash
# Minimal setup - just aiMate, Redis, and LiteLLM
docker-compose -f docker-compose.production.yml up -d
```

**Services Started:**
- aiMate Web (port 5000)
- Redis
- LiteLLM Gateway (port 4000)

### Scenario 2: Production (PostgreSQL)

Full production deployment with data persistence:

```bash
# Start with PostgreSQL
docker-compose -f docker-compose.production.yml --profile with-postgres up -d
```

**Services Started:**
- aiMate Web (port 5000)
- PostgreSQL with pgvector (port 5432)
- Redis
- LiteLLM Gateway (port 4000)

### Scenario 3: Production with Nginx

Add Nginx for SSL termination and reverse proxy:

```bash
# Start with PostgreSQL and Nginx
docker-compose -f docker-compose.production.yml \
  --profile with-postgres \
  --profile with-nginx \
  up -d
```

**Services Started:**
- aiMate Web (port 5000)
- PostgreSQL with pgvector (port 5432)
- Redis
- LiteLLM Gateway (port 4000)
- Nginx (ports 80/443)

**Note:** You'll need to provide your own `nginx.conf` and SSL certificates in the `ssl/` directory.

## Troubleshooting

### Build fails to clone from GitHub

**Problem:** "fatal: could not read Username for 'https://github.com'"

**Solution:** The repository is private. Use one of the private repository methods above.

### Database connection errors

**Problem:** "Cannot connect to database"

**Solution:**
1. Check if PostgreSQL container is running: `docker ps`
2. Verify environment variables in `.env`
3. Check PostgreSQL logs: `docker logs aimate-postgres`

### Application starts but data is lost on restart

**Problem:** Data disappears when containers restart

**Solution:** You're using InMemory database. Switch to PostgreSQL:
1. Set `DATABASE_PROVIDER=PostgreSQL` in `.env`
2. Configure PostgreSQL connection string
3. Start with PostgreSQL profile: `--profile with-postgres`

### Port already in use

**Problem:** "Bind for 0.0.0.0:5000 failed: port is already allocated"

**Solution:** Change the port mapping in `docker-compose.production.yml`:
```yaml
ports:
  - "8080:5000"  # Change 5000 to any available port
```

### Out of memory errors

**Problem:** Docker runs out of memory

**Solution:**
1. Increase Docker memory limit (Docker Desktop settings)
2. Use InMemory database instead of PostgreSQL for testing
3. Stop unused containers: `docker container prune`

## Monitoring

### View Logs

```bash
# All services
docker-compose -f docker-compose.production.yml logs -f

# Specific service
docker logs aimate-web -f
docker logs aimate-postgres -f
```

### Health Checks

All services include health checks. View status:

```bash
docker ps

# Check aiMate health endpoint
curl http://localhost:5000/health
```

### Accessing Services

- **aiMate Web:** http://localhost:5000
- **LiteLLM Gateway:** http://localhost:4000
- **PostgreSQL:** localhost:5432 (if using with-postgres profile)
- **Redis:** localhost:6379
- **Nginx:** http://localhost (if using with-nginx profile)

## Stopping and Cleaning Up

### Stop Services

```bash
# Stop all services
docker-compose -f docker-compose.production.yml down

# Stop and remove volumes (deletes all data!)
docker-compose -f docker-compose.production.yml down -v
```

### Update to Latest Version

```bash
# Pull latest changes
docker-compose -f docker-compose.production.yml pull

# Rebuild aiMate (if pulling from GitHub)
docker-compose -f docker-compose.production.yml build --no-cache aimate-web

# Restart services
docker-compose -f docker-compose.production.yml up -d
```

## Security Considerations

1. **Change default secrets:** Always change `JWT_SECRET` in production
2. **Secure API keys:** Never commit API keys to git
3. **Use HTTPS:** Enable the Nginx profile with SSL certificates for production
4. **Firewall rules:** Restrict access to database ports (5432, 6379)
5. **Regular updates:** Keep Docker images up to date

## Support

For issues and questions:
- GitHub Issues: https://github.com/ChoonForge/aiMate/issues
- Documentation: See `/docs` directory

---

**Built with ❤️ in New Zealand 🇳🇿**
