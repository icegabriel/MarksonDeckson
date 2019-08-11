using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarksonDeckson.Services
{
    public partial class AudioService
    {
        private string _fileDir;

        private IAudioClient _audioClient;
        private ISocketMessageChannel _channel;
        private IVoiceChannel _voiceChannel;

        private List<Stream> PlayList { get; set; }

        public AudioService()
        {
            PlayList = new List<Stream>();
        }

        private async Task Join()
        {
            try
            {
                _audioClient = await _voiceChannel.ConnectAsync();
            }
            catch (NullReferenceException)
            {
                await _channel.SendMessageAsync(":x:User must be in a voice channel, or a voice channel must be passed as an argument.");
            }
        }

        private async Task SaveStreamMp3(Stream stream)
        {
            var fileName = $"{stream.GetHashCode()}.mp3";
            var tmpDir = Directory.GetCurrentDirectory() + "\\tmp";

            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);

            _fileDir = tmpDir + $"\\{fileName}";

            using (var file = File.Open(_fileDir, FileMode.Create))
            {
                await stream.CopyToAsync(file);
            }
        }

        private Stream CreateLocalDiscordStream(string path)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

            return process.StandardOutput.BaseStream;
        }

        private async Task PlayPlayList()
        {
            await Join();

            using (var stream = PlayList.FirstOrDefault())
            using (var discord = _audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {

                    var hash = stream.GetHashCode();
                    if (stream != null)
                        await stream.CopyToAsync(discord);
                }
                finally
                {
                    PlayList.Remove(stream);
                    File.Delete(_fileDir);

                    await discord.FlushAsync();
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
