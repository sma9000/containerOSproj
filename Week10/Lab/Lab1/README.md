# Week 10 – Lab 1: Ingress Controller (Nginx)

In this lab you will install an **Ingress Controller** (NGINX) and use it to route external HTTP traffic to the Voting App services.

---

## Objectives
- Understand what an **Ingress Controller** is and why it’s needed
- Deploy NGINX Ingress Controller on your cluster
- Create a simple Ingress resource to expose the Voting App frontend

---

## Prerequisites
- A running cluster (Minikube recommended or Kind)
- Existing Services (`web-svc`, `redis-svc`, `postgres-svc`) from previous labs

---

## 1) Install NGINX Ingress Controller

### Minikube
```bash
minikube addons enable ingress
kubectl get pods -n ingress-nginx | grep ingress
```

### Generic (YAML)
```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.11.1/deploy/static/provider/cloud/deploy.yaml
kubectl get pods -n ingress-nginx
```

---

## 2) Create Ingress Resource

Apply:
```bash
kubectl apply -f votingapp-ingress.yaml
kubectl get ingress
```

Check the `ADDRESS` column and test:
```bash
curl http://<INGRESS-IP>/
```

--> in case of Minikube: ingress IP is internal and will need port forward to access
```bash
kubectl -n ingress-nginx port-forward svc/ingress-nginx-controller 8080:80

curl http://localhost:8000/
```

---

## 3) Test Access

- If using **Minikube**, use:
```bash
minikube ip
curl http://<minikube-ip>/
```


--> in case of Minikube: ingress IP is internal and will need port forward to access
```bash
kubectl -n ingress-nginx port-forward svc/ingress-nginx-controller 8080:80

curl http://localhost:8000/
```
---

## Cleanup
```bash
kubectl delete -f votingapp-ingress.yaml
# Optional: disable Minikube ingress addon or delete ingress-nginx namespace
```

---

## Discussion
- Ingress Controller watches **Ingress** resources and configures a reverse proxy (NGINX) dynamically.
- Can route by **hostnames**, **paths**, or TLS termination.
- Replaces the need for NodePort or LoadBalancer on each Service.
