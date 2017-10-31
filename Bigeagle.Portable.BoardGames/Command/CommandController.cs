//--------------------------------------------------------------------------
//
//  File:        CommandControler.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/3/5
//
//  Description: 命令控制器类，用于棋局的撤销重做等功能
//  
//  History:     2013/3/5 created by bigeagle  
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bigeagle.Portable.BoardGames.Command
{
    /// <summary>
    /// 命令控制器类，实现撤销/重做功能
    /// </summary>
    public class CommandController
    {
        /// <summary>
        /// 当前命令索引
        /// </summary>
        int _CurrentIndex;

        /// <summary>
        /// 获得当前命令索引
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return _CurrentIndex;
            }
            set
            {
                _CurrentIndex = value;
            }
        }

        ///// <summary>
        ///// 当前命令
        ///// </summary>
        //private Command _CurrentCommand;

        /// <summary>
        /// 获得当前命令
        /// </summary>
        public ICommand CurrentCommand
        {
            get
            {
                return _CurrentIndex < 0 ? null : _Commands[_CurrentIndex];
            }
            set
            {
                if (_CurrentIndex > _Commands.Count - 1)
                {
                    _Commands.Add(value);
                }
                else
                {
                    _Commands[_CurrentIndex] = value;
                }
            }
        }

        /// <summary>
        /// 命令集合
        /// </summary>
        List<ICommand> _Commands;

        /// <summary>
        /// 返回命令集合
        /// </summary>
        public List<ICommand> Commands
        {
            get
            {
                return _Commands;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommandController()
        {
            _Commands = new List<ICommand>();
        }

        /// <summary>
        /// 重做
        /// </summary>
        /// <returns></returns>
        public bool Undo()
        {
            if (CurrentCommand != null)
            {
                CurrentCommand.UnDo();
                if (_CurrentIndex > 0)
                {
                    _CurrentIndex--;
                }
                else
                {
                    _CurrentIndex = -1;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 重做
        /// </summary>
        /// <returns></returns>
        public bool Redo()
        {
            if (CurrentCommand != null)
            {
                CurrentCommand.Do();
                if (_CurrentIndex < _Commands.Count - 1)
                {
                    _CurrentIndex++;
                }
                else
                {
                    _CurrentIndex = -1;
                }
                return true;
            }

            return false;
        }
    }//end class
}//end namespace
