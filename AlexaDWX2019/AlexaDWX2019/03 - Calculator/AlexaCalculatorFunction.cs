using System;
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
    public static class AlexaCalculatorFunction
    {
        [FunctionName("AlexaCalculatorFunction")]
        public static async Task<SkillResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa/calculator")] HttpRequest req)
        {
            // read content as skill request
            var payload = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(payload);

            // get locale
            var locale = skillRequest.Request.Locale;

            // get type of request
            var requestType = skillRequest.GetRequestType();

            // handle launchrequest
            if (requestType == typeof(LaunchRequest))
            {
                return HandleHelpRequest(locale);
            }

            // handle intent requests
            if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                var cardTitle = locale.StartsWith("de") ? CalculatorResponses.CardTitleDE : CalculatorResponses.CardTitleEN;

                if (!intentRequest.Intent.Slots.ContainsKey("firstnum") || !intentRequest.Intent.Slots.ContainsKey("secondnum"))
                {
                    var speechOutput = locale.StartsWith("de") ? CalculatorResponses.NumbersNotProvidedSpeechOutputDE : CalculatorResponses.NumbersNotProvidedSpeechOutputEN;
                    var cardContent = locale.StartsWith("de") ? CalculatorResponses.NumbersNotProvidedCardContentDE : CalculatorResponses.NumbersNotProvidedCardContentEN;

                    return ResponseBuilder.TellWithCard(speechOutput, cardTitle, cardContent);
                }


                var num1 = Convert.ToDouble(intentRequest.Intent.Slots["firstnum"].Value);
                var num2 = Convert.ToDouble(intentRequest.Intent.Slots["secondnum"].Value);

                double result;

                switch (intentRequest.Intent.Name)
                {
                    case "AddIntent":
                        result = num1 + num2;
                        var addSpeechOutput = locale.StartsWith("de") ? CalculatorResponses.AddResultSpeechOutputDE : CalculatorResponses.AddResultSpeechOutputEN;
                        return ResponseBuilder.TellWithCard(string.Format(addSpeechOutput, num1, num2, result), cardTitle, $"{num1} + {num2} = {result}.");
                    case "SubstractIntent":
                        result = num1 - num2;
                        var subSpeechOutput = locale.StartsWith("de") ? CalculatorResponses.SubResultSpeechOutputDE : CalculatorResponses.SubResultSpeechOutputEN;
                        return ResponseBuilder.TellWithCard(string.Format(subSpeechOutput, num1, num2, result), cardTitle, $"{num1} - {num2} = {result}.");
                    case "MultiplyIntent":
                        result = num1 * num2;
                        var mulSpeechOutput = locale.StartsWith("de") ? CalculatorResponses.MulResultSpeechOutputDE : CalculatorResponses.MulResultSpeechOutputEN;
                        return ResponseBuilder.TellWithCard(string.Format(mulSpeechOutput, num1, num2, result), cardTitle, $"{num1} * {num2} = {result}.");
                    case "DivideIntent":
                        if (num2 == 0)
                        {
                            var divByZeroSpeechOutput = locale.StartsWith("de") ? CalculatorResponses.DivideByZeroSpeechOutputDE : CalculatorResponses.DivideByZeroSpeechOutputEN;
                            var divByZeroCardOutput = locale.StartsWith("de") ? CalculatorResponses.DivideByZeroCardOutputDE : CalculatorResponses.DivideByZeroCardOutputEN;
                            return ResponseBuilder.TellWithCard(divByZeroSpeechOutput, cardTitle, divByZeroCardOutput);
                        }
                        else
                        {
                            result = num1 / num2;
                            var divSpeechOutput = locale.StartsWith("de") ? CalculatorResponses.DivResultSpeechOutputDE : CalculatorResponses.DivResultSpeechOutputEN;
                            return ResponseBuilder.TellWithCard(string.Format(divSpeechOutput, num1, num2, result), cardTitle, $"{num1} / {num2} = {result:F2}.");
                        }
                    default:
                        return HandleHelpRequest(locale);
                }
            }

            return HandleHelpRequest(locale);
        }

        private static SkillResponse HandleHelpRequest(string locale)
        {
            if (locale.StartsWith("de"))
                return ResponseBuilder.AskWithCard(CalculatorResponses.HelpSpeechOutputDE, CalculatorResponses.CardTitleDE, CalculatorResponses.HelpCardContentDE, new Reprompt(CalculatorResponses.HelpRepromptDE));
            else
                return ResponseBuilder.AskWithCard(CalculatorResponses.HelpSpeechOutputEN, CalculatorResponses.CardTitleEN, CalculatorResponses.HelpCardContentEN, new Reprompt(CalculatorResponses.HelpRepromptEN));
        }
    }

    public static class CalculatorResponses
    {
        public static string AddResultSpeechOutputDE = "Das Ergebnis der Addition von {0} und {1} lautet: {2}";
        public static string SubResultSpeechOutputDE = "Das Ergebnis der Subtraktion von {0} und {1} lautet: {2}";
        public static string MulResultSpeechOutputDE = "Das Ergebnis der Multiplikation von {0} und {1} lautet: {2}";
        public static string DivResultSpeechOutputDE = "Das Ergebnis der Division von {0} und {1} lautet: {2}";

        public static string NumbersNotProvidedSpeechOutputDE = "Bitte gebe zwei Zahlen an, welche ich addieren, subtrahieren, mulitplizieren oder dividieren.";
        public static string NumbersNotProvidedCardContentDE = "Leider wurden keinen Zahlen angegeben. Bitte versuche eine Aufgabe mit zwei Zahlen.";

        public static string HelpSpeechOutputDE = "Willkommen zum Alexa-Taschenrechner. Ich kann zwei Zahlen addieren, subtrahieren, multiplizieren und sogar dividieren. Zum Beispiel: Was ist zwei plus drei?";
        public static string HelpCardContentDE = "Zum Beispiel: Was ist 2 + 3?";
        public static string HelpRepromptDE = "Welche Aufgabe soll ich lösen?";

        public static string AddResultSpeechOutputEN = "The result of adding {0} and {1} is: {2}";
        public static string SubResultSpeechOutputEN = "The result of subtracting {0} and {1} is: {2}";
        public static string MulResultSpeechOutputEN = "The result of multiplying {0} and {1} is: {2}";
        public static string DivResultSpeechOutputEN = "The result of dividing {0} and {1} is: {2}";

        public static string NumbersNotProvidedSpeechOutputEN = "Please specify two numbers to be added, subtracted, multiplied or divided.";
        public static string NumbersNotProvidedCardContentEN = "Unfortunately, no numbers were given. Please try again with a math task.";

        public static string DivideByZeroSpeechOutputEN = "You have just tried to divide by 0. This does not work. Please try with a different task.";
        public static string DivideByZeroCardOutputEN = "You have just tried to divide by 0. This does not work. Please try with a different task.";
        public static string DivideByZeroSpeechOutputDE = "Du hast versucht durch 0 zu teilen. Das funktioniert leider nicht. Versuche es bitte mit einer anderen Aufgabe.";
        public static string DivideByZeroCardOutputDE = "Du hast versucht durch 0 zu teilen. Das funktioniert leider nicht. Versuche es bitte mit einer anderen Aufgabe.";

        public static string HelpSpeechOutputEN = "Welcome to the Alexa Calculator. I can add, subtract, multiply, and even divide two numbers. For example, what is three plus two?";
        public static string HelpCardContentEN = "For example, what is 3 + 2?";
        public static string HelpRepromptEN = "What is your mathematical task?";

        public static string CardTitleDE = "Alexa Taschenrechner";
        public static string CardTitleEN = "Alexa Calculator";
    }
}
