//--------------------------------------------------------------------------
//
//  File:        Block.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/22
//
//  Description: 棋块类，一组相邻的同方棋子集合，用于计算死活/数目等
//  
//  History:     2013/1/22 created by bigeagle  
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bigeagle.Portable.BoardGames
{
    /// <summary>
    /// 块
    /// </summary>
    public class Block
    {
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
        /// 包含的棋子
        /// </summary>
        private List<Stone> _Stones;

        /// <summary>
        /// 获得包含的棋子
        /// </summary>
        public List<Stone> Stones
        {
            get { return _Stones; }
        }

        /// <summary>
        /// 棋盘
        /// </summary>
        private Board _Parent = null;

        /// <summary>
        /// 设置或获得所属的棋盘对象
        /// </summary>
        public Board Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Block()
        {
            _Stones = new List<Stone>();
        }//end method

        /// <summary>
        /// 改写构造函数
        /// </summary>
        /// <param name="side"></param>
        public Block(Stone.Sides side)
        {
            _Side = side;
            _Stones = new List<Stone>();
        }//end method

        /// <summary>
        /// 棋子是否可以加入块
        /// </summary>
        /// <param name="stone"></param>
        /// <returns></returns>
        public bool CanStoneJoin(Stone stone)
        {
            return _Stones.Count(s => s.Side == stone.Side && ((Math.Abs(stone.X - s.X) == 1 && s.Y == stone.Y)
                                       | (Math.Abs(s.Y - stone.Y) == 1 && s.X == stone.X))) == 1;

        }//end method

        /// <summary>
        /// 获取外气
        /// </summary>
        /// <returns></returns>
        public List<Board.Position> GetOutsideLiberties()
        {
            //断言，确保判断有气之前棋盘对象不为空引用
            Debug.Assert(_Parent != null, "所属棋盘对象不应该唯恐。");
            List<Board.Position> result = new List<Board.Position>();
            foreach (Stone stone in _Stones)
            {
                List<Board.Position> positions = _Parent.GetSpaces(stone);
                foreach (Board.Position p in positions)
                {
                    if (!result.Contains(p))
                    {
                        result.Add(p);
                    }
                }
            }
            return result;
        }//end method

        /// <summary>
        /// 块是否有气
        /// </summary>
        /// <returns></returns>
        public bool HasLiberty(Stone stone)
        {
            List<Board.Position> outsideLiberties = GetOutsideLiberties();
            return outsideLiberties.Count > 0;            
        }

        
    }//end class
}//end namespace
