namespace Ttc.Model.Core;

public class TtcSettings
{
    public string JwtSecret { get; set; }
    public string Issuer { get; set; }
    public string PublicImageFolder { get; set; }
    public string ConnectionString { get; set; }
}