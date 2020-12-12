dotnet build
dotnet pack -p:NuspecFile=.\Package.nuspec
REM msbuild /t:pack /p:NuspecFile=Package.nuspec Telegraphy.Office365.csproj
