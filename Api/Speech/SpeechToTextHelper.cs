using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Speech
{
    internal class SpeechToTextHelper
    {
        private static readonly string speechServiceSubscriptionKey = Environment.GetEnvironmentVariable("SpeechServiceSubscriptionKey");
        private static readonly string speechServiceRegion = Environment.GetEnvironmentVariable("SpeechServiceRegion");

        public static async Task<string> RecognizeSpeechFromFileAsync(string filePath)
        {
            var config = SpeechConfig.FromSubscription(speechServiceSubscriptionKey, speechServiceRegion);

            using (var audioConfig = AudioConfig.FromWavFileInput(filePath))
            using (var recognizer = new SpeechRecognizer(config, audioConfig))
            {
                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                   return result.Text;
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    throw new Exception($"Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    string cancellationMessage = $"Speech recognition cancelled. Reason={cancellation.Reason}.";
                    
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        cancellationMessage += Environment.NewLine + $"CANCELED: ErrorCode={cancellation.ErrorCode}";
                        cancellationMessage += Environment.NewLine + $"CANCELED: ErrorDetails={cancellation.ErrorDetails}";
                        cancellationMessage += Environment.NewLine + $"CANCELED: Did you update the subscription info?";
                    }

                    throw new Exception(cancellationMessage);
                }

                return null;
            }
        }
    }
}
