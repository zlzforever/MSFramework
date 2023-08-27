# MSFramework

A micro service template

[![Build Status](https://dev.azure.com/zlzforever/cerberus/_apis/build/status/zlzforever.MSFramework?branchName=master)](https://dev.azure.com/zlzforever/cerberus/_build/latest?definitionId=10&branchName=master)


### Add EF migrations

```
$ cd src/Ordering.Api
$ dotnet ef migrations add Init  --context OrderingContext -p ../Ordering.Infrastructure
```

### 实践说明

+ 建议使用 schema 参数来进行隔离， 不再使用 table prefix 进行隔离

### Dapr

```
dapr run --dapr-http-port 3500 --dapr-grpc-port 50001 --app-port 5001 --app-id ordering-api --log-level debug --placement-host-address 192.168.31.78 --components-path C:\Users\zlzfo\.dapr\components
dapr run --dapr-http-port 50002 --dapr-grpc-port 51002 --app-port 5002 --app-id ordering-subscribe
```