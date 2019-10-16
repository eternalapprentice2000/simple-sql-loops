using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class QueryResult
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    class Program
    {
        private static SqlConnection _conn;
        private static string ConnectionString = "ThisIsAConnectionString";

        private static SqlConnection connection
        {
            get
            {
                if (_conn == null || _conn.State != ConnectionState.Open)
                {
                    _conn = new SqlConnection(ConnectionString);
                    _conn.Open();
                }
                return _conn;
            }
        }

        public static async Task GetSqlQuery(Func<SqlDataReader, bool> CallBack)
        {
            var sql = "select id, description from Table where something = @value";

            using (var sqlCommand = new SqlCommand(sql, connection))
            {
                var parameters = new[]
                {
                    new SqlParameter("@value", SqlDbType.NVarChar){Value = "testing"}
                };

                sqlCommand.Parameters.AddRange(parameters);

                using (var dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    var isSuccess = CallBack(dataReader);

                    if (!isSuccess)
                    {
                        // something bad happened
                        throw new Exception("Something Bad Happened");
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var isQueryComplete = true;

            // loop condition
            var loopCounter = 10; // 
            var locker = new object();

            while (loopCounter > 0)
            {
                loopCounter--; //decrease by one

                if (isQueryComplete)
                {

                    lock (locker)
                    {
                        isQueryComplete = false;
                    }
                    
                    Console.WriteLine("Starting Query");
                    var task = GetSqlQuery((dataReader) =>
                                {
                                    // THIS IS THE CALLBACK
                                    // this is what happens after the query is complete
                                    // get the result with data reader

                                    var results = new List<QueryResult>();

                                    while (dataReader.Read())
                                    {
                                        Console.WriteLine("Found Results");
                                        results.Add(new QueryResult
                                        {
                                            Id = dataReader.GetString(0),
                                            Description = dataReader.GetString(1)
                                        });
                                    }

                                    // DO SOMETHING WITH THE RESULTS HERE
                                    lock (locker)
                                    {
                                        isQueryComplete = true;
                                    }
                                    // do check to see if something went wrong

                                    return true;
                                });
                }
                else
                {
                    Console.WriteLine("Skipped Query... the previous query was not complete");

                }

                Task.Delay(10 * 1000); // 10 seconds

            }
        }
    }
}

