# aiMate v2 - Production Deployment Guide

**Complete guide for deploying aiMate to production. Built for Kiwis, deployable anywhere.** 🇳🇿

---

## 📋 Prerequisites

### Required
- **Docker** 20.10+ and **Docker Compose** 2.0+
- **PostgreSQL** 16+ with **pgvector** extension
- **Domain name** (for HTTPS)
- **2GB RAM** minimum (4GB+ recommended)
- **10GB disk space** minimum

### Optional but Recommended
- **Let's Encrypt** for SSL certificates
- **Cloudflare** for DDoS protection
- **S3/Azure Blob** for file storage (instead of local)
- **Sentry** for error tracking

---

## 🚀 Quick Start (Development)

```bash
# Clone the repository
git clone https://github.com/yourusername/aiMate.git
cd aiMate/src-v2

# Copy environment template
cp .env.example .env

# Edit environment variables
nano .env

# Start all services
docker-compose up -d

# Run migrations
docker-compose exec aimate-web dotnet ef database update

# View logs
docker-compose logs -f
```

Access at: http://localhost:5000

---

## 🏭 Production Deployment

### Step 1: Server Setup

**Minimum Server Specifications:**
- **CPU:** 2 cores
- **RAM:** 4GB
- **Disk:** 20GB SSD
- **OS:** Ubuntu 22.04 LTS (recommended)

**Install Docker:**
```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo apt install docker-compose-plugin -y

# Add user to docker group
sudo usermod -aG docker $USER
newgrp docker

# Verify installation
docker --version
docker compose version
```

### Step 2: Environment Configuration

Create `.env` file in `/src-v2/`:

```bash
# PostgreSQL
POSTGRES_PASSWORD=your-super-secure-password-change-this

# JWT Authentication
JWT_SECRET=your-jwt-secret-minimum-32-characters-change-this-in-production

# LiteLLM
LITELLM_MASTER_KEY=sk-your-litellm-master-key

# API Keys (optional - for BYOK users)
OPENAI_API_KEY=sk-your-openai-key-if-needed
ANTHROPIC_API_KEY=sk-ant-your-anthropic-key
GOOGLE_API_KEY=your-google-api-key
```

**Security Checklist:**
- [ ] Change default passwords
- [ ] Generate strong JWT secret (32+ characters)
- [ ] Use environment-specific API keys
- [ ] Never commit `.env` to git
- [ ] Rotate secrets regularly

### Step 3: SSL Certificates

**Option A: Let's Encrypt (Recommended)**
```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx -y

# Obtain certificate
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# Certificates will be in:
# /etc/letsencrypt/live/yourdomain.com/fullchain.pem
# /etc/letsencrypt/live/yourdomain.com/privkey.pem

# Copy to project
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem ./ssl/cert.pem
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem ./ssl/key.pem
sudo chown $USER:$USER ./ssl/*.pem
```

**Option B: Self-Signed (Development Only)**
```bash
mkdir -p ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/key.pem -out ssl/cert.pem \
  -subj "/C=NZ/ST=Auckland/L=Auckland/O=aiMate/CN=localhost"
```

### Step 4: Database Initialization

Create `init-db.sql`:
```sql
-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create LiteLLM database
CREATE DATABASE litellm;
```

### Step 5: Deploy Services

```bash
# Build and start services
docker-compose -f docker-compose.production.yml up -d --build

# Check services are running
docker-compose -f docker-compose.production.yml ps

# View logs
docker-compose -f docker-compose.production.yml logs -f

# Run database migrations
docker-compose -f docker-compose.production.yml exec aimate-web \
  dotnet ef database update --project /app/AiMate.Infrastructure.dll
```

### Step 6: Verify Deployment

**Health Checks:**
```bash
# Check PostgreSQL
docker-compose exec postgres pg_isready -U aimate

# Check Redis
docker-compose exec redis redis-cli ping

# Check LiteLLM
curl http://localhost:4000/health

# Check aiMate Web
curl http://localhost:5000/health

# Check Nginx
curl -I http://localhost
```

**Expected Response:**
- PostgreSQL: `aimate:5432 - accepting connections`
- Redis: `PONG`
- LiteLLM: `{"status":"healthy"}`
- aiMate: HTTP 200
- Nginx: HTTP 301 redirect to HTTPS

---

## 🔧 Configuration

### LiteLLM Config

Create `litellm-config.yaml`:
```yaml
model_list:
  - model_name: gpt-4
    litellm_params:
      model: openai/gpt-4
      api_key: ${OPENAI_API_KEY}

  - model_name: claude-3-5-sonnet-20241022
    litellm_params:
      model: anthropic/claude-3-5-sonnet-20241022
      api_key: ${ANTHROPIC_API_KEY}

  - model_name: gemini-pro
    litellm_params:
      model: google/gemini-pro
      api_key: ${GOOGLE_API_KEY}

router_settings:
  routing_strategy: simple-shuffle
  num_retries: 3
  timeout: 120

general_settings:
  master_key: ${LITELLM_MASTER_KEY}
  database_url: ${DATABASE_URL}
```

### Nginx Tuning

