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


namespace KifuCollector
{


    class Program
    {
        static void Main(string[] args)
        {
            GuessNumber();         
        }
        private static void DoSometing()
        {
            Console.Write("请输入你的名字：");
            string s = Console.ReadLine();
            Console.WriteLine("你的名字是：{0}。", s);
            Console.ReadKey();
        

    
        }

        private static void GuessNumber()
        {
            int maxNumber =10000;
            int minNumber = 0;
            
            Console.WriteLine("请在心里想一个不大于{0}的数字。", maxNumber);
            while(true)
            {
                int tempNumber = (maxNumber - minNumber) / 2;
                Console.Write("这个数大于{0}吗(y/n)？", tempNumber);
                string s = Console.ReadLine();
                if(s == "y")
                {
                    //maxNumber = tempNumber;
                    minNumber = tempNumber; 


                }
                else if(s == "n")
                {
                    maxNumber = tempNumber;
                }
                else
                {
                    continue;

                }
                if(minNumber == maxNumber)
                {
                    Console.WriteLine("你想的数是{0}", maxNumber);
                    break;
                }

            }

            Console.ReadLine();
        }
    }//end class
    
}//end namespace
