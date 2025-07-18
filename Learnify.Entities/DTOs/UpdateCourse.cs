using System.ComponentModel.DataAnnotations;

public class UpdateCourse
{
    [StringLength(100)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public string? Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }
}
