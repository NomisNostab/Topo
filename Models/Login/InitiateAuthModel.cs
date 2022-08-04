namespace Topo.Models.Login
{
    public class InitiateAuthModel
    {
        public string? AuthFlow { get; set; }
        public string? ClientId { get; set; }
        public AuthParameters? AuthParameters { get; set; }
        public ClientMetadata? ClientMetadata { get; set; }
    }

    public class AuthParameters
    {
        public string? USERNAME { get; set; }
        public string? PASSWORD { get; set; }
        public string? DEVICE_KEY { get; set; }
        public string? REFRESH_TOKEN { get; set; }

    }

    public class ClientMetadata
    {
    }


}
