using System.ComponentModel.DataAnnotations;

namespace Topo.Data.Models
{
    public class Authentication
    {
        [Key]
        public int Id { get; set; }
        public string? AccessToken { get; set; }
        public int? ExpiresIn { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? TokenType { get; set; }
        public string? MemberName { get; set; }
        public DateTime? TokenExpiry { get; set; }
    }
}
