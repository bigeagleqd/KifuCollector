//--------------------------------------------------------------------------
//
//  File:        KiFUGame.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/24
//
//  Description: 打谱对局类，实现已有对局的保存及载入，打谱。
//  
//  History:     2013/1/24 created by bigeagle  
//
//--------------------------------------------------------------------------

using Bigeagle.Portable.BoardGames.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bigeagle.Portable.BoardGames.Go
{
    /// <summary>
    /// 打谱类
    /// </summary>
    public class KiFUGame : GoGameBase
    {


        ///// <summary>
        ///// 棋谱读写器
        ///// </summary>
        ///// <remarks>
        ///// 如不指定默认使用ISGFKiFUReadWriter
        ///// </remarks>
        //IKiFUReadWriter _KiFUReadWriter = null;

        ///// <summary>
        ///// 设置或获得棋谱读写器
        ///// </summary>
        //public IKiFUReadWriter ReadWriter
        //{
        //    get
        //    {
        //        return _KiFUReadWriter;
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _KiFUReadWriter = value;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 棋局命令控制器
        ///// </summary>
        //CommandController _MoveController;

        //public CommandController MoveController
        //{
        //    get
        //    {
        //        return _MoveController;
        //    }
        //}


        /// <summary>
        /// 子棋局，一般用于存储变化图
        /// </summary>
        private List<KiFUGame> _SubGames;

        /// <summary>
        /// 获得子游戏
        /// </summary>
        public List<KiFUGame> SubGames
        {
            get { return _SubGames; }
            set { _SubGames = value; }
        }

        /// <summary>
        /// 父棋局
        /// </summary>
        KiFUGame _Parent = null;

        /// <summary>
        /// 设置或获得父棋局
        /// </summary>
        public KiFUGame Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }

        /// <summary>
        /// 获取主线游戏棋步
        /// </summary>
        public List<CommentsMove> MainGameTreeMoves
        {
            get
            {
                return GetActualPlayMoves();
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public KiFUGame()
        {
            //_KiFUReadWriter = new SGFKiFUReadWriter();
            _SubGames = new List<KiFUGame>();
            //_MoveController = new CommandController();
            //_Comments = new Hashtable();
        }//end method

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        ///// <param name="readWriter">读写器</param>
        //public KiFUGame(IKiFUReadWriter readWriter)
        //{
        //    _Comments = new Hashtable();
        //    _SubGames = new List<KiFUGame>();
        //    ReadWriter = readWriter;
        //}

        /// <summary>
        /// 编写者
        /// </summary>
        private string _User;

        /// <summary>
        /// 设置或获得棋谱编写者(SGF Code:US)
        /// </summary>
        public string User
        {
            get { return _User; }
            set { _User = value; }
        }
        /// <summary>
        /// 来源
        /// </summary>
        private string _Source;

        /// <summary>
        /// 设置或获得棋谱来源(SGF Code:SO)
        /// </summary>
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }
        /// <summary>
        /// 版权
        /// </summary>
        private string _Copyright;

        /// <summary>
        /// 设置或获得棋谱版权(SGF Code:CP)
        /// </summary>
        public string Copyright
        {
            get { return _Copyright; }
            set { _Copyright = value; }
        }
        /// <summary>
        /// 注解者名字
        /// </summary>
        private string _Annotation;

        /// <summary>
        /// 设置或获得讲解者名字(SGF Code:AN)
        /// </summary>
        public string Annotation
        {
            get { return _Annotation; }
            set { _Annotation = value; }
        }

        /// <summary>
        /// 预先放置的棋子
        /// </summary>
        /// <remarks>对应SGF中的AB/AW属性</remarks>
        private List<Stone> _PreMoves = new List<Stone>();

        /// <summary>
        /// 获得预先放置的棋子(SGF Code:AB/AW)
        /// </summary>
        /// <remarks>对应SGF中的AB/AW</remarks>
        public List<Stone> PreMoves
        {
            get
            {
                return _PreMoves;
            }
        }

        /// <summary>
        /// 获得实战走棋数组
        /// </summary>
        public List<CommentsMove> GetActualPlayMoves()
        {
            //把主线子游戏的棋子都填充到主游戏的落子集合里
            List<CommentsMove> moves = new List<CommentsMove>();
            KiFUGame game = this;
            //加入主游戏树的节点集合
            while (true)
            {
                foreach (CommentsMove move in game.Moves)
                {
                    if (!moves.Contains(move))
                    {
                        if (move.IsRealMove)
                        {
                            move.Step = moves.Count(m => (m as CommentsMove).IsRealMove) + 1;
                        }
                        else
                        {
                            move.Step = -1;
                        }
                        moves.Add(move);
                    }
                }
                ///加入主游戏树的第一个子游戏节点
                if (game.SubGames.Count > 0)
                {
                    game = game.SubGames[0];
                }
                else
                {
                    break;
                }
            }
            return moves;
        }
        /// <summary>
        /// 生成棋谱程序名称
        /// </summary>
        private string _AppName = "EagleGo";

        /// <summary>
        /// 设置或获得应用程序名称(SGF Code:AP)
        /// </summary>
        public string AppName
        {
            get { return _AppName; }
            set { _AppName = value; }
        }

        ///// <summary>
        ///// 改写DoDropStone方法已添加DropStoneCommand
        ///// </summary>
        ///// <param name="stone"></param>
        ///// <returns></returns>
        //protected override List<Stone> DoDropStone(Stone stone)
        //{
        //    //提子集合
        //    List<Stone> removedStones = base.DoDropStone(stone);

        //    //如果没有指定command则新建并添加到命令集合
        //    DropStoneCommand command = _MoveController.Commands.Where(c => c is DropStoneCommand 
        //        && (c as DropStoneCommand).DroppedStone == stone).FirstOrDefault() as DropStoneCommand;
        //    if (command == null)
        //    {
        //        command = new DropStoneCommand(this, stone, removedStones);
        //        _MoveController.Commands.Add(command);
        //        _MoveController.CurrentIndex = _MoveController.Commands.Count - 1;
        //    }
        //    //else
        //    //{
        //    //    _MoveController.CurrentCommand = command;
        //    //}



        //    return removedStones;
        //}

        #region 标签结构
        public struct Label
        {
            /// <summary>
            /// 名称
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
            /// 横坐标
            /// </summary>
            private int _X;

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
            private int _Y;

            /// <summary>
            /// 设置或获得纵坐标
            /// </summary>
            public int Y
            {
                get { return _Y; }
                set { _Y = value; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="name">名称</param>
            /// <param name="x">横坐标</param>
            /// <param name="y">纵坐标</param>
            public Label(string name, int x, int y)
            {
                _Name = name;
                _X = x;
                _Y = y;
            }
        }

        /// <summary>
        /// 黑方队伍
        /// </summary>
        private string _BlackTeam;

        /// <summary>
        /// 设置或获得黑方队伍名称(SGF Code:BT)
        /// </summary>
        public string BlackTeam
        {
            get { return _BlackTeam; }
            set { _BlackTeam = value; }
        }
        /// <summary>
        /// 白方队伍
        /// </summary>
        private string _WhiteTeam;//end struct

        /// <summary>
        /// 设置或获得白方队伍(SGF Code:WT)
        /// </summary>
        public string WhiteTeam
        {
            get { return _WhiteTeam; }
            set { _WhiteTeam = value; }
        }
        #endregion//end 标签结构



    }//end class
}//end namespace
