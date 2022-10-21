
### method1
dotnet run
dapr run --dapr-http-port 50001 --dapr-grpc-port 51001 --app-port 5001 --app-id ordering

 
dapr run --dapr-http-port 50002 --app-port 5002 --app-id test -- dotnet run --project .
dapr run --dapr-http-port 50002 --app-port 5002 --app-id test -- dotnet run --project .
 