# Architettura di Open Middleware

Open Middleware riceve dati e documenti da altre applicazioni, li archivia e consente di consultarli per classificazione. I servizi comunicano tramite richieste HTTP e messaggi RabbitMQ.

## Acquisizione, archiviazione e consultazione

1. Un’applicazione invia dati o documenti a Wise.Generic. Wise.Generic invia il comando `StoreMessage` a RabbitMQ. Per un caricamento di file, Wise.Generic conserva il file nella cache e passa a Wise.Lake un indirizzo interno dal quale scaricarlo.
2. Wise.Lake riceve il comando. Conserva i documenti nel file system e le classificazioni in Microsoft SQL Server. Le [migrazioni del modulo di classificazione](../lake/wt_backend_lake/classification/Wise.Lake.Classification.Handler/Migrations/) descrivono lo schema di quel database.
3. Dopo il salvataggio, Wise.Lake pubblica `StoreNotification` su RabbitMQ. Un servizio di classificazione può usare la notifica per aggiungere tag al documento già archiviato. Il chart non include un’implementazione di quel servizio.
4. Wise.Generic consulta Wise.Lake via HTTP per leggere documenti e metadati. Il percorso temporaneo di scaricamento tra i due servizi non passa per il gateway pubblico.

## Integrazioni e limiti

| Componente | Stato |
|---|---|
| Keycloak ed Envoy | Il chart può esporre Keycloak per emettere i token ed Envoy per verificarli prima dell’accesso. L’accettazione dei token da parte dei servizi e il controllo dei permessi per spazio di lavoro restano da verificare. |
| Context Broker | Il worker attuale inoltra a FIWARE Orion gli eventi di Wise.Tasks. Il flusso dai dispositivi verso Wise.Generic non è implementato. |
| Archiviazione del broker | Il chart configura Orion-LD con MongoDB e Keycloak con PostgreSQL. Non configura la persistenza dei dati del broker in PostgreSQL. |
| Pulizia del datalake | Wise.Lake contiene un processo periodico che svuota il cestino. Il chart prevede Gesenu come processo separato, ma non ne distribuisce un’immagine. |
| Storico delle serie temporali | FIWARE QuantumLeap e CrateDB sono opzionali. Il chart non configura la sottoscrizione alle notifiche del broker. |

CityDB non rientra nell’installazione. Wise.Identity e Wise.Tasks non fanno parte del chart: i riferimenti dei servizi a queste dipendenze devono ancora essere risolti per l’installazione autonoma.

Il [README](../README.md#installazione-autonoma-in-kubernetes) descrive i prerequisiti del cluster e l’inizializzazione dei servizi.
