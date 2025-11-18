# aiMate Docker Deployment Guide 🐳

Complete guide for deploying aiMate using Docker and Docker Compose.

---

## 🚀 Quick Start (Recommended)

### Option 1: Full Stack with Docker (Recommended for Production)

```bash
# 1. Clone the repository
git clone https://github.com/your-org/aiMate.git
cd aiMate/src-v2

# 2. Run the quick-start script
./quick-start.sh
```

That's it! The script will:
- ✅ Check Docker installation
- ✅ Create `.env` from template
- ✅ Create LiteLLM configuration
- ✅ Pull and build all images
- ✅ Start all services
- ✅ Wait for health checks
- ✅ Show you the URLs

**Access aiMate:**
- Web: http://localhost:5000
- API Docs: http://localhost:5000/api/docs
- LiteLLM: http://localhost:4000

### Option 2: In-Memory Database (Quick Testing - No Docker Required!)

Perfect for quick testing or development without Docker/PostgreSQL:

```bash
# 1. Navigate to the Web project
cd src-v2/AiMate.Web

# 2. Run with in-memory database
dotnet run --launch-profile InMemory

# OR set environment variable
export ASPNETCORE_ENVIRONMENT=InMemory
dotnet run
```

**Features:**
- ✅ No database setup required
- ✅ Instant startup (~5 seconds)
- ✅ Full functionality
- ⚠️ Data lost on shutdown (in-memory only)

**Perfect for:**
- Quick demos
- Testing features
- Development without Docker
- CI/CD pipelines

---

## 📦 What Gets Deployed

### Services

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| **aiMate Web** | Custom (.NET 9) | 5000 | Main application |
| **PostgreSQL** | pgvector/pgvector:pg16 | 5432 | Database with vector support |
| **Redis** | redis:7-alpine | 6379 | Cache & sessions |
| **LiteLLM** | ghcr.io/berriai/litellm | 4000 | AI gateway (multi-provider) |

### Volumes

| Volume | Purpose |
|--------|---------|
| postgres_data | PostgreSQL database files |
| redis_data | Redis persistence |
| litellm_data | LiteLLM cache |
| uploads_data | User file uploads |
| logs_data | Application logs |

---

## ⚙️ Configuration

### Environment Variables

Copy `.env.template` to `.env` and customize:

```bash
cp .env.template .env
nano .env  # or your favorite editor
```

**Required Settings:**
```env
# Database
POSTGRES_PASSWORD=your_strong_password_here

# LiteLLM Master Key
LITELLM_MASTER_KEY=sk-your-master-key

# AI Provider API Keys (at least one)
OPENAI_API_KEY=sk-...
ANTHROPIC_API_KEY=sk-ant-...
GROQ_API_KEY=gsk_...
```

**Optional Settings:**
```env
# Ports
WEB_PORT_HTTP=5000
POSTGRES_PORT=5432
LITELLM_PORT=4000

# Timezone
TZ=Pacific/Auckland

# In-Memory Database (no PostgreSQL)
USE_IN_MEMORY_DATABASE=false
```

### LiteLLM Configuration

Edit `litellm_config.yaml` to add/remove AI models:

```yaml
model_list:
  - model_name: gpt-4-turbo
    litellm_params:
      model: gpt-4-turbo-preview
      api_key: os.environ/OPENAI_API_KEY

  - model_name: claude-3-opus
    litellm_params:
      model: claude-3-opus-20240229
      api_key: os.environ/ANTHROPIC_API_KEY
```

---

## 🏗️ Manual Deployment

### Development Mode

```bash
# Start all services
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop services
docker-compose -f docker-compose.dev.yml down
```

**Features:**
- Hot reload enabled
- Development environment
- Source code mounted as volumes
- Detailed logging

### Production Mode

```bash
# Build and start
docker-compose -f docker-compose.production.yml up -d

# View logs (production logging)
docker-compose -f docker-compose.production.yml logs -f web

# Stop
docker-compose -f docker-compose.production.yml down
```

**Features:**
- Optimized build
- Production logging
- No source mounting
- Smaller image size

---

## 🔧 Useful Commands

### Service Management

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Restart a specific service
docker-compose restart web

# View logs (all services)
docker-compose logs -f

# View logs (specific service)
docker-compose logs -f web

# Check service health
docker-compose ps
```

### Database Management

```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U aimate -d aimate

# Run migrations
docker-compose exec web dotnet ef database update

# Backup database
docker-compose exec postgres pg_dump -U aimate aimate > backup.sql

