#!/bin/bash
echo "Starting API load tests..."

# Test Catalog API
CATALOG_URL="http://eshop.local/catalog-api"
echo "Testing Catalog API..."
hey -z 3m -c 30 -q 15 $CATALOG_URL/api/v1/catalog/items &

# Test Basket API  
BASKET_URL="http://eshop.local/basket-api"
echo "Testing Basket API..."
hey -z 3m -c 20 -q 10 $BASKET_URL/api/v1/basket &

# Test Ordering API
ORDERING_URL="http://eshop.local/ordering-api"
echo "Testing Ordering API..."
hey -z 3m -c 25 -q 12 $ORDERING_URL/api/v1/orders &

wait
echo "API load tests completed!"
