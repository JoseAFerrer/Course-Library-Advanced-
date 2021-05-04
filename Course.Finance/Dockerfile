FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Course.Finance/Course.Finance.csproj", "Course.Finance/"]
RUN dotnet restore "Course.Finance/Course.Finance.csproj"
COPY . .
WORKDIR "/src/Course.Finance"
RUN dotnet build "Course.Finance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Course.Finance.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Course.Finance.dll"]
