using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Villa_API.Models;

public class VillaNumber
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption
        .None)] // disable value generation that has been set up by convention. For example, a primary key of type int is usually implicitly configured as value-generated-on-add (e.g. identity column on SQL Server).
    public int VillaNo { get; set; }

    [ForeignKey("Villa")] public int VillaId { get; set; }

    public Villa Villa { get; set; }
    public string SpecialDetails { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}