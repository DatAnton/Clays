FROM node:26 AS react-build
WORKDIR /client
COPY ReactUI/package*.json ./
RUN npm install
COPY ReactUI/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Clays/*.csproj ./Clays/
WORKDIR /src/Clays
RUN dotnet restore

COPY . ./
COPY --from=react-build /client/dist ./wwwroot

RUN dotnet publish -c Release -o /src/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /src
COPY --from=build /src/out .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Clays.dll"]