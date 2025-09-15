# Week 12 – Lab 2: Node Autoscaling (Cluster Autoscaler)

This lab focuses on **Node Autoscaling** using the **Cluster Autoscaler (CA)**.  
Because CA integrates with a **cloud provider** (GKE/EKS/AKS) or a cluster API that can add/remove nodes, there are **two tracks**:

- **Track A (Recommended): Cloud Provider (GKE example)** — real node autoscaling.
- **Track B (Local – Minikube/Kind): Simulation** — demonstrate unschedulable Pods and manual node scale-out to explain the concept.

---

## Objectives
- Understand how **Cluster Autoscaler** responds to **unschedulable Pods**
- Configure a node pool with autoscaling (cloud)
- Force scale-out by submitting Pods with higher resource requests
- Observe scale-in after idle

---

## Track A — GKE (Google Kubernetes Engine)

### 1) Create a GKE cluster with autoscaling
Use gcloud (example values; adjust region/project):
```bash
gcloud container clusters create-autopilot vote-autopilot --region us-central1
# or Standard GKE with node autoscaling:
gcloud container clusters create vote-std --zone us-central1-a   --enable-autoscaling --min-nodes 1 --max-nodes 5 --num-nodes 1
gcloud container clusters get-credentials vote-std --zone us-central1-a
```

### 2) Deploy a bin-packing workload to force more nodes
```bash
kubectl apply -f binpack-deployment.yaml
kubectl get pods -w
```
Some Pods will show **Pending** due to insufficient CPU/memory. CA will add nodes to schedule them.

### 3) Verify autoscaling events
```bash
kubectl get nodes
kubectl describe nodes | grep -i "created node" -n || true
kubectl get events --sort-by=.lastTimestamp | grep -i "scale" -n || true
```

### 4) Clean up (and observe scale-in)
```bash
kubectl delete -f binpack-deployment.yaml
# CA reclaims empty nodes after its scale-down delay.
```

---

## Track B — Local (Minikube/Kind) Simulation

Real node autoscaling is not available locally, but you can **simulate the effect**:

1) Start a multi-node cluster:
- **Kind**:
  ```bash
  kind create cluster --name kind-3 --config - <<'EOF'
  kind: Cluster
  apiVersion: kind.x-k8s.io/v1alpha4
  nodes:
  - role: control-plane
  - role: worker
  - role: worker
  EOF
  ```
- **Minikube**:
  ```bash
  minikube start --nodes 2
  ```

2) Apply the same **binpack** workload:
```bash
kubectl apply -f binpack-deployment.yaml
kubectl get pods -w
```

3) When Pods are Pending, **manually** add a node (simulating CA):
- **Kind**: add another worker (requires recreating cluster; or create a larger initial cluster)
- **Minikube**:
  ```bash
  minikube node add
  ```

4) Verify Pods get scheduled after more capacity is available.

---

## Files
- `binpack-deployment.yaml` — Deployment with **high resource requests** to force Pending Pods and trigger (or simulate) node scale-out.

---

## Cleanup
```bash
kubectl delete -f binpack-deployment.yaml
# Delete extra nodes or your test cluster as needed
```

## Notes
- CA scales **node groups** when there are **unschedulable Pods** and sufficient **quota**.
- CA also scales **down** underutilized nodes (after a delay) by evicting Pods with **PodDisruptionBudgets** respected.
- On EKS/AKS, use **Managed Node Group** / **VMSS** autoscaling with the official Cluster Autoscaler deployment (refer to provider docs).
