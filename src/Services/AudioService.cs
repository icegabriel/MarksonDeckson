using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MarksonDeckson.Services
{
    partial class AudioService
    {
        private readonly string _tempDir;

        private IAudioClient _audioClient;
        private ISocketMessageChannel _channel;

        private List<AudioOutStream> PlayList { get; set; }

        public AudioService()
        {
            _tempDir = Directory.GetCurrentDirectory() + "\\tmp";
            PlayList = new List<AudioOutStream>();
        }

        private async Task Join(IVoiceChannel target)
        {
            try
            {
                _audioClient = await target.ConnectAsync();
            }
            catch (NullReferenceException)
            {
                await _channel.SendMessageAsync(":x:User must be in a voice channel, or a voice channel must be passed as an argument.");
            }
        }

        private async Task<string> SaveStreamMp3(Stream stream)
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

        private Process CreateLocalDiscordStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task PlayLocalDiscordStream(string fileName)
        {
            var path = _tempDir + $"\\{fileName}";

            using (var ffmpeg = CreateLocalDiscordStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = _audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    await output.CopyToAsync(discord);

                    output.Dispose();
                    File.Delete(path);
                }
                finally
                {
                    await discord.FlushAsync();
                    await _channel.SendMessageAsync("Finish");
                }
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
