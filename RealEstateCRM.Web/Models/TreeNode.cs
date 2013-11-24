using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateCRM.Web.Models
{
    public class TreeNode
    {
        public int id;
        public int pId;
        public string name;
        public bool open;
        public string tag;
    }
}