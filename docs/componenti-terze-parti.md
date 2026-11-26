# Componenti di terze parti

Questo inventario distingue il codice presente nella cartella dai pacchetti e dai servizi che l’installazione recupera separatamente. Non sostituisce le attribuzioni delle singole dipendenze.

| Componente | Uso e fonte | Licenza e distribuzione |
|---|---|---|
| `FiWare.SmartDataModels` | La [libreria inclusa nei sorgenti](../lake/wt_backend_contextbroker/Fiware/FiWare.SmartDataModels/FiWare.SmartDataModels.csproj) fornisce i modelli usati dal worker. | La presenza dei file nella copia non prova la titolarità. Verificare provenienza, licenza e attribuzioni prima di applicare intestazioni al codice. |
| `FIWARE.Orion.Client` | Il [progetto del worker](../lake/wt_backend_contextbroker/Wisetown.ContextBroker.Worker/Wisetown.ContextBroker.Worker.csproj) dichiara il pacchetto per le chiamate a Orion. | Verificare le condizioni della versione risolta e conservare le attribuzioni applicabili. |
| Pacchetti Wise e Hikyaku | I file di progetto li dichiarano come dipendenze NuGet. Il [documento di sviluppo](sviluppo.md) descrive il feed configurato. | Verificare i termini delle versioni risolte e la disponibilità per chi compila senza accesso ai servizi WiseTown. |
| Microsoft SQL Server | Il modulo di classificazione di Wise.Lake lo usa come database. | Dipendenza proprietaria. L’installazione richiede un’edizione e condizioni d’uso adatte all’ambiente. |
| RabbitMQ, Keycloak, PostgreSQL, MongoDB, Envoy, Orion-LD, QuantumLeap e CrateDB | Il [chart Helm](../kube/values.yaml) configura i servizi associati alle immagini selezionate. | Il chart non include le immagini. Verificare licenza, edizione e condizioni di distribuzione delle immagini effettive prima di ridistribuirle. |

Il controllo deve includere le dipendenze transitive e le immagini effettive, non soltanto i nomi dichiarati nel progetto. Conservare gli avvisi di copyright e i testi di licenza richiesti dai componenti incorporati. Applicare gli identificatori di licenza SPDX solo dopo aver verificato titolarità e compatibilità.
