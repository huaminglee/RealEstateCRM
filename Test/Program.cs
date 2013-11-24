using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OUDAL;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to drop Database");
            Console.Read();
            System.Data.Entity.Database.SetInitializer<Context>(new ContextDropDBInitializer());
            //System.Data.Entity.Database.SetInitializer<Context>(new ContextInitializer());
            Context db = new Context();
            var query = (from o in db.SystemUsers select o).ToList();
           
            Console.WriteLine("Press any key to Exit");
            Console.Read();
        }
    }
}
