FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY ./ app/
WORKDIR /app

ENTRYPOINT ["dotnet", "Wisetown.ContextBroker.Worker.dll"]
