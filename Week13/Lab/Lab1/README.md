# Week 13 – Lab 1: Helm Basics

In this lab, you will learn to package, deploy, and manage Kubernetes applications with **Helm**.

---

## Objectives
- Understand **What is Helm** and why it's useful
- Explore **Helm architecture** (CLI, Tiller [Helm v2], client-only in Helm v3)
- Learn **Chart structure** and templating basics
- Install and use **Helm** to deploy charts
- Explore **values.yaml**, **overrides**, and chart repositories

---

## 1) Install Helm

- **Linux/macOS**
```bash
curl -fsSL https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash
helm version
```

- **Windows (PowerShell)**
```powershell
choco install kubernetes-helm
helm version
```

---

## (Option) Clean up
Just in case if there is artifact left from security lab.
```bash

helm uninstall my-nginx -n ingress-nginx
helm uninstall ingress-nginx -n ingress-nginx
kubectl delete namespace ingress-nginx
kubectl delete ingressclass nginx

kubectl delete validatingwebhookconfiguration gatekeeper-validating-webhook-configuration
kubectl delete mutatingwebhookconfiguration gatekeeper-mutating-webhook-configuration
kubectl delete ns gatekeeper-system
```


## 2) Add a Chart Repository

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm search repo nginx
```

---

## 3) Deploy Your First Chart (NGINX Example)

```bash
helm install my-nginx ingress-nginx/ingress-nginx 
kubectl get all
```

--> Note: You may have to refresh few time till all pod,svc and controller up and running

Observe:
- Deployment
- Service
- ConfigMap/Secret if included

Check the release status:
```bash
helm list
helm status my-nginx
```

---

## 4) Inspect and Override Values

Fetch the chart locally and inspect:
```bash
helm pull ingress-nginx/ingress-nginx  --untar
ls ingress-nginx
cat nginx/values.yaml
```

Install with custom values:
-->Due to limitation of minikube will need to delete existing helm first to avoid conflict with existing `IngressClass` resource
```bash
helm install my-nginx3 ingress-nginx/ingress-nginx -f values-custom.yaml
kubectl get all
```

---

## 5) Uninstall a Release

```bash
helm uninstall my-nginx
helm list
```

---

