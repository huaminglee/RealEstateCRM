using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.Xml;
using OfficeOpenXml.Style;
using System.Diagnostics;

namespace Test
{
    public class ExcelHelper
    {
        static public string ReadString(ExcelRange cell)
        {
            if (cell.Value != null) return cell.Value.ToString().Trim();
            return "";
        }
        /// <summary>
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>如果空则返回0，如果非空且不是数字则返回0</returns>
        static public decimal ReadDecimal(ExcelRange cell)
        {
            decimal d = 0;
            try
            {
                if (cell.Value != null) return Convert.ToDecimal((double)(cell.Value)); ;
            }
            catch (Exception exp)
            {
                Debug.WriteLine("错误" + cell.Address + " " + exp.Message);
            }
            return d;
        }
        static public decimal? ReadDecimalEmpty(ExcelRange cell)
        {
            
            if (cell.GetType() == typeof(double))
            {
                return Convert.ToDecimal((double)(cell.Value));
            }
            else
            {
                decimal d = 0;
                if(decimal.TryParse(cell.Value.ToString(),out d))
                {
                    return d;
                }
            }
            return null;
        }
        static public DateTime? ReadDateEmpty(ExcelRange cell)
        {
            if (cell.Value == null) return null;
            long serialDate = 0;

            if(long.TryParse(cell.Value.ToString(),out serialDate))
            {
                return DateTime.FromOADate(serialDate);
            }
            DateTime d1 = DateTime.MinValue;
            if (DateTime.TryParse(cell.Value.ToString(), out d1)) return d1;
            return null;

        }
       
    }
}
