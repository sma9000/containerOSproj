# Week 12 – Lab 1: Pod Autoscaling (HPA)

This lab demonstrates **Horizontal Pod Autoscaling (HPA)** using CPU utilization for the Voting App **web** component.

---

## Objectives
- Configure resource **requests/limits** (required for CPU-based HPA)
- Create an **HPA** for `vote-web`
- Generate load and watch the HPA scale up/down
- Understand caveats (metrics server, cooldowns, stabilization)

---

## Prerequisites
- Cluster from Week 4+ (Minikube or Kind)
- **metrics-server** installed:
  - Minikube: `minikube addons enable metrics-server`
  - Generic: `kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml`
- Working `web-svc` Service (from Week 6)

---

## Files
- `deployment-vote-web-with-resources.yaml` – `vote-web` Deployment + `web-svc` with CPU/memory requests & limits
- `hpa-vote-web.yaml` – HPA targeting 60% average CPU utilization
- `loadgen.yaml` – simple HTTP load-generator to hit `web-svc`

---

## 1) Deploy the app with resources
```bash
kubectl apply -f deployment-vote-web-with-resources.yaml
kubectl rollout status deploy/vote-web
kubectl get po,svc
```

Verify metrics are available:
```bash
kubectl top pods
kubectl top nodes
```

---

## 2) Create the HPA
```bash
kubectl apply -f hpa-vote-web.yaml
kubectl get hpa
```

The HPA targets 60% CPU utilization of the **requested** CPU across replicas.

---

## 3) Generate load
```bash
kubectl apply -f loadgen.yaml
kubectl get pods -l app=loadgen -w
```

The loadgen Pod (busybox) continuously curls the web service to increase CPU usage.

Watch HPA decisions and replica counts:
```bash
kubectl get hpa -w
kubectl get deploy vote-web -w
```

Scale-out may take ~1–2 minutes due to metrics windows and stabilization.

---

## 4) Reduce/stop load and observe scale-down
```bash
kubectl delete -f loadgen.yaml
# Observe HPA scaling back down after stabilization window
kubectl get deploy vote-web -w
```

---

## 5) Cleanup
```bash
kubectl delete -f hpa-vote-web.yaml
kubectl delete -f deployment-vote-web-with-resources.yaml
```

---

## Notes & Tips
- HPA CPU % = `current CPU usage / requested CPU`. Choose sensible **requests**.
- Ensure metrics-server is **Healthy**; otherwise HPA shows `unknown`.
- You can target **memory** or **custom/external** metrics with v2 APIs.
- To demonstrate more aggressive scaling, lower `targetCPUUtilizationPercentage` or `stabilizationWindowSeconds`.
