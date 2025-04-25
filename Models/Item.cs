namespace TaskApi.Models;

public class Item
{
    public int Id { get; set; }
    public required string Title { get; set; } 
    public string? Description { get; set; } 
    public DateTime? DueDate { get; set; } 
    public string Status { get; set; } = "Pending"; 

}