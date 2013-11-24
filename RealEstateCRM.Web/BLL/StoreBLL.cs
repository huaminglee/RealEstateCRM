using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BMS.Web.Models;
using Dapper;
using OUDAL;
using System.Text;
namespace BMS.Web.BLL
{
    public class StoreBLL
    {
        public static List<Product> GetProducts(int storeid)
        {
            using(OUContext db=new OUContext())
            {
                List<Product> list = (from o in db.Products where o.StoreId==storeid orderby o.Catalog select o).ToList();
                return list;
            }
        }
        public static List<Product> GetProducts(int storeid,bool isCardLessShine)
        {
            using (OUContext db = new OUContext())
            {
                if (isCardLessShine)
                {
                    return (from o in db.Products where o.StoreId == storeid && o.Catalog == "单次日晒" select o).ToList();
                }
                else
                {
                    return (from o in db.Products where o.StoreId == storeid && o.Catalog != "单次日晒" select o).ToList();
                }
            }
        }
        public static Product GetProduct(int id)
        {
            using (OUContext db = new OUContext())
            {
                return db.Products.Find(id);
            }
        }
        public static string GetAssetName(int id)
        {
            using (OUContext db = new OUContext())
            {
                var asset=db.Assets.Find(id);
                if(asset!=null) return asset.Name;
                return "";
            }
        }
        public static List<Asset> GetAssets(int storeid)
        {
            using (OUContext db = new OUContext())
            {
                List<Asset> list = (from o in db.Assets where o.StoreId == storeid orderby o.Name select o).ToList();
                return list;
            }
        }
        public static List<SelectListItem> GetAssetItems(int storeid,int defaultId=0)
        {
            var assetList = new List<SelectListItem>();
            assetList.Add(new SelectListItem{Text="--",Value=""});
            List<Asset> assets = GetAssets(storeid);
            foreach (Asset asset in assets)
            {
                assetList.Add(new SelectListItem { Text = asset.Name, Value = asset.Id.ToString(),Selected=(defaultId==asset.Id) });
            }
            return assetList;
        }
        public static List<SelectListItem> GetStoreItems()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem{Text="--",Value = ""});
            foreach (var department in DepartmentBLL.Departments)
            {
                if(department.DepartmentType=="门店")
                {
                    list.Add(new SelectListItem{Text=department.Name,Value = department.Id.ToString()});
                }
            }
            return list;
        }
    }
}