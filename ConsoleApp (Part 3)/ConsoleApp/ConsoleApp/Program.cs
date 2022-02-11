// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;
using ConsoleApp.Dtos;
using ConsoleApp.Helpers;
using MySql.Data.MySqlClient;
using RestSharp;


public static class Program
{
    private static Timer _timer = null;
    private const int _period = 300000; // 5min
    public static void Main()
    {
        // Create a Timer object that knows to call our TimerCallback
        // method once every 300000 milliseconds (5 min).
        _timer = new Timer(TimerCallback, null, 0, _period);
        // Wait for the user to hit <Enter>
        Console.ReadLine();
    }

    private static void TimerCallback(Object o)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        // Display the date/time when this method got called.
        Console.WriteLine("Process running: " + DateTime.Now + "\n");
        AsyncHelpers.RunSync(() => Process());

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Next update in : " + DateTime.Now.AddMilliseconds(_period));
    }

    private static async Task Process()
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            var connectionString = "server=localhost;uid=root;pwd=toor;database=etherscancodingtestdb;default command timeout=120;";
            var apiBaseUrl = "https://min-api.cryptocompare.com";

            #region 1. get all symbols from database

            var symbols = new List<string>();

            Console.WriteLine("1. Pulling token symbols from database\n");
            using (MySqlConnection conn =
                new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(@$"
                -- Get Token Symbols
                SELECT symbol FROM token;
            ", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Symbols:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"- {reader.GetString(0)}");
                        symbols.Add(reader.GetString(0));
                    }
                }

                conn.Close();
            }
            #endregion

            #region 2. get token's price from api

            var tokenPrices = new List<Tuple<string, decimal>>();

            Console.WriteLine("\n2. Pulling latest token price from API\n");
            Console.WriteLine("Prices:");
            foreach (var symbol in symbols)
            {
                Console.Write($"- {symbol} - ");
                var client = new RestClient(apiBaseUrl);
                var request = new RestRequest($"/data/price?fsym={symbol.ToUpper()}&tsyms=USD", Method.Get);
                var queryResult = await client.ExecuteAsync<TokenPrice>(request);

                Console.WriteLine(queryResult?.Data?.USD);
                if (queryResult?.Data?.USD != null)
                {
                    tokenPrices.Add(Tuple.Create(symbol, queryResult.Data.USD));
                }
            }
            #endregion

            #region 3. update token prices to database
            Console.WriteLine("\n3. Updating token price to database\n");
            var updateQuery = string.Empty;

            foreach (var tokenPrice in tokenPrices)
            {
                updateQuery += @$" UPDATE token SET price = '{tokenPrice.Item2}' WHERE symbol = '{tokenPrice.Item1}'; ";
            }

            using (MySqlConnection conn =
                new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(updateQuery, conn);

                var result = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"{result} rows affected\n\n");

                conn.Close();
            }

            #endregion
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.ToString());
        }
    }
}

