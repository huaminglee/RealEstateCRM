using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class AccessLog
    {
        public int Id { get; set; }
        public int KeyId { get; set; }
        //对字符串主键的表
        [MaxLength(50)]
        public string Code { get; set; }
        [MaxLength(50)]
        public string AccessClass { get; set; }
        [MaxLength(50)]
        public string AccessAction { get; set; }
        [MaxLength(500)]
        public string AccessInfo { get; set; }
        public DateTime AccessTime { get; set; }
        public string AccessPerson { get; set; }


        public static void AddLogAndSave(Context db, string person, int id, string logClass, string logAction, string info)
        {
            AccessLog a = new AccessLog();
            a.KeyId = id;
            a.AccessClass = logClass;
            a.AccessAction = logAction;
            a.AccessInfo = info;
            a.AccessTime = DateTime.Now;
            a.AccessPerson = person;
            db.AccessLogs.Add(a);
            db.SaveChanges();
        }
        public static List<AccessLog> GetByClass(int key, string logClass)
        {
            Context db = new Context();
            return (from a in db.AccessLogs where a.KeyId == key && a.AccessClass == logClass orderby a.AccessTime select a).ToList();
        }
        public static void AddLogAndSave(Context db, string person, string code, string logClass, string logAction, string info)
        {
            AccessLog a = new AccessLog();
            a.Code = code;
            a.AccessClass = logClass;
            a.AccessAction = logAction;
            a.AccessInfo = info;
            a.AccessTime = DateTime.Now;
            a.AccessPerson = person;
            db.AccessLogs.Add(a);
            db.SaveChanges();
        }
        public static void AddLog(Context db, string person, string code, string logClass, string logAction, string info)
        {
            AccessLog a = new AccessLog();
            a.Code = code;
            a.AccessClass = logClass;
            a.AccessAction = logAction;
            a.AccessInfo = info;
            a.AccessTime = DateTime.Now;
            a.AccessPerson = person;
            db.AccessLogs.Add(a);
        }
        public static void AddLog(Context db, string person, int id, string logClass, string logAction, string info)
        {
            AccessLog a = new AccessLog();
            a.KeyId = id;
            a.AccessClass = logClass;
            a.AccessAction = logAction;
            a.AccessInfo = info;
            a.AccessTime = DateTime.Now;
            a.AccessPerson = person;
            db.AccessLogs.Add(a);
        }
        public static List<AccessLog> GetByClass(string code, string logClass)
        {
            Context db = new Context();
            return (from a in db.AccessLogs where a.Code == code && a.AccessClass == logClass orderby a.AccessTime select a).ToList();
        }
    }
}
