# Open Middleware

Servizi per acquisire, archiviare e classificare dati e documenti.

Open Middleware consente a un’amministrazione di raccogliere dati e documenti da più applicazioni. I servizi li archiviano e permettono di consultarli per classificazione. Un’applicazione può ricevere una notifica dopo il salvataggio, per esempio per aggiungere tag al documento.

## Stato e titolarità

Il Comune di Perugia (`c_g478`) è il titolare del codice. Il [file LICENSE](LICENSE) contiene il testo integrale della GNU Affero General Public License, versione 3 (`AGPL-3.0-only`).

Il progetto è in sviluppo. La compilazione da sorgenti e l’installazione autonoma non sono verificate. Il [documento di sviluppo](docs/sviluppo.md) descrive i comandi e le dipendenze ancora necessarie. Il [changelog](CHANGELOG.md) raccoglie le modifiche non ancora rilasciate.

La [bozza della scheda Developers Italia](docs/publiccode-bozza.yml) non è pronta per l’indicizzazione. L’identità del soggetto incaricato della pubblicazione, l’indirizzo pubblico del repository, il manutentore e la licenza dei metadati restano da confermare. Il [file SECURITY.md](SECURITY.md) descrive il canale riservato da configurare prima del rilascio.

## Struttura

| Percorso | Contenuto |
|---|---|
| [`generic/`](generic/) | Wise.Generic riceve dati e documenti. |
| [`lake/wt_backend_lake/`](lake/wt_backend_lake/) | Wise.Lake archivia i documenti e gestisce le classificazioni. La directory include le migrazioni del database di classificazione. |
| [`lake/wt_backend_contextbroker/`](lake/wt_backend_contextbroker/) | Il worker attuale inoltra a FIWARE Orion gli eventi di Wise.Tasks. |
| [`kube/`](kube/) | Il chart Helm genera le risorse Kubernetes per i servizi e le relative dipendenze. |

Il [documento di architettura](docs/architettura.md) descrive i flussi di salvataggio, consultazione e notifica. L’[inventario dei componenti di terze parti](docs/componenti-terze-parti.md) distingue i sorgenti inclusi dalle dipendenze installate separatamente.

## Installazione autonoma in Kubernetes

Il [chart Helm](kube/Chart.yaml) genera le risorse Kubernetes. I [valori del chart](kube/values.yaml) separano la configurazione dai Kubernetes Secret.

1. Usare un namespace dedicato. Il cluster deve applicare le `NetworkPolicy`. Configurare una classe di archiviazione persistente e verificare backup e ripristino di database, RabbitMQ e volumi dei file. Il chart non configura alta disponibilità per i servizi con dati persistenti.
2. Impostare `storageClass` e le immagini approvate tramite digest in un file di valori locale. Usare immagini compatibili con le configurazioni del chart: PostgreSQL con `PGDATA` esplicito, Keycloak con `start` e porta di gestione `9000`, MongoDB e Orion-LD con autenticazione MongoDB, RabbitMQ con `RABBITMQ_DEFAULT_USER`, Microsoft SQL Server per `amd64`. Verificare licenza e capacità di Microsoft SQL Server.
3. Creare i Secret nel namespace. Non includere credenziali nel file di valori o nelle immagini.
4. Generare i manifesti in locale prima dell'installazione: `helm template open-middleware ./kube --namespace open-middleware -f /percorso/valori-produzione.yaml`. Verificare l'avvio e i flussi in un cluster di collaudo prima di usare i manifesti in produzione.

### Collaudo prima dell’uso in produzione

- Verificare che un ambiente esterno a WiseTown ripristini le dipendenze e compili le tre soluzioni.
- Caricare un file di prova tramite Wise.Generic. Verificare che Wise.Lake lo archivi, che il file sia consultabile e che RabbitMQ consegni la notifica dopo il salvataggio.
- Verificare che le richieste senza token valido e quelle senza i permessi per il documento siano respinte.
- Ripristinare una copia di prova del database di classificazione e del volume dei documenti. Verificare che i documenti e le classificazioni siano entrambi consultabili dopo il ripristino.
- Per ogni integrazione abilitata, verificare il flusso con dati di prova. Il semplice avvio dei contenitori non verifica l’integrazione.

