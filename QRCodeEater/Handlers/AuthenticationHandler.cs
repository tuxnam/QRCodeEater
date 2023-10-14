using System.Net.Http.Headers;
using Newtonsoft.Json;
using QRCodeEater.Models;

namespace QRCodeEater.Handlers;

class AuthenticationHandler {

    private static HttpClient _HTTPClient = new HttpClient();
    public static Bearer? _bearerToken { get; private set; }
    public static async Task<Bearer> authenticate(Config config)
    {
        _HTTPClient.DefaultRequestHeaders.Accept.Clear();
        var values = new Dictionary<string, string>{
            { "client_id", config.ClientId },
            { "client_secret", config.ClientSecret },
            { "scope", config.Scope },
            { "grant_type", "client_credentials" }
        };

        var formContent = new FormUrlEncodedContent(values);
        _HTTPClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        var httpResponseMessage = await  _HTTPClient.PostAsync("https://login.microsoftonline.com/"+config.TenantId+"/oauth2/v2.0/token", formContent);
        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
        var responseAsJSON = JsonConvert.DeserializeObject<Bearer>(responseString);

        return responseAsJSON;
    }
}