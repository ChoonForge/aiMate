#!/bin/bash

# aiMate Quick Start Script
# One-command deployment of the entire aiMate stack
# Built with ❤️ from New Zealand 🇳🇿

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Banner
echo -e "${BLUE}"
echo "╔═══════════════════════════════════════════════════════════╗"
echo "║                                                           ║"
echo "║        aiMate v2.0 - Quick Start Deployment 🚀          ║"
echo "║                                                           ║"
echo "║        Your AI Mate. Free for Kiwis. Fair for Everyone   ║"
echo "║                                                           ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo -e "${NC}"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker is not installed. Please install Docker first.${NC}"
    echo "   Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}❌ Docker Compose is not installed. Please install Docker Compose first.${NC}"
    echo "   Visit: https://docs.docker.com/compose/install/"
    exit 1
fi

echo -e "${GREEN}✓ Docker and Docker Compose are installed${NC}"
echo ""

# Check if .env file exists, if not create from template
if [ ! -f .env ]; then
    echo -e "${YELLOW}⚠ No .env file found. Creating from template...${NC}"
    if [ -f .env.template ]; then
        cp .env.template .env
        echo -e "${GREEN}✓ Created .env file from template${NC}"
        echo -e "${YELLOW}⚠ Please edit .env file to add your API keys and configuration${NC}"
        echo ""
        read -p "Press Enter to continue with default values or Ctrl+C to exit and configure..."
    else
        echo -e "${RED}❌ .env.template not found!${NC}"
        exit 1
    fi
fi

# Create LiteLLM config if not exists
if [ ! -f litellm_config.yaml ]; then
    echo -e "${YELLOW}⚠ Creating LiteLLM configuration...${NC}"
    cat > litellm_config.yaml << 'EOF'
# LiteLLM Configuration
# Supports multiple AI providers

model_list:
  # OpenAI Models
  - model_name: gpt-4-turbo
    litellm_params:
      model: gpt-4-turbo-preview
      api_key: os.environ/OPENAI_API_KEY

  - model_name: gpt-3.5-turbo
    litellm_params:
      model: gpt-3.5-turbo
      api_key: os.environ/OPENAI_API_KEY

  # Anthropic Claude Models
  - model_name: claude-3-opus
    litellm_params:
      model: claude-3-opus-20240229
      api_key: os.environ/ANTHROPIC_API_KEY

  - model_name: claude-3-sonnet
    litellm_params:
      model: claude-3-sonnet-20240229
      api_key: os.environ/ANTHROPIC_API_KEY

  # Groq (Fast inference)
  - model_name: llama3-70b
    litellm_params:
      model: groq/llama3-70b-8192
      api_key: os.environ/GROQ_API_KEY

  # Add more models as needed...

general_settings:
  master_key: os.environ/LITELLM_MASTER_KEY
  database_url: os.environ/DATABASE_URL
EOF
    echo -e "${GREEN}✓ Created litellm_config.yaml${NC}"
fi

echo ""
echo -e "${BLUE}Starting aiMate services...${NC}"
echo ""

# Determine which compose file to use
COMPOSE_FILE="docker-compose.dev.yml"
if [ "$1" == "prod" ] || [ "$1" == "production" ]; then
    COMPOSE_FILE="docker-compose.production.yml"
    echo -e "${YELLOW}⚠ Starting in PRODUCTION mode${NC}"
else
    echo -e "${GREEN}Starting in DEVELOPMENT mode${NC}"
fi

# Pull latest images
echo -e "${BLUE}📦 Pulling Docker images...${NC}"
docker-compose -f $COMPOSE_FILE pull

# Build services
echo -e "${BLUE}🔨 Building aiMate...${NC}"
docker-compose -f $COMPOSE_FILE build

# Start services
echo -e "${BLUE}🚀 Starting services...${NC}"
docker-compose -f $COMPOSE_FILE up -d

# Wait for services to be healthy
echo ""
echo -e "${BLUE}⏳ Waiting for services to be ready...${NC}"
echo ""

# Wait for PostgreSQL
echo -n "  Waiting for PostgreSQL... "
for i in {1..30}; do
    if docker-compose -f $COMPOSE_FILE exec -T postgres pg_isready -U aimate -d aimate &> /dev/null; then
        echo -e "${GREEN}✓${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

# Wait for Redis
echo -n "  Waiting for Redis...      "
for i in {1..30}; do
    if docker-compose -f $COMPOSE_FILE exec -T redis redis-cli ping &> /dev/null; then
        echo -e "${GREEN}✓${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

# Wait for LiteLLM
echo -n "  Waiting for LiteLLM...    "
for i in {1..30}; do
    if curl -s http://localhost:4000/health &> /dev/null; then
        echo -e "${GREEN}✓${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

# Wait for aiMate Web
echo -n "  Waiting for aiMate Web... "
for i in {1..60}; do
    if curl -s http://localhost:5000/health &> /dev/null; then
        echo -e "${GREEN}✓${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

echo ""
echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✨ aiMate is ready! ✨${NC}"
echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "🌐 Web Interface:     ${BLUE}http://localhost:5000${NC}"
echo -e "📚 API Documentation: ${BLUE}http://localhost:5000/api/docs${NC}"
echo -e "🤖 LiteLLM Gateway:   ${BLUE}http://localhost:4000${NC}"
echo -e "🗄️  PostgreSQL:        ${BLUE}localhost:5432${NC}"
echo -e "💾 Redis:             ${BLUE}localhost:6379${NC}"
echo ""
echo -e "${YELLOW}Useful Commands:${NC}"
echo -e "  View logs:      ${BLUE}docker-compose -f $COMPOSE_FILE logs -f${NC}"
echo -e "  Stop services:  ${BLUE}docker-compose -f $COMPOSE_FILE down${NC}"
echo -e "  Restart:        ${BLUE}docker-compose -f $COMPOSE_FILE restart${NC}"
echo -e "  Full cleanup:   ${BLUE}docker-compose -f $COMPOSE_FILE down -v${NC}"
echo ""
echo -e "${GREEN}Happy coding, mate! 🇳🇿${NC}"
echo ""
