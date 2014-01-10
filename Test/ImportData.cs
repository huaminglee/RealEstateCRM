using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OUDAL;
using System.Configuration;
using System.Data;
using System.IO;

using System.Transactions;
namespace Test
{
   
    public class ImportData
    {

        public void Fun()
        {
            string filename = "人员名单.xlsx";
            FileInfo fi = new FileInfo(filename);
            StringBuilder errorinfo = new StringBuilder();
            using (ExcelPackage package = new ExcelPackage(fi))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                int i = 1;
                using (Context db = new Context())
                {
                    //db.Configuration.AutoDetectChangesEnabled = false;
                    //using (TransactionScope tran = new TransactionScope())
                    //{
                        while (i < 100)
                        {
                            i++;
                            string name = ExcelHelper.ReadString(worksheet.Cells[i, 4]);
                            if (string.IsNullOrEmpty(name)) continue;
                            if ((from o in db.SystemUsers where o.Name == name select o).FirstOrDefault() != null)
                            {
                                Console.WriteLine(name + " 已存在");
                                continue;
                            }
                            string rolename = ExcelHelper.ReadString(worksheet.Cells[i, 2]);
                            int roleid = (from o in db.Roles where o.Name == rolename select o.Id).FirstOrDefault();
                            if (roleid == 0)
                            {
                               roleid= (from o in db.Roles where o.Name == "销售员" select o.Id).First();
                            }
                            SystemUser user = new SystemUser
                                              {
                                                  Name = name,
                                                  Password = "654321",
                                                  LoginName = name,
                                                  Email = ExcelHelper.ReadString(worksheet.Cells[i, 5])
                                              };
                            SystemUser u = new SystemUser();
                            db.SystemUsers.Add(u);
                            u.Save(db, user);
                            RoleUser ru = new RoleUser {UserId = u.Id, RoleId = (int) roleid};
                            db.RoleUsers.Add(ru);
                            string projname = ExcelHelper.ReadString(worksheet.Cells[i, 1]);
                            int project =
                                (from o in db.Departments
                                    where o.Name == projname
                                    select o.Id).First();
                            DepartmentUser du = new DepartmentUser {UserId = u.Id};
                            string d3 = ExcelHelper.ReadString(worksheet.Cells[i, 3]);
                            if (string.IsNullOrEmpty(d3))
                            {
                                d3 = "公共客户";
                            }
                            else
                            {
                                d3 = rolename;
                            }
                            Department dept =
                                (from o in db.Departments where o.PId == project && o.Name == d3 select o).First();
                            du.DepartmentId = dept.Id;
                            db.DepartmentUsers.Add(du);
                            db.SaveChanges();


                           // tran.Complete();
                      //  }

                    }
                }
            }

        }
        public void Fun1(string filename)
        {
            
            FileInfo fi = new FileInfo(filename);
            StringBuilder errorinfo = new StringBuilder();
            using (ExcelPackage package = new ExcelPackage(fi))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                int i = 1;
                using (Context db = new Context())
                {
                    //db.Configuration.AutoDetectChangesEnabled = false;
                    //using (TransactionScope tran = new TransactionScope())
                    //{
                    while (i < 6000)
                    {
                        i++;
                        string name = ExcelHelper.ReadString(worksheet.Cells[i, 3]);
                        if (string.IsNullOrEmpty(name)) continue;
                        int projectid = int.Parse(ExcelHelper.ReadString(worksheet.Cells[i, 9]));
                        DateTime? d = ExcelHelper.ReadDateEmpty(worksheet.Cells[i, 2]);
                        if (d == null)
                        {
                            Console.WriteLine("ErrorDate" + i.ToString());
                            continue;
                        }
                        string phone = ExcelHelper.ReadString(worksheet.Cells[i, 4]);
                        string way = ExcelHelper.ReadString(worksheet.Cells[i, 5]);
                        if (string.IsNullOrEmpty(way))
                        {
                            way = "-";
                        }
                        string roomtype = ExcelHelper.ReadString(worksheet.Cells[i, 6]);
                        //string u1 = ExcelHelper.ReadString(worksheet.Cells[i, 7]);
                        string u2 = ExcelHelper.ReadString(worksheet.Cells[i, 8]);
                        if (phone != "-")
                        {
                            if (
                            (from o in db.Clients where o.ProjectId == projectid && o.Phone1 == phone select o)
                                .FirstOrDefault() !=null)
                        {
                            Console.WriteLine("客户已存在" + i.ToString());
                            continue;
                        }
                        }
                        
                        int sales1 = (from o in db.SystemUsers where o.Name == u2 select o.Id).FirstOrDefault();
                        int sales;
                        if (sales1 == 0)
                        {
                            Console.WriteLine("销售员不存在" + u2+i.ToString());
                            continue;
                        }
                        sales = (int) sales1;

                        int team1 =
                            (from o in db.SystemUsers
                                join p in db.DepartmentUsers on o.Id equals p.UserId
                                where o.Name == u2
                                select p.DepartmentId).FirstOrDefault();
                        if (team1 == 0)
                        {
                            Console.WriteLine("销售组不存在" + u2);
                            continue;
                        }
                        int team=(int)team1;
                        Client client = new Client {ProjectId = projectid};
                        client.Name = name;
                        client.Phone1 = phone;
                        client.Way = way;
                        client.RoomType = roomtype;
                        client.AllPhone = phone;
                        client.Code = "导入数据";
                        client.CreateTime = (DateTime)d;
                        client.GroupId = team;
                        client.State = ClientStateEnum.来电客户;
                        client.StateDate = new DateTime(2013, 11, 30);
                        db.Clients.Add(client);
                        db.SaveChanges();
                        ClientActivity ca = new ClientActivity {ClientId = client.Id};
                        db.ClientActivities.Add(ca);
                        ca.ActualTime = client.CreateTime;
                        ca.FirstType = 1;
                        ca.Person = sales;
                        ca.Type = "来电";
                        db.SaveChanges();


                         
                          }
//tran.Complete();
//                    }
                }
            }

        }

    }
}
