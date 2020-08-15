app=registry.cn-shanghai.aliyuncs.com/zlzforever/Template
docker build --target prebuild -t $app .
docker push $app