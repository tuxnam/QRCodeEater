using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using QRCodeEater.Models;
public class CSVHandler {

    private static void CreateHeader(List<Tuple<Mail,Attachment>> list, StreamWriter sw)
    {
        sw.Write("Mail ID" + ",");
        sw.Write("Mail Subject" + ",");
        sw.Write("Mail Sender" + ",");
        sw.Write("Mail Date" + ",");
        sw.Write("Mail Recipients" + ",");
        sw.Write("Mail CC Recipients" + ",");
        sw.Write("Inline Attachment" + ",");
        sw.Write("Attachment ID" + ",");
        sw.Write("Attachment Name" + ",");
        sw.Write("Decoded String" + sw.NewLine);
    }

    private static void CreateRows(List<Tuple<Mail,Attachment>> list, StreamWriter sw)
    {
        foreach(Tuple<Mail,Attachment> l in list)
        {
            sw.Write(l.Item1.id + ",");
            sw.Write(l.Item1.subject + ",");
            sw.Write(l.Item1.sender.EmailAddress.Name + ",");
            sw.Write(l.Item1.sentDateTime.DateTime.ToUniversalTime() + ",");
            string toRecipients = "";
            if (!l.Item1.toRecipients.IsNullOrEmpty())
            {
                foreach(Microsoft.Graph.Models.Recipient r in l.Item1.toRecipients){
                    toRecipients += r.EmailAddress.Name+";";
                }
                toRecipients = toRecipients.Remove(toRecipients.Length - 1);
            }
            sw.Write(toRecipients + ",");
            string ccRecipients = "";
            if (!l.Item1.ccRecipients.IsNullOrEmpty())
            {
                foreach(Microsoft.Graph.Models.Recipient r in l.Item1.ccRecipients){
                    ccRecipients += r.EmailAddress.Name+";";
                }
                ccRecipients = ccRecipients.Remove(ccRecipients.Length - 1);
            }
            sw.Write(ccRecipients + ",");
            sw.Write(!l.Item1.hasAttachments + ",");
            sw.Write(l.Item2.id + ",");
            sw.Write(l.Item2.name + ",");
            sw.Write(l.Item2.ResolvedText + sw.NewLine);
        }
    }

    public static void CreateCSV(List<Tuple<Mail,Attachment>> list, string filePath)
    {
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            CreateHeader(list, sw);
            CreateRows(list, sw);
        }
    }
}