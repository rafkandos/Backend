using Tesseract;

namespace CualiVy_CC.Services;

public interface ICVService
{
    string GetImage(string thirdparty);
}

public class CVService : ICVService
{
    public string GetImage(string thirdparty)
    {
        var image = "";
        switch (thirdparty)
        {
            case "Glints":
                image = "https://storage.googleapis.com/logo-detail/glints-logo.png";
                break;
            case "Jobstreet":
                image = "https://storage.googleapis.com/logo-detail/jobstreet-logo.png";
                break;
            case "Kalibrr":
                image = "https://storage.googleapis.com/logo-detail/kalibrr-logo.png";
                break;
            case "LinkedIn":
                image = "https://storage.googleapis.com/logo-detail/linkedin-logo.png";
                break;
            case "Loker.id":
                image = "https://storage.googleapis.com/logo-detail/lokerid-logo.jpg";
                break;
            default:
                image = "";
                break;
        }

        return image;
    }
}