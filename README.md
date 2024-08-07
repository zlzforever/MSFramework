# MSFramework

A micro service template

[![Build Status](https://dev.azure.com/zlzforever/cerberus/_apis/build/status/zlzforever.MSFramework?branchName=master)](https://dev.azure.com/zlzforever/cerberus/_build/latest?definitionId=10&branchName=master)

### Add EF migrations

```
dotnet ef migrations add Init -s src/Ordering.Api -c OrderingContext -p src/Ordering.Infrastructure
```

### 实践说明

+ 建议使用 schema 参数来进行隔离， 不再使用 table prefix 进行隔离

### Dapr

```
dapr run --app-id ordering-api --app-port 5001 --dapr-http-port 5101 --dapr-grpc-port 5102 --config ./dapr/config.yaml --components-path ./dapr/components
```