# Restore database
docker-compose exec -T postgres psql -U aimate aimate < backup.sql
```

### Debugging

```bash
# Shell into web container
docker-compose exec web bash

# Shell into PostgreSQL
docker-compose exec postgres bash

# View real-time logs
docker-compose logs -f --tail=100

# Check container stats
docker stats
```

### Cleanup

```bash
# Stop and remove containers
docker-compose down

# Remove containers + volumes (⚠️ DELETES DATA)
docker-compose down -v

# Remove everything including images
docker-compose down -v --rmi all

# Prune all unused Docker resources
docker system prune -a
```

---

## 🔍 Health Checks

All services have health checks configured:

### Check Service Health

```bash
# All services
docker-compose ps

# PostgreSQL
docker-compose exec postgres pg_isready -U aimate

# Redis
docker-compose exec redis redis-cli ping

# Web App
curl http://localhost:5000/health

# LiteLLM
curl http://localhost:4000/health
```

### Health Check Endpoints

| Service | Endpoint | Expected Response |
|---------|----------|-------------------|
| Web | http://localhost:5000/health | {"status":"healthy"} |
| LiteLLM | http://localhost:4000/health | {"status":"healthy"} |

---

## 🐛 Troubleshooting

### Services won't start

```bash
# Check logs
docker-compose logs

# Check Docker daemon
systemctl status docker

# Restart Docker
sudo systemctl restart docker
```

### Database connection errors

```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check connection
docker-compose exec web psql -h postgres -U aimate -d aimate

# Reset database (⚠️ deletes data)
docker-compose down -v
docker-compose up -d
```

### Port conflicts

If ports are already in use, edit `.env`:

```env
WEB_PORT_HTTP=5001  # Change from 5000
POSTGRES_PORT=5433  # Change from 5432
```

### Memory issues

```bash
# Check Docker resources
docker stats

# Increase Docker memory limit (Docker Desktop)
# Settings → Resources → Memory → Increase to 4GB+
```

### Can't connect to LiteLLM

```bash
# Check LiteLLM logs
docker-compose logs litellm

# Verify API keys in .env
grep API_KEY .env

# Test connection
curl http://localhost:4000/health
```

---

## 🔒 Security Recommendations

### Production Deployment

1. **Change all default passwords**
   ```env
   POSTGRES_PASSWORD=<use strong password>
   LITELLM_MASTER_KEY=<use strong random key>
   JWT_SECRET_KEY=<use 64 char random string>
   ```

2. **Use HTTPS**
   - Add reverse proxy (nginx/Caddy)
   - Configure SSL certificates
   - Update ASPNETCORE_URLS

3. **Restrict network access**
   - Don't expose PostgreSQL/Redis ports publicly
   - Use firewall rules
   - Configure Docker networks properly

4. **Regular updates**
   ```bash
   docker-compose pull
   docker-compose up -d
   ```

5. **Backup regularly**
   ```bash
   # Automated backup script
   docker-compose exec postgres pg_dump -U aimate aimate | gzip > backup-$(date +%Y%m%d).sql.gz
   ```

---

## 📊 Monitoring

### View Resource Usage

```bash
# Real-time stats
docker stats

# Disk usage
docker system df

# Service-specific stats
docker stats aimate-web aimate-postgres
```

### Log Management

```bash
# Tail logs
docker-compose logs -f --tail=100

# Export logs
docker-compose logs > aiMate-logs-$(date +%Y%m%d).log

# Clear logs (restart containers)
docker-compose restart
```

---

## 🚀 Advanced Usage

### Custom Dockerfile

Edit `Dockerfile.production` or `Dockerfile.dev` for custom builds.

### Add Additional Services

Edit `docker-compose.yml`:

```yaml
services:
  prometheus:
    image: prom/prometheus
    ports:
      - "9090:9090"
    networks:
      - aimate-network
```

### Environment-Specific Configs

```bash
# Development
docker-compose -f docker-compose.dev.yml up

# Staging
docker-compose -f docker-compose.staging.yml up

# Production
docker-compose -f docker-compose.production.yml up
```

---

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Redis Docker Hub](https://hub.docker.com/_/redis)
- [LiteLLM Documentation](https://docs.litellm.ai/)

---

## 🆘 Getting Help

**Issues?**
- Check logs: `docker-compose logs -f`
- GitHub Issues: https://github.com/your-org/aiMate/issues
- Discord: [Your Discord Link]

**Contributing?**
- See CONTRIBUTING.md
- Submit PRs welcome!

---

**Built with ❤️ from New Zealand** 🇳🇿

*Making AI accessible to everyone, one container at a time.*
