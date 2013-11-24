using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BMS.Web.Models;
using Dapper;
using OUDAL;
using System.Text;
namespace BMS.Web.BLL
{
    public class ProductBLL
    {
        public static List<Product> GetProducts()
        {
            using(OUContext db=new OUContext())
            {
                List<Product> list = (from o in db.Products orderby o.Catalog select o).ToList();
                return list;
            }
        }
        public static Product GetProduct(int id)
        {
            using (OUContext db = new OUContext())
            {
                return db.Products.Find(id);
            }
        }
    }
}