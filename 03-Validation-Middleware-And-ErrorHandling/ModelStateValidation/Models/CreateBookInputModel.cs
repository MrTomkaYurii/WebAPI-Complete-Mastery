using System.ComponentModel.DataAnnotations;

namespace AspNet_ModelStateValidation.Models
{
    public class CreateBookInputModel
    {
        //public string? Title { get; set; }
        //public string? Description { get; set; }
        //public string? ISBN { get; set; }


        [Required]
        public string? Title { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "The ISBN must be a string with the exact length of 13.")]
        public string? ISBN { get; set; }
    }
}
