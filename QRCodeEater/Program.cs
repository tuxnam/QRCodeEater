using QRCodeEater.Handlers;
using QRCodeEater.Models;
using Microsoft.IdentityModel.Tokens;


// This is standalone program meant to be run as an executable taking an Exchange filter as a parameter but it could be adapted to:
// - Be hosted in an Azure Function (or similar serverless service elsewhere) and called from a Logic App, for instance based on a Sentinel incident (on entities) or for hunting purposes 
// - Be used in a PowerShell script or similar admin script for recurring checks 
// - Be used as a scheduled task to monitor a shared mailbox or VIP users mailboxes
// - ...
// *WARNING*: this is not production ready software and will never be, this is meant for learning purposes. 
class Program {
  public static Config? _config { get; set; }
  private static Bearer? _bearerToken { get; set; }
  private static List<Mail>? _mailsList { get; set; }
  private static List<Attachment>? _attachmentsList { get; set; }
  private static List<Attachment>? _resultingAttachmentsList { get; set; }
  private static List<string> _imagesContentType = new List<string>{"image/png","image/tiff","image/jpeg","image/gif","image/vnd.microsoft.icon","image/svg+xml","image/x-icon"};
  private const int IMAGES_FILTER = 1;

  private static List<Attachment> filterOutAttachments()
  {

    List<Attachment> suspiciousAttachmentsList = new List<Attachment>();
    foreach(Attachment att in _attachmentsList)
    {
      // We are interested in images only
      if(_imagesContentType.Contains(att.contentType)){
        suspiciousAttachmentsList.Add(att);
      }
    }

    return suspiciousAttachmentsList;
  }

  private static void PrintProgRequirements()
  {
    Console.WriteLine("Arguments:\n");
    Console.WriteLine("   --searchFilter <search_filter>  Exchange search filter - Resultset will be analyzed by QRCodeEater for Quishing matches (optional)");
    Console.WriteLine("   --mailboxList <mailboxList>     Specify one or a list of mailbox to target (comma-separated) (optional) ");
    Console.WriteLine("   --safe                          Delete attachments from disk after analysis");
    Console.WriteLine("");
    Console.WriteLine("Examples:\n");
    Console.WriteLine("   --searchFilter <search_filter>  Exchange search filter - Resultset will be analyzed by QRCodeEater for Quishing matches (optional) - Format: https://learn.microsoft.com/en-us/graph/search-query-parameter?tabs=http#using-search-on-message-collections\n");
    Console.WriteLine("       QRCodeEater.exe --searchFilter \"(from/emailAddress/address) eq 'ceo@mycompany.com'\" ");
    Console.WriteLine("       QRCodeEater.exe --searchFilter \"attachment:auth.png\" ");
    Console.WriteLine("       QRCodeEater.exe --searchFilter \"body:urgent\"\n");
    Console.WriteLine("   --mailboxList <mailboxList>     Specify a list of mailboxes for QRCodeEater to target (optional)\n");
    Console.WriteLine("       QRCodeEater.exe --mailbox \"quarantine@mycompany.com,ceo@mycompany.com\"");
    Console.WriteLine("       QRCodeEater.exe --mailbox \"quarantine@mycompany.com\"\n");       
  }     

  private static async Task LaunchEater(string[] mailboxList)
  {
      _mailsList =  new List<Mail>();
      _attachmentsList = new List<Attachment>();
      var outputPath = System.IO.Path.Combine(_config.DownloadPath,"results.csv");

      // Authenticate
      _bearerToken = await AuthenticationHandler.authenticate(_config);
      ExchangeHandler.setBearer(_bearerToken);

      // Get mails from inbox of requested mailbox
      foreach(string mailbox in mailboxList){
        var tempMailArray = await ExchangeHandler.getMailsList(_config,mailbox);
        if(!tempMailArray.IsNullOrEmpty())
        {
          _mailsList.AddRange(tempMailArray.ToList());
        }
      }

      // Get the list of attachments from the above list of emails
      foreach(string mailbox in mailboxList){
        var tempAttArray = await ExchangeHandler.getAttachmentsList(_config,_mailsList,mailbox);
        if(!tempAttArray.IsNullOrEmpty())
        {
          _attachmentsList.AddRange(tempAttArray.ToList());
        }
      }

      // Analyze attachments and filter out the ones which are not images 
      // Analyse resulting attachments by downloading, resolving and tagging them
      _resultingAttachmentsList = await ExchangeHandler.detectQRCodeAttachments(_config,filterOutAttachments());

      // building the results in a list of tuples (Mail, Attachments)
      List<Tuple<Mail,Attachment>> resultingList = new List<Tuple<Mail,Attachment>>();
      // sort attachment list by mail IDs
      _resultingAttachmentsList.Sort((p, q) => p.mailID.CompareTo(q.mailID));

      foreach(Attachment result in _resultingAttachmentsList){
        if(result.isSuspicious){
          Mail m = _mailsList.Find(item => item.id == result.mailID);
          resultingList.Add(new Tuple<Mail, Attachment>(m,result));
        }
      }

      CSVHandler.CreateCSV(resultingList,outputPath);
      Console.WriteLine("[#] Results: QRCodeEater found "+resultingList.Count()+" potential Quishing emails. Results written to: "+outputPath);
  }

  private static async Task LaunchEater(SearchFilter search)
  {

  }

  private static void DisplayError(string message)
  {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message+"\n");
    Console.ResetColor();
  }

  static async Task Main(string[] args)
  {
    Console.WriteLine("[#] QRCodeEater, a simple program developed for learning purposes, created by @Guillaume B.");
    Console.WriteLine($"[#] Args parsed {string.Join(' ', args)} \n");
    
    if (args.Length == 0)
    {
      PrintProgRequirements();
    } 
    else
    {
      // Load config
      _config = ConfigHandler.LoadSettings();

      // Search Filter scenario
      var searchFilterSet = Array.FindIndex(args,row => row.Contains("--searchFilter"));
      if (searchFilterSet > -1 && !args[searchFilterSet + 1].IsNullOrEmpty())
      {
        Console.WriteLine("Search filter set - ignoring other arguments");
        SearchFilter search = new SearchFilter
        {
          searchQuery = args[searchFilterSet + 1]
        };
        await LaunchEater(search);
      }
      else 
      {
        // List of mailboxes scenario
        var mailboxListSet = Array.FindIndex(args,row => row.Contains("--mailboxList"));
        if (mailboxListSet > -1) 
        {
          Console.WriteLine("Mailbox List search - ignoring other arguments");
          string[] mailboxList = args[mailboxListSet+1].Split(",");
          await LaunchEater(mailboxList);
        } 
        else 
        {
            DisplayError("Error - Invalid or missing arguments.");
            PrintProgRequirements();
        }
      }

    }

  }

}




