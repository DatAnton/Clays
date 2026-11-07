using System.Web;

namespace Clays.Domain.Entities
{
    public class User
    {
        public const long DefaultChatId = 503590839;
        public int Id { get; set; }
        public string? Auth0Subject { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public long? ChatId { get; set; }
        public string? TCToken { get; set; } = string.Empty;
        public string? TCRefreshToken { get; set; } = string.Empty;
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public string DisplayName => HttpUtility.HtmlEncode($"{FirstName} {LastName} (@{UserName})");
    }
}
