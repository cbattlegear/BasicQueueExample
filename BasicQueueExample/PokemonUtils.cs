using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BasicQueueExample
{
    /// <summary>
    /// Holding class based on the stucture of our initial returned data from the PokeAPI
    /// </summary>
    internal class Results
    {
        [JsonPropertyName("results")]
        public List<Pokemon> Pokemons { get; set; }
    }

    /// <summary>
    /// Further deserialization based on the JSON structure provided by the pokemon list provided by PokeAPI
    /// </summary>
    internal class Pokemon
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// The class used to pass data between our functions using both the queue and sql tables. 
    /// </summary>
    public class PokemonQueueItem
    {
        public string Name { get; set; }
        public DateTime LastProcessed { get; set; }
    }
}
