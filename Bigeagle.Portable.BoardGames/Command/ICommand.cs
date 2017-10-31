//--------------------------------------------------------------------------
//
//  File:        ICommand.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/3/4
//
//  Description: 命令接口，用于棋局的撤销重做等功能
//  
//  History:     2013/3/4 created by bigeagle  
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bigeagle.Portable.BoardGames.Command
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 做
        /// </summary>
        void Do();

        /// <summary>
        /// 撤销
        /// </summary>
        void UnDo();
    }//end interface
}//end interface