For high traffic:
```nginx
# In nginx.conf
worker_processes auto;
worker_connections 4096;

# Add caching
proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=aimate_cache:10m max_size=100m;
```

---

## 📊 Monitoring & Logging

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f aimate-web

# Last 100 lines
docker-compose logs --tail=100 aimate-web
```

### Application Logs
Located in `/app/logs` inside container:
```bash
docker-compose exec aimate-web ls -lh /app/logs
docker-compose exec aimate-web tail -f /app/logs/aiMate-*.txt
```

### Database Monitoring
```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U aimate

# Check database size
\l+

# Check table sizes
\dt+

# Monitor active connections
SELECT count(*) FROM pg_stat_activity;

# Check vector index performance
SELECT schemaname, tablename, indexname
FROM pg_indexes
WHERE tablename = 'KnowledgeItems';
```

---

## 🔄 Backups

### Automated Backup Script

Create `backup.sh`:
```bash
#!/bin/bash
BACKUP_DIR="/backups/aimate"
DATE=$(date +%Y%m%d_%H%M%S)

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup PostgreSQL
docker-compose exec -T postgres pg_dump -U aimate aimate | \
  gzip > $BACKUP_DIR/database_$DATE.sql.gz

# Backup uploads
tar -czf $BACKUP_DIR/uploads_$DATE.tar.gz ./uploads

# Backup environment
cp .env $BACKUP_DIR/env_$DATE

# Keep only last 7 days
find $BACKUP_DIR -type f -mtime +7 -delete

echo "Backup completed: $DATE"
```

**Schedule with cron:**
```bash
chmod +x backup.sh

# Add to crontab (daily at 2 AM)
crontab -e
0 2 * * * /path/to/aiMate/src-v2/backup.sh >> /var/log/aimate-backup.log 2>&1
```

### Restore from Backup
```bash
# Restore database
gunzip -c /backups/aimate/database_YYYYMMDD_HHMMSS.sql.gz | \
  docker-compose exec -T postgres psql -U aimate aimate

# Restore uploads
tar -xzf /backups/aimate/uploads_YYYYMMDD_HHMMSS.tar.gz
```

---

## 🔒 Security Hardening

### Firewall Setup (UFW)
```bash
# Enable firewall
sudo ufw enable

# Allow SSH
sudo ufw allow 22/tcp

# Allow HTTP/HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Check status
sudo ufw status
```

### Docker Security
```bash
# Run containers as non-root user
# Add to Dockerfile:
RUN useradd -m -u 1000 appuser
USER appuser

# Enable Docker Content Trust
export DOCKER_CONTENT_TRUST=1
```

### Rate Limiting
Already configured in nginx.conf:
- Login: 5 requests per minute
- API: 10 requests per second

---

## 🚦 Health Checks

Add to `AiMate.Web/Program.cs`:
```csharp
app.MapGet("/health", async (AiMateDbContext db) =>
{
    var healthy = await db.Database.CanConnectAsync();
    return healthy ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503);
});
```

---

## 📈 Scaling

### Horizontal Scaling
```yaml
# In docker-compose.production.yml
services:
  aimate-web:
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '2'
          memory: 2G
```

### Load Balancing
Nginx is already configured for upstream load balancing. Add more backend servers:
```nginx
upstream aimate_backend {
    least_conn;
    server aimate-web-1:5000;
    server aimate-web-2:5000;
    server aimate-web-3:5000;
}
```

---

## 🐛 Troubleshooting

### Common Issues

**1. Database Connection Failed**
```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check logs
docker-compose logs postgres

# Test connection
docker-compose exec postgres psql -U aimate -c "SELECT 1"
```

**2. pgvector Extension Missing**
```bash
# Connect to database
docker-compose exec postgres psql -U aimate

# Enable extension
CREATE EXTENSION IF NOT EXISTS vector;

# Verify
\dx
```

**3. Out of Memory**
```bash
# Check container memory
docker stats

# Increase limits in docker-compose.production.yml
```

**4. SSL Certificate Errors**
```bash
# Check certificate
openssl x509 -in ssl/cert.pem -text -noout

# Verify nginx config
docker-compose exec nginx nginx -t

# Reload nginx
docker-compose exec nginx nginx -s reload
```

---

## 🔄 Updates & Maintenance

### Update Application
```bash
# Pull latest code
git pull origin main

# Rebuild and deploy
docker-compose -f docker-compose.production.yml up -d --build

# Run any new migrations
docker-compose exec aimate-web dotnet ef database update
```

### Zero-Downtime Deployment
```bash
# Build new image
docker-compose build aimate-web

# Start new container
docker-compose up -d --no-deps --scale aimate-web=2 aimate-web

# Wait for health check
sleep 30

# Stop old container
docker-compose up -d --no-deps --scale aimate-web=1 aimate-web
```

---

## 📞 Support

**Issues?** Open an issue on GitHub: https://github.com/yourusername/aiMate/issues

**Community:** Join our Discord (coming soon)

**Email:** support@aimate.co.nz

---

## 📜 License

MIT License - Free for everyone, no gatekeeping.

**Built with ❤️ from New Zealand** 🇳🇿

*Making OpenWebUI obsolete, one deployment at a time.* 🚀
