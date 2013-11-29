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
                                                 new Function {Name = "基础管理", ParentName = "-", Sort = 1},
                                                 new Function {Name = "基金管理", ParentName = "基础管理", Sort = 1},
                                                 new Function {Name = "LP公司管理", ParentName = "基础管理", Sort = 2},
                                                 new Function {Name = "客户管理", ParentName = "-", Sort =3},
                                                 new Function {Name = "客户查询", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "客户编辑", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "活动查询", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "客户分配", ParentName = "客户管理", Sort = 0},
                                                 new Function {Name = "合约管理", ParentName = "-", Sort = 5},
                                                 new Function {Name = "合约录入", ParentName = "合约管理", Sort = 0},
                                                 new Function {Name = "合约编辑", ParentName = "合约管理", Sort = 0},
                                                 new Function {Name = "收款查询", ParentName = "合约管理", Sort = 0},
                                                 new Function {Name = "收款录入", ParentName = "合约管理", Sort = 0},
                                                 new Function {Name = "退伙录入", ParentName = "合约管理", Sort = 0},
                                                 new Function {Name = "减资录入", ParentName = "合约管理", Sort = 0},
                                                 new Function {Name = "IR管理", ParentName = "-", Sort = 7},
                                                  new Function {Name = "投资者查询", ParentName = "IR管理", Sort = 0},
                                                  new Function {Name = "活动查询", ParentName = "IR管理", Sort = 0},
                                                  new Function {Name = "客户意见查询", ParentName = "IR管理", Sort = 0}
                                             };
                funlist.ForEach(o => context.Functions.Add(o));
                context.SaveChanges();
                List<Role> roles = new List<Role>
                                       {
                                           new Role {Name = "系统管理员"}
                                          
                                       };
                roles.ForEach(o =>
                                  {
                                      context.Roles.Add(o);
                                      context.SaveChanges();
                                      funlist.ForEach(
                                          p =>
                                          context.RoleFunctions.Add(new RoleFunction { RoleId = o.Id, FunctionId = p.Id }));
                                      context.SaveChanges();
                                  });




                List<Department> departments = new List<Department>
                                                   {
                                                       new Department {Name = "XX公司", PId = 0, DepartmentType = "公司"},
                                                       //1
                                                       new Department {Name = "项目1", PId = 1, DepartmentType = "项目"},
                                                       //2  
                                                       new Department {Name = "项目2", PId = 1, DepartmentType = "项目"},
                               
                                                        new Department {Name = "小组1", PId = 2, DepartmentType = "小组"},
                                                         new Department {Name = "小组2", PId = 2, DepartmentType = "小组"}
                                                   };
                departments.ForEach(o => context.Departments.Add(o));
                context.SaveChanges();
                List<SystemUser> users = new List<SystemUser>
                                             {
                                                 new SystemUser
                                                     {Name = "Admin", Password = "11", State = 0, WorkNO = ""},
                                                 new SystemUser {Name="Sales1",Password="11"},
                                                 new SystemUser {Name="Sales2",Password="11"}
                                             };
                users.ForEach(o =>
                                  {
                                      o.Save(context);
                                      context.RoleUsers.Add(new RoleUser { UserId = o.Id, RoleId = roles[0].Id });
                                  }
                    );
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 1,
                    DepartmentId = 4
                });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 2,
                    DepartmentId = 4
                });
                context.DepartmentUsers.Add(new DepartmentUser
                {
                    UserId = 3,
                    DepartmentId = 5
                });
                context.SaveChanges();
                //context.Funds.Add(new Fund { Name = "二期" });
                //context.Funds.Add(new Fund { Name = "三期" });
                context.SaveChanges();
                context.Companies.Add(new Company { Name = "LP1" });
                context.Companies.Add(new Company { Name = "LP2" });

                List<OUDAL.Dictionary> dictionaries = new List<Dictionary>
                                                          {
                                                              new Dictionary {Catalog = "客户", Name = "产品类型"},
                                                              new Dictionary {Catalog = "客户", Name = "渠道类型"},
                                                              new Dictionary {Catalog = "客户", Name = "客户类别",KeyId = 1},
                                                              new Dictionary {Catalog = "客户", Name = "联系类型"},
                                                              new Dictionary {Catalog = "客户", Name = "邀约类型"}
                                                          };
                List<OUDAL.DictionaryItem> dictionaryItems = new List<DictionaryItem>
                                                               {
                                                                   new DictionaryItem{DictId = 1,IndexNum = 1,Name = "公寓"},
                                                                   new DictionaryItem{DictId = 1,IndexNum = 2,Name = "别墅"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 1,Name = "销售员开发"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 2,Name = "中介开发"},
                                                                   new DictionaryItem{DictId = 2,IndexNum = 3,Name = "公司资源"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 1,Name = "LP"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 2,Name = "战略供应商"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 3,Name = "商协会"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 4,Name = "机构"},
                                                                   new DictionaryItem{DictId = 3,IndexNum = 5,Name = "FOF"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 1,Name = "来访"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 2,Name = "来电"},
                                                                   new DictionaryItem{DictId = 4,IndexNum = 3,Name = "去电"},
                                                                   new DictionaryItem{DictId = 5,IndexNum = 1,Name = "来访邀约"},
                                                                   new DictionaryItem{DictId = 5,IndexNum = 2,Name = "办卡邀约"},
                                                                   new DictionaryItem{DictId = 5,IndexNum = 3,Name = "大定邀约"},
                                                                   new DictionaryItem{DictId = 5,IndexNum = 4,Name = "签约邀约"},
                                                               };
                dictionaries.ForEach(o => context.Dictionaries.Add(o));
                context.SaveChanges();
                dictionaryItems.ForEach(o => context.DictionaryItems.Add(o));
                context.SaveChanges();
                //                context.Clients.Add(new ClientPersonal { Name = "ABC", CreateTime = DateTime.Now, Catalog = "机构",SalesId=2 });
                //                context.Clients.Add(new ClientPersonal { Name = "某LP", CreateTime = DateTime.Now, Catalog = "机构", SalesId = 2 });
                //                context.Clients.Add(new ClientCompany { Name = "某某有限公司", CreateTime = DateTime.Now, FullName = "陈JJ、朱B", Catalog = "机构", SalesId = 2 });
                //                context.Clients.Add(new ClientPersonal { Name = "张三", CreateTime = DateTime.Now, Catalog = "机构", SalesId = 2 });
                //                              context.ClientFunds.Add(new ClientFund
                //                        {
                //                            ClientId = 1,
                //                            FundId = 1,
                //                            Intent = ClientIntentEnum.一般,
                //                            IntentDate = DateTime.Today,
                //                            IntentMoney = 1000,
                //                            PlanMoney = 1000
                //                        });
                //context.ClientFunds.Add(new ClientFund{
                //    ClientId = 1,
                //    FundId = 2,
                //    Intent = ClientIntentEnum.一般,
                //    IntentDate = DateTime.Today,
                //    IntentMoney = 1000,
                //    PlanMoney = 1000});
                //context.ClientFunds.Add(new ClientFund{
                //    ClientId = 2,
                //    FundId = 1,
                //    Intent = ClientIntentEnum.一般,
                //    IntentDate = DateTime.Today,
                //    IntentMoney = 1000,
                //    PlanMoney = 1000
                //});
                context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {

            }

        }
    }
}