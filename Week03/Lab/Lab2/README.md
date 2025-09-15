# Week 3 – Lab 2: Docker Swarm (Stacks & Services)

## Objective
Deploy the Example Voting App on **Docker Swarm** using a Compose v3 spec with deployment settings.

## What You’ll Learn
- Initialize a Swarm and use **overlay networks**
- Deploy a **stack** with `docker stack deploy`
- Scale, inspect, and update services
- Perform rolling updates and clean up the stack

> You can complete this lab on a **single machine** (Swarm works with one manager), or across multiple nodes (advanced).

---

## Prerequisites
- Docker Engine / Docker Desktop
- Week 3 Lab 1 Compose file or images from Week 2
- If on Linux with multiple nodes: open TCP/UDP ports for Swarm (2377, 7946, 4789)

---

## 1) Initialize Swarm
```bash
docker swarm init
```
- To add workers (optional in multi-node): `docker swarm join ...` (command printed by init).

Create an **overlay network** for the stack:
```bash
docker network create -d overlay app-net
```

---

## 2) Create stack file
Create **stack.yml** (or reuse your compose with a `deploy:` section).

```yaml
version: "3.9"

services:
  redis:
    image: redis:7-alpine
    networks: [app-net]
    deploy:
      placement:
        constraints: [node.role == worker] # optional

  db:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: votes
    volumes:
      - dbdata:/var/lib/postgresql/data
    networks: [app-net]
    deploy:
      placement:
        constraints: [node.role == manager] # example constraint
  worker:
    image: voting-worker
    environment:
      REDIS_HOST: redis
      POSTGRES_HOST: db
      POSTGRES_DB: votes
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    networks: [app-net]
    deploy:
      replicas: 2
      restart_policy:
        condition: on-failure
  web:
    image: voting-web
    depends_on:
      - redis
    ports:
      - target: 8000
        published: 8000
        protocol: tcp
        mode: ingress
    networks: [app-net]
    deploy:
      replicas: 2
      update_config:
        parallelism: 1
        order: start-first
  result:
    image: magdysalem/voting-rersult
    depends_on:
      - redis
    ports:
      - target: 5000
        published: 5000
        protocol: tcp
        mode: ingress
    networks: [app-net]
    deploy:
      replicas: 2
      update_config:
        parallelism: 1
        order: start-first


volumes:
  dbdata:

networks:
  app-net:
    external: true
```

> In Swarm, `deploy:` keys (replicas, placement, update_config) are **honored**, unlike plain `docker compose up`.


## 4) Deploy the stack
```bash
docker stack deploy -c stack.yml voting
```
List stack services:
```bash
docker stack services voting
```
Check service tasks:
```bash
docker service ps voting_api
docker service ps voting_web
```

---

## 5) Scale and update
Scale workers:
```bash
docker service scale voting_worker=4
```
Rolling update the API image (after tagging a new version):
```bash
docker service update --image voting-api:v2 voting_api
```

---

## 6) Access the app
- Web (frontend): http://localhost:5000
- API (backend): http://localhost:8000

Swarm publishes ports on the **ingress** network; requests are load-balanced across tasks.

---

## 7) Inspect & Logs
```bash
docker service ls
docker service ps voting_api
docker service logs -f voting_api
docker stack ps voting
docker stack services voting
```

---

## 8) Cleanup
```bash
docker stack rm voting
docker network rm app-net
docker swarm leave --force
```
This removes the stack, overlay network, and leaves the swarm.
