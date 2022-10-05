using System;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;

namespace BasicQueueExample
{
    public static class GetAllPokemon
    {
        private static readonly HttpClient client = new HttpClient();



        /// <summary>
        /// This function is used to load up the initial scheduling data set. It creates the needed tables, loads data to staging from the API
        /// and then merges the data into the "production" table. This merge method is used to prevent losing last updated data in the production
        /// table but may be over the top for most situations
        /// 
        /// This could possible all be handled via SQL Bindings but because I am doing the table creation and have the connection setup already 
        /// it made more sense to just run the queries directly. 
        /// 
        /// </summary>
        /// <param name="myTimer">Timer Trigger, currently set to run on the 3rd day of the month at 07:02:16 AM</param>
        /// <param name="log">Azure Functions log object to send telemetry</param>
        [FunctionName("GetAllPokemon")]
        public static void Run([TimerTrigger("16 2 7 3 * *")]TimerInfo myTimer, ILogger log)
        {
            string sql_azure_connection_string = Environment.GetEnvironmentVariable("SQL_AZURE_CONNECTION_STRING", EnvironmentVariableTarget.Process);

            try
            {
                using (SqlConnection cn = new SqlConnection(sql_azure_connection_string))
                {
                    cn.Open();
                    // Make sure the staging table exists
                    string query = "IF OBJECT_ID(N'dbo.PokemonStg', N'U') IS NULL BEGIN CREATE TABLE dbo.PokemonStg([Name] varchar(64) not null); END;";
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // The API call to get the objects, currently targeting the 151 original pokemon
                    var streamTask = client.GetStreamAsync("https://pokeapi.co/api/v2/pokemon?limit=151&offset=0");

                    // Using a holding class called Results to directly process the JSON, we throw this away and get the 
                    // list out of the object right away and use that moving forward
                    var pokemon = JsonSerializer.Deserialize<Results>(streamTask.Result).Pokemons;

                    // Make sure the prod table exists
                    query = "IF OBJECT_ID(N'dbo.Pokemon', N'U') IS NULL BEGIN CREATE TABLE dbo.Pokemon([Name] varchar(64) not null, [LastProcessed] datetime2 DEFAULT '2000-01-01', CONSTRAINT PK_Pokemon_PokemonName PRIMARY KEY CLUSTERED ([Name])); END;";
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Empty out the staging table
                    query = "TRUNCATE TABLE dbo.PokemonStg;";
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insert all pokemon into our staging table
                    query = "INSERT INTO dbo.PokemonStg ([Name]) VALUES (@Name);";
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        foreach(Pokemon p in pokemon)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Name", p.Name);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // UPSERT via merge to make sure we don't lose last updated time
                    query = @"
MERGE [Pokemon] AS [Target]
USING (SELECT [Name] FROM [PokemonStg]) as [Source]
    ON [Target].[Name] = [Source].[Name]
WHEN NOT MATCHED THEN
    INSERT ([Name]) VALUES ([Source].[Name]);
                        ";
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    cn.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }

            log.LogInformation($"GetAllPokemon timer function executed at: {DateTime.Now}");
        }
    }
}
