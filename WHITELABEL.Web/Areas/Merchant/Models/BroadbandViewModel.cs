﻿namespace WHITELABEL.Web.Areas.Merchant.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Web;

    public class BroadbandViewModel
    {
        [Required]
        [Display(Name = "Phone No")]
        [MaxLength(10, ErrorMessage = "Mobile no not greater then 10 digit")]
        [MinLength(10, ErrorMessage = "Mobile no not less then 10 digit")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Mobile must be number")]
        public string PhoneNo { get; set; }
        [Required]
        [Range(1, 9999)]
        [Display(Name = "Amount")]
        [RegularExpression(@"^\d*(\.\d{1,4})?$", ErrorMessage = "Enter Valid Amount")]
        public decimal RechargeAmount { get; set; }
        [Required]
        [Display(Name = "Service Provider")]
        public string ServiceName { get; set; }
        public string ServiceKey { get; set; }
        
    }
}