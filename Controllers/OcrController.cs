using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Tesseract;
using System.Text.RegularExpressions;

namespace dotnet_ocr_tesseract.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        public const string folderName = "wwwroot/";
        public const string trainedDataFolderName = "tessdata";

        [HttpPost]
        public String DoOCR([FromForm] OcrModel request)
        {

            string name = request.Image.FileName;
            var image = request.Image;

            if (image.Length > 0)
            {
                using (var fileStream = new FileStream(folderName + image.FileName, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }
            }

            string tessPath = Path.Combine(trainedDataFolderName, "");
            string result = "";

            using (var engine = new TesseractEngine(tessPath, request.DestinationLanguage, EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(folderName + name))
                {
                    var page = engine.Process(img);
                    result = page.GetText();
                }
            }

            if (String.IsNullOrWhiteSpace(result))
            {
                return "Ocr is finished. Return empty";
            }
            else
            {
                string txt = result.Replace("\n", " ").Replace("\r", " ");

                string punctuationPattern = @"[!\""£¬$%&'()*,-./:;<=>?@\[\]^_`{|}~©‘]";
                string removePunctuations = Regex.Replace(txt, punctuationPattern, " ");

                string whitespacePattern = @"\s+";
                string removeExtraWhitespaces = Regex.Replace(removePunctuations, whitespacePattern, " ");

                string[] wordsToRemove = new string[]
                {
                    "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "as",
                    "at", "required", "be", "because", "skills", "managed", "management", "manage", "manager", "skill",
                    "been", "before", "being", "below", "between", "both", "but", "by", "could", "did", "do", "does",
                    "doing", "down", "during", "each", "few", "for", "from", "further", "had", "has", "have", "having",
                    "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his",
                    "how", "how's", "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is", "it", "it's", "its",
                    "itself", "let's", "me", "more", "most", "my", "myself", "nor", "of", "on", "once", "only", "or",
                    "other", "ought", "our", "ours", "ourselves", "out", "over", "own", "same", "she", "she'd", "she'll",
                    "she's", "should", "so", "some", "such", "than", "that", "that's", "the", "their", "theirs", "them",
                    "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've",
                    "this", "those", "through", "to", "too", "under", "until", "up", "very", "was", "we", "we'd", "we'll",
                    "we're", "we've", "were", "what", "what's", "when", "when's", "where", "where's", "which", "while",
                    "who", "who's", "whom", "why", "why's", "with", "would", "you", "you'd", "you'll", "you're", "you've",
                    "your", "yours", "yourself", "yourselves"
                };

                // Construct the pattern to match the words to remove
                string pattern = @"\b(" + string.Join("|", wordsToRemove.Select(Regex.Escape)) + @")\b";

                // Remove the words from the string
                string res = Regex.Replace(removeExtraWhitespaces, pattern, string.Empty, RegexOptions.IgnoreCase);

                return res.ToLower();
            }
        }

    }

    public class OcrModel
    {
        public IFormFile Image { get; set; }
        public string DestinationLanguage { get; set; } = "eng";
    }
}