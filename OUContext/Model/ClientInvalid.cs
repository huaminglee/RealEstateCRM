using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace OUDAL
{
    public class ClientInvalid
    {
        public int Id { get; set; }

        [DisplayName("客户")]
        public int ClientId { get; set; }
        [DisplayName("判定时间")]
        public DateTime TransferDate { get; set; }
        [DisplayName("判定原因")]
        public string Reason { get; set; }
       
        [DisplayName("操作人")]
        public int Person { get; set; }
    }


    public class ClientInvalidView
    {
        [DisplayName("客户姓名")]
        public string ClientName { get; set; }
        [DisplayName("转移时间")]
        public DateTime TransferDate { get; set; }
        [DisplayName("转移原因")]
        public string Reason { get; set; }
        [DisplayName("转出销售组")]
        public string OutGroup { get; set; }
        [DisplayName("转入销售组")]
        public string InGroup { get; set; }
        [DisplayName("操作人")]
        public string PersonName { get; set; }

        public static string sql = @"select cf.TransferDate,cf.Reason,c.name as clientName ,u.name as PersonName
,d1.name as ingroup ,d2.name as outgroup
from clienttransfers cf join clients c on c.id=cf.clientid join systemusers u on u.id=cf.person 
join departments d1 on d1.id=cf.ingroup
join departments d2 on d2.id=cf.outgroup
where 1=1";
//public static string sqlIn = @"select Clients.Name as ClientName,AccessTime as TransferTime,SUBSTRING(AccessInfo,(CHARINDEX('从',AccessInfo)+1),(CHARINDEX('转移到',AccessInfo)-CHARINDEX('从',AccessInfo)-1)) as OutGroup,SUBSTRING(AccessInfo,PATINDEX('%转移到%',AccessInfo)+3,LEN(AccessInfo)) as InGroup, AccessPerson as Person from AccessLogs join Clients on KeyId=Clients.Id where AccessClass='客户' and AccessAction='客户转移' and AccessInfo LIKE '从%转移到{0}'";
//        public static string sqlOut = @"select Clients.Name as ClientName,AccessTime as TransferTime,SUBSTRING(AccessInfo,(CHARINDEX('从',AccessInfo)+1),(CHARINDEX('转移到',AccessInfo)-CHARINDEX('从',AccessInfo)-1)) as OutGroup,SUBSTRING(AccessInfo,PATINDEX('%转移到%',AccessInfo)+3,LEN(AccessInfo)) as InGroup, AccessPerson as Person from AccessLogs join Clients on KeyId=Clients.Id where AccessClass='客户' and AccessAction='客户转移' and AccessInfo LIKE '从{0}转移到%'";
    }
}