# Week 13 – Lab 2: Advanced Helm

This lab goes beyond basics and covers:
- **Dependencies & subcharts** (Bitnami Redis/PostgreSQL + local web/worker)
- **Templating** (conditionals, functions, named templates)
- **Values schema** validation
- **Hooks** (pre-install Job), **tests** (helm test), and **NOTES.txt**
- **Upgrades, rollbacks, and atomic installs**
- **Best practices** for chart layout and promotion via values files

You’ll use a parent chart **`voting-app/`** that deploys:
- `web` (local subchart)
- `worker` (local subchart)
- `redis` (Bitnami dependency)
- `postgresql` (Bitnami dependency)

> This is a *teaching* chart for the Example Voting App. You can later swap `web/worker` images for your own.

---

## 0) Prereqs

- Helm v3 installed (`helm version`)
- A Kubernetes cluster (Minikube/Kind)
- (Optional) Internet to pull Bitnami dependencies

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

---

## 1) Explore the chart

```
voting-app/
  Chart.yaml                 # name, version, dependencies
  values.yaml                # default values
  values-production.yaml     # prod-ish overrides
  values.schema.json         # values validation
  templates/
    _helpers.tpl             # named templates
    ingress.yaml             # optional Ingress for web
    hooks-job.yaml           # pre-install/upgrade Job example
    NOTES.txt                # post-install instructions
  charts/
    web/                     # local subchart
      Chart.yaml
      values.yaml
      templates/deployment.yaml
      templates/service.yaml
    worker/                  # local subchart
      Chart.yaml
      values.yaml
      templates/deployment.yaml
```

---

## 2) Vendor dependencies

`Chart.yaml` references Bitnami `redis` & `postgresql`. Pull them into `charts/`:

```bash
cd voting-app
helm dependency update
ls charts/
```

You should see tarballs for `redis` and `postgresql` or expanded directories depending on Helm version.

---

## 3) Install (dev defaults)

```bash
helm install vote ./voting-app --create-namespace --namespace vote --wait
helm status vote -n vote
kubectl get all -n vote
```

Verify NOTES:
```bash
helm get notes vote -n vote
```

---

## 4) Upgrade with overrides (production-ish)

- Try changing `web.image.tag` or `replicaCount` and upgrade again.

```bash
helm upgrade vote ./voting-app --namespace vote \
  --set worker.replicaCount=3
kubectl get all -n vote
```


Rollback example:
```bash
helm history vote -n vote
helm rollback vote 1 -n vote
```

---

## 5) Hooks & Tests

A **pre-install** Job runs a quick schema check (demo). After install, run chart tests:

```bash
helm test vote -n vote --logs
```

---

## 6) Lint and template

```bash
helm lint ./voting-app
helm template ./voting-app -f values-production.yaml | head -n 60
```

---

## 7) Cleanup

```bash
helm uninstall vote -n vote
kubectl delete ns vote
```

---

