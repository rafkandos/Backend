using System.Text.RegularExpressions;
using Tesseract;

namespace CualiVy_CC.Services;

public interface IOCRService
{
    string GetExtractedText(string base64Img);
}

public class OCRService : IOCRService
{
    public string GetExtractedText(string base64Img)
    {
        try
        {
            var tesDataPath = @"./tessdata";
            if (OperatingSystem.IsLinux())
            {
                tesDataPath = @"/usr/share/tesseract-ocr/4.00/tessdata";
            }

            // Decode Base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(base64Img);

            // Create Pix object from byte array
            using (var ms = new MemoryStream(imageBytes))
            using (var image = Pix.LoadFromMemory(imageBytes))
            {
                // Create a Tesseract instance and set the language
                using (var engine = new TesseractEngine(tesDataPath, "eng", EngineMode.Default))
                {
                    // Set page segmentation mode
                    engine.SetVariable("tessedit_pageseg_mode", "6");

                    // Set the image to be recognized
                    using (var page = engine.Process(image))
                    {
                        // Get the extracted text
                        string extractedText = page.GetText();

                        string txt = extractedText.Replace("\n", " ").Replace("\r", " ");

                        string punctuationPattern = @"[!\""£¬$%&'()*,-./:;<=>?@\[\]^_`{|}~©‘]";
                        string removePunctuations = Regex.Replace(txt, punctuationPattern, " ");

                        string whitespacePattern = @"\s+";
                        string removeExtraWhitespaces = Regex.Replace(removePunctuations, whitespacePattern, " ");

                        string[] wordsToRemove = new string[]
                        {
                            "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "as",
                            "at", "required", "be", "because", "skills", "managed", "management", "manage", "manager", "skill",
                            "been", "before", "being", "below", "between", "both", "but", "by", "could", "com", "did", "do", "does",
                            "doing", "down", "during", "each", "few", "for", "from", "further", "gmail", "had", "has", "have", "having",
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

                        return Regex.Replace(res.ToLower(), @"\s+", " ");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return "";
        }
    }
}
