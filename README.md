# SqlServer-Debezium-Kafka-NetCore
Exemplo de como aplicar o padrão de estrangulamento do legado utilizando a stack SQL SERVER + Debezium + KAFKA + Consumer feito em .Net Core. 

## Docker compose 
Primeiro passo é executar o docker compose:
`
docker-compose up -d
`

## Executar os scripts de Banco de dados
Acessar a base de dados connection string:
`
localhost,1433
`
- Executar o arquivo `create-database-advisor-legacy.sql` para criar a base de dados do legado.
- Executar o arquivo `create-database-advisor-new.sql` para criar a base de dados nova. 
- Executar o arquivo `enable-CDC-advisor-legancy.sql` para habilitar o CDC na base de dados do legado. 

## Configurar o Debezium 
```curl
curl --location --request POST 'http://localhost:8083/connectors' \
--header 'Content-Type: application/json' \
--data-raw '{
    "name": "sqlserverAdvisorConnector",
    "config" : {
        "connector.class": "io.debezium.connector.sqlserver.SqlServerConnector",
        "tasks.max" : 1,
        "database.hostname": "mssql-server",
        "database.port": 1433,
        "database.user": "sa",
        "database.password": "P@ssw0rd",
        "database.dbname" : "AdvisorLegacy",
        "database.server.name" : "mssql-server",
        "database.history.kafka.bootstrap.servers" : "kafka:9092",
        "database.history.kafka.topic" : "dbhistory.ADVISOR_CLIENT_ACCOUNT",
        "decimal.handling.mode": "string"
    }
}'
```
Acessar o Kafdrop para visualizar os tópicos do Kafka: `http://localhost:9005`

## Executar o consumero em .Net Core
Executar o projeto `consumer.sln`. 

## Na prática

Com o ambiente configurado e executando o consumer qualquer comando de insert, update e delete no banco de dados do legado será replicado de maneira automática para o novo banco de dados. 

Seguem os comandos para testes: 

```sql
insert into ADVISOR_CLIENT_ACCOUNT values (1,405060,1,GETDATE(),null);
UPDATE ADVISOR_CLIENT_ACCOUNT SET SINACOR_ADVISOR_ID = 2, UPDATE_DATE = GETDATE() WHERE ACCOUNT_ID = 405060; 
delete from ADVISOR_CLIENT_ACCOUNT WHERE ACCOUNT_ID = 405060;
```

## Refêrencias

- https://martinfowler.com/bliki/StranglerFigApplication.html
- https://debezium.io/documentation/faq/
- https://www.tiagotartari.net/estrangulamento-de-legado-na-pratica-com-codigo-criando-uma-um-servico-de-alta-performance/
- https://www.dirceuresende.com/blog/sql-server-como-monitorar-e-auditar-alteracoes-de-dados-em-tabelas-utilizando-change-data-capture-cdc/
