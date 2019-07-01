using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace AlexaDWX2019
{
    public static class AlexaHelloWorldFunction
    {
        [FunctionName("AlexaHelloWorldFunction")]
        public static object RunV1([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/helloworld")] HttpRequest req)
        {
            return new
            {
                version = "1.0",
                response = new
                {
                    outputSpeech = new
                    {
                        type = "PlainText",
                        text = "Hallo Welt aus einer Azure Function"
                    },
                    //card = new
                    //{
                    //    type = "Simple",
                    //    title = "Hallo Welt",
                    //    content = "Azure Function"
                    //},
                    shouldEndSession = true
                }
            };
        }
    }
}
