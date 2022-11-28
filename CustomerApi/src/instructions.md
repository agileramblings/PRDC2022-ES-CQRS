#Instructions

##Docker Images & Containers

###Seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

#### Query to get Controller execution times
```sql 
select mean(ElapsedMilliseconds) from stream 
where Component = 'CustomerController' 
and @Timestamp > now() - 1h 
group by Operation, time(1m)
```

###SQLServer
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd" -p 1433:1433 --name localSqlServer --hostname localSqlServer -d  mcr.microsoft.com/mssql/server:2022-latest

###CosmosDB
Windows Emulator -
https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21


Docker Linux Container Emulator - 
https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator-on-docker-windows?tabs=cli

```powershell
md $env:LOCALAPPDATA\CosmosDBEmulator\bind-mount 2>null
```

```powershell
docker run --publish 8081:8081 `
    --publish 10250-10255:10250-10255 `
    --memory 3g --cpus=2.0 `
    --name=cosmosdb-emulator `
    --mount "type=bind,source=$env:LOCALAPPDATA\CosmosDBEmulator\bind-mount,destination=C:\CosmosDB.Emulator\bind-mount" `
    --env AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 `
    --env AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true `
    --env AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=127.0.0.1 `
    --interactive `
    --tty `
    mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator
```

```powershell
cd /d %LOCALAPPDATA%\CosmosDBEmulatorCert
powershell .\importcert.ps1
```

