using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilePathDelonger;

namespace TestTime
{
    class Program
    {
        static string dir = "+";
        static string file = " ";
        static string spacer = "  ";

        static void Main(string[] args)
        {
            ///FileTree ft = PathTools.BuildTree(@"C:\Users\arober49\Desktop");
            ///PrintTree(ft);
            ///
            
        }

        static void PrintTree(FileTree ft)
        {
            Console.WriteLine(dir + ft.Directory.Substring(ft.Directory.LastIndexOf(@"\")));

            foreach (string f in ft.Files)
                Console.WriteLine(file + f);

            indent();
            foreach (FileTree f in ft.Directories)
                PrintTree(f);
            undent();
        }

        static void indent()
        {
            dir = spacer + dir;
            file = spacer + file;
        }

        static void undent()
        {
            dir = dir.Substring(1);
            file = file.Substring(1);
        }
    }
}
