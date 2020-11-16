call dotnet restore
pushd .
cd Telegraphy.Net
call build.bat
popd
pushd .
cd Telegraphy.Azure.EventHub
call build.bat
popd
pushd .
cd Telegraphy.Azure.ServiceBus
call build.bat
popd
pushd .
cd Telegraphy.Azure.Storage
call build.bat
popd
pushd .
cd Telegraphy.Msmq
call build.bat
popd
pushd .
cd Telegraphy.IO
call build.bat
popd
pushd .
cd Telegraphy.Office365
call build.bat
popd
cd Telegraphy.Http
call build.bat
popd