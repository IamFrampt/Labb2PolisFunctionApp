using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PolisAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace PolisAPI;

public class PolisAPIFunction
{
    static List<PolisCrimes> CrimeList = new();

    [FunctionName("CrimeByCity")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "City")] HttpRequest req,
        ILogger log)
    {
        try
        {
            string functionKey = req.Headers["Authorization"]; // Retrieve the Authorization header
            string searchWord = req.Query["searchWord"];
            string apiUrl = $"http://polisen.se/api/events?locationName={searchWord}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", functionKey); // Pass the function key in the Authorization header
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                string content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    log.LogError($"API request failed with status code: {response.StatusCode}");
                    return new StatusCodeResult((int)response.StatusCode);
                }

                List<PolisCrimes> crimeList = JsonConvert.DeserializeObject<List<PolisCrimes>>(content);

                return new OkObjectResult(crimeList);
            }
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
