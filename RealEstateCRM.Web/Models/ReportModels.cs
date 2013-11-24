using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OUDAL;
namespace BMS.Web
{
    
    public class SaleItem
    {
        public int store;
        public string cardId;
        public decimal money;
        public string remark;
    }
    public class CardClient
    {
        public string CardId { get; set; }
        public string CardCode { get; set; }
        public ClientState State { get; set; }
        public string CardType { get; set; }
        public decimal Money { get; set; }
        public string Remark { get; set; }
        public string UserName { get; set; }
        public string Mobile { get; set; }
        public GenderEnum Gender { get; set; }
        public string Ages { get; set; }
    }
    public class CardStoreReport
    {
        public string Store { get; set; }
        public decimal Money { get; set; }
        public int Num { get; set; }
    }
    public class CardSaleReport
    {
        public string Store { get; set; }
        public string CardCode { get; set; }
        public string Sales { get; set; }
        public string SaleTime { get; set; }
        public decimal Money { get; set; }
        public string Remark { get; set; }
    }
    public class CardRechargeReport
    {
        public string Store { get; set; }
        public string CardCode { get; set; }
        public string Sales { get; set; }
        public string RechargeTime { get; set; }
        public decimal Money { get; set; }
        public string Remark { get; set; }
        public bool IsActiveRecharge { get; set; }
    }
} 