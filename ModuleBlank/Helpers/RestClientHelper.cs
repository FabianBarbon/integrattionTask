using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JupiterPlugin.Models;
using Newtonsoft.Json;
using RestSharp;

namespace JupiterPlugin.Helpers
{
    internal class RestClientHelper
    {
        public static async Task<bool> CreateDynamicLayout(object requestData, string api_host)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var flagSuccess = false;
                    var endpoint = $"{api_host}/api/canvas/CreateDynamicLayout";

                    var requestDataJson = JsonConvert.SerializeObject(requestData);

                    var payload = new StringContent(requestDataJson, Encoding.UTF8, "application/json");

                    var result = await client.PostAsync(endpoint, payload);
                    var resultContent = await result.Content.ReadAsStringAsync();



                    // Deserializar la respuesta
                    var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);

                    // Verificar el estado de la respuesta
                    if (response != null && response.status)
                    {
                        // Verificar el éxito de la creación del diseño dinámico
                        if (response.message != null && response.message.success != null && response.message.success.Count > 0)
                        {
                            Console.WriteLine("Ventanas creadas exitosamente:");
                            foreach (var successItem in response.message.success)
                            {
                                Console.WriteLine($"ID de ventana: {successItem.idWindow}, Respuesta: {successItem.response}");
                            }
                            flagSuccess = true;
                        }
                        else
                        {
                            Console.WriteLine("No se crearon ventanas con éxito.");
                        }

                        // Verificar el estado del guardado del diseño
                        if (response.message != null && response.message.layoutSave != null && response.message.layoutSave.Count > 0)
                        {
                            var layoutSaveItem = response.message.layoutSave[0];
                            Console.WriteLine($"Estado de guardado del diseño: {layoutSaveItem.savedStatus}, Respuesta: {layoutSaveItem.response}, Diseño: {layoutSaveItem.layoutName}, Pared: {layoutSaveItem.wall}");
                        }
                        else
                        {
                            Console.WriteLine("No se obtuvo información de guardado del diseño.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("La respuesta del servidor indica un estado falso.");
                    }

                    return flagSuccess;
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción, loguear, etc.
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
