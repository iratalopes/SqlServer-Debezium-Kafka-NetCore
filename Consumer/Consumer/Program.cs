using Confluent.Kafka;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        public const string CreatedOld = "r";
        public const string Created = "c";
        public const string Updated = "u";
        public const string Deleted = "d";
        public const string ConnectionString = "Server=localhost;Database=Advisor;User Id=sa;Password=P@ssw0rd;";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Inicio");
                StartAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Task StartAsync()
        {
            var conf = new ConsumerConfig
            {
                GroupId = "group-id-new",
                BootstrapServers = "localhost:19092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe("mssql-server.dbo.ADVISOR_CLIENT_ACCOUNT");

                try
                {
                    while (true)
                    {
                        var message = c.Consume();

                        var messageValue = message?.Message?.Value;

                        if (messageValue != null)
                        {
                            dynamic data = JsonConvert.DeserializeObject(messageValue);

                            var payload = data?.payload;
                            var action = payload?.op?.Value;

                            if (action == Created || action == CreatedOld)
                            {
                                var body = payload?.after;
                                Insert(body);
                            }

                            if (action == Updated)
                            {
                                var body = payload?.after;
                                Update(body);
                            }

                            if (action == Deleted)
                            {
                                var body = payload?.before;
                                Delete(body);
                            }

                            Console.WriteLine("======================================================== \n");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    c.Close();
                }
            }

            return Task.CompletedTask;
        }

        private static void Insert(dynamic payload)
        {
            var clientId = payload?.CLIENT_ID.Value;
            var advisorId = payload?.SINACOR_ADVISOR_ID.Value;
            var account = payload?.ACCOUNT_ID.Value;
            var enabled = payload?.ENABLED.Value;
            var registerDate = UnixTimeToDateTime((long)payload?.REGISTER_DATE.Value);

            using (var connection = new SqlConnection(ConnectionString))
            {
                var sql = $"INSERT INTO ADVISOR_CLIENT_ACCOUNT VALUES (@clientId,@advisorId,@account,@enabled,@registerDate,null);";

                connection.Execute(sql, new {
                    clientId,
                    advisorId,
                    account,
                    enabled,
                    registerDate
                });
                Console.WriteLine($"Criado - Conta: {account}");
            }
        }


        private static void Update(dynamic payload)
        {
            var clientId = payload?.CLIENT_ID.Value;
            var advisorId = payload?.SINACOR_ADVISOR_ID.Value;
            var account = payload?.ACCOUNT_ID.Value;
            var enabled = payload?.ENABLED.Value;
            var updateDate = UnixTimeToDateTime((long)payload?.UPDATE_DATE.Value);

            using (var connection = new SqlConnection(ConnectionString))
            {
                var sql = $"UPDATE ADVISOR_CLIENT_ACCOUNT SET CLIENT_ID = @clientId, SINACOR_ADVISOR_ID = @advisorId,ENABLED=@enabled,UPDATE_DATE=@updateDate WHERE ACCOUNT_ID = @account;";

                connection.Execute(sql, new
                {
                    clientId,
                    advisorId,
                    account,
                    enabled,
                    updateDate
                });
                Console.WriteLine($"Atualizado - Conta: {account}");
            }
        }


        private static void Delete(dynamic payload)
        {
            var account = payload?.ACCOUNT_ID.Value;

            using (var connection = new SqlConnection(ConnectionString))
            {
                var sql = $"DELETE FROM ADVISOR_CLIENT_ACCOUNT WHERE ACCOUNT_ID = @account";

                connection.Execute(sql, new
                {
                    account
                });
                Console.WriteLine($"Deletado - Conta: {account}");
            }
        }

        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixtime).ToLocalTime();
            return dtDateTime;
        }
    }
}
