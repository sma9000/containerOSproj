# Week 8 – Lab 2: Persistent Storage with PV & PVC (PostgreSQL)

In this lab you’ll persist database data for the Voting App by using **PersistentVolumeClaims (PVC)** and **PersistentVolumes (PV)**.  
You’ll try **dynamic provisioning** (default StorageClass) and a **static hostPath PV** fallback (useful on kind or clusters without a default provisioner).

> We’ll use **PostgreSQL** as the stateful component. A simple **Deployment** mounts the PVC at `/var/lib/postgresql/data`.  
> (You’ll learn about **StatefulSets** later; the same PVC concepts apply.)

---

## Objectives
- Create a **PVC** and mount it into PostgreSQL
- Verify that DB data survives Pod restarts
- (Optional) Create a **hostPath PV** and bind a PVC when dynamic provisioning isn’t available

---

## Prerequisites
- Kubernetes cluster (Minikube recommended; kind also works)
- From prior labs: `postgres-svc` Service (ClusterIP) or use the one provided here
- `kubectl` configured

---

## Files in this Lab
- `pvc-dynamic.yaml` – PVC that uses the **default StorageClass** (dynamic provisioning)
- `deployment-postgres-pvc.yaml` – Postgres Deployment that mounts the PVC
- `postgres-svc.yaml` – ClusterIP Service for Postgres (use if you don’t already have one)
- `pv-hostpath.yaml` – **Static PV** backed by a node directory (hostPath) – for clusters without dynamic provisioning
- `pvc-static.yaml` – PVC that binds to the above static PV

---

## 1) Option A – Dynamic Provisioning (Default StorageClass)

Apply the PVC and Deployment:
```bash
kubectl apply -f pvc-dynamic.yaml
kubectl apply -f postgres-svc.yaml   # if you don't have postgres-svc yet
kubectl apply -f deployment-postgres-pvc.yaml
```

Wait for the Pod to become Ready:
```bash
kubectl get pods -l app=vote-postgres -w
```

Confirm volumes:
```bash
kubectl get pvc,pv
kubectl describe pvc postgres-data
```

---

## 2) Test Persistence

1) Connect to Postgres and create a test table:
```bash
POD=$(kubectl get pods -l app=vote-postgres -o jsonpath='{.items[0].metadata.name}')
kubectl exec -it $POD -- psql -U postgres -d votes -c "CREATE TABLE demo (id serial primary key, note text);"
kubectl exec -it $POD -- psql -U postgres -d votes -c "INSERT INTO demo (note) VALUES ('hello-pvc');"
kubectl exec -it $POD -- psql -U postgres -d votes -c "SELECT * FROM demo;"
```

2) Delete the Pod (the Deployment will recreate it):
```bash
kubectl delete pod $POD
kubectl get pods -l app=vote-postgres -w
```

3) Re-check the data (should still be there):
```bash
POD=$(kubectl get pods -l app=vote-postgres -o jsonpath='{.items[0].metadata.name}')
kubectl exec -it $POD -- psql -U postgres -d votes -c "SELECT * FROM demo;"
```

---

## 3) Option B – Static hostPath PV (for clusters without dynamic provisioning)

> **Use either Option A or B** (not both). This option is useful on clusters without a default StorageClass.

1) Create a static PV and the matching PVC:
```bash
kubectl apply -f pv-hostpath.yaml
kubectl apply -f pvc-static.yaml
```

2) Point your Deployment to the static PVC by editing `deployment-postgres-pvc.yaml` (change `claimName: postgres-data` to `postgres-data-static`), then apply it:
```bash
kubectl apply -f deployment-postgres-pvc.yaml
```

3) Repeat the **Test Persistence** steps above.

---

## 4) Cleanup

```bash
kubectl delete -f deployment-postgres-pvc.yaml
kubectl delete -f postgres-svc.yaml
kubectl delete -f pvc-dynamic.yaml
kubectl delete -f pvc-static.yaml
kubectl delete -f pv-hostpath.yaml
```
> Note: Deleting a PVC may or may not delete the underlying PV, depending on the StorageClass **reclaimPolicy** (often `Delete` for dynamic, `Retain` for static). For static `hostPath` PVs, the data directory remains on the node until you remove it manually.

---

## Discussion
- **PVC** requests storage; **PV** fulfills it (dynamically or statically).
- Dynamic provisioning uses the cluster’s default **StorageClass** (e.g., Minikube’s `standard`).  
- **hostPath PV** ties storage to a specific node (not portable); good for demos/dev only.
- Mount path for Postgres: `/var/lib/postgresql/data` — this is where the DB stores its data.
