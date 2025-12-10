#!/bin/bash
echo "Starting load test for WebMVC..."

# Get the external IP/URL of your ingress
WEBMVC_URL="http://webmvc.eshop.local"

# Start multiple concurrent load tests
echo "Running load test for 5 minutes with 50 concurrent connections..."
hey -z 5m -c 50 -q 10 $WEBMVC_URL &

echo "Running burst load test..."
hey -z 2m -c 100 -q 20 $WEBMVC_URL/catalog &

wait
echo "Load tests completed!"
