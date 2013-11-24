using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
namespace RealEstateCRM.Web.BLL
{
    public class ADHelper
    {
    }
    public static class PropertyExtension
    {
        static public string GetProperty(this DirectoryEntry de, string name)
        {
            if (de.Properties[name] == null) return "";
            if (de.Properties[name].Value == null) return "";
            return de.Properties[name].Value.ToString();
        }
    }
    
}