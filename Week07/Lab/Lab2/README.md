# Week 7 – Lab 2: Kubernetes Secrets (Voting App)

This lab moves sensitive data (like database passwords) out of ConfigMaps and into **Kubernetes Secrets**, then wires Deployments to consume those secrets securely.

> Use **Secrets** for credentials, tokens, certificates. Keep them out of Git, and avoid hardcoding in YAML. Prefer external secret managers in production.

---

## Objectives
- Create an **Opaque Secret** for PostgreSQL credentials
- Consume the Secret as **environment variables** and **mounted files**
- Rotate the Secret and trigger a **rolling restart**
- Compare ConfigMap vs Secret and discuss security considerations

---

## Prerequisites
- A running cluster (Minikube/Kind)
- From Week 6 Lab 2 and Week 7 Lab 1:
  - Deployments/Services for `vote-web`, `vote-worker`, `vote-redis`, `vote-postgres`
  - ConfigMap `app-config` for non-secret config (e.g., REDIS_HOST, POSTGRES_HOST, POSTGRES_DB)
- `kubectl` configured

---

## Files in this lab
- `secret-app.yaml` – Secret containing `POSTGRES_PASSWORD`
- `deployments-with-secrets.yaml` – Updates to `vote-postgres` and `vote-worker` to read secrets via env and file mount

---

## 1) Create the Secret

### Option A: Declarative (recommended in class, do not commit real values)
Edit `secret-app.yaml` (default uses `postgres` as password for demo) and apply:
```bash
kubectl apply -f secret-app.yaml
kubectl get secrets app-secrets -o yaml
```

### Option B: Imperative (alternative)
```bash
kubectl create secret generic app-secrets   --from-literal=POSTGRES_PASSWORD=postgres
```

> In production, do **not** check real secrets into Git. Use `kubectl create secret` or external managers (Vault, AWS/GCP/Azure Secrets Manager), or templating tools (e.g., Helm with sealed-secrets/external-secrets).

---

## 2) Wire Secrets into Deployments

Apply the updated deployments:
```bash
kubectl apply -f deployments-with-secrets.yaml
kubectl rollout status deployment/vote-postgres
kubectl rollout status deployment/vote-worker
```

What changed:
- `vote-postgres` now takes `POSTGRES_PASSWORD` from the Secret
- `vote-worker` reads `POSTGRES_PASSWORD` from the Secret (redis/postgres hosts still from ConfigMap)
- Also mounts the entire Secret at `/etc/creds` so you can read `POSTGRES_PASSWORD` from a file if needed

Verify:
```bash
# Check env and mounted files in worker
POD=$(kubectl get pods -l app=vote-worker -o jsonpath='{.items[0].metadata.name}')
kubectl exec -it $POD -- sh -c 'echo $POSTGRES_PASSWORD; ls -l /etc/creds; cat /etc/creds/POSTGRES_PASSWORD'
```

---

## 3) Rotate the Secret

Update the password (demo value):
```bash
# Update the Secret data
kubectl create secret generic app-secrets   --from-literal=POSTGRES_PASSWORD=newpassword   -o yaml --dry-run=client | kubectl apply -f -
```

> Pods do **not** automatically pick up new env values. Trigger a rolling restart:
```bash
kubectl rollout restart deployment/vote-postgres
kubectl rollout restart deployment/vote-worker
```

If your app reads the password from a **mounted file**, it may see updates without restart depending on app behavior.

---

## 4) Cleanup
```bash
kubectl delete -f deployments-with-secrets.yaml
kubectl delete -f secret-app.yaml
```

---

## Discussion
- **Secrets vs ConfigMaps:** Secrets are base64-encoded by default; enable **encryption at rest** in cluster configuration for stronger protection.
- Avoid printing secrets in logs or `kubectl describe` output. Use `kubectl get secret -o jsonpath=...` cautiously.
- For production, consider **External Secrets Operator**, **Sealed Secrets**, or cloud secret stores with IAM.
