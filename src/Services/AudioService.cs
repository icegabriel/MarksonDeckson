using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarksonDeckson.Services
{
    public class AudioService
    {
        private const string TTS_BASE_URI = "https://translate.google.com/translate_tts";

        private readonly string _tempDir;

        private IAudioClient _audioClient;

        public AudioService()
        {
            _tempDir = Directory.GetCurrentDirectory() + "\\tmp";
        }

        public async Task TextToSpeech(string lang, string text, IVoiceChannel target, ISocketMessageChannel channel)
        {
            var builder = new UriBuilder(TTS_BASE_URI);
            var formatedText = Uri.EscapeDataString(text);

            builder.Query = $"ie=UTF-8&q={formatedText}&tl={lang}&client=gtx&ttsspeed=1";

            try
            {
                using (var stream = await GetSpeechStream(builder.Uri))
                {
                    var fileName = await SaveStream(stream);

                    _audioClient = await target.ConnectAsync();

                    if (_audioClient != null)
                    {
                        await SendAsync(_audioClient, fileName);
                    }
                    else
                        await channel.SendMessageAsync(":x:User must be in a voice channel, or a voice channel must be passed as an argument.");
                }
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(e.Message);
            }
        }

        private async Task<string> SaveStream(Stream stream)
        {
            var fileName = $"{stream.GetHashCode()}.mp3";

            if (!Directory.Exists(_tempDir))
                Directory.CreateDirectory(_tempDir);

            using (var file = File.Open($"tmp\\{fileName}", FileMode.Create))
            {
                await stream.CopyToAsync(file);
                return fileName;
            }
        }

        private async Task<Stream> GetSpeechStream(Uri uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.RequestUri = uri;
                request.Method = HttpMethod.Get;
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
                request.Headers.Add("Accept", "audio/mpeg");
                request.Headers.Host = "translate.google.com";

                var result = await client.SendAsync(request);

                if (result.IsSuccessStatusCode)
                {
                    var stream = await result.Content.ReadAsStreamAsync();

                    return stream;
                }
                else
                {
                    throw new Exception(":x:Could not get connection with text reader, try again later!!");
                }
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string fileName)
        {
            var path = _tempDir + $"\\{fileName}";

            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    await output.CopyToAsync(discord);

                    output.Dispose();
                    File.Delete(path);
                }
                finally { await discord.FlushAsync(); }
            }
        }

        public string GetTextParam(string[] param, int indice)
        {
            var text = "";

            for (int i = indice; i < param.Length; i++)
                text = text + $"{param[i]} ";

            return text;
        }
    }
}
