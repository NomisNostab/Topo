namespace Topo.Models.Login
{

    public class AuthenticationResultModel
    {
        public AuthenticationResult? AuthenticationResult { get; set; }
        public ChallengeParameters? ChallengeParameters { get; set; }
    }

    public class AuthenticationResult
    {
        public string? AccessToken { get; set; }
        public int? ExpiresIn { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? TokenType { get; set; }
    }

    public class ChallengeParameters
    {
    }
}
