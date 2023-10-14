using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using QRCodeEater.Models;
using Microsoft.OData;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace QRCodeEater.Handlers;

class ExchangeHandler {

    private static HttpClient _HTTPClient = new HttpClient();

    public static void setBearer(Bearer bearer){

        _HTTPClient.DefaultRequestHeaders.Accept.Clear();
        _HTTPClient.DefaultRequestHeaders.Add("Authorization","Bearer "+bearer.Access_token);
    }

    public static async Task<Mail[]> getMailsList(Config config, string mailbox)
    {
        try{
            var httpResponseMessage = await  _HTTPClient.GetStringAsync("https://graph.microsoft.com/v1.0/users/"+mailbox+"/messages");
            var responseContent =  JsonConvert.DeserializeObject<ODataResponse<Mail>>(httpResponseMessage);

            return responseContent.Value;
        } 
        catch(HttpRequestException e)
        {
            Console.WriteLine("Warning - One of the parameter passed is invalid or you do not have permissions to access this mailbox.");
            return null;
        }
    }

    public static List<Uri> FetchLinksFromSource(string htmlSource)
    {
        List<Uri> links = new List<Uri>();
        string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
        MatchCollection matchesImgSrc = Regex.Matches(htmlSource, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match m in matchesImgSrc)
        {
            string href = m.Groups[1].Value;
            links.Add(new Uri(href));
        }
        return links;
    }

    public static async Task<List<Attachment>> getAttachment(string mailbox, string mailid, string? attachmentID)
    {
        string httpResponseMessage;
        List<Attachment> listAttachments = new List<Attachment>();
        // Get attachment(s) and add to attachments list
        ODataResponse<Attachment> responseContent;
        if(attachmentID.IsNullOrEmpty()){
            httpResponseMessage = await  _HTTPClient.GetStringAsync("https://graph.microsoft.com/v1.0/users/"+mailbox+"/messages/"+mailid+"/attachments");
        } else {
            httpResponseMessage = await  _HTTPClient.GetStringAsync("https://graph.microsoft.com/v1.0/users/"+mailbox+"/messages/"+mailid+"/attachments/?filter=isInline eq true");
        }
        responseContent =  JsonConvert.DeserializeObject<ODataResponse<Attachment>>(httpResponseMessage);
                // Add Mail ID to the properties
                for(var i=0; i<responseContent.Value.Length; i++){
                    responseContent.Value[i].mailID = mailid;
                }
                listAttachments.AddRange(responseContent.Value.ToList());
        
        return listAttachments;
    }

    public static async Task<List<Attachment>> getAttachmentsList(Config config, List<Mail> mailsList, string mailbox)
    {
        List<Attachment> listAttachments = new List<Attachment>();
        ODataResponse<Attachment> responseContent = new ODataResponse<Attachment>();
                
        foreach(Mail m in mailsList){
            // We check if the mail as attachment(s)
            if(m.hasAttachments){
                listAttachments.AddRange(getAttachment(mailbox, m.id, "").Result);
            }
            else 
            {
                // could be inline attachments, we have to parse the body
                if(m.body.Content.Contains("<img src=")){
                    foreach(Uri link in FetchLinksFromSource(m.body.Content)){
                        Console.WriteLine(link.ToString());
                        listAttachments.AddRange(getAttachment(mailbox, m.id, link.ToString()).Result);
                    }
                }
            }
        }
        return listAttachments;
    }

    public static async Task<List<Attachment>> detectQRCodeAttachments(Config config, List<Attachment> attachmentsList)
    {
        List<Attachment> listAttachments = new List<Attachment>();
        Byte[] httpResponseMessage = new Byte[]{};
        ODataResponse<Attachment> responseContent = new ODataResponse<Attachment>();
        if(!config.DownloadPath.IsNullOrEmpty()) {
           Directory.CreateDirectory(config.DownloadPath);
        }
    
        var resultingList = new List<Attachment>();
        foreach(Attachment m in attachmentsList)
        {
            // We downlaod the attachment in the defined folder
            httpResponseMessage = await  _HTTPClient.GetByteArrayAsync("https://graph.microsoft.com/v1.0/users/"+config.QuarantineMailbox+"/messages/"+m.mailID+"/attachments/"+m.id+"/$value");
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(config.DownloadPath,m.name),httpResponseMessage);

            // predict if QRCode and resolve it, add info to the Attachment object
            PredictionHandler.setAPIKey(config);
            resultingList.Add(await PredictionHandler.submitData(config,m));            
        }

        return resultingList;
    }
}
