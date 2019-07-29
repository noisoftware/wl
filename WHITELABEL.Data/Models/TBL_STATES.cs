namespace WHITELABEL.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;

    [Table("STATE")]
    public class TBL_STATES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long STATEID { get; set; }
        public string STATENAME { get; set; }
    }
}
