//--------------------------------------------------------------------------
//
//  File:        Board.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/22
//
//  Description: 棋盘类，使用围棋盘的棋类游戏棋盘类，如围棋、五子棋等
//  
//  History:     2013/1/22 created by bigeagle  
//               2013/2/12 修改为可移植类库，修改命名空间
//               2017/11/7 修改为.net standard类库，修改命名空间
//
//--------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bigeagle.BoardGames
{
    /// <summary>
    /// 棋盘类
    /// </summary>
    public class Board
    {

        #region 成员变量及属性

        /// <summary>
        /// 棋盘大小
        /// </summary>
        /// <remarks>只能是大于10小于20的奇数</remarks>
        private int _BoardSize = 19 ;

        /// <summary>
        /// 设置或获得棋盘大小，只能是大于10小于20的奇数，默认是19
        /// </summary>
        public int BoardSize
        {
            get
            {
                return _BoardSize ;
            }
            set
            {
                if(value > 0 && value <= 52 && value % 2 == 1)
                {
                    int oldValue = _BoardSize;
                    _BoardSize = value ;
                    if (oldValue != value)
                    {
                        OnSizeChanged(EventArgs.Empty);
                    }
                }
                else
                {
                    throw(new ArgumentException()) ;
                }
            }
        }


        /// <summary>
        /// 棋盘上的位置数组
        /// </summary>
        private Position[,] _Positions;

        /// <summary>
        /// 死子集合
        /// </summary>
        //private List<Stone> _DeadStones;

        /// <summary>
        /// 棋盘尺寸改变委托
        /// </summary>
        private EventHandler _SizeChanged = null;

        /// <summary>
        /// 棋盘尺寸改变事件引发方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnSizeChanged(EventArgs e)
        {
            InitPositions();
            
            if (_SizeChanged != null)
            {
                _SizeChanged(this, e);
            }
        }

        /// <summary>
        /// 棋盘尺寸改变委托
        /// </summary>
        public event EventHandler SizeChanged
        {
            add
            {
                _SizeChanged += value;
            }
            remove
            {
                _SizeChanged -= value;
            }
        }

        #endregion//end 成员变量及属性

        /// <summary>
        /// 初始化棋盘位置二维数组
        /// </summary>
        public void InitPositions()
        {
            _Positions = new Position[_BoardSize, _BoardSize];
            for (int i = 0; i < _BoardSize; i++)
            {
                for (int j = 0; j < _BoardSize; j++)
                {
                    _Positions[i, j] = new Position(i, j);
                    _Positions[i, j].TheStone = null;
                }
            }

        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public Board()
        {
            //BoardSize = _BoardSize;
            //_DeadStones = new List<Stone>();
            InitPositions();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="boardSize"></param>
        public Board(int boardSize)
        {
            BoardSize = boardSize;
            //_Positions = new Position[_BoardSize, _BoardSize];
            //_DeadStones = new List<Stone>();
            //InitPositions();
        }

        /// <summary>
        /// 是否可以落子
        /// </summary>
        /// <param name="stone">当前要落的子</param>
        /// <returns></returns>
        public bool CanDropStone(Stone stone)
        {
            bool result = false;

            ///查找当前位置已有的落子
            Position positon = _Positions[stone.X, stone.Y];
            Stone stoneExists = positon.TheStone;
            

            if (stoneExists != null) //如果已经有子则不能落子
            {
                result = false;
            }
            else  //如果当期位置没有子
            {
                //通常可以落子
                result = true;
                //
                //TODO:判断如果四边有对方的子则不能落子
                //

                //TODO:判断如果是打劫则不能立刻落子

            }

            return result;
        }

        /// <summary>
        /// 落子
        /// </summary>
        /// <param name="stone">要落的子</param>
        /// <returns>落子</returns>
        public void DropStone(Stone stone)
        {
            if(CanDropStone(stone))
            {
                _Positions[stone.X , stone.Y ].TheStone = stone ;
                _Positions[stone.X, stone.Y].X = stone.X;
                _Positions[stone.X, stone.Y].Y = stone.Y;
                stone.IsAlive = true;
            }
        }//ene method

        /// <summary>
        /// 移除棋子
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RemoveStone(int x, int y)
        {
            _Positions[x, y].TheStone = null;
            
        }

        public List<Position> GetPositionsWithStone()
        {
            List<Position> result = new List<Position>();
            for (int i = 0; i < _BoardSize; i++)
            {
                for (int j = 0; j < _BoardSize; j++)
                {
                    if (_Positions[i, j].TheStone != null)
                    {
                        result.Add(_Positions[i, j]);
                    }
                }
            }

            return result;
        }//end method
        /// <summary>
        /// 获得位置对象
        /// </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        /// <returns></returns>
        public Position GetPosition(int x, int y)
        {
            if (x >= 0 && x < _Positions.GetLength(0) && y >= 0 && y < _Positions.GetLength(1))
            {
                return _Positions[x, y];
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 获得指定的棋子周边的气
        /// </summary>
        /// <param name="stone"></param>
        /// <returns></returns>
        public List<Position> GetSpaces(Stone stone)
        {
            List<Position> result = new List<Position>();
            Position[] temp = new Position[]{GetPosition(stone.X - 1, stone.Y) 
                , GetPosition(stone.X + 1, stone.Y),GetPosition(stone.X , stone.Y - 1)
                ,GetPosition(stone.X , stone.Y + 1) 
            };

            foreach (Position p in temp)
            {
                if (p != null && p.TheStone == null)
                {
                    result.Add(p);
                }
            }
            return result;
        }//end method

        /// <summary>
        /// 查找指定棋子周边的棋子
        /// </summary>
        /// <param name="stone"></param>
        /// <returns></returns>
        internal List<Stone> FindNearbyStones(Stone stone)
        {
            List<Stone> stones = new List<Stone>();
            List<Position> positions = new List<Position>();
            if (stone.X > 0)
            {
                positions.Add(_Positions[stone.X - 1, stone.Y]);
            }
            if (stone.X < _BoardSize - 1)
            {
                positions.Add(_Positions[stone.X + 1, stone.Y]);
            }
            if (stone.Y > 0)
            {
                positions.Add(_Positions[stone.X, stone.Y - 1]);
            }
            if (stone.Y < _BoardSize - 1)
            {
                positions.Add(_Positions[stone.X, stone.Y + 1]);
            }
            stones.AddRange(positions.Where(p => p.TheStone != null).Select(p => p.TheStone));
            return stones;
        }

        /// <summary>
        /// 找到指定棋子所在的块
        /// </summary>
        /// <param name="s">指定棋子</param>
        /// <returns>棋子所在的块</returns>
        internal Block GetBlock(Stone stone)
        {
            //断言
            Debug.Assert(stone != null, "棋子不应该为空。");

            Block block = new Block(stone.Side);
            block.Parent = this;
            block.Stones.Add(stone);
            AddStoneToBlock(block, stone);

            return block;
        }

        /// <summary>
        /// 添加临近的本方棋子到块
        /// </summary>
        /// <param name="block"></param>
        /// <param name="stone"></param>
        private void AddStoneToBlock(Block block, Stone stone)
        {
            List<Stone> stones = FindNearbyStones(stone).Where(s => s.Side == stone.Side).ToList();
            foreach (Stone s in stones)
            {
                if (!block.Stones.Contains(s))
                {
                    block.Stones.Add(s);
                    AddStoneToBlock(block, s);
                }
            }
        }


        #region 棋盘位置类

        /// <summary>
        /// 棋盘位置结构
        /// </summary>
        public class Position
        {
            /// <summary>
            /// 横坐标
            /// </summary>
            int _X;

            /// <summary>
            /// 设置或获得横坐标
            /// </summary>
            public int X
            {
                get { return _X; }
                set { _X = value; }
            }

            

            /// <summary>
            /// 纵坐标
            /// </summary>
            int _Y;

            /// <summary>
            /// 设置或获得纵坐标
            /// </summary>
            public int Y
            {
                get { return _Y; }
                set { _Y = value; }
            }

            /// <summary>
            /// 棋子
            /// </summary>
            Stone _Stone;

            /// <summary>
            /// 设置或获得棋子对象
            /// </summary>
            public Stone TheStone
            {
                get { return _Stone; }
                set { _Stone = value; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public Position()
            {
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="x">横坐标</param>
            /// <param name="y">纵坐标</param>
            public Position(int x, int y)
            {
                _X = x;
                _Y = y;
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="x">横坐标</param>
            /// <param name="y">纵坐标</param>
            /// <param name="stone">棋子</param>
            public Position(int x, int y, Stone stone)
            {
                _X = x;
                _Y = y;
                _Stone = stone;
            }

           
        }//end class
        #endregion

    }//end class
}//end namespace
