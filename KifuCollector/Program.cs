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
using System.Text;
using System.Diagnostics;

namespace KifuCollector
{


    class Program
    {
        static void Main(string[] args)
        {
            string sgfFile =  string.Format("{0}/1.sgf", ".");
            //SGFKiFUSerializer serializer = new SGFKiFUSerializer();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                SGFKiFUSerializer serializer = new SGFKiFUSerializer();
                serializer.KiFUEncoding = Encoding.GetEncoding("gb2312");
                using (Stream sr = File.OpenRead(sgfFile))
                {
                    // GoGameInfo gameInfo = serializer.ReadGameInfo(sr);
                    // Console.WriteLine("name:{0}", gameInfo.Name);
                    // Console.WriteLine("black:{0}", gameInfo.BlackPlayer.Name);
                    // Console.WriteLine("white:{0}", gameInfo.WhitePlayer.Name);
                    // Console.WriteLine("result :{0}", gameInfo.Result.ChineseString);
                    byte[] buffer = new byte[sr.Length];
                    sr.Read(buffer, 0 , buffer.Length);
                    string s = serializer.KiFUEncoding.GetString(buffer);
                    #if DEBUG
                    Console.WriteLine(s);
                    #endif


                    KiFUGame game = serializer.DeSerialize(s);
                    Console.WriteLine("对局 : {0}", game.GameInfo.Name);
                    Console.WriteLine("赛事：{0}", game.GameInfo.Event);
                    Console.WriteLine("black:{0}", game.GameInfo.BlackPlayer.Name);
                    Console.WriteLine("white:{0}", game.GameInfo.WhitePlayer.Name);
                    Console.WriteLine("result :{0}", game.GameInfo.Result.ChineseString);
                    sr.Close();
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
