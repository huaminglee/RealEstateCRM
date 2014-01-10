using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    
    public class WayReportVO
    {
        public int  ProjectId { get; set; }
        public int ProjectName { get; set; }
        public string Way { get; set; }
        public string RoomType { get; set; }
        public int CallInNum { get; set; }
        public int CallVisitNum { get; set; }
        public int VisitNum { get; set; }
        public int Card1Num { get; set; }
        public int Card2Num { get; set; }
        public int OrderNum { get; set; }
        public int ContractNum { get; set; }
        public static WayReportVO GetVo(List<WayReportVO> list, string way)
        {
            foreach (var i in list)
            {
                if (i.Way == way) return i;
            }
            list.Add(new WayReportVO {Way = way});
            return list[list.Count - 1];
        }
        public static WayReportVO GetVo(List<WayReportVO> list,int projectid, string way)
        {
            foreach (var i in list)
            {
                if (i.Way == way&&i.ProjectId==projectid) return i;
            }
            list.Add(new WayReportVO { Way = way,ProjectId=projectid });
            return list[list.Count - 1];
        }
    }
   
}