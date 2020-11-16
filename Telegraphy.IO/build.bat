#msbuild /t:pack /p:NuspecFile=Package.nuspec Telegraphy.IO.csproj /p:Version=2.0.1
dotnet pack -p:NuspecFile=Package.nuspec Telegraphy.IO.csproj
