namespace Topo.Models.Login
{
    public class AuthenticationResultModel
    {
        public AuthenticationSuccessResultModel AuthenticationSuccessResultModel { get; set; } = new AuthenticationSuccessResultModel();
        public AuthenticationErrorResultModel AuthenticationErrorResultModel { get; set; } = new AuthenticationErrorResultModel();
    }
}
