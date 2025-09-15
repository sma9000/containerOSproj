# Week 9 – Lab 1: Probes & Health Checks (Voting App)

In this lab, you’ll add **startup**, **readiness**, and **liveness** probes to the Voting App components. You’ll observe how probes affect scheduling and restarts, and learn how to debug failing probes.

We will:
- Use **HTTP** probes for the web frontend
- Use **TCP** probes for Redis and Postgres
- (Optional) Use a simple **exec** probe pattern for the worker

> Remember:  
> - **startupProbe** gates liveness/readiness until the app has booted.  
> - **readinessProbe** controls if a Pod gets traffic.  
> - **livenessProbe** restarts a container if it becomes unhealthy.

---

## Objectives
- Add probes to multiple components
- Verify readiness gates traffic while liveness restarts unhealthy containers
- Practice debugging probe failures

---

## Prerequisites
- Working cluster (Minikube/Kind)
- From Week 6: Services `web-svc`, `redis-svc`, `postgres-svc`
- You can reuse Deployments from Week 6 Lab 2 or apply the manifests provided here

---

## Files in this Lab
- `deployment-web-with-probes.yaml` – `vote-web` with **startup**, **readiness**, and **liveness** HTTP probes  
- `deployment-redis-with-probes.yaml` – `vote-redis` with **liveness/readiness** **TCP** probes  
- `deployment-postgres-with-probes.yaml` – `vote-postgres` with **liveness/readiness** **TCP** probes  
- `deployment-worker-with-probes.yaml` – optional `vote-worker` with a trivial **exec** readiness probe

---
## Create Result deployment
Based on `worker` and `web` deployment file. Create `result` deployment with probes.
---
## 1) Apply the manifests
```bash
kubectl apply -f deployment-web-with-probes.yaml
kubectl apply -f deployment-redis-with-probes.yaml
kubectl apply -f deployment-postgres-with-probes.yaml
kubectl apply -f deployment-worker-with-probes.yaml   # optional
```

Check status:
```bash
kubectl get deploy,po
kubectl describe deploy vote-web
kubectl describe pod -l app=vote-web
```

---

## 2) Verify probes

### Web (HTTP)
```bash
POD=$(kubectl get pods -l app=vote-web -o jsonpath='{.items[0].metadata.name}')
kubectl describe pod $POD | sed -n '/Containers:/,$p' | sed -n '1,80p'
kubectl logs $POD -c web --tail=50
```

Readiness should go **True** once `/` returns HTTP 200 on port 80. Liveness will keep checking periodically.

### Redis & Postgres (TCP)
```bash
kubectl describe pod -l app=vote-redis | sed -n '/Containers:/,$p' | sed -n '1,80p'
kubectl describe pod -l app=vote-postgres | sed -n '/Containers:/,$p' | sed -n '1,80p'
```

### Worker (exec readiness)
```bash
kubectl describe pod -l app=vote-worker | sed -n '/Containers:/,$p' | sed -n '1,80p'
```

---

## 3) Simulate a failure (optional)

### A) Web liveness failure
Port-forward the web and send a request loop, then **temporarily** change liveness path to a non-existent endpoint to watch restarts:
```bash
kubectl patch deployment vote-web --type=json -p='[
  {"op":"replace","path":"/spec/template/spec/containers/0/livenessProbe/httpGet/path","value":"/badpath"}
]'
kubectl get pods -l app=vote-web -w
```

Observe `RESTARTS` increase in `kubectl get pods`. Restore the path:
```bash
kubectl rollout undo deployment vote-web
```

### B) Redis readiness failure
Lower `initialDelaySeconds` for readiness to 0 and `periodSeconds` to 2, then restart the pod to show flapping readiness:
```bash
kubectl patch deployment vote-redis --type=json -p='[
  {"op":"replace","path":"/spec/template/spec/containers/0/readinessProbe/initialDelaySeconds","value":0},
  {"op":"replace","path":"/spec/template/spec/containers/0/readinessProbe/periodSeconds","value":2}
]'
kubectl rollout restart deployment vote-redis
kubectl get pods -l app=vote-redis -w
```

Restore defaults with:
```bash
kubectl rollout undo deployment vote-redis
```

---

## 4) Debugging checklist
- `kubectl describe pod <pod>` (Events show probe failures)
- `kubectl logs <pod> -c <container>`
- Port-forward to hit the same HTTP path locally:
  ```bash
  kubectl port-forward deploy/vote-web 5000:80
  curl -i http://localhost:5000/
  ```
- If using exec probes, ensure the command exists in the container image

---

## 5) Cleanup
```bash
kubectl delete -f deployment-web-with-probes.yaml
kubectl delete -f deployment-redis-with-probes.yaml
kubectl delete -f deployment-postgres-with-probes.yaml
kubectl delete -f deployment-worker-with-probes.yaml
```

---

## Notes
- Don’t make probes too aggressive; allow for startup times.
- Prefer **startupProbe** for slow-boot apps to avoid premature liveness kills.
- Readiness flapping can cause intermittent traffic blackholes.
