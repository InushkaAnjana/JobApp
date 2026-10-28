using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class QualNames
    {
        [Required]
        public int QNameID { get; set; }

        [DisplayName("Qualification Name")]
        [MaxLength(50)]
        [Required]
        public string QualName { get; set; } = string.Empty;

        [DisplayName("Qualification Type")]
        [Required]
        public int QTypeID { get; set; }
    }

    public class QualNameViewModel: QualNames
    {
        [DisplayName("Qualification Type")]
        public string? QualType { get; set; }
    }
}
