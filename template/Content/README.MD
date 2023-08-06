# Template

## EF add migration

```
$ cd src/Template.API
$ dotnet ef migrations add Init -p ../Template.Infrastructure/Template.Infrastructure.csproj
```

## Dapr

```
dapr run --dapr-http-port 30001 --dapr-grpc-port 31001 --app-port 5000 --app-id template-api
```