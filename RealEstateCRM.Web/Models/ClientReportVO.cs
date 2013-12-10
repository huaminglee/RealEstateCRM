using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public class ClientStateProjectReportVO
    {
        public int  GroupId { get; set; }
        public string GroupName { get; set; }
        public int CallInNum { get; set; }
        public int CallVisitNum { get; set; }
        public int VisitNum { get; set; }
        public int Card1Num { get; set; }
        public int Card2Num { get; set; }
        public int OrderNum { get; set; }
        public int ContractNum { get; set; }
    }

    public class ClientListVO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ActionGroupId{get;set;}
        public string Person { get; set; }
        public string RoomType { get; set; }
        public string Name { get; set; }
        public DateTime StateDate { get; set; }
        public string Way { get; set; }
        public string WayExtend { get; set; }
        public string GroupName { get; set; }
        public string Action { get; set; }
        public string Remark { get; set; }
    }
   
}