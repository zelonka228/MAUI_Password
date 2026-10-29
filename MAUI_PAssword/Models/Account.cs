namespace MAUI_PAssword.Models;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string Password { get; set; } = "";
    public List<string> Tags { get; set; } = [];
}
