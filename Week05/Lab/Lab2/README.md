# Week 5 – Lab 2: Single Deployment (Multi-Container Pod) for Web, Worker, Redis, and PostgreSQL


## Objectives
- Create Deployment files for each of **five containers**: web (vote), result, worker, redis, and postgres.
- Observe that scaling the Deployment replicates **the whole stack per Pod**.
- Practice rollout/scale/inspect commands.



## 1) Apply the Deployment
```bash
kubectl apply -f redis-deployment.yaml
kubectl apply -f postgres-deployment.yaml

```

## 2) Update Deployment files
- Update `image` section with your image reference

- Capture the IP for redis and postgress pod and update deployment files the deploy it.

```bash
kubectl apply -f vote-deployment.yaml
kubectl apply -f worker-deployment.yaml
kubectl apply -f result-deployment.yaml
```


## 3) Inspect Containers and Logs
```bash
kubectl get pod -l app=vote-stack -o name
# Replace <pod-name> below with the actual name:
kubectl describe <pod-name>
kubectl logs <pod-name> -c vote
kubectl logs <pod-name> -c worker
kubectl logs <pod-name> -c redis
kubectl logs <pod-name> -c postgres
```

## 4) Scale the Whole Stack
> Scaling the Deployment increases the number of **Pods**, each Pod running **all four containers**.
```bash
kubectl scale deployment/vote --replicas=2
kubectl get pods -l app=vote
```

## 5) (Optional) Port-Forward for Ad-Hoc Testing (no Services yet)
```bash
# Forward web (vote) on containerPort 80
kubectl port-forward deploy/vote 5000:80
# In another terminal you can also forward API if you add it later.
```

## 6) Cleanup
```bash
kubectl delete -f vote-deployment.yaml
kubectl delete -f worker-deployment.yaml
kubectl delete -f result-deployment.yaml
kubectl delete -f redis-deployment.yaml
kubectl delete -f postgres-deployment.yaml


```

