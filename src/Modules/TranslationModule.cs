using Discord.Commands;
using MarksonDeckson.Services;
using System.Threading.Tasks;

namespace MarksonDeckson.Modules
{
    public class TranslationModule : ModuleBase<SocketCommandContext>
    {
        private readonly TranslationService _translationService;

        private const string LANG_DEFAULT = "en";

        public TranslationModule(TranslationService translationService)
        {
            _translationService = translationService;
        }

        [Command("translate")]
        [Summary("Translate message from default language to other language.")]
        public async Task Translate([Remainder] [Summary("Target language code and text to translate.")] string input)
        {
            var param = input.Split(' ');
            var text = "";

            for (int i = 1; i < param.Length; i++)
                text = text + $"{param[i]} ";

            var result = await _translationService.TranlateAsync(LANG_DEFAULT, param[0], text);

            await Context.Channel.SendMessageAsync(result);
        }

        [Command("translateto")]
        [Summary("Translate message from selected language to other language.")]
        public async Task TranslateTo([Summary("Native and Target language code and text to translate.")] string input)
        {
            var param = input.Split(' ');
            var text = "";

            for (int i = 2; i < param.Length; i++)
                text = text + $"{param[i]} ";

            var result = await _translationService.TranlateAsync(param[0], param[1], text);

            await Context.Channel.SendMessageAsync(result);
        }
    }
}
