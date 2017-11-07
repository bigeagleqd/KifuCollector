//--------------------------------------------------------------------------
//
//  File:        CommentsStone.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/30
//
//  Description: 带有讲解的棋子，继承自Stone类。
//               主要根据SGF的走子设置来定义，
//               SGF的详细定义见：http://wenku.baidu.com/view/33a47a49e518964bcf847c4f.html
//               目前没有实现全部定义，将来可能会补全。
//  
//  History:     2013/1/30 created by bigeagle  
//               2013/2/15 转移到可移植类库
//               2017/11/7 修改为.net standard类库，修改命名空间
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Bigeagle.BoardGames.Go
{
    /// <summary>
    /// 带有讲解的棋子
    /// </summary>
    /// <remarks>对应SGF中的Node。</remarks>
    public class CommentsMove : Stone
    {
        /// <summary>
        /// 数据，用于存放用户数据
        /// </summary>
        private object _Tag = null;

        /// <summary>
        /// 设置或获得用户数据
        /// </summary>
        public object Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

        /// <summary>
        /// 是否是走子，如果为真则是真正的走子，否则是注解
        /// </summary>
        private bool _IsRealMove = true;

        /// <summary>
        /// 设置或获得是否是真正的走子
        /// </summary>
        public bool IsRealMove
        {
            get { return _IsRealMove; }
            set { _IsRealMove = value; }
        }

        /// <summary>
        /// 标签集合
        /// </summary>
        private List<KiFUGame.Label> _Labels = new List<KiFUGame.Label>();

        /// <summary>
        /// 获得标签集合
        /// </summary>
        public List<KiFUGame.Label> Labels
        {
            get
            {
                return _Labels;
            }
        }
        /// <summary>
        /// 命名
        /// </summary>
        private string _Name = string.Empty;

        /// <summary>
        /// 设置或获得名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        /// <summary>
        /// 注解
        /// </summary>
        private string _Comment = string.Empty;

        /// <summary>
        /// 设置或获得注解
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; }
        }
        /// <summary>
        /// 强制设置的手数,-1说明没有设置
        /// </summary>
        /// <remarks>对应SGF的MN(Set move number) ，设定手数的数值，就是指定该节点中走子具有这个确定的手数。该属性对于分支或打印很有用</remarks>
        private int _ForceMoveNumber = -1;

        /// <summary>
        /// 设置或获得强制设定的手数
        /// </summary>
        public int ForceMoveNumber
        {
            get { return _ForceMoveNumber; }
            set { _ForceMoveNumber = value; }
        }
        /// <summary>
        /// 走子评价
        /// </summary>
        /// <remarks>目前忽略SGF中的程度值，都当做无值处理。</remarks>
        private MoveAssess _Assess;

        /// <summary>
        /// 设置或获得走子评价
        /// </summary>
        public MoveAssess Assess
        {
            get { return _Assess; }
            set { _Assess = value; }
        }

        /// <summary>
        /// 预先放置的棋子
        /// </summary>
        /// <remarks>对应SGF中的AB/AW属性</remarks>
        private List<Stone> _PreMoves = new List<Stone>();

        /// <summary>
        /// 获得预先放置的棋子
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
        /// 引用的棋局，用于变化图
        /// </summary>
        private List<GoGameBase> _ReferenceGames = new List<GoGameBase>();

        /// <summary>
        /// 设置或获得引用的棋局，用于变化图
        /// </summary>
        public List<GoGameBase> ReferenceGame
        {
            get
            {
                return _ReferenceGames;
            }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CommentsMove()
        {
        }//end method

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="step"></param>
        /// <param name="side"></param>
        public CommentsMove(int x, int y, int step, Stone.Sides side)
            : base(x, y, step, side)
        {
        }//end method

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="step"></param>
        /// <param name="side"></param>
        /// <param name="dropTime"></param>
        public CommentsMove(int x, int y, int step, Stone.Sides side , DateTime dropTime)
            : base(x, y, step, side , dropTime)
        {
        }//end method


        #region 走子评估枚举

        /// <summary>
        /// 走子评估枚举
        /// </summary>
        public enum MoveAssess
        {
            /// <summary>
            /// 无评估
            /// </summary>
            None = 0,

            /// <summary>
            /// 妙手
            /// </summary>
            /// <remarks>TE 手筋或妙手(Tesuji or Good move)</remarks>
            GoodMove ,
            /// <summary>
            /// 恶手
            /// </summary>
            /// <remarks>BM(Bad move)</remarks>
            BadMove,
            /// <summary>
            /// 疑问手
            /// </summary>
            /// <remarks>DO(Doubtful)</remarks>
            Doubtful,
            /// <summary>
            /// 有趣手
            /// </summary>
            /// <remarks>IT</remarks>
            Interesting,
            #endregion //走子评估枚举
        }
    }//end class
}//end namespace
