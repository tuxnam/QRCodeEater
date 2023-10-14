using System.Collections.ObjectModel;
using Microsoft.Graph.Models;

namespace QRCodeEater.Models;

public class Mail {
    public string? id {get; set;}
    public string? subject {get; set;}
    public bool hasAttachments {get; set;}
    public Recipient? sender {get; set;}
    public Collection<Recipient>? toRecipients {get; set;}
    public DateTimeOffset sentDateTime {get; set;}
    public Collection<Recipient>? ccRecipients {get; set;}
    public ItemBody? body {get; set;}
}