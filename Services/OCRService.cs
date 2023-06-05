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
            // Decode Base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(base64Img);

            // Create Pix object from byte array
            using (var ms = new MemoryStream(imageBytes))
            using (var image = Pix.LoadFromMemory(imageBytes))
            {
                // Create a Tesseract instance and set the language
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    // Set page segmentation mode
                    engine.SetVariable("tessedit_pageseg_mode", "6");

                    // Set the image to be recognized
                    using (var page = engine.Process(image))
                    {
                        // Get the extracted text
                        string extractedText = page.GetText();

                        return extractedText.Replace("\n", " ").Replace("\r", " ");
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
