using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using System.Collections.Generic;

namespace AlexaDWX2019
{
    public static class AlexaHighLowGameFunction
    {
        public static class AlexaHighLowGame
        {
            private const string AmazonCancelIntent = "AMAZON.CancelIntent";
            private const string AmazonHelpIntent = "AMAZON.HelpIntent";
            private const string AmazonStopIntent = "AMAZON.StopIntent";
            private const string AmazonYesIntent = "AMAZON.YesIntent";
            private const string AmazonNoIntent = "AMAZON.NoIntent";

            private const string NumberIntent = "NumberIntent";

            private const string GameStateAttribute = "GameState";
            private const string GuessNumberAttribute = "GuessNumber";

            private const string GameStateStarted = "STARTED";
            private const string GameStateWon = "WON";


            [FunctionName("AlexaHighLowGame")]
            public static async Task<SkillResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/highlow")] HttpRequest req)
            {
                // read content as skill request
                var payload = await req.ReadAsStringAsync();
                var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(payload);

                // get type of request
                var requestType = skillRequest.GetRequestType();

                // handle launchrequest
                if (requestType == typeof(LaunchRequest))
                {
                    return CreateSkillResponse("Herzlich willkommen bei Höher Tiefer. Ich denke mir eine Zahl zwischen 1 und 100, wollen wir das Spiel starten?", false);
                }

                // handle intentrequests
                if (requestType == typeof(IntentRequest))
                {
                    var intentRequest = skillRequest.Request as IntentRequest;

                    switch (intentRequest.Intent.Name)
                    {
                        case AmazonHelpIntent:
                            return CreateSkillResponse("Wir spielen gemeinsam ein kleines Spiel. Ich denke mir eine Zahl zwischen 1 und 100 und du versuchst diese zu erraten. Sollen wir das Spiel starten?", false);
                        case AmazonCancelIntent:
                            return CreateSkillResponse("Ok", true);
                        case AmazonStopIntent:
                            return CreateSkillResponse("Alles klar.", true);
                        case AmazonNoIntent:
                            return CreateSkillResponse("Ciao", true);
                        case AmazonYesIntent:
                            return HandleYesIntent(skillRequest);
                        case NumberIntent:
                            return HandleNumberIntent(skillRequest, intentRequest);
                    }
                }

                return CreateSkillResponse("Hier ist leider etwas schief gelaufen. Bitte noch einmal versuchen...", true);
            }

            private static SkillResponse HandleNumberIntent(SkillRequest request, IntentRequest intentRequest)
            {
                try
                {
                    var attributes = request.Session.Attributes;

                    if (attributes.ContainsKey(GameStateAttribute))
                    {
                        var gameState = attributes[GameStateAttribute] as string;
                        if (gameState == GameStateStarted)
                        {
                            var guessNumber = Convert.ToInt32(intentRequest.Intent.Slots["number"].Value);
                            var targetNumber = Convert.ToInt32(attributes[GuessNumberAttribute]);

                            if (guessNumber > targetNumber)
                                return CreateSkillResponse("Leider ist deine Zahl größer als meine. Versuche es doch bitte noch einmal.", false);

                            if (guessNumber < targetNumber)
                                return CreateSkillResponse("Leider ist deine Zahl kleiner als meine. Versuche es doch bitte noch einmal.", false);

                            attributes[GameStateAttribute] = GameStateWon;
                            return CreateSkillResponse("Herzlichen Glückwunsch. Genau an diese Zahl hatte ich gedacht. Möchtest du ein neues Spiel starten?", false, attributes);
                        }
                        else
                        {
                            return CreateSkillResponse("Wir spielen gemeinsam ein kleines Spiel. Ich denke mir eine Zahl zwischen 1 und 100 und du versuchst diese zu erraten. Sollen wir das Spiel starten?", false);
                        }
                    }
                    else
                    {
                        return CreateSkillResponse("Wir spielen gemeinsam ein kleines Spiel. Ich denke mir eine Zahl zwischen 1 und 100 und du versuchst diese zu erraten. Sollen wir das Spiel starten?", false);
                    }
                }
                catch
                {
                    return CreateSkillResponse("Wir spielen gemeinsam ein kleines Spiel. Ich denke mir eine Zahl zwischen 1 und 100 und du versuchst diese zu erraten. Sollen wir das Spiel starten?", false);
                }
            }

            private static SkillResponse HandleYesIntent(SkillRequest input)
            {
                var attributes = input.Session.Attributes ?? new Dictionary<string, object>();

                if (attributes.ContainsKey(GameStateAttribute))
                    attributes[GameStateAttribute] = GameStateStarted;
                else
                    attributes.Add(GameStateAttribute, GameStateStarted);

                if (attributes.ContainsKey(GuessNumberAttribute))
                    attributes[GuessNumberAttribute] = new Random().Next(1, 100);
                else
                    attributes.Add(GuessNumberAttribute, new Random().Next(1, 100));

                return CreateSkillResponse("Dann legen wir doch los. Ich denke an eine Zahl zwischen 1 und 100, welche ist es wohl?", false, attributes);
            }

            private static SkillResponse CreateSkillResponse(string outputSpeech, bool shouldEndSession, Dictionary<string, object> attributes = null)
            {
                var response = new ResponseBody
                {
                    ShouldEndSession = shouldEndSession,
                    OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
                };

                var skillResponse = new SkillResponse
                {
                    Response = response,
                    Version = "1.0"
                };

                if (attributes != null)
                    skillResponse.SessionAttributes = attributes;

                return skillResponse;
            }
        }
    }
}
