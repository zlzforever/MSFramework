rm -rf MSFramework.AspNetCore.Template.*.nupkg
rm -rf template/.DS_Store
rm -rf template/Content/.DS_Store
rm -rf template/Content/.idea

rm -rf template/Content/src/.DS_Store

rm -rf template/Content/src/Template.Domain/.DS_Store
rm -rf template/Content/src/Template.Domain/bin
rm -rf template/Content/src/Template.Domain/obj

rm -rf template/Content/src/Template.Infrastructure/.DS_Store
rm -rf template/Content/src/Template.Infrastructure/bin
rm -rf template/Content/src/Template.Infrastructure/obj
rm -rf template/Content/src/Template.Infrastructure/Migrations

rm -rf template/Content/src/Template.Application/.DS_Store
rm -rf template/Content/src/Template.Application/bin
rm -rf template/Content/src/Template.Application/obj

rm -rf template/Content/src/Template.API/.DS_Store
rm -rf template/Content/src/Template.API/bin
rm -rf template/Content/src/Template.API/obj

nuget pack template/MSFramework.AspNetCore.Template.nuspec
nuget push MSFramework.AspNetCore.Template.*.nupkg -SkipDuplicate  -Source http://nuget.pamirs.com/v3/index.json
sudo cp MSFramework.AspNetCore.Template.*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder