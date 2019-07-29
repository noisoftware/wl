﻿namespace WHITELABEL.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using WHITELABEL.Data.Models;

    public class DBContext : DbContext
    {
        public DBContext() : base("name=DefaultConnection") {
            this.Configuration.ProxyCreationEnabled = false;
        }
        public DbSet<TBL_AUTH_ADMIN_USER> TBL_AUTH_ADMIN_USERS { get; set; }
        public DbSet<TBL_MASTER_MEMBER> TBL_MASTER_MEMBER { get; set; }
        public DbSet<TBL_MASTER_MEMBER_ROLE> TBL_MASTER_MEMBER_ROLE { get; set; }
        public DbSet<TBL_WHITE_LEVEL_HOSTING_DETAILS> TBL_WHITE_LEVEL_HOSTING_DETAILS { get; set; }
        public DbSet<TBL_SETTINGS_SERVICES_MASTER> TBL_SETTINGS_SERVICES_MASTER { get; set; }
        public DbSet<TBL_WHITELABLE_SERVICE> TBL_WHITELABLE_SERVICE { get; set; }
        public DbSet<TBL_SETTINGS_BANK_DETAILS> TBL_SETTINGS_BANK_DETAILS { get; set; }
        public DbSet<TBL_BALANCE_TRANSFER_LOGS> TBL_BALANCE_TRANSFER_LOGS { get; set; }
        public DbSet<TBL_ACCOUNTS> TBL_ACCOUNTS { get; set; }
        public DbSet<TBL_STATES> TBL_STATES { get; set; }
        public DbSet<TBL_API_RESPONSE_OUTPUT> TBL_API_RESPONSE_OUTPUT { get; set; }
        public DbSet<TBL_OPERATOR_MASTER> TBL_OPERATOR_MASTER { get; set; }
        public DbSet<TBL_BANK_MASTER> TBL_BANK_MASTER { get; set; }
        public DbSet<TBL_API_SETTING> TBL_API_SETTING { get; set; }
        public DbSet<TBL_DMR_API_RESPONSE> TBL_DMR_API_RESPONSE { get; set; }
        public DbSet<TBL_DMR_APPLICANT_INFO> TBL_DMR_APPLICANT_INFO { get; set; }
        public DbSet<TBL_SERVICE_PROVIDERS> TBL_SERVICE_PROVIDERS { get; set; }
        public DbSet<TBL_INSTANTPAY_RECHARGE_RESPONSE> TBL_INSTANTPAY_RECHARGE_RESPONSE{ get; set; }
        public DbSet<TBL_DMR_REMITTER_INFORMATION> TBL_DMR_REMITTER_INFORMATION { get; set; }
        public DbSet<TBL_REMITTER_BENEFICIARY_INFO> TBL_REMITTER_BENEFICIARY_INFO { get; set;    }
        public DbSet<TBL_DMR_FUND_TRANSFER_DETAILS> TBL_DMR_FUND_TRANSFER_DETAILS { get; set; }
        public DbSet<TBL_API_COMMISION_STRUCTURE> TBL_API_COMMISION_STRUCTURE { get; set; }
        public DbSet<TBL_WHITE_LEVEL_COMMISSION_SLAB> TBL_WHITE_LEVEL_COMMISSION_SLAB { get; set; }
        public DbSet<TBL_COMM_SLAB_MOBILE_RECHARGE> TBL_COMM_SLAB_MOBILE_RECHARGE { get; set; }
        public DbSet<TBL_COMM_SLAB_UTILITY_RECHARGE> TBL_COMM_SLAB_UTILITY_RECHARGE { get; set; }
        public DbSet<TBL_COMM_SLAB_DMR_PAYMENT> TBL_COMM_SLAB_DMR_PAYMENT { get; set; }
        public DbSet<TBL_DETAILS_MEMBER_COMMISSION_SLAB> TBL_DETAILS_MEMBER_COMMISSION_SLAB { get; set; }
        public DbSet<TBL_AIRPORT_DETAILS> TBL_AIRPORT_DETAILS { get; set; }
        public DbSet<TBL_MERCHANT_OUTLET_INFORMATION> TBL_MERCHANT_OUTLET_INFORMATION { get; set; }
        public DbSet<TBL_API_TOKEN> TBL_API_TOKEN { get; set; }
    }
    
}