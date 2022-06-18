using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunctionSalud.Servicio;
using FunctionSalud.Models;
using Microsoft.Azure.Devices;
using System.Text;

namespace FunctionSalud
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Analisis/")] 
        HttpRequest req,
            [CosmosDB(
                databaseName: "dbparcial2",
                collectionName: "Documentos",
                ConnectionStringSetting = "connectionJuan")]
                IAsyncCollector<Documentos> itemsDocumentos,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Documentos doc = JsonConvert.DeserializeObject<Documentos>(requestBody);
                var response = Analisis.healthExample(doc.Descripcion,doc.ID, doc.Pasar);
                var documento = new Documentos
                {
                    Descripcion = doc.Descripcion,
                    ID = doc.ID,
                    Paciente = doc.Paciente,
                    resultado = response,
                    Pasar = "1"
                };
                await itemsDocumentos.AddAsync(documento);


                string connectionString = "HostName=hubLandivar.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=wdmVTws0rIx8gm7p09NqR6fY0A9Mq97lJRWF+rI5ugI=";
                string targetDevice = "esp8266";

                ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
                string mensaje = documento.Pasar;
                log.LogInformation(mensaje);
                var commandMessage = new Message(Encoding.ASCII.GetBytes(mensaje));
                commandMessage.Ack = DeliveryAcknowledgement.Full;
                await serviceClient.SendAsync(targetDevice, commandMessage);




                return new OkObjectResult(response);
            }
            catch (Exception e)
            {

                return new BadRequestObjectResult(e);
            }
        }
    }
}



