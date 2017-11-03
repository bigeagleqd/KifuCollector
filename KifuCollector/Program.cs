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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace KifuCollector
{


    class Program
    {
        static void Main(string[] args)
        {
            GetPage();


            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
        }

        private static void ReadKifu()
        {
            string sgfFile = string.Format("{0}/1.sgf", ".");
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
                    sr.Read(buffer, 0, buffer.Length);
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
                    foreach (CommentsMove move in game.MainGameTreeMoves)
                    {
                        Console.WriteLine("{0}: {1}({2},{3})---{4}", move.Step, move.Side, move.X, move.Y, move.Comment);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取{0}出错:{1}", sgfFile, ex);
            }

        }
        private static void GetPage()
        {
            string firstPageUrl = "http://weiqi.qq.com/qipu/index/p/1.html";
            string baseUrl = "http://weiqi.qq.com";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string s = wc.DownloadString(firstPageUrl);
                    //Console.WriteLine(s);
                    List<string> pageUrls = new List<string>();
                    pageUrls.Add(s);


                    //分析分页
                    Regex regex = new Regex("<a href='(?<url>[^']+)'\\s>末页");
                    Match match = regex.Match(s);
                    if (match.Success)
                    {
                        string lastPageUrl = match.Groups["url"].Value;
#if DEBUG
                        Console.WriteLine("last page url is :{0}", lastPageUrl);
#endif//DEBUG

                        //分析最后一页的页数
                        regex = new Regex("qipu/index/p/(?<pageNo>\\d+).html");
                        match = regex.Match(lastPageUrl);
                        if (match.Success)
                        {
                            string pageNoStr = match.Groups["pageNo"].Value;
#if DEBUG
                            Console.WriteLine("最后一页的页码是：{0}", pageNoStr);
#endif
                            int pageNo;
                            if (int.TryParse(pageNoStr, out pageNo))
                            {
                                for (int i = 2; i <= pageNo; i++)
                                {
                                    pageUrls.Add(string.Format("http://weiqi.qq.com/qipu/index/p/{0}.html", i));

                                }
                            }
                        }
                    }
#if DEBUG
                    Console.WriteLine("共有{0}个棋谱目录页。", pageUrls.Count);
#endif
                    for (int i = 0; i < pageUrls.Count; i ++)
                    {
                        if (i > 0)
                        {

                            s = wc.DownloadString(pageUrls[i]);
                        }

                        //分析
                        regex = new Regex("a class=\"px14\" href=\"(?<url>[^>]+)\"[>]");
                        MatchCollection matches = regex.Matches(s);
                        List<string> urls = new List<string>();
                        string url = string.Empty;
                        foreach (Match m in matches)
                        {
                            //Console.WriteLine();
                            url = string.Format("{0}{1}", baseUrl, m.Groups["url"].Value);
#if DEBUG
                            Console.WriteLine(url);
#endif
                            urls.Add(url);
                        }
                        Console.WriteLine("共发现{0}页棋谱需要下载。", urls.Count);

                        //创建目录
                        if (!Directory.Exists("./kifu"))
                        {
                            Directory.CreateDirectory("./kifu");
                        }

                        Parallel.ForEach(urls,
                () => new WebClient(),
                (pageUrl, loopstate, index, webclient) =>
                {
                    string pageString = webclient.DownloadString(pageUrl);
                    Console.WriteLine("{0}:{1}, length:{2}"
                    , Thread.CurrentThread.ManagedThreadId, pageUrl, pageString.Length);

                    //抓取棋谱
                    Regex regex1 = new Regex("(;GM[^)]+)");
                    Match match1 = regex1.Match(pageString);
                    if (match1.Success)
                    {
                        string sgf = string.Format("({0})", match1);
                        SGFKiFUSerializer serializer = new SGFKiFUSerializer();
                        KiFUGame game = serializer.DeSerialize(sgf);
                        Console.WriteLine("读取{0}", game.GameInfo.Name);
                        string path = string.Format("./kifu/{0}.sgf", game.GameInfo.Name);
                        //Console.WriteLine(sgf);
                        //保存到文件
                        try
                        {
                            using (StreamWriter sw = File.CreateText(path))
                            {
                                sw.Write(sgf);
                                sw.Flush();
                                sw.Close();
                                Console.WriteLine("保存到文件{0}", path);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("保存到文件{0}失败：{1}", ex, path);
                        }


                    }
                    return webclient;
                },
                    (webclient) => { });



                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        
        }

    }//end class
}//end namespace
