@echo off

@mkdir ..\Packaged

echo Updating NuGet
.\.nuget\nuget.exe Update -self

echo Building library package
.\.nuget\nuget.exe pack .\NuGetPackage-Lib\bin\release\NRConfig.Library.nuspec -OutputDirectory .\NuGetPackage-Lib\bin\release\
copy .\NuGetPackage-Lib\bin\release\NRConfig.Library.*.nupkg ..\Packaged\

echo Building tool package
.\.nuget\nuget.exe pack .\NuGetPackage-Tool\bin\release\NRConfig.Tool.nuspec -OutputDirectory .\NuGetPackage-Tool\bin\release\
copy .\NuGetPackage-Tool\bin\release\NRConfig.Tool.*.nupkg ..\Packaged\

echo Building MSBuild package
.\.nuget\nuget.exe pack .\NuGetPackage-MSBuild\bin\release\NRConfig.MSBuild.nuspec -OutputDirectory .\NuGetPackage-MSBuild\bin\release\
copy .\NuGetPackage-MSBuild\bin\release\NRConfig.MSBuild.*.nupkg ..\Packaged\

echo Packaging complete.