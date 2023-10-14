using System.Text.Json.Nodes;

namespace QRCodeEater.Models;

public class Prediction {
    public string? id { get; set; }
    public string? project { get; set; }
    public string? iteration { get; set; }
    public string? created { get; set; }
    public Label[]? predictions { get; set; }
}