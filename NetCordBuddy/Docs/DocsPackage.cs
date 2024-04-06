using System.ComponentModel.DataAnnotations;

namespace NetCordBuddy.Docs;

#nullable disable

public class DocsPackage
{
    [Required]
    public string Name { get; init; }

    [Required]
    public string Framework { get; init; }
}
