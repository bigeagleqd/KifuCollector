//--------------------------------------------------------------------------
//
//  File:        Stone.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/22
//
//  Description: 棋子类，使用围棋盘的棋类游戏棋子抽象基类，如围棋、五子棋等
//  
//  History:     2013/1/22 created by bigeagle  
//               2013/2/12 修改为可移植类库，修改命名空间
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;


namespace Bigeagle.Portable.BoardGames
{
    #region 棋子类

    /// <summary>
    /// 围棋棋子类
    /// </summary>
    /// <remarks>坐标0-18</remarks>
    public class Stone
    {

        #region 成员变量及属性

        /// <summary>
        /// 棋子X坐标
        /// </summary>
        private int _X;

        /// <summary>
        /// 设置或获得棋子X坐标
        /// </summary>
        public int X
        {
            get 
            { 
                return _X; 
            }
            set 
            {
                if (value >= 0 && value <= 19)
                {
                    _X = value;
                }
                else
                {
                    throw (new ArgumentException());
                }
            }
        }

        /// <summary>
        /// 棋子Y坐标
        /// </summary>
        private int _Y;

        /// <summary>
        /// 设置或获得棋子Y坐标
        /// </summary>
        public int Y
        {
            get 
            { 
                return _Y; 
            }
            set 
            {
                if (value >= 0 && value < 19)
                {
                    _Y = value;
                }
                else
                {
                    throw (new ArgumentException());
                }
            }
        }

        /// <summary>
        /// 对局方
        /// </summary>
        private Sides _Side;

        /// <summary>
        /// 设置或获得对局方
        /// </summary>
        public Sides Side
        {
            get { return _Side; }
            set { _Side = value; }
        }

        /// <summary>
        /// 是否存活
        /// </summary>
        private bool _IsAlive;

        /// <summary>
        /// 设置或获得棋子是否存活
        /// </summary>
        public bool IsAlive
        {
            get { return _IsAlive; }
            set { _IsAlive = value; }
        }
        /// <summary>
        /// 多少步
        /// </summary>
        private int _Step;

        /// <summary>
        /// 设置或获得当前为多少步
        /// </summary>
        public int Step
        {
            get { return _Step; }
            set { _Step = value; }
        }

        /// <summary>
        /// 落子时间
        /// </summary>
        private DateTime _DropTime;

        /// <summary>
        /// 设置或获得落子时间
        /// </summary>
        public DateTime DropTime
        {
            get { return _DropTime; }
            set { _DropTime = value; }
        }

        /// <summary>
        /// 是否是放弃一手
        /// </summary>
        private bool _IsGiveUp = false;

        public bool IsGiveUp
        {
            get
            {
                return _IsGiveUp;
            }
            set
            {
                _IsGiveUp = value;
            }
        }

        #endregion//end 成员变量及属性

        /// <summary>
        /// 构造函数
        /// </summary>
        public Stone()
        {
            _IsAlive = true;
            _DropTime = DateTime.Now;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">横坐标，0为左边1线</param>
        /// <param name="y">纵坐标，0为下边1线</param>
        /// <param name="step">步数</param>
        /// <param name="side">对局方</param>
        public Stone(int x, int y, int step, Sides side)
        {
            X = x;
            Y = y;
            Step = step;
            Side = side;
            IsAlive = true;
            _DropTime = DateTime.Now;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">横坐标，0为左边1线</param>
        /// <param name="y">纵坐标，0为下边1线</param>
        /// <param name="step">步数</param>
        /// <param name="side">对局方</param>
        /// <param name="dropTime">落子时间</param>
        public Stone(int x, int y, int step, Sides side, DateTime dropTime)
        {
            X = x;
            Y = y;
            Step = step;
            Side = side;
            IsAlive = true;
            _DropTime = dropTime;
        }

        /// <summary>
        /// 判断两个棋子是不是一样
        /// </summary>
        /// <param name="stone"></param>
        /// <returns></returns>
        public bool EqualsPosition(Stone stone)
        {
            return stone.X == _X && stone.Y == _Y && stone.Side == _Side;
        }

        #region 对局方枚举

        /// <summary>
        /// 对局方
        /// </summary>
        public enum Sides
        {
            /// <summary>
            /// 黑子
            /// </summary>
            Black,

            /// <summary>
            /// 白子
            /// </summary>
            White
        }
        #endregion//对局方枚举

    }//end class

    #endregion//end 棋子类

    #region 棋子事件参数类及委托定义

    /// <summary>
    /// 落子委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void StoneEventHandler(object sender , StoneEventArgs e) ;

    /// <summary>
    /// 棋子事件参数类
    /// </summary>
    public class StoneEventArgs : EventArgs
    {
        /// <summary>
        /// 棋子
        /// </summary>
        Stone _Stone;

        /// <summary>
        /// 设置或获得当前棋子
        /// </summary>
        public Stone TheStone
        {
            get
            {
                return _Stone;
            }
            set
            {
                _Stone = value;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StoneEventArgs()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stone"></param>
        public StoneEventArgs(Stone stone)
        {
            _Stone = stone;
        }
    }//end class

    /// <summary>
    /// 落子委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void StoneCancelEventHandler(object sender, StoneCancelEventArgs e);

    /// <summary>
    /// 棋子取消事件参数类
    /// </summary>
    public class StoneCancelEventArgs : CancelEventArgs
    {
        /// <summary>
        /// 棋子
        /// </summary>
        Stone _Stone;

        /// <summary>
        /// 设置或获得当前棋子
        /// </summary>
        public Stone TheStone
        {
            get
            {
                return _Stone;
            }
            set
            {
                _Stone = value;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StoneCancelEventArgs()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stone"></param>
        public StoneCancelEventArgs(Stone stone)
        {
            _Stone = stone;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stone"></param>
        /// <param name="cancel"></param>
        public StoneCancelEventArgs(Stone stone, bool cancel)
            : base(cancel)
        {
            _Stone = stone;
        }
    }//end class
    #endregion//end 棋子事件参数及委托定义

}//end namespace
