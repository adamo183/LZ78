using System;
using LZ78;
using System.IO;

namespace LZ78
{
    class Program
    {
        static void Main(string[] args)
        {
            
            LZ78 kompress_file = new LZ78();
            kompress_file.InsertFileName();
            kompress_file.insertMode();
            
            kompress_file.showProgress += lz78_showProgres;
            if(kompress_file.mode == 1)
            {
                kompress_file.Compress();
            }
            else if(kompress_file.mode == 2)
            {
                kompress_file.Decompress();
            }


            
            
            
            kompress_file.ReadDataFromFile();
        }

        static void lz78_showProgres(object sender, ShowProgressArgs e)
        {
           
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop-2);
            Console.WriteLine();
            int x = (int)(((double)e.current_size / (double)e.full_size)*100);
            Console.WriteLine(x + "% complete");
        }
    }
}
