using System.Net.Http.Headers;
using QRCodeEater.Models;
using Newtonsoft.Json;
using ZXing.SkiaSharp.Rendering;
using System.Drawing;
using ZXing.SkiaSharp;
using SkiaSharp;

public class PredictionHandler {

    private static HttpClient _HTTPClient = new HttpClient();

    public static void setAPIKey(Config config)
    {

        _HTTPClient.DefaultRequestHeaders.Accept.Clear();
        _HTTPClient.DefaultRequestHeaders.Add("Prediction-Key",config.PredictionKey);
    }

    private static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

    private static bool verifyIfSuspicious(Config config, double probability){

        if(probability >= config.SuspiciousThreshold)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static async Task<Attachment> submitData(Config config, Attachment attachment)
    {
        _HTTPClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        byte[] byteData = GetImageAsByteArray(System.IO.Path.Combine(config.DownloadPath,attachment.name));
        // Make a prediction against the new project
        var content = new ByteArrayContent(byteData);
        content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(attachment.contentType);
        var result = _HTTPClient.PostAsync("https://qrcodemlengine-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/"+config.PredictionProjectID+"/classify/iterations/"+config.ModelName+"/image",content);
        var responseContent =  JsonConvert.DeserializeObject<Prediction>(result.Result.Content.ReadAsStringAsync().Result);

        // Defining if attachment is suspicious
        attachment.isSuspicious =  verifyIfSuspicious(config, Convert.ToDouble(responseContent.predictions[0].probability));

        if(attachment.isSuspicious){
            // If suspicious, resolving the QRCode and add the resolved URL to the attachment     
            BarcodeReader b = new BarcodeReader();
            ZXing.Result response = b.Decode(SKBitmap.Decode(byteData));
            if(response != null){
                attachment.ResolvedText = response.Text;
            }
        }

        return attachment;
    }
}