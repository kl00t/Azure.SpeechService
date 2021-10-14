using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace Azure.SpeechService
{
    class Program
    {
        public const string SubscriptionKey = "";
        public const string Region = "westeurope";
        public const string SourceDirectory = ".//Input";

        static async Task Main()
        {
            //await SynthesizeAudioAsyncFiles();
            await SynthesisToSpeakerAsync();
        }

        public static async Task SynthesisToSpeakerAsync()
        {
            var configuration = CreateSpeechConfiguration();
            var files = Directory.EnumerateFiles(SourceDirectory, "*.xml");
            foreach (var file in files)
            {
                Console.WriteLine($"Reading file ---> {Path.GetFileName(file)}");

                await SpeakSsmlToSpeaker(configuration, file);
    
                Console.WriteLine();
            }
        }

        public static async Task SynthesizeAudioAsyncFiles()
        {
            var configuration = CreateSpeechConfiguration();
            var files = Directory.EnumerateFiles(SourceDirectory, "*.xml");
            foreach (var file in files)
            {
                Console.WriteLine($"Reading file ---> {Path.GetFileName(file)}");
                var fileName = Path.GetFileNameWithoutExtension(file);
                
                var speechSynthesisResult = await CreateSpeechFromSsmlFile(configuration, file);
                await WriteAudioStreamToFile(speechSynthesisResult, $"{fileName}.wav");
                Console.WriteLine();
            }
        }

        public static SpeechConfig CreateSpeechConfiguration()
        {
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            // The default language is "en-us".
            var speechConfiguration = SpeechConfig.FromSubscription(SubscriptionKey, Region);
            speechConfiguration.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);
            return speechConfiguration;
        }

        public static async Task<SpeechSynthesisResult> CreateSpeechFromSsmlFile(SpeechConfig config, string file)
        {
            Console.WriteLine($"Creating speech from file ---> {Path.GetFileName(file)}");
            using (var synthesizer = new SpeechSynthesizer(config, null))
            {
                var ssml = ReadFile(file);

                using (var speechSynthesisResult = await synthesizer.SpeakSsmlAsync(ssml))
                {
                    if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        Console.WriteLine($"Speech created from file for [{Path.GetFileName(file)}]");
                    }
                    else if (speechSynthesisResult.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }

                    return speechSynthesisResult;
                }
            }
        }

        public static async Task<SpeechSynthesisResult> SpeakSsmlToSpeaker(SpeechConfig config, string file)
        {
            Console.WriteLine($"Speaking speech from file ---> {Path.GetFileName(file)}");
            var ssml = ReadFile(file);

            using (var synthesizer = new SpeechSynthesizer(config))
            {
                using (var speechSynthesisResult = await synthesizer.SpeakSsmlAsync(ssml))
                {
                    if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        Console.WriteLine($"Speech synthesized to speaker for text [{Path.GetFileName(file)}]");
                    }
                    else if (speechSynthesisResult.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }

                    return speechSynthesisResult;
                }
            }
        }

        public static string ReadFile(string file)
        {
            Console.WriteLine($"Reading file ---> {Path.GetFileName(file)}");
            return File.ReadAllText(file);
        }

        public static async Task WriteAudioStreamToFile(SpeechSynthesisResult speechSynthesisResult, string fileName)
        {
            Console.WriteLine($"Writing audio file ---> {fileName}");
            using var stream = AudioDataStream.FromResult(speechSynthesisResult);
            await stream.SaveToWaveFileAsync($"{fileName}.wav");
        }
    }
}