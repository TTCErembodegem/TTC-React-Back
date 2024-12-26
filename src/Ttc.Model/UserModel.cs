namespace Ttc.Model;

public class User
{
    #region Properties
    public int PlayerId { get; set; }
    public string Alias { get; set; }
    public ICollection<int> Teams { get; set; }
    public ICollection<string> Security { get; set; }
    public string Token { get; set; }
    #endregion

    #region Constructor
    public User()
    {
        Teams = new List<int>();
        Security = new List<string>();
    }
    #endregion

    public override string ToString() => $"PlayerId={PlayerId}, Teams='{string.Join(", ", Teams)}', Security='{string.Join(", ", Security)}'";
}

public class UserCredentials
{
    #region Properties
    public int PlayerId { get; set; }
    public string Password { get; set; }
    #endregion

    public override string ToString() => $"PlayerId: {PlayerId}, Password: {Password}";
}

public class PasswordCredentials
{
    #region Properties
    public int PlayerId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    #endregion

    public override string ToString() => $"PlayerId: {PlayerId}, OldPassword: {OldPassword}, NewPassword: {NewPassword}";
}

public class NewPasswordLinkRequest
{
    #region Properties
    public int PlayerId { get; set; }
    public string Email { get; set; }
    #endregion

    public override string ToString() => $"PlayerId={PlayerId}, Email={Email}";
}

public class NewPasswordRequest
{
    #region Properties
    public Guid Guid { get; set; }
    public int PlayerId { get; set; }
    public string Password { get; set; }
    #endregion

    public override string ToString() => $"PlayerId={PlayerId}, Guid={Guid}";
}
