from mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /webApi

EXPOSE 80
EXPOSE 7296

#COPY PROJECT FILES
COPY ./*.csproj ./
RUN dotnet restore

#Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

#Build image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /webApi
COPY --from=build /webApi/out .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]