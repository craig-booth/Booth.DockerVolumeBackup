FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

RUN apk add --update tzdata

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
	
WORKDIR /app
COPY /deploy .

WORKDIR /app
ENTRYPOINT ["dotnet", "Booth.DockerTest.dll"]