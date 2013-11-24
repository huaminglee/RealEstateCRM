using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BMS.Web.Models
{
    public class CardReportModel
    {
        //public string CardType { get; set; }
        public string Department { get; set; }
        public string Company { get; set; }
        public int Num { get; set; }
        public decimal ChargedMoney { get; set; }
        public decimal ConsumeMoney { get; set; }
        public decimal LeftMoney { get; set; }
    }
}