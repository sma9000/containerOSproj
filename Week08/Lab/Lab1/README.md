# Week 8 – Lab 1: Volumes with the Example Voting App

This lab introduces the **Kubernetes Volume types** you can use in Pods:
- `emptyDir` – scratch space shared by all containers in a Pod
- `hostPath` – mounts a file or directory from the node
- `configMap` and `secret` – injects configuration and sensitive data
- `downwardAPI` – exposes Pod metadata to containers

> **PersistentVolume (PV) and PersistentVolumeClaim (PVC)** will be covered in **Lab 2**. This lab focuses only on ephemeral or directly mounted volumes.

---

## Objectives
- Add different volume types to the Voting App Pods
- See how multiple containers in a Pod share an `emptyDir`
- Mount configuration and secrets as files
- Expose Pod metadata to the containers using `downwardAPI`

---

## Prerequisites
- A running Kubernetes cluster (Minikube/Kind)
- Previous labs: 
  - ConfigMap (`app-config`)
  - Secret (`app-secrets`)
  - Deployments/Services for vote-web, vote-worker, vote-redis, vote-postgres

---

## Files in this lab
- `deployment-with-volumes.yaml` – demonstrates all volume types in the `vote-web` Pod

---

## 1) Apply the Deployment with Volumes

```bash
kubectl apply -f deployment-with-volumes.yaml
kubectl get pods
```

---

## 2) Inspect Volumes in Containers

Check which volumes are mounted:
```bash
POD=$(kubectl get pods -l app=vote-web -o jsonpath='{.items[0].metadata.name}')
kubectl describe pod $POD | grep -A 5 "Volumes:"
```

Open a shell and inspect mounts:
```bash
kubectl exec -it $POD -- sh

# Look at shared scratch directory
ls -l /scratch

# Inspect mounted config and secrets
ls -l /etc/config
cat /etc/config/appsettings.yaml
ls -l /etc/creds
cat /etc/creds/POSTGRES_PASSWORD

# Downward API files
cat /etc/podinfo/name
cat /etc/podinfo/namespace
exit
```

---

## 3) HostPath Demo (Only in Minikube)

> **Caution:** HostPath mounts a directory from the node's filesystem and ties the Pod to a specific node.

Look at the `hostPath` volume in `deployment-with-volumes.yaml`:
- Mounts `/mnt/hostdata` from the node into `/hostdata` inside the container.

Write a file:
```bash
kubectl exec -it $POD -- sh -c 'echo "hello from pod" > /hostdata/test.txt'
```

Check the file exists on the Minikube VM:
```bash
minikube ssh
ls /mnt/hostdata
```

---

## 4) Cleanup

```bash
kubectl delete -f deployment-with-volumes.yaml
```

---

## Discussion
- `emptyDir` is wiped when the Pod is deleted.
- `hostPath` ties the Pod to a specific node; avoid in production unless necessary.
- ConfigMap and Secret volumes provide automatic updates when the data changes.
- `downwardAPI` gives containers access to Pod metadata and resource limits.
