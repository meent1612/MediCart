# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first so Docker can cache the package restore
COPY MediCart.Web.csproj ./
RUN dotnet restore MediCart.Web.csproj

# Copy the rest of the code and publish
COPY . ./
RUN dotnet publish MediCart.Web.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

# Render looks for port 10000 by default
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# The .dll name always matches the .csproj name: MediCart.Web.csproj -> MediCart.Web.dll
ENTRYPOINT ["dotnet", "MediCart.Web.dll"]
