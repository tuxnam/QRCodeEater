using System;
using System.Text.Json.Serialization;

namespace QRCodeEater.Models;

public sealed record class Bearer(
    [property: JsonPropertyName("token_type")] string Token_type,
    [property: JsonPropertyName("expires_in")] string Expires_in,
    [property: JsonPropertyName("ext_expires_in")] string Ext_expires_in,
    [property: JsonPropertyName("access_token")] string Access_token)
{
}