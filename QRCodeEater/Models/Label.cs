using System.Text.Json.Nodes;

namespace QRCodeEater.Models;

public class Label {
    public string? probability { get; set; }
    public string? tagId { get; set; }
    public string? tagName { get; set; }

}