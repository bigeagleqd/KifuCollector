//--------------------------------------------------------------------------
//
//  File:        Game.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/21
//
//  Description: 对局类，棋类游戏抽象基类，如围棋、五子棋等
//               
//  
//  History:     2013/1/21 created by bigeagle  
//               2013/1/31 删除GameBoard属性，把_GameBoard从私有改为保护，以加强
//                         封装
//               2013/2/12 修改为可移植类库，修改命名空间
//               2013/3/4  修改DropStone和DoDropStone方法的返回值为落子后的提子集合
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Bigeagle.Portable.BoardGames
{
    /// <summary>
    /// 棋局类
    /// </summary>
    public abstract class Game
    {

        /// <summary>
        /// 棋步
        /// </summary>
        private List<Stone> _Moves = new List<Stone>();

        /// <summary>
        /// 设置或获得棋步
        /// </summary>
        public List<Stone> Moves
        {
            get
            {
                return _Moves;
            }
        }


        /// <summary>
        /// 棋盘
        /// </summary>
        private Board _GameBoard;

        public Board GameBoard 
        {
            get
            {
                return _GameBoard;
            }
            set
            {
                _GameBoard = value;
            }
        }


        /// <summary>
        /// 设置或获得棋盘大小
        /// </summary>
        public  int  BoardSize
        {
            get
            {
                return _GameBoard.BoardSize;
                
            }
            set
            {
                _GameBoard.BoardSize = value;
            }
        }

        /// <summary>
        /// 将要走子事件
        /// </summary>
        private StoneCancelEventHandler _Moving;

        /// <summary>
        /// 将要走子委托
        /// </summary>
        public event StoneCancelEventHandler Movinging
        {
            add
            {
                _Moving += value ;
            }
            remove
            {
                _Moving -= value ;
            }
        }

        /// <summary>
        /// 落子默认处理方法
        /// </summary>
        /// <param name="e"></param>
        protected void OnStoneDropping(StoneCancelEventArgs e)
        {
            if (_Moving != null)
            {
                _Moving(this, e);
            }
        }

        /// <summary>
        /// 走子事件
        /// </summary>
        private StoneEventHandler _Moved ;

        /// <summary>
        /// 走子委托
        /// </summary>
        public event StoneEventHandler Moved
        {
            add
            {
                _Moved += value;
            }
            remove
            {
                _Moved -= value;
            }
        }

        /// <summary>
        /// 棋盘大小改变委托
        /// </summary>
        private EventHandler _BoardSizeChanged = null;

        /// <summary>
        /// 棋盘大小改变事件引发方法
        /// </summary>
        /// <param name="e"></param>
        protected void OnBoardSizeChanged(System.EventArgs e)
        {
            if (_BoardSizeChanged != null)
            {
                _BoardSizeChanged(this, e);
            }
        }

        /// <summary>
        /// 棋盘大小改变事件
        /// </summary>
        public event EventHandler BoardSizeChanged
        {
            add
            {
                _BoardSizeChanged += value;
            }
            remove
            {
                _BoardSizeChanged -= value;
            }
        }


        /// <summary>
        /// 落子默认处理方法
        /// </summary>
        /// <param name="e"></param>
        protected void OnDropped(StoneEventArgs e)
        {
            if (_Moved != null)
            {
                //e.TheStone.DropTime = DateTime.Now;
                e.TheStone.Step = _Moves.Count;
                _GameBoard.DropStone(e.TheStone);
                _Moves.Add(e.TheStone);
                _Moved(this, e);
            }
        }



        /// <summary>
        /// 走棋
        /// </summary>
        public List<Stone> DropStone(Stone stone)
        {
            List<Stone> result = new List<Stone>();
            if (CanDropStone(stone))
            {
                StoneCancelEventArgs e = new StoneCancelEventArgs(stone);
                this.OnStoneDropping(e);
                if (!e.Cancel)
                {
                    result = DoDropStone(stone);
                    this.OnDropped(new StoneEventArgs(stone));
                }
            }

            return result;
        }

        /// <summary>
        /// 落子
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected abstract List<Stone> DoDropStone(Stone stone);

        /// <summary>
        /// 是否可以落子
        /// </summary>
        /// <param name="stone"></param>
        /// <returns></returns>
        public abstract bool CanDropStone(Stone stone);

        /// <summary>
        /// 获取下一步的对局方
        /// </summary>
        /// <returns></returns>
        public abstract Stone.Sides GetNextSide();

        /// <summary>
        /// 构造函数
        /// </summary>
        public Game()
        {
            _GameBoard = new Board();
            _GameBoard.SizeChanged += _GameBoard_SizeChanged;
        }

        /// <summary>
        /// 棋盘大小改变处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _GameBoard_SizeChanged(object sender, EventArgs e)
        {
            OnBoardSizeChanged(EventArgs.Empty);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Game()
        {
            //移除棋盘对象的大小改变事件，以确保释放相关资源。
            _GameBoard.SizeChanged -= _GameBoard_SizeChanged;
        }//end method

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        ///// <param name="black">执黑方</param>
        ///// <param name="white">执白方</param>
        //public Game(Player black, Player white)
        //{
        //    _BlackPlayer = black;
        //    _WhitePlayer = white;
        //    _GameBoard = new Board();
        //    _GameBoard.SizeChanged += _GameBoard_SizeChanged;
        //}

        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _StartTime;

        /// <summary>
        /// 设置或获得开始时间
        /// </summary>
        public DateTime StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _EndTime;//end method

        /// <summary>
        /// 设置或获得结束时间
        /// </summary>
        public DateTime EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; }
        }

    }//end class

    /// <summary>
    /// 对局者类
    /// </summary>
    public class Player
    {
        /// <summary>
        /// 名字
        /// </summary>
        private string _Name;

        /// <summary>
        /// 设置或获得名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        /// <summary>
        /// 对局方
        /// </summary>
        private Stone.Sides _Side;

        /// <summary>
        /// 设置或获得对局方
        /// </summary>
        public Stone.Sides Side
        {
            get { return _Side; }
            set { _Side = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Player()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名字</param>
        /// <param name="side">对局方</param>
        public Player(string name, Stone.Sides side)
        {
            _Name = name;
            _Side = side;
        }

        
    }//end class

    /// <summary>
    /// 棋局类型
    /// </summary>
    /// <remarks>
    /// 棋局类型，定义根据SGF标准。
    /// 围棋 = 1，黑白棋 = 2，国际象棋 = 3
    /// ，五子棋 = 4 九子棋 = 5，西洋双陆棋 = 6
    /// ，中国象棋 = 7，日本将棋 = 8 集结棋 = 9
    /// ，攻击棋 = 10，六角棋 = 11
    /// </remarks>
    public enum GameType
    {

        /// <summary>
        /// 围棋
        /// </summary>
        Go = 1,
        /// <summary>
        /// 黑白棋
        /// </summary>
        /// <remarks>
        /// 黑白棋，又叫反棋（Reversi）、奥赛罗棋（Othello）、苹果棋或翻转棋。
        /// 黑白棋在西方和日本很流行。游戏通过相互翻转对方的棋子，最后以棋盘上谁的棋子多来判断胜负。
        /// 它的游戏规则简单，因此上手很容易，但是它的变化又非常复杂。
        /// 有一种说法是：只需要几分钟学会它，却需要一生的时间去精通它。
        /// </remarks>
        Othello = 2,
        /// <summary>
        /// 国际象棋
        /// </summary>
        Chess = 3,
        /// <summary>
        /// 五子棋
        /// </summary>
        FIR = 4,
        /// <summary>
        /// 九子棋
        /// </summary>
        /// <remarks>
        /// 九子棋（Nine Men's Morris）是一个非常古老的智力游戏。
        /// 其历史甚至可以追溯到公元前1400多年的古埃及时代。
        /// 棋盘有24个格点，对弈双方各有九个棋子，轮流下到棋盘的空位上。
        /// 如果一方有三个棋子连成一线，也就是形成一个工厂，
        /// 就可以选择吃掉对方的一个棋子。
        /// 被吃的棋子不可以是位于对方所形成的工厂之内，
        /// 除非对方所有棋子都形成了工厂。
        /// 在九个棋子都布放到棋盘上以后，可以沿棋盘上的线条移动到相邻的位置来形成厂以吃掉对方的棋子。
        /// 一个工厂可以开开合合重复使用。如果一方所剩下的棋子只有三个时，
        /// 棋子可以“飞”到任何位置而不受只能移动到相邻位置的限制。
        /// 当一方只剩下两个棋子或者他的所有棋子都不能移动时就算输
        /// </remarks>
        NineMensMorris = 5,
        /// <summary>
        /// 西洋双陆棋
        /// </summary>
        Backgammon = 6,
        /// <summary>
        /// 中国象棋
        /// </summary>
        ChineseChess = 7,
        /// <summary>
        /// 日本将棋
        /// </summary>
        Shogi = 8,
        /// <summary>
        /// 集结棋
        /// </summary>
        LinesOfAction = 9,
        /// <summary>
        /// 六角棋
        /// </summary>
        HEX = 11,
    }

}//end namespace
