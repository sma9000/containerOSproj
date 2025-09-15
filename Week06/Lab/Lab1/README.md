# Week 6 – Lab 1: Services (Wire the All‑in‑One Deployment)

In Week 5 (Lab 2) we covered creating **Deployments** files to deploy Pods  (web, result, worker, redis, postgres).  
In this lab, you will **add Kubernetes Services** so components can discover each other by stable DNS names.


---

## Objectives
- Add **ClusterIP Services** for web, sevice, worker, redis, and postgres
- Verify DNS service discovery inside the cluster
- Use **kubectl port-forward** to reach the web Service from your laptop (we still avoid NodePort/Ingress for now)

---

## Files
Deployment files and Services under folder `Assest`

---
## Update Deployment file
Review deployment files and update `image` section with your own docker hub vote image reference.

---
## Create  `worker-service.yaml` file
Review services files and create one for worker service.

---

## 1) Apply the manifests
```bash
kubectl apply -f redis-deployment.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f vote-deployment.yaml
kubectl apply -f worker-deployment.yaml
kubectl apply -f result-deployment.yaml


kubectl apply -f redis-service.yaml
kubectl apply -f postgres-service.yaml
kubectl apply -f db-service.yaml
kubectl apply -f vote-service.yaml
kubectl apply -f worker-service.yaml
kubectl apply -f result-service.yaml

kubectl get deploy,svc,pods -o wide
```

Expected Services:
```
NAME            TYPE        CLUSTER-IP     PORT(S)
vote-svc        ClusterIP   <cluster-ip>   80/TCP
redis-svc       ClusterIP   <cluster-ip>   6379/TCP
postgres-svc    ClusterIP   <cluster-ip>   5432/TCP
```

---

## 2) Verify DNS and connectivity inside the Pod

1) Exec into any Pod replica and test DNS:
```bash
kubectl get pods 
kubectl logs <worker pod id>
kubectl logs <result pod id>
```

---

## 3) Access the web through the Service (port-forward)
```bash
kubectl port-forward svc/vote-svc 5000:80
```
Open: http://localhost:5000

> We still avoid NodePort/Ingress in Week 6. Port-forward is sufficient for testing.

---

## 4) Cleanup
Delete all deployment and service
---
