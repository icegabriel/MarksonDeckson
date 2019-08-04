using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarksonDeckson.Services
{
    public class TranslationService
    {
        const string TRANSLATOR_BASE_URI = "https://translate.google.com/m";
        public async Task<string> TranlateAsync(string langCode, string targetLagCode, string text)
        {
            var builder = new UriBuilder(TRANSLATOR_BASE_URI);
            builder.Query = $"hl=pt&sl={langCode}&tl={targetLagCode}&ie=UTF-8&prev=_m&q={text}";

            using (var client = new HttpClient())
            {
                var request = await client.GetAsync(builder.Uri);

                if (request.IsSuccessStatusCode)
                {
                    var result = await request.Content.ReadAsStringAsync();
                    var code = "(?<=(<div dir=\"ltr\" class=\"t0\">)).*?(?=(<\\/div>))";

                    var rx = new Regex(code, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var match = rx.Match(result);

                    return match.Value;
                }
                else
                {
                    return "ERROR: Could not translate text try again later !!!";
                }
            }
        }
    }
}
