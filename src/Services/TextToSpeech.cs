using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarksonDeckson.Services
{
    partial class AudioService
    {
        private const string TTS_BASE_URI = "https://translate.google.com/translate_tts";

        public async Task TextToSpeech(string lang, string text, IVoiceChannel target, ISocketMessageChannel channel)
        {
            var builder = new UriBuilder(TTS_BASE_URI);
            var formatedText = Uri.EscapeDataString(text);

            builder.Query = $"ie=UTF-8&q={formatedText}&tl={lang}&client=gtx&ttsspeed=1";

            _channel = channel;

            try
            {
                using (var stream = await GetSpeechStream(builder.Uri))
                {
                    var fileName = await SaveStreamMp3(stream);

                    await Join(target);

                    if (_audioClient != null)
                    {
                        await PlayLocalDiscordStream(fileName);
                    }
                }
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(e.ToString());
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
    }
}
