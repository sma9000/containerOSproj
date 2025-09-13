# 📦 Week 2 Homework: Building Dockerfiles for eShopOnContainers Microservices

## 🧠 Objective
This week, you'll gain hands-on experience building Dockerfiles for a complex real-world application composed of many microservices.

---


## 🧰 Your Task

### ✅ Step 1: Clone the Repository

```bash
git clone https://github.com/msalemcode/Container_Operating_Systems.git
cd Container_Operating_Systems/Code/Homework
```

---

### ✅ Step 2: Focus on These Microservices

You will be `Build` Dockerfiles for the following key services: Review each service and get familiar with Dockerfile

#### Core Services

- [ ] Catalog.API → `src/Services/Catalog/Catalog.API`
- [ ] Basket.API → `src/Services/Basket/Basket.API`
- [ ] Ordering.API → `src/Services/Ordering/Ordering.API`
- [ ] Identity.API → `src/Services/Identity/Identity.API`
- [ ] Payment.API → `src/Services/Payment/Payment.API`

#### Frontend Services
- [ ] WebMVC → `src/Web/WebMVC`
- [ ] WebSPA → `src/Web/WebSPA`
- [ ] WebStatus → `src/Web/WebStatus`
- [ ] Gateway Mobil API → `src/ApiGateways/Mobile.Bff.Shopping/aggregator`
- [ ] Gateway Web API → `src/ApiGateways/Web.Bff.Shopping/aggregator`
- [ ] Ordering.SignalrHub → `src/ApiGateways/Web.Bff.Shopping/aggregator`
- [ ] Webhooks.API → `src/Web/WebhookClient`  
- [ ] Ordering.BackgroundTasks → `src/Services/Ordering/Ordering.BackgroundTasks`

---

### 🧪  Build Docker files

Example for Catalog.API:
```bash
cd src
docker build -t catalog-api -f Services/Catalog/Catalog.API/Dockerfile .

```

Repeat for each microservice.

---

## 📝 Submission Instructions

1. Create a public DockerHub account
2. Push docker images to DockerHub
4. Submit/Email screenhot for your DockerHub 

---

## ⏱️ Deadline
Submit before the next Homework date.

