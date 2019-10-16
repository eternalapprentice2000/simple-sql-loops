using System;
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

        public static async Task<bool> FakeSqlQuery(Func<bool, bool> CallBack)
        {
            var rnd = new Random();
            var counter = rnd.Next(1, 20);

            await Task.Delay(counter * 1000);

            return CallBack(true);
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

        private static void Log(string msg)
        {
            msg = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] - {msg}";
            Console.WriteLine(msg);
        }

        static void Main(string[] args)
        {
            var isQueryComplete = true;

            // loop condition
            var loopCounter = 10; // 
            var locker = new object();

            while (loopCounter > 0)
            {
                Log("Attempting Query Run...");
                loopCounter--; //decrease by one

                if (isQueryComplete)
                {
                    Log("Running Query");
                    lock (locker)
                    {
                        isQueryComplete = false;
                    }
                    
                    Log("Starting Query");

                    // Faked For Testing
                    Func<bool,bool> fakeCallBack = (b) =>
                    {
                        Log("Query Complete");
                        lock (locker)
                        {
                            isQueryComplete = true;
                        }
                        
                        return b;
                    };

                    var fakeSqlTask = FakeSqlQuery(fakeCallBack);

                    /*********************
                     * Real query.. i commented out for testing

                    Func<SqlDataReader, bool> callback = (dataReader) =>
                    {
                        // THIS IS THE CALLBACK
                        // this is what happens after the query is complete
                        // get the result with data reader
                        Log("Found Results");
                        var results = new List<QueryResult>();

                        while (dataReader.Read())
                        {
                            
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
                    };

                    var task = GetSqlQuery(callback);
                    ******************/
                }
                else
                {
                    Log("Skipped Query... the previous query was not complete");

                }

                Task.Delay(10 * 1000).GetAwaiter().GetResult(); // 10 seconds

            }
        }
    }
}

