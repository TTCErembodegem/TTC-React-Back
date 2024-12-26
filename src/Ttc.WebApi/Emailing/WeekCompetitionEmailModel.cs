namespace Ttc.WebApi.Emailing;

public class WeekCompetitionEmailModel
{
    public string Title { get; set; } = "";
    public string Email { get; set; } = "";
    
    public override string ToString() => $"{Title}: {Email}";
}