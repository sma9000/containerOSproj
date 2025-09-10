# Lab 2 – Advanced Dockerfile 

This lab builds an optimized **Worker** image (multi-stage), then runs a **local integration test** using a user-defined **Docker network** to connect **webapp, API, Redis, and PostgreSQL** — all without Docker Compose.

---

## Part A – Advanced Dockerfile (Worker)

### Objective
Write an optimized Dockerfile for the worker service using **multi-stage builds**.

## Prerequisites
- Docker Desktop or Docker Engine installed
- Cloned repo:
  ```bash
   git clone https://github.com/msalemcode/Container_Operating_Systems.git
   cd Container_Operating_Systems/Code/Lab/voting-app
  ```
  
## **Steps**

1. Navigate into the `worker` folder and create a `Dockerfile`:
    ```dockerfile
      # build compiles the program for the builder's local platform
      FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:7.0 AS build
      ARG TARGETPLATFORM
      ARG TARGETARCH
      ARG BUILDPLATFORM
      RUN echo "I am running on $BUILDPLATFORM, building for $TARGETPLATFORM"

      WORKDIR /source
      COPY *.csproj .
      RUN dotnet restore -a $TARGETARCH

      COPY . .
      RUN dotnet publish -c release -o /app -a $TARGETARCH --self-contained false --no-restore

      # app image
      FROM mcr.microsoft.com/dotnet/runtime:7.0
      WORKDIR /app
      COPY --from=build /app .
      ENTRYPOINT ["dotnet", "Worker.dll"]
  ```

2. Optional `.dockerignore` at repo root:
    ```
    __pycache__/
    *.pyc
    *.pyo
    *.pyd
    .git
    .gitignore
    venv/
    node_modules/
    dist/
    build/
    ```

3. Build the image:
  ```bash
  docker build -t voting-worker -f  .
  ```

4. Navigate into the `vote` folder and create a `Dockerfile`:
   ```dockerfile
      # base defines a base stage that uses the official python runtime base image
      FROM python:3.11-slim AS base

      # Add curl for healthcheck
      RUN apt-get update && \
         apt-get install -y --no-install-recommends curl && \
         rm -rf /var/lib/apt/lists/*

      # Set the application directory
      WORKDIR /usr/local/app

      # Install our requirements.txt
      COPY requirements.txt ./requirements.txt
      RUN pip install --no-cache-dir -r requirements.txt

      # dev defines a stage for development, where itll watch for filesystem changes
      FROM base AS dev
      RUN pip install watchdog
      ENV FLASK_ENV=development
      CMD ["python", "app.py"]

      # final defines the stage that will bundle the application for production
      FROM base AS final

      # Copy our code from the current folder to the working directory inside the container
      COPY . .

      # Make port 80 available for links and/or publish
      EXPOSE 80

      # Define our command to be run when launching the container
      CMD ["gunicorn", "app:app", "-b", "0.0.0.0:80", "--log-file", "-", "--access-logfile", "-", "--workers", "4", "--keep-alive", "0"]

   ```
3. Build the image:
  ```bash
  docker build -t voting-web  .
  ```


---

## Part B – Run Services on a User-Defined Docker Network 

We will create an **isolated bridge network** and run **Redis**, **PostgreSQL**, **API**, **Worker**, and **Webapp** containers attached to it.  
> Container names will act as DNS hostnames on the network (e.g., `redis`, `db`).

### 1) Create a network
```bash
docker network create voting-net
```

### 2) Start Redis
```bash
docker run -d --name redis --network voting-net redis:7-alpine
```

### 3) Start PostgreSQL
```bash
docker run -d --name db --network voting-net  -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=votes   -p 5432:5432 postgres:15-alpine
```
> Port `5432` publish is optional but useful for local DB tools. Inside the network, other containers reach it by hostname `db` on port `5432`.


### 5) Build & run the Worker
Build (done earlier):
```bash
cd worker/
docker build -t voting-worker -f Dockerfile .
```
Run (no ports, same network):
```bash
docker run -d --name worker --network voting-net   -e REDIS_HOST=redis -e POSTGRES_HOST=db   -e POSTGRES_DB=votes -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres   voting-worker
```

### 6) Build & run the Webapp

```bash
cd vote/
docker build -t voting-web -f Dockerfile .
# Example; adjust path/port according to your frontend
docker run -d --name web --network voting-net -p 5000:80 voting-web
```
Now visit: http://localhost:5000
---


### 7) Pull & run Result container

```bash
docker pull magdysalem/voting-result:latest

docker run -d --name result --network voting-net -p 8000:80 voting-result
```
Now visit: htp://localhost:8000
---




## Verifications

- **Network members:**
  ```bash
  docker network inspect voting-net --format='{{json .Containers}}' | jq .
  ```
- **Check logs:**
  ```bash
    docker logs -f web
    docker logs -f worker
  ```

---

## Cleanup

```bash
docker rm -f web api worker db redis
docker network rm voting-net
```

> If you published DB volume, remove it as needed (not created in this lab).

## Push images to docker hub

```bash
docker tag vote-worker <your dockerhub username>/vote-worker
docker tag vote-web <your dockerhub username>/vote-web
docker tag vote-result <your dockerhub username>/vote-result

docker push <your dockerhub username>/vote-worker
docker push <your dockerhub username>/vote-web
docker push <your dockerhub username>/vote-result
```