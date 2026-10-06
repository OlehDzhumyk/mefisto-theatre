using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace O_Dzhumyk_MefistoTheatre.Services
{
    // Post bodies may contain basic HTML (headings, paragraphs, quotes). Anything that could run
    // script is stripped before it reaches the page, and excerpts are reduced to plain text.
    public partial class PostContentFormatter
    {
        private readonly HtmlSanitizer _sanitizer = new();

        public string ToSafeHtml(string content) => _sanitizer.Sanitize(content);

        public string ToExcerpt(string content, int maxLength = 200)
        {
            // Turn block-level tags into spaces so words from different paragraphs don't run together
            var text = TagPattern().Replace(content, " ");
            text = WebUtility.HtmlDecode(text);
            text = WhitespacePattern().Replace(text, " ").Trim();

            if (text.Length <= maxLength) return text;

            var cut = text.LastIndexOf(' ', maxLength);
            return text[..(cut > 0 ? cut : maxLength)].TrimEnd(',', '.', ';', ':') + "…";
        }

        [GeneratedRegex("<[^>]*>")]
        private static partial Regex TagPattern();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespacePattern();
    }
}
