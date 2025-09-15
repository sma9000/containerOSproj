# Week 1  Docker CLI & Container Fundamentals

This guide will walk you through the essential websites, tools, and downloads you'll use throughout the course.  

---


## 1️⃣ Docker
**Website:** [https://www.docker.com](https://docs.docker.com/)

### Key Points to Explore:
- What is Docker? (landing page overview)
- Docker Desktop for Windows, Mac, and Linux
- Docker Hub for images: [https://hub.docker.com](https://hub.docker.com)

### Download Links:
- [Download Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Create a free Docker Hub account](https://hub.docker.com/)

---

## 4️⃣ GitHub
**Website:** [https://github.com](https://github.com)

### Key Points to Explore:
- What is GitHub? (code hosting & collaboration)
- Explore an open-source project (e.g., [Kubernetes GitHub](https://github.com/kubernetes/kubernetes))
- Create account

---

## 5️⃣ Visual Studio Code (VS Code)
**Website:** [https://code.visualstudio.com](https://code.visualstudio.com)

### Key Points to Explore:
- Lightweight editor with built-in Git support
- Extensions Marketplace (e.g., Docker extension, Kubernetes extension)
- Integrated terminal

### Download Link:
- [Download VS Code](https://code.visualstudio.com/Download)



### Task 1: Docker Installation
Install Docker on your local system (Mac, Windows, or Linux)

### Task 2: Pull and Run a Container
```bash
docker pull nginx
docker run -d -p 8080:80 --name webserver nginx
docker ps
```

### Task 3: Explore Running Container
```bash
docker exec -it webserver bash
apt update && apt install curl -y
curl localhost
```

### Task 4: Container Lifecycle
```bash
docker stop webserver
docker start webserver
docker logs webserver
docker rm -f webserver
```

### Task 5: Inspect Images and System
```bash
docker images
docker inspect nginx
docker system df
```


### Task 6: Create a custom bridge network
```bash
docker docker network create nginx-net
```

### Task 7: Start two containers on the same network
```bash
docker pull redis
docker run -d --name redis --network nginx-net redis
docker run -d --name web --network nginx-net nginx
```

### Task 8: Inspect the network
```bash
docker network ls
docker network inspect nginx-net
```

### Task 9: Test container-to-container communication
```bash
docker run -it --rm --network nginx-net busybox
# Inside container shell:
ping redis
```

### Task 10: Clean Up
```bash
docker container prune
docker image prune -a
docker network prune

```

## Submission
- Screenshots of CLI showing container running
- Screenshot of curl response from within container