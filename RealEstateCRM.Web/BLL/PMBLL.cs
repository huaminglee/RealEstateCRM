using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using OUDAL;
namespace RealEstateCRM.Web.BLL
{
    public class PMBLL
    {
        
        static public int GetDeptByName(string name)
        {
            Department d = (from o in DepartmentBLL.Departments where o.Name == name select o).FirstOrDefault();
            if (d == null) return 0;
            return d.Id;
        }

   

    }
}