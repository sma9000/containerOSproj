# Week 7 – Lab 1: ConfigMaps with the Example Voting App

In this lab, you will introduce **ConfigMaps** to externalize non‑secret configuration for the Example Voting App.
You’ll create a `ConfigMap` with app settings and wire it into the **web (vote)** and **worker** Deployments using both **environment variables** and a **mounted config file**.

> Keep secrets (passwords/tokens) out of ConfigMaps. We will use **Secrets** in a later week.

---

## Objectives
- Create a **ConfigMap** for app configuration
- Consume ConfigMap values in Deployments via **env** and **volumes**
- Verify the pods receive the configuration and observe changes after updates

---

## Prerequisites
- A running K8s cluster (Minikube/Kind)
- From Week 6 Lab 2 (Networking), you can reuse:
  - `vote-web` and `vote-worker` Deployments
  - `redis-svc` and `postgres-svc` ClusterIP Services
- `kubectl` configured

---

## Files in this lab
- `configmap-app.yaml` – ConfigMap with web/worker settings
- `deployments-with-configmap.yaml` – Updated Deployments that read from the ConfigMap
- `config-template/appsettings.yaml` – Optional application config file to show file‑based mount

---

## 1) Create the ConfigMap
Apply the ConfigMap:
```bash
kubectl apply -f configmap-app.yaml
kubectl get configmap app-config -o yaml
```

**What’s inside**
- `APP_TITLE`: text to show on the web frontend (demonstration variable)
- `REDIS_HOST`: logical DNS name of the Redis Service (`redis-svc`)
- `POSTGRES_HOST`: logical DNS name of the Postgres Service (`postgres-svc`)
- `POSTGRES_DB`: name of the database (non‑secret)
- `FEATURE_FLAGS`: demo value to show how to inject comma-separated flags
- A small YAML file (via `config-template/appsettings.yaml`) to demonstrate **volume mounts**

---

## 2) Use ConfigMap in Deployments
Apply the updated Deployments:
```bash
kubectl apply -f deployments-with-configmap.yaml
kubectl rollout status deployment/vote-web
kubectl rollout status deployment/vote-worker
```

### How it’s wired
- **Environment variables** (env / envFrom) for web & worker
- **Volume mount** that writes `appsettings.yaml` into the containers at `/config/appsettings.yaml`

Verify:
```bash
kubectl get deploy,po
POD=$(kubectl get pods -l app=vote-web -o jsonpath='{.items[0].metadata.name}')
kubectl exec -it $POD -- sh -c 'env | grep -E "APP_TITLE|REDIS_HOST|POSTGRES_HOST|POSTGRES_DB|FEATURE_FLAGS" || true; ls -l /config; cat /config/appsettings.yaml || true'
```

---

## 3) Update the ConfigMap and observe changes
Edit `configmap-app.yaml` (e.g., change `APP_TITLE`) and re-apply:
```bash
kubectl apply -f configmap-app.yaml
```
> Note: Environment variables do **not** update inside a running container automatically. The app sees new env values only after a **Pod restart/rollout**. File mounts from ConfigMaps update on the node (with a short delay), but many apps read config only at startup.

Trigger a rolling restart for web to pick env changes:
```bash
kubectl rollout restart deployment/vote-web
kubectl get pods -l app=vote-web
```

---

## 4) Cleanup
```bash
kubectl delete -f deployments-with-configmap.yaml
kubectl delete -f configmap-app.yaml
```

---

## Discussion
- Use **ConfigMaps** for non‑secret configuration; use **Secrets** for passwords/tokens.
- Env vs volume:
  - **Env** is easy, but requires Pod restart to pick changes.
  - **Mounted files** update on the node; app must re-read to benefit.
- Config can be namespaced per environment (dev/stage/prod) with separate ConfigMaps.
