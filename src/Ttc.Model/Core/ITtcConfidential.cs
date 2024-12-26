namespace Ttc.Model.Core;

/// <summary>
/// Hide properties when the user is not logged in
/// </summary>
public interface ITtcConfidential
{
    void Hide();
}
