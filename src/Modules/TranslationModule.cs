using Discord.Commands;
using MarksonDeckson.Services;
using System.Threading.Tasks;

namespace MarksonDeckson.Modules
{
    [Group("translate")]
    public class TranslationModule : ModuleBase<SocketCommandContext>
    {
        private readonly TranslationService _translationService;
        

        public TranslationModule(TranslationService translationService)
        {
            _translationService = translationService;
        }

        [Command("")]
        [Summary("Translate message from english to other language.")]
        public async Task Translate([Summary("Target language code and text to translate.")] params string[] param)
        {
            var text = "";

            for (int i = 1; i < param.Length; i++)
                text = text + $"{param[i]} ";

            var result = await _translationService.TranlateAsync("pt", param[0], text);

            await Context.Channel.SendMessageAsync(result);
        }
    }
}
