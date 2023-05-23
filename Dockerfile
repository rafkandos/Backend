# Menggunakan image .NET 6 SDK sebagai dasar
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Menyalin file proyek dan file lain yang diperlukan
COPY *.csproj ./
RUN dotnet restore

# Menyalin seluruh kode proyek
COPY . ./

# Build proyek
RUN dotnet publish -c Release -o out

# Menjalankan aplikasi saat container dijalankan
CMD ["dotnet", "bin/Debug/net6.0/CualiVy-CC.dll"]