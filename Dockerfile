FROM node:26 AS react-build
WORKDIR /client
COPY ReactUI/package*.json ./
RUN npm install
COPY ReactUI/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Clays/Clays.csproj ./Clays/
WORKDIR /src/Clays
RUN dotnet restore

COPY . ./

WORKDIR /src/Clays
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
COPY --from=react-build /client/dist ./wwwroot

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Clays.dll"]