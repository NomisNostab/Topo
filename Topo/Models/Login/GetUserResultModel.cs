namespace Topo.Models.Login
{
    public class GetUserResultModel
    {
        public UserAttribute[]? UserAttributes { get; set; }
        public string? Username { get; set; }
    }

    public class UserAttribute
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

}
