# Week 4 – K8 Tooling & Ecosystem Walkthrough

This guide will walk you through the essential websites, tools, and downloads you'll use throughout the course.  

---


## 3️⃣ Minikube & Kind (K8s Local Tools)
**Minikube:** [https://minikube.sigs.k8s.io](https://minikube.sigs.k8s.io)  
**Kind (Kubernetes in Docker):** [https://kind.sigs.k8s.io](https://kind.sigs.k8s.io)

### Key Points to Explore:
- Browse docs and learn difference: Minikube (VM-based) vs Kind (Docker-based)
- Installation guides:
  - [Install Minikube](https://minikube.sigs.k8s.io/docs/start/)
  - [Install Kind](https://kind.sigs.k8s.io/docs/user/quick-start/)

---



## Installation Commands (Optional)

### Windows (PowerShell)
```powershell
choco install minikube -y
```

### macOS (Homebrew)
```bash
brew install minikube kind
```

### Linux (Debian/Ubuntu)
```bash
sudo apt-get update
curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube
```

---
# Part 2: Start local cluster
```bash
   minikube start
```



## Part 3: Explore the Cluster with kubectl

1. Verify the cluster connection:

   ```bash
   kubectl cluster-info
   ```

2. List all nodes:

   ```bash
   kubectl get nodes
   ```

3. List system pods:

   ```bash
   kubectl get pods -A
   ```

4. Get detailed info about a node:

   ```bash
   kubectl describe node <node-name>
   ```

---
