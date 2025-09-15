# Week 14 – Lab 1: CI/CD with GitHub Actions (Basics)

This lab introduces **CI/CD concepts** and a simple **GitHub Actions workflow** for building and deploying a containerized app to Kubernetes.

---

## Objectives
- Understand **What is CI/CD**
- Learn why **GitHub Actions** is useful for DevOps
- Explore **Workflow basics**: triggers, jobs, runners
- Configure a **simple CI/CD pipeline**:
  1. Build and push Docker image
  2. Deploy to Kubernetes using `kubectl`

---

## 1) What is CI/CD?
- **Continuous Integration (CI)**: Automatically build, test, and integrate changes to detect errors quickly.
- **Continuous Delivery/Deployment (CD)**: Automatically deliver or deploy changes to production-like environments.

**Benefits:**
- Faster feedback and delivery cycles
- Reduced manual errors
- Standardized release process

---

## 2) Why GitHub Actions?
- Native CI/CD integrated with GitHub repositories
- Supports **event-driven workflows**
- Marketplace of pre-built actions
- Runners support Linux, Windows, macOS, and self-hosted

---

## 3) Workflow Basics
- **Workflow**: `.github/workflows/<name>.yml`
- **Event triggers**: push, pull_request, schedule, workflow_dispatch
- **Job**: Collection of steps that run on a runner
- **Step**: Individual task in a job

---

## 4) Prepare Repository

1. Create or clone your GitHub repository:
```bash
git clone https://github.com/<your-org>/voting-app.git
cd voting-app
```

2. Add a `.github/workflows` directory:
```bash
mkdir -p .github/workflows
```

---

## 5) Create Simple Build and Deploy Workflow

Create `.github/workflows/build-deploy.yml`:

```yaml
name: Build and Deploy Voting App

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Log in to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}

      - name: Build Docker image
        run: docker build -t ${{ secrets.DOCKER_USERNAME }}/vote-web:${{ github.sha }} ./vote

      - name: Push Docker image
        run: docker push ${{ secrets.DOCKER_USERNAME }}/vote-web:${{ github.sha }}

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up kubectl
        uses: azure/setup-kubectl@v3
        with:
          version: v1.30.0

      - name: Configure Kubeconfig
        run: |
          mkdir -p $HOME/.kube
          echo "${{ secrets.KUBECONFIG_CONTENTS }}" > $HOME/.kube/config

      - name: Deploy to Kubernetes
        run: |
          kubectl set image deployment/vote-web vote-web=${{ secrets.DOCKER_USERNAME }}/vote-web:${{ github.sha }} -n vote
          kubectl rollout status deployment/vote-web -n vote
```

---

## 6) Secrets Required
- `DOCKER_USERNAME` / `DOCKER_PASSWORD` – Docker Hub credentials
- `KUBECONFIG_CONTENTS` – base64 or raw kubeconfig for cluster access

Add in **GitHub → Repository Settings → Secrets and variables → Actions → New repository secret**.

---

## 7) Test Your Pipeline
1. Commit and push changes to the `main` branch:
```bash
git add .
git commit -m "Add basic CI/CD workflow"
git push origin main
```
2. Check GitHub → Actions tab → see workflow run
3. Verify:
```bash
kubectl get deploy -n vote
kubectl get pods -n vote
```

---

## Cleanup
- Delete workflow if no longer needed
- Revoke temporary kubeconfig credentials if shared

---

## Discussion
- **Jobs** run in parallel by default; `needs:` defines dependencies.
- **Secrets** securely store credentials.
- **workflow_dispatch** lets you manually trigger a run.
- This pattern builds the foundation for **advanced CI/CD** with testing, Helm, and multi-env deploys.
