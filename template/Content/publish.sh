app=registry.cn-shanghai.aliyuncs.com/zlzforever/Template
docker build --target build -t $app .
docker push $app