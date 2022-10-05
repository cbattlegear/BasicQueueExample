using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BasicQueueExample
{
    [StorageAccount("STORAGE_ACCOUNT")]
    public static class SchedulePokemonQueue
    {

        /// <summary>
        /// This function takes all of the items in our production table and adds them to the queue to be processed
        /// I am heavily using Azure Function Bindings here to simplify the code:
        /// 
        /// The [Queue("pokemon-queue")] is the output binding that puts each item based on the PokemonQueueItem type onto the queue
        /// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp
        /// 
        /// The Sql item is the input binding that queries the table and converts the info to the PokemonQueueItem type which allows us to 
        /// directly land the data onto the queue without manually programming any serialization/deserialization processes. 
        /// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql-input?tabs=in-process&pivots=programming-language-csharp
        /// 
        /// </summary>
        /// <param name="myTimer">Timer trigger for the function currently set to run every day at 12:13 AM</param>
        /// <param name="collector">The Queue Output binding, note the storage account it uses is set by the StorageAccount attribute above</param>
        /// <param name="queueitems">The SQL Input binding, currently it queries all items in the Pokemon table</param>
        /// <param name="log">Azure Function log access</param>
        [FunctionName("SchedulePokemonQueue")]
        // Runs every day at 12:13 AM
        public static void Run([TimerTrigger("0 13 0 * * *")] TimerInfo myTimer,
            [Queue("pokemon-queue")] ICollector<PokemonQueueItem> collector,
            [Sql("SELECT [Name], [LastProcessed] FROM [Pokemon]", CommandType = System.Data.CommandType.Text, ConnectionStringSetting = "SQL_AZURE_CONNECTION_STRING")] IEnumerable<PokemonQueueItem> queueitems,
            ILogger log)
        {
            // Loop through all items queried by the SQL Input Binding and place them on the Queue Output binding
            foreach (var pokemon in queueitems) {
                collector.Add(pokemon);
            }
            log.LogInformation($"SchedulePokemonQueue function executed at: {DateTime.Now}");
        }
    }
}
