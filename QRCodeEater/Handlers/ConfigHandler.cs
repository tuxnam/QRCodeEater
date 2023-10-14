using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCodeEater.Models;

namespace QRCodeEater.Handlers;

public class ConfigHandler
{
    public string? ClientId { get; set; }
    public string? TenantId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
    public string? QuarantineMailbox { get; set; }
    public string? DownloadPath { get; set; }

    public static Config LoadSettings()
    {

        // Load settings
        IConfiguration config = new ConfigurationBuilder()
            // appsettings.json is required
            .AddJsonFile("Config.json", optional: false)
            // appsettings.Development.json" is optional, values override Config.json
            .AddJsonFile($"Config.Development.json", optional: true)
            // User secrets are optional, values override both JSON files
            .AddUserSecrets<Program>()
            .Build();

        return config.GetRequiredSection("Settings").Get<Config>() ??
            throw new Exception("Could not load app settings. See README for configuration instructions.");
    }
}