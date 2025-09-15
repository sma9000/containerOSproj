# Week 11 – Lab 2: Kubernetes Security Deep Dive

Focus areas:
- **Security Contexts & Pod-Level Controls**
- **Pod Security Standards (PSS)** — PSP replacement
- **Network Policies & Isolation**
- **Secrets Management Best Practices**
- **Admission Controllers & OPA/Gatekeeper (bonus: Kyverno)**
- **Supply Chain Security (image signing & provenance)**

> This lab is hands-on but modular — you can run each section independently in a fresh namespace.

---

## Prerequisites
- Working Kubernetes cluster (Minikube/Kind)
- `kubectl` configured
- From earlier weeks you may reuse the Voting App components, but this lab provides self-contained secure examples.

---

## Overview of Files
- `ns-security.yaml` — Namespace labeled to **enforce PSS: restricted** (and audit/warn baseline)
- `deployment-secure.yaml` — Example app with **strict securityContext** (non-root, seccomp, read-only FS, no privilege escalation, drop caps)
- `networkpolicy-default-deny.yaml` — Default deny **ingress & egress**
- `networkpolicy-allow-web-to-redis-postgres.yaml` — Minimal **allow list** example
- `secret-app.yaml` — Example **Opaque Secret** and **secure consumption** patterns
- `gatekeeper-install-notes.md` — Quick install options and sanity checks
- `gatekeeper-ct-allowedrepos.yaml` — **ConstraintTemplate** to restrict image registries
- `gatekeeper-c-allowedrepos.yaml` — **Constraint** to only allow images from approved registries
- `gatekeeper-ct-restrictroot.yaml` — **ConstraintTemplate** to forbid `runAsRoot`
- `gatekeeper-c-restrictroot.yaml` — **Constraint** enforcing non-root containers
- `kyverno-verify-images.yaml` — (Optional) **Kyverno** policy to verify container image signatures (supply chain)
- `supply-chain-notes.md` — **Cosign** sign/verify quickstart + policy options

---

## 1) Create a Security-Scoped Namespace (PSS)
Apply Pod Security Standards at the **namespace** level:
```bash
kubectl apply -f ns-security.yaml
kubectl get ns security-lab -o yaml | grep pod-security -A2
```
PSS labels used here:
- `enforce: restricted` — blocks privileged/unsafe settings
- `warn/audit: baseline` — provides guidance as you harden further

> If a Pod/Deployment is rejected, check `kubectl describe` events for PSS violations.

---

## 2) Secure Workload: Pod & Container Security Context
Deploy a minimal web app with strict **securityContext**:
```bash
kubectl apply -n security-lab -f secret-app.yaml
kubectl apply -n security-lab -f deployment-secure.yaml
kubectl get pods -n security-lab
kubectl describe pod -n security-lab -l app=secure-web
```
Highlights:
- `runAsNonRoot: true`, `runAsUser: 10001`, `runAsGroup: 10001`
- `allowPrivilegeEscalation: false`
- `readOnlyRootFilesystem: true`
- `capabilities: drop: ["ALL"]`
- `seccompProfile: runtime/default`

> If you change the image to one that requires root or writes to `/`, you’ll see failures — a learning opportunity.

---

## 3) Network Isolation with NetworkPolicies
Start with **default deny** for both ingress and egress:
```bash
kubectl apply -n security-lab -f networkpolicy-default-deny.yaml
```
Then allow only what is required (example: web → redis/postgres ports):
```bash
kubectl apply -n security-lab -f networkpolicy-allow-web-to-redis-postgres.yaml
```
Validate:
```bash
kubectl get netpol -n security-lab
```

---

## 4) Secrets: Best Practices
Create a demo Secret and show two **safe** consumption patterns (env + mounted file):
```bash
kubectl apply -n security-lab -f secret-app.yaml
kubectl rollout restart deploy secure-web -n security-lab
kubectl exec -it -n security-lab deploy/secure-web -- sh -c 'printenv | grep DEMO_| true; ls -l /etc/creds; cat /etc/creds/DEMO_PASSWORD'
```
Guidance:
- Avoid committing real secrets to git (use `kubectl create secret` or external managers)
- Prefer **file mount** for rotation without restart (app must re-read)
- Enable **encryption at rest** for Secrets in cluster config (out of scope here)

---

## 5) Admission Controllers & OPA/Gatekeeper
> Use **either** Helm or static manifests to install Gatekeeper (see `gatekeeper-install-notes.md`).

Apply policies:
```bash
# Restrict allowed image registries (e.g., only ghcr.io/company or gcr.io/company)
kubectl apply -f gatekeeper-ct-allowedrepos.yaml
kubectl apply -f gatekeeper-c-allowedrepos.yaml

# Enforce non-root containers cluster-wide
kubectl apply -f gatekeeper-ct-restrictroot.yaml
kubectl apply -f gatekeeper-c-restrictroot.yaml
```
Test by trying to deploy a violating workload and observing **Deny** in admission.



## Cleanup
```bash
kubectl delete ns security-lab
# Optionally uninstall Gatekeeper/Kyverno per your install method
```
