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

