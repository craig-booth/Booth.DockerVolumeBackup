FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

RUN apk add --update tzdata

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
	
WORKDIR /app
COPY /deploy ./volumes

WORKDIR /app
ENTRYPOINT ["dotnet", "Booth.DockerVolumeBackup.WebApi.dll"]