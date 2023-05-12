using System;
using System.IO;
using System.Threading;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure.Identity;

public class AudioToText
{
    private readonly SpeechConfig config;

    public event EventHandler<string> TextReceived;

    public AudioToText()
    {
        var cred = new DefaultAzureCredential();
        config = SpeechConfig.FromAuthorizationToken(Environment.GetEnvironmentVariable("AudioToken"), "australiaeast");
        config.SpeechRecognitionLanguage = "en-US";
        Console.WriteLine("AudioToText initialized");
    }

    public async Task StartRecognitionAsync()
    {
        try
        {
            Console.WriteLine("Starting recognition");
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new SpeechRecognizer(config, audioConfig);

            recognizer.Recognizing += Recognizer_Recognizing;
            recognizer.Recognized += Recognizer_Recognized;
            recognizer.Canceled += Recognizer_Canceled;
            recognizer.SessionStopped += Recognizer_SessionStopped;

            await recognizer.StartContinuousRecognitionAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public async Task StopRecognitionAsync()
    {
        try
        {
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new SpeechRecognizer(config, audioConfig);

            await recognizer.StopContinuousRecognitionAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private void Recognizer_Recognizing(object sender, SpeechRecognitionEventArgs e)
    {
        Console.WriteLine($"Recognizing: {e.Result.Text}");
    }
    private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
    {
        TextReceived?.Invoke(this, e.Result.Text);
        Console.WriteLine($"Recognized: {e.Result.Text}");
    }
    private void Recognizer_Canceled(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        Console.WriteLine($"Canceled: {e.Reason}");
    }

    private void Recognizer_SessionStopped(object sender, SessionEventArgs e)
    {
        Console.WriteLine($"Session stopped.");
    }
}