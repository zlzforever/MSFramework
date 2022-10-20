dapr run --dapr-http-port 5101 --app-port 5001 --app-id ordering dotnet run
dapr run --dapr-http-port 50001 --dapr-grpc-port 40001 --app-port 5001 --app-ssl --app-id ordering
dapr run --dapr-http-port 50002 --dapr-grpc-port 40002 --app-port 5008 --app-ssl --app-id test