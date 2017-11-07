//--------------------------------------------------------------------------
//
//  File:        DropStoneCommand.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/3/4
//
//  Description: 走子命令，用于走棋的撤销重做等功能
//  
//  History:     2013/3/4 created by bigeagle  
//               2017/11/7 修改为.net standard类库，修改命名空间
//
//--------------------------------------------------------------------------

using Bigeagle.BoardGames.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bigeagle.BoardGames.Go
{
    /// <summary>
    /// 走子命令
    /// </summary>
    public class DropStoneCommand : ICommand
    {
        /// <summary>
        /// 棋局
        /// </summary>
        private GoGameBase _TheGame;

        /// <summary>
        /// 设置或获得棋局
        /// </summary>
        public GoGameBase TheGame
        {
            get { return _TheGame; }
            set { _TheGame = value; }
        }

        /// <summary>
        /// 落子
        /// </summary>
        private Stone _DroppedStone;

        /// <summary>
        ///  设置或获得落子
        /// </summary>
        public Stone DroppedStone
        {
            get { return _DroppedStone; }
            set { _DroppedStone = value; }
        }

        /// <summary>
        /// 移除的棋子列表
        /// </summary>
        /// <remarks>落子后导致的提子列表</remarks>
        private List<Stone> _RemovedStones;

        /// <summary>
        /// 获得落子后提子的列表
        /// </summary>
        public List<Stone> RemovedStones
        {
            get { return _RemovedStones; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DropStoneCommand()
        {
            _RemovedStones = new List<Stone>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="game">棋局</param>
        /// <param name="dropedStone">落子</param>
        public DropStoneCommand(GoGameBase game, Stone dropedStone)
        {
            _DroppedStone = dropedStone;
            _TheGame = game;
            _RemovedStones = new List<Stone>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="game">棋局</param>
        /// <param name="dropedStone">落子</param>
        /// <param name="removedStones">提子集合</param>
        public DropStoneCommand(GoGameBase game, Stone dropedStone, IEnumerable<Stone> removedStones)
        {
            _DroppedStone = dropedStone;
            _TheGame = game;
            _RemovedStones = new List<Stone>();
            _RemovedStones.AddRange(removedStones);
        }

        #region 实现ICommand接口方法
        /// <summary>
        /// 做
        /// </summary>
        public void Do()
        {
            _TheGame.DropStone(_DroppedStone);
            
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void UnDo()
        {
            _TheGame.RemoveStone(_DroppedStone);

            foreach (Stone s in _RemovedStones)
            {
                _TheGame.DropStone(s);
            }
        }
        #endregion//end ICommand接口方法
    }//end class
}//end namespace
