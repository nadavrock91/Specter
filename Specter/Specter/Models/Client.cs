using SQLite;

namespace Specter.Models;

[Table("clients")]
public class Client
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string ClientNumber { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
