using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Azure.Devices;

namespace FunctionSalud.Servicio
{
    public class Analisis
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=hubLandivar.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=wdmVTws0rIx8gm7p09NqR6fY0A9Mq97lJRWF+rI5ugI=";
        static string targetDevice = "esp8266";
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("b2c6759451904d2f97411f647bddcfbc");
        private static readonly Uri endpoint = new Uri("https://salud.cognitiveservices.azure.com/");

        public static string healthExample(string doc, string id, string pasar)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);

            List<TextDocumentInput> batchInput = new List<TextDocumentInput>()
            {
                new TextDocumentInput(id,doc)
                {
                    Language = "en"
                }
            };

            AnalyzeHealthcareEntitiesOptions options = new AnalyzeHealthcareEntitiesOptions()
            {
                IncludeStatistics = true
            };

            // start analysis process
            AnalyzeHealthcareEntitiesOperation healthOperation = client.StartAnalyzeHealthcareEntities(batchInput, options);

            // wait for completion with manual polling
            TimeSpan pollingInterval = new TimeSpan(1000);

            while (true)
            {
                healthOperation.UpdateStatus();
                if (healthOperation.HasCompleted)
                {
                    break;
                }

                Thread.Sleep(pollingInterval);
            }

            string respuesta = "";

            // view operation results
            foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in healthOperation.GetValues())
            {
                foreach (AnalyzeHealthcareEntitiesResult result in documentsInPage)
                {
                    respuesta += $"Reconoció las siguientes {result.Entities.Count} entidades de salud: ";

                    // view recognized healthcare entities
                    foreach (HealthcareEntity entity in result.Entities)
                    {
                        respuesta += $"{entity.Text}, ";
                    }

                    respuesta += $"\r\n" + $"\r\n" + $"Encontramos {result.EntityRelations.Count} relaciones en el documento actual: ";

                    // view recognized healthcare relations
                    foreach (HealthcareEntityRelation relations in result.EntityRelations)
                    {
                        respuesta += $"{relations.RelationType}: ";
                        // view relation roles
                        foreach (HealthcareEntityRelationRole role in relations.Roles)
                        {
                            respuesta += $"{role.Entity.Text}, ";
                        }
                    }
                }
            }
            return respuesta;
        }

        public async static Task SendCloudToDeviceMessageAsync(string pasar)
        {
            var commandMessage = new
            Message(Encoding.ASCII.GetBytes(pasar));
            await serviceClient.SendAsync(targetDevice, commandMessage);
        }

        public static string Main(string doc, string id, string pasar)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            string prueba = healthExample(doc, id, pasar);
            SendCloudToDeviceMessageAsync(prueba).Wait();
            return prueba;
        }

    }
}
