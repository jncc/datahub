
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Datahub.Web/*.csproj ./Datahub.Web/
RUN dotnet restore

# copy everything else and build app
COPY Datahub.Web/. ./Datahub.Web/
WORKDIR /app/Datahub.Web
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=build /app/Datahub.Web/out ./
ENTRYPOINT ["dotnet", "Datahub.Web.dll"]

