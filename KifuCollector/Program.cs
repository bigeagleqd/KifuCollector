/************************************************
*
* File: Program.cs
*
* Author: bigeagleqd@hotmail.com
*
*********************************************/

using System;
using System.IO;
using System.Net;
using System.Linq;
using Bigeagle.Portable.BoardGames;
using Bigeagle.Portable.BoardGames.Go;

namespace KifuCollector
{


    class Program
    {
        static void Main(string[] args)
        {
            string sgfFile =  string.Format("{0}/1.sgf", ".");
            SGFKiFUSerializer serializer = new SGFKiFUSerializer();
            try
            {
                using (StreamReader sr = File.OpenText(sgfFile))
                {
                    string s = sr.ReadToEnd();
                    KiFUGame game = serializer.DeSerialize(s);
                    Console.WriteLine(game.GameInfo.Name);


                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("读取{0}出错:{1}", sgfFile, ex);
            }

            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
        }
    }//end class
    
}//end namespace
