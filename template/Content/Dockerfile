# First stage of multi-stage build
FROM registry-docker.pamirs.com/dotnetsdk-yarn:latest AS build-env
WORKDIR /template-api
# copy the contents of agent working directory on host to workdir in container
COPY . ./

RUN cd src/Template.API && yarn install && cd ../..
# dotnet commands to build, test, and publish
RUN dotnet publish -c Release -o out

# Second stage - Build runtime image
FROM registry-docker.pamirs.com/aspnet:3.1
WORKDIR /template-api
COPY --from=build-env /template-api/out/ .
ENTRYPOINT ["dotnet", "template.API.dll"]