//--------------------------------------------------------------------------
//
//  File:       SGFKiFUGameSerialize.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/22
//
//  Description: SGF格式棋谱游戏序列化类
//  
//  History:     2013/2/15 created by bigeagle  
//               2013/2/17 添加棋谱编码属性
//               2012/2/20 修正反序列化时如果根节点直接跟子游戏树
//                         会被丢失的错误。
//               2017/11/7 修改为.net standard类库，修改命名空间
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Bigeagle.BoardGames.Go
{
    /// <summary>
    /// SGF格式棋谱游戏序列化类
    /// </summary>
    public class SGFKiFUSerializer : IKiFUGameSerialize
    {
        /// <summary>
        /// 棋谱编码
        /// </summary>
        Encoding _KiFUEncoding = null;

        /// <summary>
        /// 设置或获得棋谱编码
        /// </summary>
        public Encoding KiFUEncoding
        {
            get { return _KiFUEncoding; }
            set { _KiFUEncoding = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SGFKiFUSerializer()
        {
            _KiFUEncoding = Encoding.UTF8;
        }

        public SGFKiFUSerializer(Encoding encoding)
        {
            _KiFUEncoding = encoding;
        }

        #region 实现IKiFUGameSerialize接口

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public KiFUGame DeSerialize(string content)
        {
            KiFUGame result = new KiFUGame();
            
            SGF sgf = new SGF(content);
            sgf.Read();

            if (sgf.Games.Count > 0)
            {
                ParseGame(result , sgf.Games[0]);

            }
            //SGF.Node rootNode = sgf.RootNode;
            //if (rootNode != null)
            //{
            //    ParseGameInfo(result, rootNode);
            //}

            Debug.WriteLine("共{0}手。", sgf.GetMainGameTreeNodes().Count);
            return result;
        }


        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="game">要序列化的游戏</param>
        /// <returns>sgf格式字符串</returns>
        public string Serialize(KiFUGame game)
        {
            string result = string.Empty;

            //构造SGF
            SGF sgf = new SGF();

            ///主线游戏
            ConvertGameTree(sgf, game,null);



            result = sgf.Write();
            return result;
        }

        /// <summary>
        /// 读取游戏信息
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public GoGameInfo ReadGameInfo(Stream stream)
        {
            GoGameInfo gameInfo = new GoGameInfo();
            SGF sgf = new SGF();
            SGF.Node rootNode = sgf.ReadRootNode(stream, _KiFUEncoding);
            if (rootNode != null && rootNode.Properties.Count > 0)
            {
                KiFUGame game = new KiFUGame();
                ParseGameInfo(game, rootNode);
                return game.GameInfo;
            }
            else
            {
                throw (new ArgumentNullException());
            }
            //return null;
            //this.ParseGameInfo(
        }//end method
        #endregion//end 实现IKiFUGameSerialize接口

        

        #region 私有方法

        private void ConvertGameTree(object parent, KiFUGame game, SGF.Node parentNode)
        {
            SGF.GameTree gameTree = new SGF.GameTree();
            if (parentNode != null)
            {
                parentNode.ReferenceGames.Add(gameTree);
            }
            gameTree.Parent = parent;


            if (parent is SGF) //如果是主线游戏则解析根节点
            {
                (parent as SGF).Games.Add(gameTree);
                //根节点
                SGF.Node rootNode = new SGF.Node();
                rootNode.Parent = gameTree;

                gameTree.Nodes.Add(rootNode);
                //
                //Todo: 添加属性
                //

                //游戏类型，固定为1
                rootNode.Properties.Add(new SGF.Property("GM", "1"));


                //棋谱应用程序
                if (!string.IsNullOrEmpty(game.AppName))
                {
                    rootNode.Properties.Add(new SGF.Property("AP", game.AppName));
                }

                //讲解者

                if (!string.IsNullOrEmpty(game.Annotation))
                {
                    rootNode.Properties.Add(new SGF.Property("AN", game.Annotation));
                }

                //黑方棋手
                if (!string.IsNullOrEmpty(game.GameInfo.BlackPlayer.Name))
                {
                    //黑方棋手名字
                    rootNode.Properties.Add(new SGF.Property("PB", game.GameInfo.BlackPlayer.Name));

                    //黑方段位
                    if (game.GameInfo.BlackPlayer.Rank.Rank > 0)
                    {
                        rootNode.Properties.Add(new SGF.Property("BR", game.GameInfo.BlackPlayer.Rank.ToString()));
                    }
                    if (!string.IsNullOrEmpty(game.BlackTeam))
                    {
                        rootNode.Properties.Add(new SGF.Property("BT", game.BlackTeam));

                    }
                }

                //白方棋手
                if (!string.IsNullOrEmpty(game.GameInfo.WhitePlayer.Name))
                {
                    //黑方棋手名字
                    rootNode.Properties.Add(new SGF.Property("PW", game.GameInfo.WhitePlayer.Name));

                    //黑方段位
                    if (game.GameInfo.WhitePlayer.Rank.Rank > 0)
                    {
                        rootNode.Properties.Add(new SGF.Property("WR", game.GameInfo.WhitePlayer.Rank.ToString()));
                    }
                    if (!string.IsNullOrEmpty(game.BlackTeam))
                    {
                        rootNode.Properties.Add(new SGF.Property("WT", game.WhiteTeam));

                    }
                }

                //比赛结果
                if (game.GameInfo.Result != null)
                {
                    rootNode.Properties.Add(new SGF.Property("RE", game.GameInfo.Result.ToSGFString()));
                }
                //棋盘大小
                rootNode.Properties.Add(new SGF.Property("SZ", game.BoardSize.ToString()));

                //棋局注释
                if (!string.IsNullOrEmpty(game.GameInfo.Comment))
                {
                    rootNode.Properties.Add(new SGF.Property("GC", game.GameInfo.Comment));
                }

                //棋谱版权
                if (!string.IsNullOrEmpty(game.Copyright))
                {
                    rootNode.Properties.Add(new SGF.Property("CP", game.Copyright));
                }

                //比赛日期
                if (!string.IsNullOrEmpty(game.GameInfo.Date))
                {
                    rootNode.Properties.Add(new SGF.Property("DT", game.GameInfo.Date));
                }

                //赛事
                if (!string.IsNullOrEmpty(game.GameInfo.Event))
                {
                    rootNode.Properties.Add(new SGF.Property("EV", game.GameInfo.Event));
                }

                //让子数
                if (game.GameInfo.Handicap > 0)
                {
                    rootNode.Properties.Add(new SGF.Property("HA", game.GameInfo.Handicap.ToString()));
                }

                //贴目数
                if (game.GameInfo.Komi > 0)
                {
                    rootNode.Properties.Add(new SGF.Property("KM", game.GameInfo.Komi.ToString()));

                }

                //棋谱名称
                if (!string.IsNullOrEmpty(game.GameInfo.Name))
                {
                    rootNode.Properties.Add(new SGF.Property("GN", game.GameInfo.ToString()));
                }

                //比赛时间限制
                if (!string.IsNullOrEmpty(game.GameInfo.OverTime))
                {
                    rootNode.Properties.Add(new SGF.Property("OT", game.GameInfo.OverTime));
                }

                //比赛地点
                if (!string.IsNullOrEmpty(game.GameInfo.Place))
                {
                    rootNode.Properties.Add(new SGF.Property("PC", game.GameInfo.Place));
                }

                //番棋回合
                if (!string.IsNullOrEmpty(game.GameInfo.Round))
                {
                    rootNode.Properties.Add(new SGF.Property("RO", game.GameInfo.Round));
                }

                //比赛规则
                if (!string.IsNullOrEmpty(game.GameInfo.Rule))
                {
                    rootNode.Properties.Add(new SGF.Property("RU", game.GameInfo.Rule));
                }

                //棋谱来源
                if (!string.IsNullOrEmpty(game.Source))
                {
                    rootNode.Properties.Add(new SGF.Property("SO", game.Source));
                }

                //棋谱编写者
                if (!string.IsNullOrEmpty(game.User))
                {
                    rootNode.Properties.Add(new SGF.Property("US", game.User));
                }

                if (game.PreMoves.Count > 0)
                {
                    SGF.Property property = new SGF.Property();
                    property.Ident = string.Format("A{0}", game.PreMoves[0].Side == Stone.Sides.Black ? "B" : "W");
                    foreach (Stone move in game.PreMoves)
                    {
                        property.Values.Add(ConvertStonePosition2Move(move.X, move.Y));
                    }
                    rootNode.Properties.Add(property);
                }
            }
            else
            {
                (parent as SGF.GameTree).GameTrees.Add(gameTree);
            }

            //添加普通节点
            foreach (CommentsMove move in game.Moves)
            {
                SGF.Node node = new SGF.Node();
                node.Parent = gameTree;
                gameTree.Nodes.Add(node);
                //落子名称
                if (!string.IsNullOrEmpty(move.Name))
                {
                    node.Properties.Add(new SGF.Property("N", move.Name));
                }

                //走子
                if (move.IsRealMove)
                {
                    node.Properties.Add(new SGF.Property(move.Side == Stone.Sides.Black ? "B" : "W"
                        , ConvertStonePosition2Move(move.X, move.Y)));
                }

                //标记
                if (move.Labels.Count > 0)
                {
                    SGF.Property property = new SGF.Property();
                    property.Ident = "LB";
                    foreach (KiFUGame.Label label in move.Labels)
                    {
                        property.Values.Add(string.Format("{0}:{1}"
                            , ConvertStonePosition2Move(label.X, label.Y), label.Name));
                    }
                    node.Properties.Add(property);
                }

                //放弃
                if (move.IsGiveUp)
                {
                    node.Properties.Add(new SGF.Property(move.Side == Stone.Sides.Black ? "B" : "W", "tt"));
                }

                //解说
                if (!string.IsNullOrEmpty(move.Comment))
                {
                    node.Properties.Add(new SGF.Property("C", move.Comment));
                }

                //递归生成子GameTree
                if (move.ReferenceGame.Count > 0)
                {
                    foreach (KiFUGame subGame in move.ReferenceGame)
                    {
                        ConvertGameTree(gameTree, subGame, node);
                    }
                }
            }

        }//end method


        /// <summary>
        /// 解析游戏
        /// </summary>
        /// <param name="result"></param>
        /// <param name="gameTree"></param>
        private void ParseGame(KiFUGame game, SGF.GameTree gameTree)
        {
            foreach (SGF.Node node in gameTree.Nodes)
            {
                //如果是根节点
                if (gameTree.Parent is SGF && node == (gameTree.Parent as SGF).RootNode)
                {
                    this.ParseGameInfo(game, node);
                    if (node.ReferenceGames.Count > 0)
                    {
                        CommentsMove move = ParseMove(game, node);
                        game.Moves.Add(move);

                        foreach (SGF.GameTree subGameTree in node.ReferenceGames)
                        {
                            KiFUGame subGame = new KiFUGame();
                            move.ReferenceGame.Add(subGame);
                            game.SubGames.Add(subGame);
                            ParseGame(subGame, subGameTree);
                        }
                    }

                }
                else
                {
                    CommentsMove move = ParseMove(game, node);
                    game.Moves.Add(move);
                    if (node.ReferenceGames.Count > 0)
                    {
                        foreach (SGF.GameTree subGameTree in node.ReferenceGames)
                        {
                            KiFUGame subGame = new KiFUGame();
                            move.ReferenceGame.Add(subGame);
                            game.SubGames.Add(subGame);
                            ParseGame(subGame, subGameTree);
                        }
                    }
                }
            }
        }//end method

        /// <summary>
        /// 从sgf node 解析棋子
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private CommentsMove ParseMove(KiFUGame game, SGF.Node node)
        {
            CommentsMove result = new CommentsMove();
            result.IsAlive = true;
            result.IsRealMove = false;
            result.Assess = CommentsMove.MoveAssess.None;
            result.Tag = node;

            CommentsMove lastMove = game.Moves.LastOrDefault(m => (m as CommentsMove).IsRealMove) as CommentsMove;
            if (lastMove != null)
            {
                result.Step = lastMove.Step + 1;
            }
            else
            {
                result.Step = 1;
            }
            //result.Step = 
            int x, y;
            foreach (SGF.Property p in node.Properties)
            {

                switch (p.Ident)
                {
                    case "B":  //黑走子
                        result.Side = Stone.Sides.Black;
                        //转换坐标
                        if (p.Value == "tt" || string.IsNullOrEmpty(p.Value))
                        {
                            result.IsGiveUp = true;
                        }
                        else
                        {
                            ConvertMove2StonePosition(p.Value, out x, out y);
                            if (x >= 0 && y >= 0) //如果转换的坐标不正确则不作为真正的走子
                            {
                                result.X = x;
                                result.Y = y;
                                result.IsRealMove = true;
                            }
                        }
                        break;
                    case "W":  //黑走子
                        result.Side = Stone.Sides.White;
                        //转换坐标
                        if (p.Value == "tt" || string.IsNullOrEmpty(p.Value))
                        {
                            result.IsGiveUp = true;
                        }
                        else
                        {
                            ConvertMove2StonePosition(p.Value, out x, out y);
                            if (x >= 0 && y >= 0) //如果转换的坐标不正确则不作为真正的走子
                            {
                                result.X = x;
                                result.Y = y;
                                result.IsRealMove = true;
                            }
                        }
                        break;
                    case "N": //名称
                        result.Name = p.Value;
                        break;
                    case "C": //注解
                        result.Comment = p.Value;
                        break;
                    case "MN": //强制设置手数
                        int number = 0;
                        if (int.TryParse(p.Value, out number))
                        {
                            result.ForceMoveNumber = number;
                            result.Step = number;
                        }
                        break;
                    case "TE": //妙手，目前忽略程度值
                        result.Assess = CommentsMove.MoveAssess.GoodMove;
                        break;
                    case "BM": //恶手，目前忽略程度值
                        result.Assess = CommentsMove.MoveAssess.BadMove;
                        break;
                    case "DO": //疑问手
                        result.Assess = CommentsMove.MoveAssess.Doubtful;
                        break;
                    case "IT": //有趣手
                        result.Assess = CommentsMove.MoveAssess.Interesting;
                        break;
                    case "AB": //添加黑子
                        result.PreMoves.AddRange(ParsePreMoves(Stone.Sides.Black, p.Values));
                        break;
                    case "AW": //添加白子
                        result.PreMoves.AddRange(ParsePreMoves(Stone.Sides.White, p.Values));
                        break;
                    case "LB":  //标签，重要
                        result.Labels.AddRange(ParseLabels(p.Values));
                        break;

                }
            }

            return result;
        }

        /// <summary>
        /// 分析棋局基本信息
        /// </summary>
        /// <param name="rootNode"></param>
        private void ParseGameInfo(KiFUGame game, SGF.Node rootNode)
        {
            //断言
            Debug.Assert(rootNode != null && rootNode.Properties.Count > 0
                , "根节点不应该为空并且属性数量应该大于0。");

            foreach (SGF.Property p in rootNode.Properties)
            {
                switch (p.Ident.ToUpper())
                {
                    case "GM": //判断棋谱是不是有效的围棋谱

                        int gameType = 0;
                        if (int.TryParse(p.Value, out gameType))
                        {
                            if (gameType != (int)GameType.Go)
                            {
                                 throw (new InvalidOperationException("Not valid Go SGF file format."));
                            }
                        }
                        break;
                    case "GC": //棋谱备注
                        game.GameInfo.Comment += p.Value;
                        break;
                    case "C": //注释
                        game.GameInfo.Comment += p.Value;
                        break;
                    case "FF": //暂时不实现，都当做ff4处理

                        break;
                    case "SZ": //棋盘大小
                        int boardSize = 19; //默认19
                        if (int.TryParse(p.Value, out boardSize))
                        {
                            game.BoardSize = boardSize;
                        }
                        break;
                    case "AP": //应用程序
                        game.AppName = p.Value;
                        break;
                    case "GN": //棋谱名称
                        game.GameInfo.Name = p.Value;
                        break;
                    case "US": //棋谱编写者
                        game.User = p.Value;
                        break;
                    case "SO": //棋谱来源
                        game.Source = p.Value;
                        break;
                    case "CP": //棋谱版权
                        game.Copyright = p.Value;
                        break;
                    case "AN": //注解者
                        game.Annotation = p.Value;
                        break;
                    case "EV": //赛事
                        game.GameInfo.Event = p.Value;
                        break;
                    case "RO": //回合，用于番棋
                        game.GameInfo.Round = p.Value;
                        break;
                    case "DT": //比赛日期
                        game.GameInfo.Date = p.Value;
                        break;
                    case "PC": //地点
                        game.GameInfo.Place = p.Value;
                        break;
                    case "RU": //比赛规则，暂时只是用于显示，后续用于比赛结果计算
                        game.GameInfo.Rule = p.Value;
                        break;
                    case "TM": //棋局时间，如果是标准秒数表示则赋给TimeLimit的NormalTime，否则忽略
                        int seconds = 0;
                        if (int.TryParse(p.Value, out seconds))
                        {
                            game.GameInfo.TimeLimit = new MatchTime(seconds, 0, 0);
                        }
                        break;
                    case "OT": //读秒方式，目前只用于显示，将来会赋给读秒时间
                        game.GameInfo.OverTime = p.Value;
                        break;
                    case "PB": //黑方棋手姓名
                        game.GameInfo.BlackPlayer.Name = p.Value;
                        break;
                    case "BR": //黑方段位，
                        game.GameInfo.BlackPlayer.Rank = ParsePlayerRank(p.Value);
                        break;

                    case "PW": //白方棋手姓名
                        game.GameInfo.WhitePlayer.Name = p.Value;
                        break;
                    case "WR": //白方段位，
                        game.GameInfo.WhitePlayer.Rank = ParsePlayerRank(p.Value);
                        break;
                    case "BT": //黑方队伍
                        game.WhiteTeam = p.Value;
                        break;
                    case "WT": //白方队伍
                        game.WhiteTeam = p.Value;
                        break;
                    case "KM": //贴目
                        float komi = 0;
                        if (float.TryParse(p.Value, out komi))
                        {
                            game.GameInfo.Komi = komi;
                        }
                        break;

                    case "HA": //让子
                        int ha = 0;
                        if (int.TryParse(p.Value, out ha))
                        {
                            game.GameInfo.Handicap = ha;
                        }
                        break;
                    case "RE": //比赛结果
                        game.GameInfo.Result = ParseGameResult(p.Value);
                        break;
                    case "AB": //添加黑子
                        game.PreMoves.AddRange(ParsePreMoves(Stone.Sides.Black, p.Values));
                        break;
                    case "AW":  //添加白子
                        game.PreMoves.AddRange(ParsePreMoves(Stone.Sides.White, p.Values));
                        break;
                }
            }
        }//end method

        /// <summary>
        /// 解析段位
        /// </summary>
        /// <param name="p">段位字符串</param>
        /// <returns>段位结构</returns>
        /// <remarks>
        /// 解析比较复杂，目前默认提供两种解析，一种是标准写法(9d)，另外是国内棋谱写法(九段)
        /// </remarks>
        private GoPlayer.GoPlayerRank ParsePlayerRank(string rankString)
        {
            GoPlayer.GoPlayerRank rank = new GoPlayer.GoPlayerRank();

            //首先把可能的汉语格式转换成中文格式
            rankString = rankString.Replace("段", "d").Replace("级", "k");
            //string chars = "十一二三四五六七八九";
            //for (int i = 0; i < chars.Length; i++)
            //{
            //    rankString = rankString.Replace(chars[i].ToString(), i.ToString());
            //}
            //解析标准格式，如9d或9 dan , 13k 或 13 kyu
            Regex regex = new Regex("(?<number>.+)\\s*(?<type>[k|kyu|d|dan|p]+)");
            Match m = regex.Match(rankString);
            if (m.Success)
            {

                ///处理有零的情况
                string numberStr = m.Groups["number"].Value;

                int number = ConvertChineseNumber(numberStr);

                if (number > -1)
                {
                    rank.Rank = number;
                }
                string rankType = m.Groups["type"].Value.ToLower();
                switch (rankType)
                {
                    case "d":
                        rank.Type = GoPlayer.RankType.Dan;
                        break;
                    case "dan":
                        rank.Type = GoPlayer.RankType.Dan;
                        break;
                    case "p":
                        rank.Type = GoPlayer.RankType.ProfessionalDan;
                        break;
                    case "k":
                        rank.Type = GoPlayer.RankType.Kyu;
                        break;
                    case "kyu":
                        rank.Type = GoPlayer.RankType.Kyu;
                        break;
                }

                //return rank;
            }

            return rank;
        }//end method

        /// <summary>
        /// 转换中文数字到数字
        /// </summary>
        /// <param name="number"></param>
        /// <returns>解析成功返回数字，否则返回-1</returns>
        /// <remarks>只支持到10位，本程序目前够用，以后有时间做个完整版本。</remarks>
        private int ConvertChineseNumber(string number)
        {
            int result = 0;
            string chineseChar = "一二三四五六七八九十";
            string numberChar = "1234567890";
            for (int i = 0; i < chineseChar.Length; i++)
            {
                number = number.Replace(chineseChar[i], numberChar[i]);
            }


            if (number.IndexOf("0") == 0)
            {
                number = number.Replace("0", number.Length == 1 ? "10" : "1");
            }
            else if (number.IndexOf("0") > 0)
            {
                string[] temp = number.Split('0');
                number = temp[0] + temp[1];
            }

            if (int.TryParse(number, out result))
            {

                return result;
            }
            else
            {
                return -1;
            }
        }//end method

        /// <summary>
        /// 解析比赛结果
        /// </summary>
        /// <param name="resultString">比赛结果字符串</param>
        /// <returns>比赛结果结构</returns>
        /// <remarks>解析标准写法，如B+score，也可解析中文写法如白胜半目</remarks>
        private GameResult ParseGameResult(string resultString)
        {
            GameResult result = new GameResult();

            //按照标准写法解析
            Regex regex = new Regex("(?<winner>[BW])[+](?<score>.+)");
            Match m = regex.Match(resultString);
            if (m.Success)   //如果符合标准写法
            {
                string winner = m.Groups["winner"].Value;
                result.Winner = winner.ToLower() == "b" ? Stone.Sides.Black : Stone.Sides.White;

                string score = m.Groups["score"].Value;
                float s = 0;
                if (score.ToLower() == "resign" || score.ToLower() == "r")
                {
                    result.Type = GameResultType.Resign;
                }
                else if (float.TryParse(score, out s))
                {
                    result.Type = GameResultType.Normal;
                    result.Score = s;
                }
            }
            else  //否则按照中文写法解析
            {
                //首先解析中盘胜的情况
                regex = new Regex("(?<winner>[白黑])中盘胜");
                m = regex.Match(resultString);
                if (m.Success)
                {
                    result.Type = GameResultType.Resign;
                    result.Winner = m.Groups["winner"].Value.Trim() == "黑" 
                        ? Stone.Sides.Black : Stone.Sides.White;
                }
                else //分析目数胜负的情况
                {
                    float rank = 0f;
                    regex = new Regex("(?<winner>[白黑])胜(?<front>[^目]+)目(?<behind>.*)");
                    m = regex.Match(resultString);
                    if (m.Success)
                    {
                        result.Winner = m.Groups["winner"].Value.Trim() == "黑"
                            ? Stone.Sides.Black : Stone.Sides.White;
                        result.Type = GameResultType.Normal;
                        string front = m.Groups["front"].Value.Trim();
                        if (front == "半")
                        {
                            rank = 0.5f;
                        }
                        else
                        {
                            int number = ConvertChineseNumber(front);
                            if (number != -1)
                            {
                                rank += number;
                                if (!string.IsNullOrEmpty(m.Groups["behind"].Value))
                                {
                                    rank += 0.5f;
                                }
                            }

                        }
                        result.Score = rank;
                    }
                    else
                    {
                        regex = new Regex("(?<winner>[白黑])半目胜");
                        m = regex.Match(resultString);
                        if (m.Success)
                        {
                            result.Winner = m.Groups["winner"].Value.Trim() == "黑"
    ? Stone.Sides.Black : Stone.Sides.White;
                            result.Type = GameResultType.Normal;
                            result.Score = 0.5f;
                        }
                        
                    }
                }
            }

            return result;
        }//end method
        /// <summary>
        /// 解析预置棋子
        /// </summary>
        /// <param name="sides"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<Stone> ParsePreMoves(Stone.Sides side, List<string> moves)
        {
            List<Stone> result = new List<Stone>();
            foreach (string s in moves)
            {
                Stone stone = new Stone();
                stone.Side = side;
                int x, y;
                ConvertMove2StonePosition(s, out x, out y);
                stone.X = x;
                stone.Y = y;
                result.Add(stone);
            }
            return result;
        }//end method

        /// <summary>
        /// 转换走子字符串到棋子的纵横坐标
        /// </summary>
        /// <param name="move">走子字符串，SGF中的B/W的值</param>
        /// <param name="x">棋子的横坐标</param>
        /// <param name="y">棋子的纵坐标</param>
        private void ConvertMove2StonePosition(string move, out int x, out int y)
        {
            if (!string.IsNullOrEmpty(move) && move.Length == 2
                && move[0] >= 'a' && move[0] <= 't'
                && move[1] >= 'a' && move[1] <= 't')
            {
                x = move[0] - 'a';
                y = move[1] - 'a';
            }
            else
            {
                throw (new ArgumentException());
            }
        }//end method

        /// <summary>
        /// 转换棋子位置到SGF位置标记
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private string ConvertStonePosition2Move(int x, int y)
        {
            return string.Format("{0}{1}", (char)('a' + x), (char)('a' + y));
        }

        /// <summary>
        /// 解析标签
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<KiFUGame.Label> ParseLabels(List<string> labels)
        {
            List<KiFUGame.Label> result = new List<KiFUGame.Label>();
            for (int i = 0; i < labels.Count; i++)
            {
                KiFUGame.Label label = new KiFUGame.Label();
                string[] temp = labels[i].Split(':');
                if (temp.Length == 2)
                {
                    label.Name = temp[1];
                    if (!string.IsNullOrEmpty(temp[0]))
                    {
                        int x, y;
                        ConvertMove2StonePosition(temp[0], out x, out y);
                        if (x >= 0 && y >= 0)
                        {
                            label.X = x;
                            label.Y = y;
                            result.Add(label);
                        }
                    }
                }
            }
            return result;
        }//end method

        #endregion//end 私有方法

    }//end class
}//end namespace
