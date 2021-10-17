## Running this example

```bash
k3d cluster create -p "80:80@loadbalancer"

k create namespace sidecar-auth
kubens sidecar-auth

k create -k k8s

curlie :80/call?param1=arg1
```

## Routing the traffic through the sidecar

### Simple

Don't expose the port of the main container.
Only expose the port of your sidecar container.

**excerpt of _deployment.yaml_**
```yaml
  containers:
  - name: echo
    image: mniak/echo
    # ports:
    # - containerPort: 80 ## Do NOT expose
  - name: proxy
    image: mniak/sidecar_proxy_demo
    env:
    - name: Proxy__Upstream
      value: "http://localhost:80"
    ports:
    - containerPort: 8080 ## Do expose
```

Then change the _service_ to point to the sidecar port.

**excerpt of _service.yaml_**
```yaml
  ports:
    - protocol: TCP
      port: 80
      targetPort: 8080 ## Point to th sidecar port
```

### Using a FORWARD iptables rule

_Useful in some corner cases._

Add an init container:

**excerpt of _deployment.yaml_**
```yaml
  initContainers:
  - name: proxy-init ## New initcontainer with the iptables rule
    image: mniak/iptables
    args: [ "-t", "nat", "-A", "PREROUTING", "-p", "tcp", "-i", "eth0", "--dport", "80", "-j", "REDIRECT", "--to-port", "8080" ]
    securityContext:
      capabilities:
        add:
        - NET_ADMIN
  containers:
  - name: echo
    image: mniak/echo
    ports:
    - containerPort: 80 ## Do expose application port
  - name: proxy
    image: mniak/sidecar_proxy_demo
    env:
    - name: Proxy__Upstream
      value: "http://localhost:80"
    # ports:
    # - containerPort: 8080 ## Do NOT expose sidecar port
```

And then you don't need to expose the port of the sidecar container.
