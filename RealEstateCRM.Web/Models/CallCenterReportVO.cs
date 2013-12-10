using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OUDAL;

namespace RealEstateCRM.Web.Models
{


    public class CallCentenListVO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public string Person { get; set; }
        public string CallPerson { get; set; }
        public ClientStateEnum State { get; set; }
        public string InvalidReason { get; set; }
        public DateTime? VisitTime { get; set; }
        //public DateTime PlanTime { get; set; }
        public DateTime? CardDate { get; set; }
        public DateTime? CardCancelDate { get; set; }
        public DateTime? OrderDate { get; set; }
        //public DateTime SignDate { get; set; }
        public DateTime? OrderCancelDate { get; set; }

    }
   
}