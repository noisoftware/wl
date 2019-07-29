﻿namespace WHITELABEL.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("ACCOUNTS")]
    public class TBL_ACCOUNTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ACC_NO { get; set; }
        public long API_ID { get; set; }
        public long MEM_ID { get; set; }
        public string MEMBER_TYPE { get; set; }
        public string TRANSACTION_TYPE { get; set; }
        public DateTime TRANSACTION_DATE { get; set; }
        public DateTime TRANSACTION_TIME { get; set; }
        public string DR_CR { get; set; }
        public decimal AMOUNT { get; set; }
        public string NARRATION { get; set; }
        public decimal OPENING { get; set; }
        public decimal CLOSING { get; set; }
        public long REC_NO { get; set; }
        public decimal COMM_AMT { get; set; }
        [NotMapped]
        public string UserName { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
        [NotMapped]
        public string timevalue { get; set; }
        [NotMapped]
        public long SerialNo { get; set; }
    }
}