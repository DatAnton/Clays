namespace Clays.Domain.Entities;

public class CompanyComment
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int AuthorId { get; set; }

    public Company Company { get; set; } = null!;
    public User Author { get; set; } = null!;
}
