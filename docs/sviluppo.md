# Sviluppo e compilazione

Le tre soluzioni principali richiedono il kit di sviluppo .NET 10. Alcune utility legacy, esterne alle soluzioni, dichiarano versioni diverse nei rispettivi file di progetto.

```sh
dotnet build wt_backend_generic.sln
dotnet build lake/wt_backend_lake/wt_backend_lake.sln
dotnet build lake/wt_backend_contextbroker/wt_backend_contextbroker.sln
```

I comandi richiedono accesso alle dipendenze dei file `nuget.config`. La configurazione attuale include un feed WiseTown. L’accesso alle librerie necessarie da un ambiente esterno non è verificato. Wise.Lake e il worker del Context Broker usano ancora librerie di altri moduli WiseTown. Il worker usa eventi di Wise.Tasks.

I file `appsettings.json` non forniscono credenziali o indirizzi degli ambienti esistenti. Per lo sviluppo, impostare le variabili d’ambiente richieste dal servizio, per esempio `rabbitmq__HostName`, `rabbitmq__UserName` e `rabbitmq__Password`. Per Wise.Generic e Wise.Lake, impostare anche le variabili `JWT__ValidIssuer`, `JWT__SigningKey` e gli indirizzi `middleware__IndexingServiceUri` e `middleware__IngestionServiceUri` quando richiesti. Wise.Lake richiede `ConnectionStrings__sqldb`; il worker del Context Broker richiede `orion__BaseUrl`. Usare soltanto dati di prova.

Il [README](../README.md#installazione-autonoma-in-kubernetes) descrive il chart, le dipendenze di produzione e i Secret. Microsoft SQL Server è una dipendenza proprietaria del modulo di classificazione. La licenza del progetto non copre le immagini o i pacchetti di terze parti.
