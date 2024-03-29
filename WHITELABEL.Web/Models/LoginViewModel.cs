﻿using System.ComponentModel.DataAnnotations;

namespace WHITELABEL.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Member Mobile")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Mobile must be number")]
        public string MEMBER_MOBILE { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}