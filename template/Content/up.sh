app=template
docker build -f Dockerfile.Development -t $app .
docker stop $app
docker rm $app
docker run -d --name $app --restart always \
    -e ASPNETCORE_ENVIRONMENT=Development \
    -p 8080:8080 \
    $app

