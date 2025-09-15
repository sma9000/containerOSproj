# Week 10 – Lab 2: Advanced Ingress Routing (NGINX Ingress)

In this lab you’ll practice **advanced HTTP routing** with the NGINX Ingress Controller:
- **Host-based** routing (multiple hosts to different backends)
- **Path-based** routing (e.g., `/` → web, `/api` → api)
- **URL rewrite** and **regex paths**
- **TLS termination** with a Kubernetes TLS Secret
- **Canary routing** (traffic splitting) using NGINX annotations
- **Rate limiting** with NGINX annotations

> Prerequisite: Week 10 Lab 1 (Ingress Controller installed) and your `web-svc`.  
> Optional: If you don’t have an `api-svc`, this lab includes a lightweight demo API service.

---

## Files in this Lab
- `ingress-advanced.yaml` – host-based & path-based routing + rewrites/regex + rate limit
- `tls-secret.yaml` – template for creating a TLS secret for `vote.local`
- `ingress-tls.yaml` – TLS-enabled ingress
- `api-demo.yaml` – (optional) a tiny API Deployment/Service for `/api` testing
- `canary-web-v2.yaml` – (optional) a second web Deployment/Service to receive canary traffic
- `ingress-canary.yaml` – NGINX **canary** Ingress to shift a % of traffic to `web-v2-svc`

---

## 0) Prepare DNS / hosts file
Add entries in your **hosts** file pointing to your Ingress Controller IP (Minikube users can use `minikube ip`):

```
<INGRESS-IP> vote.local
<INGRESS-IP> api.local
```

---

## 1) (Optional) Deploy a demo API service
Skip if you already have `api-svc`.

```bash
kubectl apply -f api-demo.yaml
kubectl get svc api-svc
```

---

## 2) Advanced routing without TLS
Apply:
```bash
kubectl apply -f ingress-advanced.yaml
kubectl get ingress
```
--> Note: In case of minikube, please use port-forward
```bash
 kubectl -n ingress-nginx port-forward svc/ingress-nginx-controller 8080:80
```

**Test**
```bash
curl -H "Host: vote.local" http://<INGRESS-IP>/
curl -H "Host: vote.local" http://<INGRESS-IP>/api/status
curl -H "Host: api.local"  http://<INGRESS-IP>/v1/echo/hello
```

Notes:
- `vote.local` host routes `/` to `web-svc:80`
- `vote.local` host routes `/api` to `api-svc:80` (pathPrefix `/api` kept as-is)
- `api.local` host uses **regex** path `^/v1/(.*)` and rewrites to `/$1` on `api-svc`
- Requests to `vote.local` are **rate limited** via annotations (RPS)

---

## 3) TLS termination
1) Create a self-signed cert (example dev-only):
```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048   -keyout vote.local.key -out vote.local.crt -subj "/CN=vote.local/O=dev"
kubectl create secret tls vote-local-tls --cert=vote.local.crt --key=vote.local.key
```

## Delete existing votingapp-simple
Before continue make sure to delete existing votingapp-simple

2) Apply TLS ingress:
```bash
kubectl apply -f ingress-tls.yaml
```

--> Note: In case of minikube, please use port-forward 
```bash
kubectl -n ingress-nginx port-forward svc/ingress-nginx-controller 4433:443
```
The following command will fail
Test:
```bash
curl  -H "Host: vote.local" https://<INGRESS-IP>/
```

This command will pass
```bash
curl -k -H "Host: vote.local" https://<INGRESS-IP>/
```

---

## 4) Canary routing (optional)
1) Deploy a **v2** version of the web (can be the same image; just for visual difference you could change a ConfigMap/env):
```bash
kubectl apply -f canary-web-v2.yaml
kubectl get svc web-v2-svc
```

2) Apply **canary ingress** (20% traffic to v2):
```bash
kubectl apply -f ingress-canary.yaml
```

Test many times:
```bash
for i in {1..20}; do curl -s -H "Host: vote.local" http://<INGRESS-IP>/ | head -n1; done
```

```ps
1..20 | ForEach-Object {curl.exe -s -H "Host: vote.local" http://<INGRESS-IP>/ | Select-Object -First 1}
```
You should see responses from both `web-svc` and `web-v2-svc` over multiple attempts.

---

## 5) Cleanup
```bash
kubectl delete -f ingress-canary.yaml
kubectl delete -f canary-web-v2.yaml
kubectl delete -f ingress-tls.yaml
kubectl delete -f tls-secret.yaml   # only if you created it via yaml
kubectl delete -f ingress-advanced.yaml
kubectl delete -f api-demo.yaml
```

---

## Troubleshooting
- Check Ingress Controller logs:
  ```bash
  kubectl logs -n ingress-nginx deploy/ingress-nginx-controller
  ```
- Ensure Services exist and ports match your backends.
- On Kind, you may need to expose NodePorts or port-forward the ingress controller service.

---

## Discussion
- Ingress allows **layer-7** routing; Services are layer-4.
- NGINX annotations provide advanced behaviors (rewrites, rate limits, canary).
- TLS can be terminated at the Ingress to offload cert handling.
