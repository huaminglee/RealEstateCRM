using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public enum UserCheckResult { 验证通过, 用户密码错误, 用户不存在, 用户登录锁定 }
    public enum UserState { Enabled, Disabled }
    public class SystemUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 0=正常 1=禁用
        /// </summary>
        public int State{get;set;}
        [Required]
        [DisplayName("姓名")]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [DisplayName("密码")]
        [MaxLength(100)]
        public string Password { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        [DisplayName("AD账号")]
        public string WorkNO { get; set; }
        [DisplayName("邮箱")]
        [MaxLength(50)]
        public string Email { get; set; }
        public SystemUser()
        {
            WorkNO = ""; Email = "";
        }
        public bool CheckPassword(string password)
        {
            return Password == EncryptPassword(Name.ToLower(), password);
        }
        /// <summary>
        /// 修改密码或用户名时候调用。调用时password应该是加密前password
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool Save(Context db)
        {
            Name = Name.Trim();
            int count =
                db.Database.SqlQuery<int>("select count(1) from systemusers where id!={0} and (name={1} )", Id,
                                          Name).FirstOrDefault();
            if (count > 0)
            {
                return false;
            }
            Password = EncryptPassword(Name.ToLower(), Password);
            if (Id == 0) db.SystemUsers.Add(this);
            db.SaveChanges();
            return true;
        }
        static string EncryptPassword(string pre, string password)
        {
            System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
            byte[] byteResult = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pre + password));
            return BitConverter.ToString(byteResult);
        }
        static Dictionary<string, int> logonFailedList = new Dictionary<string, int>(); 
        static public UserCheckResult CheckUser(string name, string password, out SystemUser user)
        {
            if (logonFailedList.ContainsKey(name))
            {
                if (logonFailedList[name] > 4)
                {
                    user = null;
                    return UserCheckResult.用户登录锁定;
                }
            }
            Context db = new Context();
            user = (from o in db.SystemUsers where o.State == (int)UserState.Enabled && (name == o.Name ) select o).FirstOrDefault();
            if (user != null)
            {
                if (user.CheckPassword(password))
                {
                    return UserCheckResult.验证通过;
                }
                else
                {
                    if (logonFailedList.ContainsKey(name))
                    {
                        logonFailedList[name]++;
                    }
                    else
                    {
                        logonFailedList.Add(name, 1);
                    }
                    user = new SystemUser { Id = logonFailedList[name] };
                    return UserCheckResult.用户密码错误;
                }
            }
            else
            {
                return UserCheckResult.用户不存在;
            }

        }
    }
}
