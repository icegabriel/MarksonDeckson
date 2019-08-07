using Discord.WebSocket;
using MarksonDeckson.Exceptions;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarksonDeckson.Services
{
    public class TranslationService
    {
        private const string TRANSLATOR_BASE_URI = "https://translate.google.com/m";

        public async Task<string> TranlateAsync(string langCode, string targetLagCode, string text)
        {
            var builder = new UriBuilder(TRANSLATOR_BASE_URI);
            var formatedText = Uri.EscapeDataString(text);

            builder.Query = $"hl=pt&sl={langCode}&tl={targetLagCode}&ie=UTF-8&prev=_m&q={formatedText}";

            try
            {
                var result = await GetTraslation(builder.Uri);

                return result;
            }
            catch (TranslationException e)
            {
                return e.Message;
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        private async Task<string> GetTraslation(Uri uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.RequestUri = uri;
                request.Method = HttpMethod.Get;
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.Headers.Host = "translate.google.com";

                var result = await client.SendAsync(request);

                var html = await result.Content.ReadAsStringAsync();
                var code = "(?<=(<div dir=\"ltr\" class=\"t0\">)).*?(?=(<\\/div>))";

                var rx = new Regex(code, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var match = rx.Match(html);

                if (!match.Success)
                {
                    code = "(?<=(<div dir=\"rtl\" class=\"t0\">)).*?(?=(<\\/div>))";

                    rx = new Regex(code, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    match = rx.Match(html);
                }

                if (result.IsSuccessStatusCode && match.Success)
                {
                    return System.Net.WebUtility.HtmlDecode(match.Value);
                }

                throw new TranslationException(":x:ERROR: Could not translate text try again later !!!");
            }
        }
    }
}
