using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;


namespace Pepper
{
    class FileSystem
    {
        static public void saveUIDs(List<string> Uids)
        {

            System.IO.Directory.CreateDirectory(Variables.PathNotes);

            //System.IO.File.WriteAllLines(Variables.PathUID, Uids);
            Console.WriteLine("save uids is " + Uids.Count.ToString());

        }
        static public List<string> readUIDs()
        {
            try
            {

                //string[] readData = System.IO.File.ReadAllLines(Variables.PathUID);
                //Console.WriteLine("read uids is " + readData.Count().ToString());
                return new List<string>();//readData); 
            }
            catch
            {
                return null;

            }
        }
        static public void saveUSER(String[] lines)
        {
            System.IO.File.WriteAllLines(System.IO.Directory.GetCurrentDirectory() + @"\psywatch.usr", lines);
        }

        
        static public string[] readUSER()
        {
            try
            {
                return System.IO.File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + @"\psywatch.usr");
            }
            catch
            {
                return null;

            }
        }
        static public string[] readQuotes()
        {
            try
            {
                return System.IO.File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + @"\quotes.txt");
            }
            catch
            {
                return null;

            }
        }

    }
}
