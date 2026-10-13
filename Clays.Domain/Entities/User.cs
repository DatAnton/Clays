using System.Web;

namespace Clays.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public long ChatId { get; set; }
        public string DisplayName => HttpUtility.HtmlEncode($"{FirstName} {LastName} (@{UserName})");
    }
}
