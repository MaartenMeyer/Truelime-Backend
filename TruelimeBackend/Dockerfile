FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY bin/Release/PublishOutput/ ./
CMD export ASPNETCORE_URLS=http://*:$PORT && dotnet TruelimeBackend.dll