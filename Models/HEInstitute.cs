using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class HEInstitute
    {
        [Required]
        public int HEInstituteID { get; set; }

        [DisplayName("Higher Education Institute Name")]
        [MaxLength(50)]
        [Required]
        public string HEInstituteName { get; set; } = string.Empty;

    }
}
