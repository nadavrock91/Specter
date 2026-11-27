using SQLite;
namespace Specter.Models;

[Table("client_categories")]
public class ClientCategory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
}
