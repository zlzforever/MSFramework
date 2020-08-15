# First stage: restore
FROM registry.cn-shanghai.aliyuncs.com/zlzforever/dotnetsdk-yarn:3.1-latest AS prebuild
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN cd .. && rm -rf app

# Second stage - Build runtime image
FROM registry.cn-shanghai.aliyuncs.com/zlzforever/Template as build-env
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o out

# Third stage - Copy to host env
FROM registry-docker.pamirs.com/aspnet:3.1 as release
WORKDIR /app
COPY --from=build-env /app/out/ .
ENTRYPOINT ["dotnet", "template.API.dll"]