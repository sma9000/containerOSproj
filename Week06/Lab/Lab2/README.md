# Week 6 – Lab 2: Kubernetes Networking in Practice


In this lab you will:
- Refactor the all-in-one stack into **separate Deployments** (web, worker, redis, postgres)
- Create **ClusterIP Services** for stable Service-to-Pod communication
- Validate **DNS** with CoreDNS using a test pod
- Apply **NetworkPolicies** to restrict traffic and then allow only the required flows

> Cluster type: Minikube or Kind from Week 4. The default CNI is sufficient for this lab (NetworkPolicies require a policy-capable CNI such as Calico/Cilium. Minikube with `--cni=calico` or Kind+Calico recommended for the NetworkPolicy section).

---

## 1) Apply Deployments and Services

```bash
kubectl apply -f k8s-deployments.yaml
kubectl apply -f k8s-services.yaml

kubectl get deploy,po,svc -o wide
```

**What you created**
- Deployments:
  - `vote-web` (frontend) – container port 80
  - `vote-result` (frontend) – container port 80
  - `vote-worker` (background) – no port
  - `vote-redis` – port 6379
  - `vote-postgres` – port 5432 with starter DB env
- Services (ClusterIP):
  - `web-svc` -> pods with `app=vote-web` on port 80
  - `result-svc` -> pods with `app=vote-result` on port 80
  - `redis-svc` -> `app=vote-redis` on 6379
  - `postgres-svc` -> `app=vote-postgres` on 5432

---

## 2) Validate DNS and Service-to-Pod

Run a **debug pod** with `nslookup`/`curl` tools:
```bash
kubectl apply -f tools-dnsutils.yaml
kubectl get pod dnsutils
kubectl exec -it dnsutils -- sh
```

Inside the shell:
```sh
nslookup web-svc
nslookup redis-svc
nslookup postgres-svc

# Optional: if curl is present (or use wget/busybox):
wget -qO- http://web-svc
# You should at least see an HTTP response from the frontend.
exit
```

> CoreDNS resolves `<service>.<namespace>.svc.cluster.local`. Short names (like `redis-svc`) also work inside the same namespace due to search domains.

---

## 3) (Optional) Port-forward for a quick local check
```bash
kubectl port-forward svc/web-svc 5000:80
```
Open: http://localhost:5000

> We are not adding Ingress yet—this is just for quick verification.

---

## 4) Network Policies
> If your cluster CNI **does not** enforce NetworkPolicies, you can still apply them, but they will have **no effect**. Use Minikube with a policy-capable CNI (e.g., `minikube start --cni=calico`).

### 4.1 Default Deny Ingress
```bash
kubectl apply -f netpol-default-deny.yaml
```
- This denies **all incoming traffic** to Pods in the namespace unless explicitly allowed.

### 4.2 Allow Required App Flows
Now allow the app’s minimal flows:
- `vote-web` -> `vote-redis` (frontend pushes votes into Redis)
- `vote-worker` -> `vote-redis` (worker reads votes)
- `vote-worker` -> `vote-postgres` (worker writes results)

```bash
kubectl apply -f netpol-allow-app.yaml
```

Validate quickly:
```bash
# From dnsutils pod, test name resolution still works
kubectl exec -it dnsutils -- sh -c 'nslookup redis-svc && nslookup postgres-svc'

# Optional: run a temporary busybox in the same namespace and try to reach redis/postgres:
kubectl run tmp --rm -it --image=busybox:1.36 --restart=Never -- sh -c "wget -qO- redis-svc:6379 || true"
```
> Non-allowed Pod-to-Pod connections should fail once policies are active.

---

## 5) Clean Up
```bash
kubectl delete -f netpol-allow-app.yaml
kubectl delete -f netpol-default-deny.yaml
kubectl delete -f tools-dnsutils.yaml
kubectl delete -f k8s-services.yaml
kubectl delete -f k8s-deployments.yaml
```

---
