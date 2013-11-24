using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{

    public enum ContractState{意向,签约,退伙,转让}
    public class Contract
    {
        public static string LogClass = "合同";
        public int Id { get; set; }
        public ContractState State { get; set; }
        [DisplayName("基金")]
        public int FundId { get; set; }
        [DisplayName("客户")]
        public int ClientId { get; set; }
        [DisplayName("意向日期")]
        public DateTime? IntentDate { get; set; }
        [DisplayName("签约日期")]
        public DateTime? ContractDate { get; set; }
        [DisplayName("金额")]
        public decimal Money { get; set; }
        [DisplayName("备注")]
        public string Remark { get; set; }
    }   
    public class ContractPlan
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
         [DisplayName("计划金额")]
        public decimal Money { get; set; }
         [DisplayName("计划日期")]
        public DateTime PlanDate { get; set; }
    }
    public class  ContractPay
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
         [DisplayName("付款金额")]
        public decimal Money { get; set; }
         [DisplayName("付款日期")]
        public DateTime PlanDate { get; set; }
        [DisplayName("备注")]
        public string Remark { get; set; }
    }
}