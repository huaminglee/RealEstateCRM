using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public class MaterialDealDate
    {
        public static string sqlIn = @"select item.MaterialId,i.InDate as dealDate,item.unitprice,item.num,item.num as [left],item.unitprice*item.num as price
from materialins i join materialinitems item on i.id=item.inid";
        public static string sqlOut = @"select item.MaterialId,i.outDate as dealDate,item.num ,item.num as [left]
from materialOuts i join materialOutitems item on i.id=item.outid";
        public int MaterialId { get; set; }
        public DateTime DealDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Num { get; set; }
        public decimal Price { get; set; }
        public decimal Left { get;set; }
    }
}