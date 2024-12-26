namespace Ttc.DataAccess.Utilities;

public class EmailConfig
{
    public string EmailFrom { get; set; }
    public string SendGridApiKey { get; set; }

    public EmailConfig(string emailFrom, string sendGridApiKey)
    {
        EmailFrom = emailFrom;
        SendGridApiKey = sendGridApiKey;
    }

    public override string ToString() => $"{EmailFrom}, ${SendGridApiKey}";
}
