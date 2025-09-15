# Gatekeeper Install Notes

## Option A: Helm (recommended)
helm repo add gatekeeper https://open-policy-agent.github.io/gatekeeper/charts
helm repo update
helm install gatekeeper gatekeeper/gatekeeper --namespace gatekeeper-system --create-namespace

## Option B: Static Manifests
kubectl apply -f https://raw.githubusercontent.com/open-policy-agent/gatekeeper/master/deploy/gatekeeper.yaml

## Verify
kubectl get pods -n gatekeeper-system
