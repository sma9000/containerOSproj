# Week 5 – Lab 1: Pods for Web, Worker, Redis, and PostgreSQL (Example Voting App)

## Objective
Create **four standalone Pods** (no Services yet) for the Example Voting App stack:
- Web (vote frontend)
- Worker (background processor)
- Redis (queue/cache)
- PostgreSQL (database)

> This lab focuses on **Pod basics only**. Connectivity between components will be addressed later when you learn Services.

---

## Prerequisites
- A running Kubernetes cluster (Minikube or Kind from Week 4)
- `kubectl` installed and configured

---

## Files in this Lab
- `pod-vote.yaml` — Web frontend Pod (containerPort 80)
- `pod-worker.yaml` — Worker Pod (no exposed port)
- `pod-redis.yaml` — Redis Pod (containerPort 6379)
- `pod-postgres.yaml` — PostgreSQL Pod (containerPort 5432, env vars for DB)
---

## 1) Build Result Pod file
Create `pod-result.yaml`  based on `pod-worker.yaml` or `pod-vote.yaml`

## 2) Apply the Pods

```bash
kubectl apply -f pod-redis.yaml
kubectl apply -f pod-postgres.yaml
```

## 3) Capture POD Information
Capture and save `redis` pod IP address

```bash
kubectl describe pod redis-pod

```
Repeat step for `postgres` pod

## 4) Update Vote, Result and Worker Pod files

Edit Vote, Result and Worker Pod files and update it with IPs information from step #3

## 5) Apply vote, Result and Worker Pods

```bash
kubectl apply -f pod-result.yaml
kubectl apply -f pod-vote.yaml
kubectl apply -f pod-worker.yaml
```


Check status:
```bash
kubectl get pods -o wide
```

Describe one pod (example):
```bash
kubectl describe pod vote-pod
```

View logs (examples):
```bash
kubectl logs vote-pod
kubectl logs result-pod
kubectl logs worker-pod
kubectl logs redis-pod
kubectl logs postgres-pod
kubectl logs result-pod
```

---

## 6) Optional: Quick Local Tests

Since we haven't created Services yet, pods are **not exposed** and don't have stable DNS names. You can still test some behavior:

### A) Port-forward the web frontend
```bash
kubectl port-forward pod/vote-pod 5000:80
```
Open http://localhost:5000 (the app may fail to fully function until Services are added later, but you can see the container is running).

### B) Check Redis is up
```bash
kubectl exec -it redis-pod -- redis-cli PING
```
Expected output: `PONG`

### C) Check PostgreSQL is up
```bash
kubectl exec -it postgres-pod -- psql -U postgres -c "\l"
```
You should see a list of databases including `votes`.

> Note: The worker will try to reach Redis/DB via environment variables in later labs. For now, it will just run and log its startup; connectivity comes after we introduce Services/Env/Wiring.

---

## 7) Cleanup

```bash
kubectl delete -f pod-vote.yaml
kubectl delete -f pod-result.yaml
kubectl delete -f pod-worker.yaml
kubectl delete -f pod-redis.yaml
kubectl delete -f pod-postgres.yaml
```

---

