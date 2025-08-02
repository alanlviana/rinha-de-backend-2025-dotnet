FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

RUN apk add clang build-base zlib-dev

WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish api/api.csproj -c Release --property PublishDir=out

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS prod
WORKDIR /app
COPY --from=build /app/api/out .

ENTRYPOINT ["./api"]