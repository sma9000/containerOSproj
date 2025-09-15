# Week 9 – Lab 2: Observability – Metrics, Logs, and Traces

In this lab you will deploy a full **Observability stack** to monitor the Voting App. This includes:
- **Metrics:** Prometheus & Alertmanager for time-series data and alerting
- **Logs:** Loki & Grafana for centralized logs
- **Traces:** Jaeger for distributed tracing

---

## Objectives
1. Understand **Metrics, Logs, Traces** and why they are key to observability
2. Deploy Prometheus and exporters to collect metrics from your cluster and application
3. Write and test **PromQL queries** and sample **alerting rules**
4. Deploy Alertmanager and trigger alerts
5. Set up Loki & Grafana to aggregate and search logs
6. Deploy Jaeger and trace requests across microservices

---

## Components to Deploy

### 1) Metrics – Prometheus & Alertmanager
- Prometheus scrapes metrics from instrumented targets (e.g., `/metrics` endpoints).
- PromQL queries allow aggregating and alerting on time-series data.

### 2) Logs – Loki & Grafana
- Loki stores container logs (like a log database).
- Grafana queries Loki and shows dashboards.

### 3) Traces – Jaeger
- Jaeger visualizes request flow through the application.

---

## Step 1: Deploy Prometheus and Exporters

Apply:
```bash
kubectl create secret generic alertmanager-smtp  --from-literal=smtp_password='Manager1@'

kubectl apply -f prometheus-configmap.yaml
kubectl apply -f prometheus-deployment.yaml
kubectl apply -f prometheus-service.yaml
kubectl apply -f alertmanager-config.yaml
kubectl apply -f alertmanager-deployment.yaml
kubectl apply -f alertmanager-service.yaml
kubectl apply -f deployment-redis-with-exporter.yaml
kubectl apply -f deployment-postgres-with-exporter.yaml
kubectl apply -f exporters-services.yaml
```

Port-forward Prometheus and Alertmanager:
```bash
kubectl port-forward svc/prometheus-service 9090:9090 
kubectl port-forward svc/alertmanager 9093:9093 
```

Open:
- Prometheus: http://localhost:9090
- Alertmanager: http://localhost:9093

### Sample PromQL Queries
```
http_requests_total
```

### Trigger a Sample Alert
- In `prometheus-configmap.yaml`, alert if any target is `down` for 1 minute.
- Stop the Redis exporter Pod to simulate failure.

---

## Step 2: Deploy Loki & Grafana (Logs)

Apply:
```bash
kubectl apply -f loki-stack.yaml
kubectl port-forward svc/grafana 3000:80
```

## Create Loki Service
Create file to deploy Loki on Cluster port 3100

Login to Grafana:
- URL: http://localhost:3000
- Default creds: `admin/admin`

Add data source:
- Type: **Loki**
- URL: http://localhost:3100

Query logs for the `vote-web` Pod:
```
{app="vote-web"}
```

---

## Step 3: Deploy Jaeger (Traces)

Apply:
```bash
kubectl apply -f jaeger-all-in-one.yaml
kubectl port-forward svc/jaeger 16686:16686
```

Open Jaeger: http://localhost:16686

Trigger some requests to the Voting App and view traces for the `vote-web` service.

---

## Cleanup

```bash
kubectl delete -f jaeger-all-in-one.yaml
kubectl delete -f loki-stack.yaml
kubectl delete -f prometheus-service.yaml
kubectl delete -f prometheus-deployment.yaml
kubectl delete -f prometheus-configmap.yaml
kubectl delete -f alertmanager-service.yaml
kubectl delete -f alertmanager-deployment.yaml
kubectl delete -f alertmanager-config.yaml
kubectl delete -f exporters-services.yaml
kubectl delete -f deployment-postgres-with-exporter.yaml
kubectl delete -f deployment-redis-with-exporter.yaml
```

---

## Discussion
- **Metrics**: Time-series, numeric, actionable (Prometheus + Alertmanager)
- **Logs**: Detailed text records (Loki + Grafana)
- **Traces**: Request path visualization (Jaeger)
- **Alertmanager** sends alerts to email, Slack, etc.
