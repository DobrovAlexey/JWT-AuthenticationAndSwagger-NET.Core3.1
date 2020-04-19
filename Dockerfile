FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["JWT-AuthenticationAndSwagger-NET.Core3.1.csproj", ""]
RUN dotnet restore "./JWT-AuthenticationAndSwagger-NET.Core3.1.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "JWT-AuthenticationAndSwagger-NET.Core3.1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JWT-AuthenticationAndSwagger-NET.Core3.1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JWT-AuthenticationAndSwagger-NET.Core3.1.dll"]