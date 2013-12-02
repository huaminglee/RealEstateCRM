using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Configuration;
using System.Data.Entity.Validation;
namespace OUDAL
{//this is notebook v1.0
    //desktop 1.1
    public class Context : DbContext
    {
        public Context()
            : base(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString)
        { }

        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentUser> DepartmentUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleUser> RoleUsers { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<RoleFunction> RoleFunctions { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Dictionary> Dictionaries { get; set; }
        public DbSet<DictionaryItem> DictionaryItems { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<SysCode> SysCode { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientActivity> ClientActivities { get; set; }
        public DbSet<ClientTransfer> ClientTransfers { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractPay> ContractPays { get; set; }
        public DbSet<ContractPlan> ContractPlans { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentUser>().HasKey(p => new { p.UserId, p.DepartmentId });
            modelBuilder.Entity<RoleUser>().HasKey(p => new { p.UserId, p.RoleId });
            modelBuilder.Entity<RoleFunction>().HasKey(p => new { p.RoleId, p.FunctionId });


            base.OnModelCreating(modelBuilder);
        }
    }
    public class ContextInitializer : DropCreateDatabaseIfModelChanges<Context>//DropCreateDatabaseAlways<Context>// 
    {
        protected override void Seed(Context context)
        {
            base.Seed(context);
            Seeding.Seed(context);
        }
    }
    public class ContextDropDBInitializer : DropCreateDatabaseAlways<Context>// 
    {
        protected override void Seed(Context context)
        {
            base.Seed(context);
            Seeding.Seed(context);
        }
    }
    internal class Seeding
    {
        public static void Seed(Context context)
        {
            try
            {
                List<Function> funlist = new List<Function>
                                             {
                                                 new Function {Name = "系统管理", ParentName = "-", Sort = 100},
                                                 new Function {Name = "部门管理", ParentName = "系统管理", Sort = 0},
                                                 new Function {Name = "用户管理", ParentName = "系统管理", Sort = 0},
                                                 new Function {Name = "角色管理", ParentName = "系统管理", Sort = 0},
                                                 new Function {Name = "数据字典", ParentName = "系统管理", Sort = 0},
                                                 new Function {Name = "参数配置", ParentName = "系统管理", Sort = 0},
                                                 new Function {Name = "项目管理", ParentName = "-", Sort = 1},
                                                 //new Function {Name = "项目管理", ParentName = "项目管理", Sort = 1},
                                                 new Function {Name = "参数配置", ParentName = "项目管理", Sort = 0},
                                                 new Function {Name = "数据字典", ParentName = "项目管理", Sort = 0},
                                                 new Function {Name = "客户管理", ParentName = "-", Sort =3},
                                                 new Function {Name = "查看所有客户", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "查看项目客户", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "查看本组客户", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "前台", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "客户查询", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "客户编辑", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "客户删除", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "小卡大卡", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "大定签约", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "退卡", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "退定退房", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "客户分配", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "辅助功能", ParentName = "-", Sort =5},
                                                 new Function {Name = "销售员面板", ParentName = "辅助功能", Sort = 0}
                                             };
                funlist.ForEach(o => context.Functions.Add(o));
                
                context.SaveChanges();
                List<Role> roles = new List<Role>
                                       {
                                           new Role {Name = "系统管理员"},
                                           new Role {Name = "案场经理"},
                                           new Role {Name = "前台"},
                                           new Role {Name = "销售经理"}
                                          
                                       };
                roles.ForEach(o =>
                                  {
                                      context.Roles.Add(o);
                                      context.SaveChanges();
                                      funlist.ForEach(
                                          p =>
                                          {
                                              if (o.Id == 1||o.Id==2)
                                              {
                                                  context.RoleFunctions.Add(new RoleFunction
                                                                            {
                                                                                RoleId = o.Id,
                                                                                FunctionId = p.Id
                                                                            });

                                              }
                                          });

                                  });
context.SaveChanges();



                List<Department> departments = new List<Department>
                                                   {
                                                       new Department {Name = "事业二部", PId = 0, DepartmentType = "公司"},
                                                       new Department {Name = "营销部", PId = 1, DepartmentType = "部门"},
                                                       new Department {Name = "北外滩", PId = 1, DepartmentType = "项目"},
                                                       new Department {Name = "绿地未来中心", PId = 1, DepartmentType = "项目"},
                                                       new Department {Name = "绿地梧桐院", PId = 1, DepartmentType = "项目"},
                                                       new Department {Name = "前台", PId = 3, DepartmentType = "小组"},
                                                       new Department {Name = "小组1", PId = 3, DepartmentType = "小组"},
                                                       new Department {Name = "小组2", PId = 3, DepartmentType = "小组"},
                                                       new Department {Name = "公共客户", PId = 3, DepartmentType = "小组"},
                                                       new Department {Name = "沉睡客户", PId = 3, DepartmentType = "小组"},
                                                       new Department {Name = "前台", PId = 4, DepartmentType = "小组"}
                                                   };
                departments.ForEach(o => context.Departments.Add(o));
                context.SaveChanges();
                List<SystemUser> users = new List<SystemUser>
                                             {
                                                 new SystemUser
                                                     {LoginName = "Admin", Password = "11", State = 0, Name = "管理员"},
                                                 new SystemUser {LoginName="Manager",Name="销售经理",Password="11"},
                                                 new SystemUser {LoginName="Reception",Name="前台",Password="11"},
                                                 new SystemUser {LoginName="Sales1",Name="Sales1",Password="11"},
                                                 new SystemUser {LoginName="Sales2",Name="Sales2",Password="11"}
                                             };
                users.ForEach(o =>
                              {
                                  SystemUser user = new SystemUser();
                                  user.Save(context, o);
                              });
                context.SaveChanges();
                context.RoleUsers.Add(new RoleUser {RoleId = 1, UserId = 1});
                context.RoleUsers.Add(new RoleUser { RoleId = 2, UserId = 2 });
                context.RoleUsers.Add(new RoleUser { RoleId = 3, UserId = 3 });
                context.RoleUsers.Add(new RoleUser { RoleId = 4, UserId = 4 });
                context.RoleUsers.Add(new RoleUser { RoleId = 1, UserId = 5 });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 1,
                    DepartmentId = 2
                });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 2,
                    DepartmentId = 5
                });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 3,
                    DepartmentId = 6
                });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 4,
                    DepartmentId = 7
                });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 5,
                    DepartmentId = 7
                });
                context.SaveChanges();

                List<OUDAL.Dictionary> dictionaries = new List<Dictionary>
                                                          {
                                                              new Dictionary {Catalog = "客户", Name = "产品类型"},
                                                              new Dictionary {Catalog = "客户", Name = "渠道类型"},
                                                              //new Dictionary {Catalog = "客户", Name = "客户类别",KeyId = 1},
                                                              new Dictionary {Catalog = "客户", Name = "联系类型"},
                                                              new Dictionary {Catalog = "客户", Name = "邀约类型"}
                                                          };
                List<OUDAL.DictionaryItem> dictionaryItems = new List<DictionaryItem>
                                                               {
                                                                   new DictionaryItem{DictId = 1,IndexNum = 1,Name = "公寓"},
                                                                   new DictionaryItem{DictId = 1,IndexNum = 2,Name = "别墅"},
                                                                   new DictionaryItem{DictId = 1,IndexNum = 3,Name = "写字楼"},
                                                                   new DictionaryItem{DictId = 1,IndexNum = 4,Name = "商铺"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 1,Name = "老客户开发"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 2,Name = "短信"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 3,Name = "DM"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 4,Name = "道旗"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 5,Name = "网络"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 6,Name = "巡展"},
                                                                   //new DictionaryItem{DictId = 3,IndexNum = 1,Name = "LP"},
                                                                   //new DictionaryItem{DictId = 3,IndexNum = 2,Name = "战略供应商"},
                                                                   //new DictionaryItem{DictId = 3,IndexNum = 3,Name = "商协会"},
                                                                   //new DictionaryItem{DictId = 3,IndexNum = 4,Name = "机构"},
                                                                   //new DictionaryItem{DictId = 3,IndexNum = 5,Name = "FOF"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 1,Name = "来访"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 2,Name = "来电"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 3,Name = "去电"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 1,Name = "来访邀约"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 2,Name = "办卡邀约"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 3,Name = "升卡邀约"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 4,Name = "大定邀约"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 5,Name = "签约邀约"},
                                                               };
                dictionaries.ForEach(o => context.Dictionaries.Add(o));
                context.SaveChanges();
                dictionaryItems.ForEach(o => context.DictionaryItems.Add(o));
                context.SaveChanges();
                
                context.SaveChanges();
            }
            catch (Exception dbEx)
            {
                var i = dbEx;
                throw (dbEx);
            }

        }
    }
}