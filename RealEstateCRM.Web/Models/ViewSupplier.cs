using System;
using System.Collections.Generic;
using System.Linq;

namespace BMS.Web.Models
{
      
    public class ViewSupplier
    {
        
        public int id { get; set; }
       
        public int State { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Catalog { get; set; }
        public int Level { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Phone { get; set; }
        public string Brand { get; set; }
        public string Qualification { get; set; }
        public string Remark { get; set; }

    }   
}