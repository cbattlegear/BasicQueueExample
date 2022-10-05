using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;


namespace BasicQueueExample
{
    public static class ProcessPokemonQueue
    {
        private static readonly HttpClient client = new HttpClient();


        /// <summary>
        /// This function takes an individual queue item, uses the information provided in the item to download data from a rest api and then 
        /// take the data downloaded and upload it to an Azure Storage location, once uploaded it updates a SQL row showing the time the data 
        /// was updated.
        /// 
        /// This function triggers off of items placed into the queue using the Queue Trigger binding, this auto deserializes into the PokemonQueueItem type
        /// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp
        /// 
        /// I utilized the IBinder interface which allows for declarative binding based on any computed values, I use this to have a date based folder and a 
        /// file name based on queue contents.
        /// https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#binding-at-runtime
        /// 
        /// The Sql Output binding is used to update the row once completed, this output binding serializes the data, then uses openjson and merge to upsert
        /// the data into the list of items. Useful for minimal coding, may not be highly efficient on large datasets.
        /// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql-output?tabs=in-process&pivots=programming-language-csharp
        /// 
        /// </summary>
        /// <param name="pokemon">The Queue Trigger binding that fires the function based on queue message of type PokemonQueueItem</param>
        /// <param name="binder">The Declarative Storage Output binding using the IBinder interface to output the data to a specific folder/file</param>
        /// <param name="pokemonupdate">The SQL Output binding to update the item in sql with the Last Updated time</param>
        /// <param name="log">Azure Function log access</param>
        /// <returns></returns>
        [FunctionName("ProcessPokemonQueue")]
        public static async Task Run([QueueTrigger("pokemon-queue", 
            Connection = "STORAGE_ACCOUNT")]PokemonQueueItem pokemon,
            IBinder binder,
            [Sql("dbo.Pokemon", ConnectionStringSetting = "SQL_AZURE_CONNECTION_STRING")] IAsyncCollector<PokemonQueueItem> pokemonupdate,
            ILogger log)
        {
            // Make our REST API call to get our data contents
            var stringTask = client.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{pokemon.Name}");

            // Get the current date for our folder structure and for updating SQL
            var date = DateTime.Now;

            // Bind to our calculated blob storage area, the connection string is set via the Connection property and uses the matching
            // environment variable set at the function level (or local.settings.json for development)
            var attribute = new BlobAttribute($"pokemon/{date.Year}/{date.Month}/{date.Day}/{pokemon.Name}.json", FileAccess.Write);
            attribute.Connection = "DATA_LAKE";

            // Open our bound blob and use the TextWriter class to write the contents to the blob
            using (var poke_file = await binder.BindAsync<TextWriter>(attribute))
            {
                poke_file.Write(await stringTask);
            }

            // Set our last processed date in the newly processed data and use the SQL binding to update the row.
            pokemon.LastProcessed = date;
            await pokemonupdate.AddAsync(pokemon);

            log.LogInformation($"C# Queue trigger ProcessPokemonQueue function processed: {pokemon.Name}");
        }
    }
}
