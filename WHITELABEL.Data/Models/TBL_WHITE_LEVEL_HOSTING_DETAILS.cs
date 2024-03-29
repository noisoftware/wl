﻿namespace WHITELABEL.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("WHITE_LEVEL_HOSTING_DETAILS")]
    public class TBL_WHITE_LEVEL_HOSTING_DETAILS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SLN { get; set; }
        public long MEM_ID { get; set; }
        [Required]
        [Display(Name = "Company Name")]
        [StringLength(255, ErrorMessage = "Company name must be 255 digit")]
        public string COMPANY_NAME { get; set; }
        [Required]
        [Display(Name = "Domain Name")]
        [StringLength(255, ErrorMessage = "Domain name must be 255 digit")]
        public string DOMAIN { get; set; }
        [Required]
        [Display(Name ="Long Code")]
        [StringLength(2, ErrorMessage = "Long code must be 2 digit")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Long code must be number")]
        public string LONG_CODE { get; set; }
        public string MOTO { get; set; }
        public string LOGO { get; set; }
        public string BANNER { get; set; }
        [NotMapped]
        public string memberName { get; set; }
    }
}
