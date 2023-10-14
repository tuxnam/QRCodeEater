namespace QRCodeEater.Models;

public class Config {
    public string? ClientId { get; set; }
    public string? TenantId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
    public string? QuarantineMailbox { get; set; }
    public string? DownloadPath { get; set; } = "";
    public string? PredictionKey { get; set; }
    public string? PredictionProjectID { get; set; }
    public string? ModelName { get; set; }
    public double? SuspiciousThreshold { get; set; }
}