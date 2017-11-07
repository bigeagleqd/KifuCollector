//--------------------------------------------------------------------------
//
//  File:        GoGame.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/22
//
//  Description: 围棋类，实现围棋的基本逻辑，比如走棋、提子、形势判断
//               、计时、保存棋谱、读取棋谱等
//  
//  History:     2013/1/22 created by bigeagle  
//               2013/2/17 把棋局信息合并为GoGameInfo类，以便读取棋谱时可以只读棋局信息
//               2013/2/25 修正提子后未把相应棋盘位置置为空的bug。
//               2013/3/4  修改DropStone和DoDropStone方法的返回值为落子后的提子集合
//               2017/11/7 修改为.net standard类库，修改命名空间
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Bigeagle.BoardGames.Go
{

    /// <summary>
    /// 围棋类
    /// </summary>
    public class GoGameBase : Game
    {

        #region 成员变量及属性

        /// <summary>
        /// 棋局信息
        /// </summary>
        GoGameInfo _GameInfo = null;

        /// <summary>
        /// 设置或获得棋局信息
        /// </summary>
        public GoGameInfo GameInfo
        {
            get { return _GameInfo; }
            set { _GameInfo = value; }
        }

        /// <summary>
        /// 死子
        /// </summary>
        List<Stone> _DeadStones = null;

        /// <summary>
        /// 获得死子集合
        /// </summary>
        public List<Stone> DeadStones
        {
            get { return _DeadStones; }
            set { _DeadStones = value; }
        }

        /// <summary>
        /// 将要提子事件
        /// </summary>
        private StoneCancelEventHandler _StoneRemoving;

        /// <summary>
        /// 将要提子委托
        /// </summary>
        public event StoneCancelEventHandler StoneRemoving
        {
            add
            {
                _StoneRemoving += value;
            }
            remove
            {
                _StoneRemoving -= value;
            }
        }

        /// <summary>
        /// 提子默认处理方法
        /// </summary>
        /// <param name="e"></param>
        protected void OnStoneRemoving(StoneCancelEventArgs e)
        {
            if (_StoneRemoving != null)
            {
                _StoneRemoving(this, e);
            }
        }

        /// <summary>
        /// 提子事件
        /// </summary>
        private StoneEventHandler _StoneRemoved;

        /// <summary>
        /// 提子委托
        /// </summary>
        public event StoneEventHandler StoneRemoved
        {
            add
            {
                _StoneRemoved += value;
            }
            remove
            {
                _StoneRemoved -= value;
            }
        }

        /// <summary>
        /// 提子默认处理方法
        /// </summary>
        /// <param name="e"></param>
        protected void OnRemoved(StoneEventArgs e)
        {
            if (_StoneRemoved != null)
            {
                //e.TheStone.DropTime = DateTime.Now;

                _StoneRemoved(this, e);
            }
        }

        #endregion//end 成员变量及属性

        /// <summary>
        /// 获得落子后提掉的块
        /// </summary>
        /// <param name="stone"></param>
        /// <returns>要提掉的块集合</returns>
        private List<Block> GetDeadBlocks(Stone stone)
        {
            List<Block> blocks = new List<Block>() ;

            //首先找到落子周边对方的棋子，每个棋子生成一个块
            List<Stone> stones = GameBoard.FindNearbyStones(stone).Where(s => s.Side != stone.Side).ToList();
            foreach (Stone s in stones)
            {
                Block block = GameBoard.GetBlock(s);
                blocks.Add(block);
            }

            //把可以合并的块合并在一起
            if (blocks.Count > 1)
            {
                int i = 1;
                while (i < blocks.Count - 1 && blocks.Count > 1)
                {
                    Stone s = blocks[0].Stones.Where(s0 => blocks[i].CanStoneJoin(s0)).FirstOrDefault();
                    if (s != null)
                    {
                        foreach (Stone s1 in blocks[i].Stones)
                        {
                            if (!blocks[0].Stones.Contains(s1))
                            {
                                blocks[0].Stones.Add(s1);
                            }
                        }
                        blocks.Remove(blocks[i]);
                        if (blocks.Count == 1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            
            //判断逻辑是不是本方棋块并且只剩一气，气的位置和落子相同。
            return blocks.Where(b => b.Side != stone.Side 
                && b.GetOutsideLiberties().Count == 1 
                && b.GetOutsideLiberties()[0].X == stone.X 
                && b.GetOutsideLiberties()[0].Y == stone.Y).ToList();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public GoGameBase()
        {
            InitMembers();
        }

        /// <summary>
        /// 初始化成员
        /// </summary>
        void InitMembers()
        {
            Moves.Clear();
            _DeadStones = new List<Stone>();
            GameBoard = new Board();
            _GameInfo = new GoGameInfo();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            InitMembers();
        }

        /// <summary>
        /// 移除棋子
        /// </summary>
        /// <param name="stone"></param>
        public void RemoveStone(Stone stone)
        {
            if (Moves.Contains(stone))
            {
                StoneCancelEventArgs e = new StoneCancelEventArgs(stone);
                this.OnStoneRemoving(e);
                if (!e.Cancel)
                {
                    Moves.Remove(stone);
                    GameBoard.RemoveStone(stone.X, stone.Y);
                    this.OnRemoved(new StoneEventArgs(stone));
                }
            }
        }

        #region 实现父类的抽象方法

        /// <summary>
        /// 落子
        /// </summary>
        /// <param name="stone"></param>
        protected override List<Stone> DoDropStone(Stone stone)
        {
            List<Stone> result = new List<Stone>();

            //判断提子
            List<Block> deadBlocks = GetDeadBlocks(stone);
            //提子
            foreach (Block deadBlock in deadBlocks)
            {
                foreach (Stone s in deadBlock.Stones)
                {
                    RemoveStone(s);
                    result.Add(s);
                    //_LastDeadStonePosition = GameBoard.GetPosition(s.X, s.Y);
                }
            }
            return result;
        }//end method


        /// <summary>
        /// 是否可以落子
        /// </summary>
        /// <param name="stone"></param>
        /// <returns></returns>
        public override bool CanDropStone(Stone stone)
        {
            //return true;
            //棋盘默认判断，子不能重叠。
            bool result = GameBoard.CanDropStone(stone);

            //判断是否是打劫
            //判断逻辑是：落子后要提的是一个子并且是最后一步的落子并且落子后处于被打吃状态
            List<Block> deadBlocks = GetDeadBlocks(stone);
            bool noLiberty =  GameBoard.GetSpaces(stone).Count == 0;
            Block ownBlock = GameBoard.GetBlock(stone);
            if (ownBlock.Stones.Count == 1)
            {
                foreach (Block deadBlock in deadBlocks)
                {
                    //newBlock.Parent = _GameBoard;
                    if (deadBlock != null && deadBlock.Stones.Count == 1
                        && deadBlock.Stones[0].Step == Moves.Count - 1
                        && noLiberty)
                    {
                        return false;
                    }
                }
            }
            

            //判断是不是自杀
            //判断逻辑是：如果落子后不能提子并且自己是死子
            //Block ownBlock = _GameBoard.GetBlock(stone) ;
            List<Board.Position> liberties = ownBlock.GetOutsideLiberties();

            if (deadBlocks.Count == 0 && liberties.Count == 1 
                && liberties[0].X == stone.X && liberties[0].Y == stone.Y 
                ||deadBlocks.Count == 0 && !ownBlock.HasLiberty(stone))
            {
                return false;
            }
            return result;
        }//end method

        /// <summary>
        /// 获取下一步的对局方
        /// </summary>
        /// <returns></returns>
        public override Stone.Sides GetNextSide()
        {
            return Moves.Count % 2 == 0 ? Stone.Sides.Black : Stone.Sides.White;
        }

        #endregion 实现父类的抽象方法

    }//end class

    /// <summary>
    /// 围棋游戏信息
    /// </summary>
    public class GoGameInfo
    {
        /// <summary>
        /// 黑方棋手
        /// </summary>
        private GoPlayer _BlackPlayer = new GoPlayer();

        /// <summary>
        /// 设置或获得黑方棋手
        /// </summary>
        public GoPlayer BlackPlayer
        {
            get { return _BlackPlayer; }
            set { _BlackPlayer = value; }
        }
        /// <summary>
        /// 白方棋手
        /// </summary>
        private GoPlayer _WhitePlayer = new GoPlayer();

        /// <summary>
        /// 设置或获得白方棋手
        /// </summary>
        public GoPlayer WhitePlayer
        {
            get { return _WhitePlayer; }
            set { _WhitePlayer = value; }
        }
        /// <summary>
        /// 对局时间
        /// </summary>
        private MatchTime _TimeLimit;

        /// <summary>
        /// 设置或获得对局时间
        /// </summary>
        public MatchTime TimeLimit
        {
            get { return _TimeLimit; }
            set { _TimeLimit = value; }
        }

        /// <summary>
        /// 贴目
        /// </summary>
        private float _Komi;

        /// <summary>
        /// 设置或获得贴目(SGF Code:KM)
        /// </summary>
        public float Komi
        {
            get { return _Komi; }
            set { _Komi = value; }
        }
        /// <summary>
        /// 让子
        /// </summary>
        private int _Handicap;

        /// <summary>
        /// 设置或获得让子数目(SGF Code:HA)
        /// </summary>
        public int Handicap
        {
            get { return _Handicap; }
            set { _Handicap = value; }
        }

        /// <summary>
        /// 比赛进行的时间
        /// </summary>
        /// <remarks>
        /// 强制使用以下格式：
        /// 使用 ISO 标准格式 "YYYY-MM-DD" 不要使用：
        /// 其它的分隔符诸如 "/" 或 " " 或 "."。 
        /// 特别情况一：允许使用部分日期 （1）"YYYY" – 比赛时间为YYYY年 
        /// （2）"YYYY-MM" – 比赛时间为YYYY年MM月 
        /// 特别情况二：比赛持续多天进行，允许使用逗号（没有空格！）
        /// 分隔列出其它天数 如下列简写方式： （1）"MM-DD" – 
        /// 前面是 YYYY-MM-DD、YYYY-MM、MM-DD、 MM 或 DD 
        /// （2）"MM" – 前面是 YYYY-MM 或 MM （3）"DD" – 
        /// 前面是 YYYY-MM-DD、MM-DD 或 DD 
        /// 示例：（1）1997-03-05 = 比赛在1997年3月5日进行 
        /// （2）1996-05,06 = 比赛在1996年5、6月进行 
        /// （3）1996-05-06,07,08 = 比赛在1996年5月6、7、8日进行 
        /// （4）1996,1997 = 比赛在1996年、1997年进行 
        /// （5）1996-12-27,28,1997-01-03,04 = 比赛在1996年的12月27、28日和1997年的1月3、4日进行 
        /// （6）1997年5月5、6日记为：1997-05-05,06而不是1997-05-05,1997-05-0
        /// </remarks>
        private string _Date;

        /// <summary>
        /// 设置或获得棋局时间(SGF Code:DT)
        /// </summary>
        /// <remarks>
        /// 强制使用以下格式：
        /// 使用 ISO 标准格式 "YYYY-MM-DD" 不要使用：
        /// 其它的分隔符诸如 "/" 或 " " 或 "."。 
        /// 特别情况一：允许使用部分日期 （1）"YYYY" – 比赛时间为YYYY年 
        /// （2）"YYYY-MM" – 比赛时间为YYYY年MM月 
        /// 特别情况二：比赛持续多天进行，允许使用逗号（没有空格！）
        /// 分隔列出其它天数 如下列简写方式： （1）"MM-DD" – 
        /// 前面是 YYYY-MM-DD、YYYY-MM、MM-DD、 MM 或 DD 
        /// （2）"MM" – 前面是 YYYY-MM 或 MM （3）"DD" – 
        /// 前面是 YYYY-MM-DD、MM-DD 或 DD 
        /// 示例：（1）1997-03-05 = 比赛在1997年3月5日进行 
        /// （2）1996-05,06 = 比赛在1996年5、6月进行 
        /// （3）1996-05-06,07,08 = 比赛在1996年5月6、7、8日进行 
        /// （4）1996,1997 = 比赛在1996年、1997年进行 
        /// （5）1996-12-27,28,1997-01-03,04 = 比赛在1996年的12月27、28日和1997年的1月3、4日进行 
        /// （6）1997年5月5、6日记为：1997-05-05,06而不是1997-05-05,1997-05-0
        /// </remarks>
        public string Date
        {
            get { return _Date; }
            set { _Date = value; }
        }
        /// <summary>
        /// 比赛地点
        /// </summary>
        private string _Place;

        /// <summary>
        /// 设置或获得比赛地点(SGF Code:PC)
        /// </summary>
        public string Place
        {
            get { return _Place; }
            set { _Place = value; }
        }
        /// <summary>
        /// 比赛规则
        /// </summary>
        private string _Rule;

        /// <summary>
        /// 设置或获得比赛规则(SGF Code:RU)
        /// </summary>
        public string Rule
        {
            get { return _Rule; }
            set { _Rule = value; }
        }
        /// <summary>
        /// 读秒方式
        /// </summary>
        private string _OverTime;

        /// <summary>
        /// 设置或获得读秒规则(SGF Code:OT)
        /// </summary>
        public string OverTime
        {
            get { return _OverTime; }
            set { _OverTime = value; }
        }
        /// <summary>
        /// 棋局名称
        /// </summary>
        private string _Name;

        /// <summary>
        /// 设置或获得棋谱名称(SGF Code:GN)
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Comment;

        /// <summary>
        /// 设置或获得棋局介绍(SGF Code:C)
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; }
        }
        /// <summary>
        /// 赛事
        /// </summary>
        private string _Event;

        /// <summary>
        /// 设置或获得赛事名称(SGF Code:EV)
        /// </summary>
        public string Event
        {
            get { return _Event; }
            set { _Event = value; }
        }
        /// <summary>
        /// 比赛的回合数
        /// </summary>
        private string _Round;

        /// <summary>
        /// 设置或获得赛事回合(SGF Code:RO)
        /// </summary>
        public string Round
        {
            get { return _Round; }
            set { _Round = value; }
        }

        /// <summary>
        /// 棋局结果
        /// </summary>
        private GameResult _Result;

        /// <summary>
        /// 设置或获得棋局结果
        /// </summary>
        public GameResult Result
        {
            get { return _Result; }
            set { _Result = value; }
        }

        /// <summary>
        /// 棋局的默认中文名称
        /// </summary>
        public string DefaultChineseName
        {
            get
            {
                if (string.IsNullOrEmpty(_Name))
                {
                    if (!string.IsNullOrEmpty(_BlackPlayer.Name)
                        && !string.IsNullOrEmpty(_WhitePlayer.Name))
                    {
                        return string.Format("{0} vs {1}"
                            , _BlackPlayer.ToChineseString()
                            , _WhitePlayer.ToChineseString());
                    }

                }
                return _Name;
            }
        }

    }//end class

            /// <summary>
        /// 棋局结果类
        /// </summary>
        public class GameResult
        {
            /// <summary>
            /// 棋局结果类型
            /// </summary>
            GameResultType _Type;

            public GameResultType Type
            {
                get { return _Type; }
                set { _Type = value; }
            }

            /// <summary>
            /// 获胜者
            /// </summary>
            Stone.Sides _Winner;

            public Stone.Sides Winner
            {
                get { return _Winner; }
                set { _Winner = value; }
            }

            /// <summary>
            /// 获胜数量
            /// </summary>
            float _Score;

            public float Score
            {
                get { return _Score; }
                set { _Score = value; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public GameResult()
            {

            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="type">棋局结果类型</param>
            /// <param name="winner">获胜者</param>
            public GameResult(GameResultType type, Stone.Sides winner)
            {
                _Type = type;
                _Winner = winner;

            }//end method

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="type">棋局结果类型</param>
            /// <param name="winner">获胜者</param>
            /// <param name="winCount">获胜的数目</param>
            public GameResult(GameResultType type, Stone.Sides winner , float winCount)
            {
                _Type = type;
                _Winner = winner;
                _Score = winCount;
            }//end method

            /// <summary>
            /// 转换为中文字符串
            /// </summary>
            /// <returns></returns>
            public string ChineseString
            {
                get
                {
                    return string.Format("{0}{1}", _Winner == Stone.Sides.Black
                        ? "黑" : "白"
                        , _Type == GameResultType.Resign ? "中盘胜" : "胜" + _Score.ToString() + "目");
                }
            }

            /// <summary>
            /// 转换为SGF字符串
            /// </summary>
            /// <returns></returns>
            public string ToSGFString()
            {
                if (_Type == GameResultType.Draw)
                {
                    return "0";
                }
                else if (_Type == GameResultType.Normal)
                {
                    return string.Format("{0}+{1}", _Winner == Stone.Sides.Black ? "B" : "W", _Score);
                }
                else
                {
                    return string.Format("{0}+R", _Winner == Stone.Sides.Black ? "B" : "W");
                }

            }

        }//end class

        /// <summary>
        /// 棋局结果类型枚举
        /// </summary>
        public enum GameResultType
        {
            /// <summary>
            /// 普通数目胜负
            /// </summary>
            Normal , 

            /// <summary>
            /// 中盘胜
            /// </summary>
            Resign ,

            /// <summary>
            /// 和棋
            /// </summary>
            Draw 
        }



    #region 棋局时间设定结构

    /// <summary>
    /// 棋局时间设定
    /// </summary>
    public struct MatchTime
    {
        /// <summary>
        /// 正常对局时间的秒数，单位秒
        /// </summary>
        private int _NormalSeconds;

        /// <summary>
        /// 设置或获得正常对局时间的秒数，单位秒
        /// </summary>
        public int NormalSeconds
        {
            get { return _NormalSeconds; }
            set { _NormalSeconds = value; }
        }
        /// <summary>
        /// 读秒时间，单位秒
        /// </summary>
        private int _LimitSeconds;

        /// <summary>
        /// 读秒时间，单位秒
        /// </summary>
        public int LimitSeconds
        {
            get { return _LimitSeconds; }
            set { _LimitSeconds = value; }
        }
        /// <summary>
        /// 重复读秒的步数
        /// </summary>
        private int _LimitSteps;

        /// <summary>
        /// 设置或获得重复读秒的步数
        /// </summary>
        public int LimitSteps
        {
            get { return _LimitSteps; }
            set { _LimitSteps = value; }
        }
        /// <summary>
        /// 保留时间，单位秒
        /// </summary>
        private int _KeepTimeSeconds;

        /// <summary>
        /// 设置或获得保留时间，单位秒
        /// </summary>
        public int KeepTimeSeconds
        {
            get { return _KeepTimeSeconds; }
            set { _KeepTimeSeconds = value; }
        }
        /// <summary>
        /// 保留时间的次数
        /// </summary>
        private int _KeepTimes;

        /// <summary>
        /// 设置或获得保留时间的次数
        /// </summary>
        public int KeepTimes
        {
            get { return _KeepTimes; }
            set { _KeepTimes = value; }
        }

        /// <param name="normalSeconds">正常对局秒数</param>
        /// <param name="LimitSeconds">对秒时间</param>
        /// <param name="LimitSteps">读秒手术</param>
        /// <param name="keepTimeSeconds">保留时间的秒数</param>
        /// <param name="KeepTimes">保留时间的次数</param>
        public MatchTime(int normalSeconds , int limitSeconds, int limitSteps, int keepTimeSeconds = 0, int KeepTimes = 0)
        {
            _NormalSeconds = normalSeconds;
            _LimitSeconds = limitSeconds;
            _LimitSteps = limitSeconds;
            _KeepTimeSeconds = keepTimeSeconds;
            _KeepTimes = KeepTimes;
        }//end method
    }//end struct

    #endregion//end 棋局时间结构

    

}//end namespace
