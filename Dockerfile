FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY *.csproj .
RUN dotnet restore

# copy everything else and build app
COPY . ./routerfilter/
WORKDIR /app/routerfilter/
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/routerfilter/out ./
ENV Port=3422
ENV SchoolCode=NI56Q
ENV GameServicePort=3434
ENV GameServiceIP=127.0.0.1
ENTRYPOINT ["dotnet", "RouterFilter.dll"]