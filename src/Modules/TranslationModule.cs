using Discord;
using Discord.Audio;
using Discord.Commands;
using MarksonDeckson.Services;
using System.Threading.Tasks;

namespace MarksonDeckson.Modules
{
    [Summary("Translation")]
    public class TranslationModule : ModuleBase<SocketCommandContext>
    {
        private readonly TranslationService _translationService;
        private readonly AudioService _audioService;

        private const string LANG_DEFAULT = "en";

        public TranslationModule(TranslationService translationService, AudioService audioService)
        {
            _translationService = translationService;
            _audioService = audioService;
        }

        [Command("translate")]
        [Summary("Translate message from default language to other language.")]
        public async Task Translate([Remainder] [Summary("Target language code and text to translate.")] string input)
        {
            var param = input.Split(' ');
            var text = GetTextParam(param, 1);

            var result = await _translationService.TranlateAsync(LANG_DEFAULT, param[0], text);

            await Context.Channel.SendMessageAsync(result);
        }

        [Command("translateto")]
        [Summary("Translate message from selected language to other language.")]
        public async Task TranslateTo([Remainder] [Summary("Default and Target language code and text to translate.")] string input)
        {
            var param = input.Split(' ');
            var text = GetTextParam(param, 2);

            var result = await _translationService.TranlateAsync(param[0], param[1], text);

            await Context.Channel.SendMessageAsync(result);
        }

        [Command("translateplay", RunMode = RunMode.Async)]
        [Summary("Translate the text from default language to other language and read the text.")]
        public async Task TranslateVoice([Remainder] [Summary("Target language code and text to translate.")] string input)
        {
            var param = input.Split(' ');
            var text = GetTextParam(param, 1);

            var result = await _translationService.TranlateAsync(LANG_DEFAULT, param[0], text);

            await Context.Channel.SendMessageAsync(":fast_forward:Playing: " + result);

            await _audioService.TextToSpeech(param[0], result, (Context.User as IVoiceState).VoiceChannel);
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
