# First stage: restore
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Second stage - Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY . ./
RUN dotnet publish "src/Template.API" -c Release -o /output

# Third stage - Copy to host env
FROM base as final
WORKDIR /app
COPY --from=publish /output .
ENTRYPOINT ["dotnet", "template.API.dll"]