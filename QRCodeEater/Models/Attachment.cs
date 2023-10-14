namespace QRCodeEater.Models;
public class Attachment {
    public string? contentType {get; set;}
    public string? id {get; set;}
    public bool? isInline {get; set;}
    public DateTimeOffset? lastModifiedDateTime	 {get; set;}
    public string? name	{get; set;}
    public int? size {get; set;}
    public string? mailID {get; set;}
    public bool isSuspicious {get; set;}
    public string? ResolvedText { get; set; }
}