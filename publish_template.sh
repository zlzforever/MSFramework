rm -rf ./MSFramework.AspNetCore.Template.*.nupkg
rm -rf ./template/.DS_Store
rm -rf ./template/Content/.DS_Store
rm -rf ./template/Content/.idea

rm -rf ./template/Content/Template.Domain/.DS_Store
rm -rf ./template/Content/Template.Domain/bin
rm -rf ./template/Content/Template.Domain/obj

rm -rf ./template/Content/Template.Infrastructure/.DS_Store
rm -rf ./template/Content/Template.Infrastructure/bin
rm -rf ./template/Content/Template.Infrastructure/obj

rm -rf ./template/Content/Template.Application/.DS_Store
rm -rf ./template/Content/Template.Application/bin
rm -rf ./template/Content/Template.Application/obj

rm -rf ./template/Content/Template.API/.DS_Store
rm -rf ./template/Content/Template.API/bin
rm -rf ./template/Content/Template.API/obj

nuget pack ./template/MSFramework.AspNetCore.Template.nuspec
nuget push ./MSFramework.AspNetCore.Template.*.nupkg -SkipDuplicate  -Source http://nuget.pamirs.com/v3/index.json
sudo cp ./MSFramework.AspNetCore.Template.*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder