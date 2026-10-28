using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class PQInstitute
    {
        [Required]
        public int PQInstituteID { get; set; }

        [DisplayName("Professional Qualification Institute Name")]
        [MaxLength(50)]
        [Required]
        public string PQInstituteName { get; set; } = string.Empty;
    }
}
