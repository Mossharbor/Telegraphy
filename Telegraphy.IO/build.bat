dotnet build
dotnet pack -p:NuspecFile=.\Package.nuspec
REM msbuild /t:pack /p:NuspecFile=Package.nuspec Telegraphy.IO.csproj /p:Version=2.0.1
REM dotnet pack -p:NuspecFile=Package.nuspec Telegraphy.IO.csproj
