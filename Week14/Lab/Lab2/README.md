# Week 14 – Lab 2: Advanced GitHub Actions for CI/CD to Kubernetes

This lab extends the **Week 14 Lab 1** basics with **advanced GitHub Actions concepts**:

- **Matrix Builds & Parallelism**
- **Secrets & Environments**
- **Reusable Workflows**
- **Caching Strategies**
- **Deployment Strategies (Blue/Green, Rolling)**
- **GitHub Actions for Kubernetes (kubectl & Helm)**

---

## 1) Matrix Builds & Parallelism

Example: Build multiple images for different components (`web` and `worker`) **in parallel**:

```yaml
name: Matrix Build Voting App

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        component: [web, worker]
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}
      - name: Build and push ${{ matrix.component }}
        run: |
          docker build -t ${{ secrets.DOCKER_USERNAME }}/vote-${{ matrix.component }}:${{ github.sha }} ./vote-${{ matrix.component }}
          docker push ${{ secrets.DOCKER_USERNAME }}/vote-${{ matrix.component }}:${{ github.sha }}
```

- `matrix.component` iterates over `[web, worker]` for parallel builds.

---

## 2) Secrets & Environments

GitHub **Environments** allow per-env secrets and deployment protection rules.

- Add environments: `dev`, `staging`, `prod`
- Configure secrets like `KUBECONFIG_CONTENTS` per environment

Example job using environment secrets:

```yaml
deploy:
  runs-on: ubuntu-latest
  environment: staging
  steps:
    - uses: actions/checkout@v4
    - uses: azure/setup-kubectl@v3
    - run: |
        mkdir -p ~/.kube
        echo "${{ secrets.KUBECONFIG_CONTENTS }}" > ~/.kube/config
    - run: kubectl rollout status deploy/vote-web -n vote
```

---

## 3) Reusable Workflows

Create `.github/workflows/deploy-template.yml`:

```yaml
name: Reusable Deploy Workflow
on:
  workflow_call:
    inputs:
      namespace:
        required: true
        type: string
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: azure/setup-kubectl@v3
      - run: |
          mkdir -p ~/.kube
          echo "${{ secrets.KUBECONFIG_CONTENTS }}" > ~/.kube/config
          kubectl set image deployment/vote-web vote-web=${{ secrets.DOCKER_USERNAME }}/vote-web:${{ github.sha }} -n ${{ inputs.namespace }}
          kubectl rollout status deploy/vote-web -n ${{ inputs.namespace }}
```

Call it from another workflow:

```yaml
jobs:
  call-deploy:
    uses: ./.github/workflows/deploy-template.yml
    with:
      namespace: vote
```

---

## 4) Caching Strategies

Use GitHub **cache** to speed up builds (Node.js example):

```yaml
- name: Cache npm
  uses: actions/cache@v3
  with:
    path: ~/.npm
    key: ${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}
    restore-keys: ${{ runner.os }}-npm-
```

For Docker builds, leverage **buildx caching** with `docker/build-push-action`.

---

## 5) Deployment Strategies

### Rolling Update (default in K8s)

```bash
kubectl set image deployment/vote-web vote-web=<newimage>
kubectl rollout status deployment/vote-web
```

### Blue/Green or Canary

- Deploy new version to `vote-web-v2`
- Switch Service selector or Ingress to route traffic gradually
- Use Helm values or separate namespace for full blue/green

---

## 6) Combined Advanced Workflow Example

See `.github/workflows/advanced-build-deploy.yml` for:
- Matrix builds
- Docker caching
- Environment-specific deploy
- Helm upgrade for voting app

---

## Cleanup

- Remove test workflows
- Rotate credentials if used for demo
- Delete temporary namespaces

---

## Discussion
- Matrix builds maximize parallelism
- Environments protect production
- Reusable workflows reduce duplication
- Caching reduces CI/CD runtime
- Helm + kubectl combine well for multi-env deploys
