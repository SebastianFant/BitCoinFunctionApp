using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Bitcoin.Function
{
    public static class OrchestratorFunction
    {
        [FunctionName("OrchestratorFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, [Queue("testqueue"),StorageAccount("AzureWebJobsStorage")] ICollector<string> msg)
        {
            string outputs ="{\"BitcoinDTO\":";

            
            outputs += (await context.CallActivityAsync<string>(nameof(GetBitcoinRate), "https://api.coindesk.com/v1/bpi/currentprice.json"));
            outputs += ",\"ExchangeRateDTO\":";
            outputs += (await context.CallActivityAsync<string>(nameof(GetExchangeRate), "https://v6.exchangerate-api.com/v6/6c3a3f678dca8ef7edfd1369/latest/USD"));
            outputs += "}";
            
            msg.Add(outputs);
            
            
        }

        [FunctionName(nameof(GetBitcoinRate))]
        public static async Task <string> GetBitcoinRate([ActivityTrigger] string url, ILogger log)
        {
            using var client = new HttpClient(); 
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode){
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            log.LogInformation($"Saying hello");
            return "";
        }
                [FunctionName(nameof(GetExchangeRate))]
        public static async Task <string> GetExchangeRate([ActivityTrigger] string url, ILogger log)
        {
            using var client = new HttpClient(); 
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode){
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            log.LogInformation($"Saying hello");
            return "";
        }

        [FunctionName("OrchestratorFunction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}