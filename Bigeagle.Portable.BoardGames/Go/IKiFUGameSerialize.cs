//--------------------------------------------------------------------------
//
//  File:       IKiFUGameSerialize.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/22
//
//  Description: 棋谱游戏序列化接口
//  
//  History:     2013/2/15 created by bigeagle  
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bigeagle.Portable.BoardGames.Go
{
    /// <summary>
    /// 棋谱游戏序列化接口
    /// </summary>
    public interface IKiFUGameSerialize
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="content">棋谱内容字符串</param>
        /// <returns>棋谱游戏</returns>
        KiFUGame DeSerialize(string content);

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="game">棋谱游戏</param>
        /// <returns>棋谱内容字符串</returns>
        string Serialize(KiFUGame game);

        /// <summary>
        /// 读取对局信息
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        GoGameInfo ReadGameInfo(System.IO.Stream stream);

    }//end interface
}//end namespace
