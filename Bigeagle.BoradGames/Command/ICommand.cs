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
//               2017/11/7 修改为.net standard类库，修改命名空间
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bigeagle.BoardGames.Command
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
