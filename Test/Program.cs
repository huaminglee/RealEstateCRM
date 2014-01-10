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
            //System.Data.Entity.Database.SetInitializer<Context>(null);
            Context db = new Context();
            var query = (from o in db.SystemUsers select o).ToList();
            db.Dispose();
            ImportData import = new ImportData();
            string filename = "历史客户.xlsx";
            //import.Fun1(filename);
            Console.WriteLine("Press any key to Exit");
            Console.Read();
        }
    }
}
