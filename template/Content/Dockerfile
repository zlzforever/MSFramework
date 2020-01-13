# First stage of multi-stage build
FROM registry-docker.pamirs.com/dotnetsdk-yarn:latest AS build-env
WORKDIR /pamirs-order-api
# copy the contents of agent working directory on host to workdir in container
COPY . ./

RUN cd src/Template.API && yarn install && cd ../..
# dotnet commands to build, test, and publish
RUN dotnet publish -c Release -o out

# Second stage - Build runtime image
FROM registry-docker.pamirs.com/aspnet:3.1
WORKDIR /pamirs-order-api
COPY --from=build-env /pamirs-order-api/out/ .
ENTRYPOINT ["dotnet","Pamirs.Order.API.dll"]