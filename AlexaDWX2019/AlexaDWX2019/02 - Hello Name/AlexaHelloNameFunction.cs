using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET;

namespace AlexaDWX2019
{
    public static class AlexaHelloNameFunction
    {
        [FunctionName("AlexaHelloNameFunction")]
        public static async Task<SkillResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/helloname")] HttpRequest req)
        {
            // read content as skill request
            var payload = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(payload);

            // get type of request
            var requestType = skillRequest.GetRequestType();

            if (requestType == typeof(LaunchRequest))
            {
                return ResponseBuilder
                    .Ask("Herzlich willkommen. Ich kann Leute begrüßen. Bitte nenne mir einen Namen.",
                        new Reprompt("Wen soll ich begrüßen?"));
            }

            if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                if (intentRequest.Intent.Name == "GreetingIntent")
                {
                    var name = intentRequest.Intent.Slots["name"].Value;
                    return ResponseBuilder.TellWithCard($"Hallo {name}. Freut mich dich kennenzulernen.",
                        "Hallo!", $"{name.ToUpper()}");
                }
            }

            // default response
            return ResponseBuilder.Tell("Ups, hier ist etwas schiefgelaufen.");
        }
    }
}
