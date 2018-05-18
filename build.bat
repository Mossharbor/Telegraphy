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