# Week 3 – Demo 1: Docker Compose (Example Voting App)

## Objective
Use **Docker Compose** to run the Example Voting App locally as a multi-service application.

## What You’ll Learn
- Define multi-container apps with `docker-compose.yml`
- Configure service dependencies, networks, and environment variables
- Build, run, and observe logs for all services
- Scale services with Compose

---

## Prerequisites
- Docker Desktop or Docker Engine installed
- Cloned repo:
  ```bash
  git clone https://github.com/msalemcode/Container_Operating_Systems.git
   cd Container_Operating_Systems/Code/Lab/voting-app
  ```
- Working Dockerfiles from Week 2 for `vote` and `worker` (or use the project’s existing Dockerfiles if present).

---

## 1) Create docker-compose.yml
Create a file named **docker-compose.yml** at the repo root:

```yaml
# version is now using "compose spec"
# v2 and v3 are now combined!
# docker-compose v1.27+ required
services:
  vote:
    image: voting-web
    depends_on:
      redis:
        condition: service_healthy
    healthcheck: 
      test: ["CMD", "curl", "-f", "http://localhost"]
      interval: 15s
      timeout: 5s
      retries: 3
      start_period: 10s
    volumes:
     - ./vote:/usr/local/app
    ports:
      - "8080:80"
    networks:
      - vote-tier

  result:
    image: magdysalem/voting-result:latest
    depends_on:
      db:
        condition: service_healthy 
    ports:
      - "5000:80"
    networks:
      - vote-tier

  worker:
    image: voting-worker
    depends_on:
      redis:
        condition: service_healthy 
      db:
        condition: service_healthy 
    networks:
      - vote-tier

  redis:
    image: redis:alpine
    volumes:
      - "./healthchecks:/healthchecks"
    healthcheck:
      test: /healthchecks/redis.sh
      interval: "5s"
    networks:
      - vote-tier

  db:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: "postgres"
      POSTGRES_PASSWORD: "postgres"
    volumes:
      - "db-data:/var/lib/postgresql/data"
      - "./healthchecks:/healthchecks"
    healthcheck:
      test: /healthchecks/postgres.sh
      interval: "5s"
    networks:
      - vote-tier
volumes:
  db-data:

networks:
  vote-tier:
```

---

## 2) Bring the stack up
```bash
docker compose up 
```

**Open apps:**
- Web (frontend): http://localhost:8080
- Result (backend): http://localhost:8081

---

## 3) Useful commands
- Run in detached mode:
  ```bash
  docker compose up -d
  ```
- See logs:
  ```bash
  docker compose logs -f vote-1
  docker compose logs -f worker-1
  ```
- Scale a service:
  ```bash
  docker compose up -d --scale worker=3
  ```
- List containers:
  ```bash
  docker compose ps
  ```




## 5) Cleanup
```bash
docker compose down -v
```
This stops containers, removes the network and persistent volume `dbdata`.
