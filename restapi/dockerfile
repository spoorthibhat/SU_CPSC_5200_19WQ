FROM microsoft/dotnet:2.2.0-aspnetcore-runtime-alpine

WORKDIR /app
COPY bin/Debug/netcoreapp2.2/publish/ .

ARG APP_PORT=5000
ENV ASPNETCORE_URLS http://+:$APP_PORT
EXPOSE $APP_PORT

CMD ["dotnet","/app/restapi.dll"]
