using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactureMatch
{
    class Program
    {
        static void Main(string[] args)
        {
            Matcher matcher = new Matcher();
            matcher.GenerateDummyTable();
            matcher.ProcessDummyTables();
            Console.ReadKey();
        }
    }
}
  