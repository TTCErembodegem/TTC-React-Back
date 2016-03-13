namespace Ttc.WebApi.Utilities.Auth
{
    public class ValidateTokenRequest
    {
        public string Token { get; set; }

        public override string ToString()
        {
            return $"Token: {Token}";
        }
    }
}