```bash
k3d cluster create -p "80:80@loadbalancer"

k create namespace sidecar-auth
kubens sidecar-auth

k create -k k8s

curlie :80/call?param1=arg1
```