using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FunctionSalud.Models
{
    public class Documentos
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("paciente")]
        public string Paciente { get; set; }
        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }

        public string resultado { get; set; }

        [JsonProperty("pasar")]
        public string Pasar { get; set; }
    }
}
