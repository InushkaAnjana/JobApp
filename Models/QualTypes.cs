using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class QualTypes
    {
        [Required]
        public int QTypeID { get; set; }

        [DisplayName("Qualification Type")]
        [MaxLength(50)]
        [Required]
        public string QualType { get; set; } = string.Empty;
    }
}
