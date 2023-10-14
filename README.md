# Documentation

This program detects attempt of email quishing using Azure Computer vision. 
The program’s goal is to connect to Exchange through Graph API, and analyze emails in a mailbox, a list of mailboxes or resulting from a filter query. All emails with attachments are filtered out to keep only images content-types. Resulting attachments are downloaded, send to a Computer Vision prediction model built in Azure, and based on the results (probability threshold defined in a configuration file), defined as suspicious or not. Suspicious attachments are further decoded by a QRCode library and the resulting URL is added to the resulting analysis. The program outputs a .csv file with all the emails analyzed which contains a potentially malicious QR Code.

---

[#] QRCodeEater, a simple program developed for learning purposes, created by @Guillaume B.
[#] Args parsed  

Arguments:

   --searchFilter <search_filter>  Exchange search filter - Resultset will be analyzed by QRCodeEater for Quishing matches (optional)
   --mailboxList <mailboxList>     Specify one or a list of mailbox to target (comma-separated) (optional)
   --safe                          Delete attachments from disk after analysis

Examples:

   --searchFilter <search_filter>  Exchange search filter - Resultset will be analyzed by QRCodeEater for Quishing matches (optional) - Format: https://learn.microsoft.com/en-us/graph/search-query-parameter?tabs=http#using-search-on-message-collections

       QRCodeEater.exe --searchFilter "(from/emailAddress/address) eq 'ceo@mycompany.com'"
       QRCodeEater.exe --searchFilter "attachment:auth.png"
       QRCodeEater.exe --searchFilter "body:urgent"

   --mailboxList <mailboxList>     Specify a list of mailboxes for QRCodeEater to target (optional)

       QRCodeEater.exe --mailbox "quarantine@mycompany.com,ceo@mycompany.com"
       QRCodeEater.exe --mailbox "quarantine@mycompany.com"

## Configuration file - Config.json

- ClientId: The Azure AD application ID (service principal)
- ClientSecret: The secret created in the Azure AD application
- TenantId: The Azure AD tenant ID
- QuarantineMailbox: Only used if no parameters are specified in the program arguments (mailbox which will be scanned)
- DownloadPath: Path to download the attachments before analysis (they can be deleted afterwards automaticaly for safety)
- PredictionKey: The API key to the Prediction API
- PredictionProjectID: The ID of the Prediction project
- ModelName: The model name
- SuspiciousThreshold: The probability threshold to consider an image as a QRCode

