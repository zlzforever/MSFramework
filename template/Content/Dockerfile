FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/Template.API/Template.API.csproj -c Release -o /output

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /output .
# 框架启动时创建 storage/oss；非 root 用户需要可写存储目录。
RUN mkdir -p /app/storage/oss && chown -R app:app /app/storage
USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Template.API.dll"]
