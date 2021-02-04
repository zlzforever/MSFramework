# MSFramework
A micro service template

[![Build Status](https://dev.azure.com/zlzforever/cerberus/_apis/build/status/zlzforever.MSFramework?branchName=master)](https://dev.azure.com/zlzforever/cerberus/_build/latest?definitionId=10&branchName=master)

### Add EF migrations 

```
$ cd src/Ordering.Api
$ dotnet ef migrations add init -p ../Ordering.Infrastructure/Ordering.Infrastructure.csproj
```
 
 
 思考
 1. 若是 DbContext1 修改了聚合根 1，发出了领域事件，可能触发再次修改这个聚合根吗？
 
 若出现这样的情况，则是不正确的，聚合就是一个边界，在这个边界内的事件可以直接处理掉，只有通知别的边界（聚合）时才会发出事件。
 若是发生这种错误，会导致框架异常：DbContext1 add 聚合/实体，在别的地方又找到这个对象（要么因为没有提交查询不到，要么把 Added 状态改
 为 Modified 导致 Ef 的状态错误

dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true