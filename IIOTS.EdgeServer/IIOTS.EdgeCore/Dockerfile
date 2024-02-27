#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IIOTS.EdgeServer/IIOTS.EdgeCore/IIOTS.EdgeCore.csproj", "IIOTS.EdgeServer/IIOTS.EdgeCore/"]
COPY ["IIOTS.Models/IIOTS.Models.csproj", "IIOTS.Models/"]
COPY ["IIOTS.Enum/IIOTS.Enum.csproj", "IIOTS.Enum/"]
COPY ["IIOTS.Util/IIOTS.Util.csproj", "IIOTS.Util/"]
COPY ["IIOTS.Interface/IIOTS.Interface.csproj", "IIOTS.Interface/"]
COPY ["IIOTS.EdgeServer/IIOTS.EdgeDriver/IIOTS.EdgeDriver.csproj", "IIOTS.EdgeServer/IIOTS.EdgeDriver/"]
COPY ["IIOTS.Drivers/IIOTS.Driver.Fins/IIOTS.Driver.Fins.csproj", "IIOTS.Drivers/IIOTS.Driver.Fins/"]
COPY ["IIOTS.Drivers/IIOTS.Driver/IIOTS.Driver.csproj", "IIOTS.Drivers/IIOTS.Driver/"]
COPY ["IIOTS.Communication/IIOTS.Communication.csproj", "IIOTS.Communication/"]
COPY ["IIOTS.Drivers/IIOTS.Driver.MC3E/IIOTS.Driver.MC3E.csproj", "IIOTS.Drivers/IIOTS.Driver.MC3E/"]
COPY ["IIOTS.Drivers/IIOTS.Driver.ModbusRtu/IIOTS.Driver.ModbusRtu.csproj", "IIOTS.Drivers/IIOTS.Driver.ModbusRtu/"]
COPY ["IIOTS.Drivers/IIOTS.Driver.ModbusTcp/IIOTS.Driver.ModbusTcp.csproj", "IIOTS.Drivers/IIOTS.Driver.ModbusTcp/"]
COPY ["IIOTS.Drivers/IIOTS.Driver.OPCUA/IIOTS.Driver.OPCUA.csproj", "IIOTS.Drivers/IIOTS.Driver.OPCUA/"]
RUN dotnet restore "./IIOTS.EdgeServer/IIOTS.EdgeCore/IIOTS.EdgeCore.csproj"
COPY . .
WORKDIR "/src/IIOTS.EdgeServer/IIOTS.EdgeCore"
RUN dotnet build "./IIOTS.EdgeCore.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IIOTS.EdgeCore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .  
ENV LANG C.UTF-8
ENV TZ=Asia/Shanghai
ENTRYPOINT ["dotnet", "IIOTS.EdgeCore.dll"]