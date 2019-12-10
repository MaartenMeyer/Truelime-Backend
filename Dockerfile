
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /TruelimeBackend
COPY bin/Release/netcoreapp2.2/publish ./
CMD export ASPNETCORE_URLS=http://*:$PORT && dotnet TruelimeBackend.dll