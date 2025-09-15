# Week 11 – Lab 1: Namespaces and RBAC

This lab will teach you about **Kubernetes Namespaces** and **Role-Based Access Control (RBAC)**.

---

## Objectives
- Understand **Namespaces** and their purpose
- Explore default namespaces
- Create new namespaces and assign resources
- Understand **RBAC** basics: Roles, RoleBindings, ServiceAccounts
- Grant permissions scoped to a namespace

---

## Topics
1. **What is a Namespace?**
   - Logical partition for resources in a cluster
   - Default namespaces: `default`, `kube-system`, `kube-public`, `kube-node-lease`

2. **Why use Namespaces?**
   - Organize resources by team or project
   - Apply resource quotas and network policies
   - Simplify RBAC by scoping Roles to namespaces

3. **RBAC Basics**
   - **Role**: Defines permissions within a namespace
   - **ClusterRole**: Defines cluster-wide permissions
   - **RoleBinding**: Binds a Role to a user/group/ServiceAccount within a namespace

---

## Step 1: Create a New Namespace

```bash
kubectl create namespace team-a
kubectl get ns
```

---

## Step 2: Deploy Resources in the Namespace

Deploy the `vote-web` service into `team-a` namespace:

```bash
kubectl apply -n team-a -f vote-web-deployment.yaml
kubectl get all -n team-a
```

---

## Step 3: Create a Role and RoleBinding

Create a **Role** allowing read-only access to Pods in `team-a`:

```yaml
# role-view-pods.yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: team-a
  name: pod-reader
rules:
- apiGroups: [""]
  resources: ["pods"]
  verbs: ["get", "list"]
```

Bind the role to a new **ServiceAccount**:

```yaml
# rolebinding-view-pods.yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: read-pods-binding
  namespace: team-a
subjects:
- kind: ServiceAccount
  name: team-a-sa
  namespace: team-a
roleRef:
  kind: Role
  name: pod-reader
  apiGroup: rbac.authorization.k8s.io
```

Apply the manifests:

```bash
kubectl create sa team-a-sa -n team-a
kubectl apply -f role-view-pods.yaml
kubectl apply -f rolebinding-view-pods.yaml
```

---

## Step 4: Test Access

Switch to the new ServiceAccount and try listing Pods:

```bash
TOKEN=$(kubectl get secret $(kubectl get sa team-a-sa -n team-a -o jsonpath="{.secrets[0].name}") -n team-a -o jsonpath="{.data.token}" | base64 --decode)

kubectl --token=$TOKEN --server=$(kubectl config view --minify -o jsonpath='{.clusters[0].cluster.server}') get pods -n team-a
```

---

## Step 5: Cleanup

```bash
kubectl delete namespace team-a
```

---

## Discussion
- Namespaces isolate resource names and policies.
- Roles + RoleBindings control permissions per namespace.
- ClusterRoles and ClusterRoleBindings are for cluster-wide access.
