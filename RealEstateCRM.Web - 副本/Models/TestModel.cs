using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public enum StateEnum { Normal, Disabled }
    public class TestModel
    {
        public string Str { get; set; }
        public DateTime Date { get; set; }
        public StateEnum State { get; set; }
        public List<TestSubModel> Children { get; set; }
    }
    public class TestSubModel
    {
        public int Id { get; set; }
        public string Str { get; set; }
    }
}