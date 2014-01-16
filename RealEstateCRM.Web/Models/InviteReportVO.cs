using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OUDAL;

namespace RealEstateCRM.Web.Models
{
    
    public class InviteReportVO
    {
        public int  Id { get; set; }
        public string Name { get; set; }
        public string RoomType { get; set; }
        public int VisitNum { get; set; }
        public int Card1Num { get; set; }
        public int Card2Num { get; set; }
        public int OrderNum { get; set; }
        public int ContractNum { get; set; }

        public int VisitVisitNum { get; set; }
        public int Card1VisitNum { get; set; }
        public int Card2VisitNum { get; set; }
        public int OrderVisitNum { get; set; }
        public int ContractVisitNum { get; set; }
        public int VisitDoneNum { get; set; }
        public int Card1DoneNum { get; set; }
        public int Card2DoneNum { get; set; }
        public int OrderDoneNum { get; set; }
        public int ContractDoneNum { get; set; }
    }
   
}