### Secret e inizializzazione

| Secret | Chiavi e uso |
|---|---|
| `postgresql` | `user`, `password`. Account di amministrazione PostgreSQL. |
| `keycloak` | `db-user`, `db-password`, `admin-user`, `admin-password`. Creare il database `keycloak` e assegnarlo all'account dedicato prima dell'avvio di Keycloak. Limitare l'accesso amministrativo alla rete privata. |
| `mssql` | `edition`, `sa-password`. Creare il database di classificazione e un account dedicato prima di abilitare Wise.Lake. Non usare l'account `sa` per l'applicazione. |
| `mongodb` | `admin-user`, `admin-password`. Account di inizializzazione MongoDB. |
| `orion` | `mongodb-user`, `mongodb-password`. Creare l'account con accesso al database `orion` prima di avviare Orion-LD. Verificare il database di autenticazione richiesto dall'immagine scelta. |
| `rabbitmq` | `user`, `password`. Account iniziale del broker. Configurare account e permessi specifici per produttori e sottoscrittori. |
| `generic-runtime`, `lake-runtime`, `contextbroker-runtime` | Variabili riservate dei servizi .NET. Fornire le credenziali RabbitMQ e le impostazioni di autenticazione richieste dall'immagine. Per Lake fornire anche `ConnectionStrings__sqldb`. |
| `gesenu-runtime`, `classifier-runtime` | Configurazione riservata dei worker, secondo il contratto delle immagini scelte. |

Per l'inizializzazione in più passaggi, disabilitare `workloads.keycloak.enabled` e `workloads.orion.enabled` finché i relativi database e account non sono pronti. Il chart non crea gli account applicativi dei database.

### Pubblicazione e integrazioni

- **Keycloak e controllo di accesso.** Impostare dominio del login, dominio delle interfacce applicative, classe Ingress, namespace del controller e riferimenti ai Secret TLS. Il controller deve forzare HTTPS e sovrascrivere gli header `X-Forwarded-*`. L'Ingress pubblica solo il realm configurato e le sue risorse statiche. Envoy verifica firma, scadenza, emittente e destinatario dei token prima di inoltrare le richieste. La verifica del token da parte delle applicazioni resta necessaria. Non abilitare il gateway prima di verificare ruoli, workspace e compatibilità dei token Keycloak con le immagini applicative.
- **Wise.Generic e Wise.Lake.** I volumi persistenti conservano la cache dei caricamenti e i documenti del datalake. Il percorso `/api/i/{id}/once` resta interno. Abilitare Wise.Generic e Wise.Lake solo con immagini che funzionano senza Wise.Identity. Configurare e verificare i flussi di salvataggio, lettura e notifica su RabbitMQ.
- **Context Broker.** Il broker FIWARE Orion-LD resta interno e conserva le entità in MongoDB. PostgreSQL serve Keycloak: il chart non implementa il salvataggio delle entità del broker in PostgreSQL previsto dal diagramma. Il worker copiato usa Wise.Tasks e non acquisisce dati dai dispositivi. Abilitare il worker solo dopo aver verificato una versione autonoma e il flusso di inoltro verso Wise.Generic.
- **Gesenu e classificazione.** Il chart accetta immagini dedicate, ma non le fornisce. Wise.Lake contiene già un worker per la pulizia del cestino; non eseguire un secondo worker sulle stesse operazioni. Configurare sottoscrittori e permessi RabbitMQ per la classificazione e per i servizi esterni. Il chart non espone RabbitMQ fuori dal namespace.
- **FIWARE QuantumLeap.** Il chart può distribuire QuantumLeap e il suo archivio CrateDB come opzione. Verificare le immagini compatibili e il formato delle notifiche del broker. La configurazione della sottoscrizione, o di un adattatore se necessario, non è inclusa.

CityDB, Wise.Identity e Wise.Tasks non fanno parte del chart.
