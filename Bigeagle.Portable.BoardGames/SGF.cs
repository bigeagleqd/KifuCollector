//--------------------------------------------------------------------------
//
//  File:        SGF.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/28
//
//  Description: SGF类，通用的棋类棋谱记录格式解析类
//  
//  History:     2013/1/28 created by bigeagle  
//               2013/1/30 舍弃以前的作法，不使用递归已避免大文件时堆栈溢出
//               2013/2/5  添加Node类的NextNode和PrevNode属性用于寻址
//               2013/2/12 修改为可移植类库，修改命名空间
//               2013/2/15 修正解析中\]转义及括号的错误。
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Bigeagle.Portable.BoardGames
{
    /// <summary>
    /// SGF类
    /// </summary>
    public class SGF
    {
        #region 成员变量及属性

        /// <summary>
        /// 唯一索引
        /// </summary>
        int _UniqueIndex = -1;

        /// <summary>
        /// SGF棋谱内容
        /// </summary>
        string _Content = string.Empty;

        /// <summary>
        /// 设置或获得SGF棋谱内容
        /// </summary>
        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
            }
        }

        /// <summary>
        /// 游戏集合类
        /// </summary>
        /// <returns></returns>
        List<GameTree> _Games;

        /// <summary>
        /// 获得游戏集合类
        /// </summary>
        public List<GameTree> Games
        {
            get
            {
                return _Games;
            }
        }

        /// <summary>
        /// 获得根节点
        /// </summary>
        public Node RootNode
        {
            get
            {
                if (_Games.Count > 0 && _Games[0].Nodes.Count > 0)
                {
                    return _Games[0].Nodes[0];
                }

                return null;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SGF()
        {
            _Games = new List<GameTree>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="content"></param>
        public SGF(string content)
        {
            _Games = new List<GameTree>();
            _Content = content;
            //this.ParseGames(content);
            //Console.WriteLine("number:{0}" , content.Count(c => c == ')' )) ;
        }

        #endregion//end 构造函数

        #region 公共方法

        public Node ReadRootNode(Stream stream, Encoding encoding)
        {
            Node rootNode = new Node();
            StringBuilder builder = new StringBuilder();
            bool beginRead = false;
            List<byte> bytes = new List<byte>() ;
            int b = int.MaxValue;
            while (b > 0)
            {
                b = stream.ReadByte();
                char ch = (char)b;
                if (ch == ';'  && !beginRead)                 
                {
                    beginRead = true;
                }

                else if (beginRead && (ch == ';' || ch == '('))
                {
                    break;
                }
                if (beginRead)
                {
                    bytes.Add((byte)b);
                }

            }
            string rootString = encoding.GetString(bytes.ToArray(), 0, bytes.Count);
            if (!string.IsNullOrEmpty(rootString))
            {
                rootNode.Content = rootString;
                ParseProperty(rootNode);
            }
            return rootNode;
        }

        /// <summary>
        /// 读取
        /// </summary>
        public void Read()
        {
            if (!string.IsNullOrEmpty(_Content))  //如果内容不为空可读取，否则抛出异常
            {
                _Games.Clear();
                this.ParseGames(_Content);
            }
            else
            {
                throw (new InvalidOperationException());
            }
        }//end method

        /// <summary>
        /// 写入
        /// </summary>
        /// <returns>sgf字符串</returns>
        public string Write()
        {
            StringBuilder result = new StringBuilder(string.Empty);
            
            //写入主游戏树
            GameTree gameTree = _Games[0];
            WriteGameTree(result , gameTree);
            return result.ToString();
        }

        /// <summary>
        /// 递归写入GameTree
        /// </summary>
        /// <param name="result"></param>
        /// <param name="gameTree"></param>
        private void WriteGameTree(StringBuilder result, GameTree gameTree)
        {
            result.Append("(");     //写入gametree开始标志

            //写入节点
            for (int i = 0; i < gameTree.Nodes.Count; i++)
            {
                Node node = gameTree.Nodes[i];
                result.Append(";");
                foreach (Property property in node.Properties)
                {
                    result.Append(property.Ident);
                    foreach (string value in property.Values)
                    {
                        result.Append(string.Format("[{0}]", SGF.SGFEncode(value)));
                    }
                }

                if (node.ReferenceGames.Count > 0)
                {
                    foreach (GameTree gameTree1 in node.ReferenceGames)
                    {
                        WriteGameTree(result, gameTree1);
                    }
                }
            }
            result.Append(")");

        }

        /// <summary>
        /// 获取主游戏树节点集合
        /// </summary>
        /// <remarks>依然不适用递归避免嵌套太多造成堆栈溢出</remarks>
        public List<Node> GetMainGameTreeNodes()
        {
            List<Node> nodes = new List<Node>();
            if (_Games.Count > 0 && _Games[0].Nodes.Count > 0)
            {
                GameTree gameTree = _Games[0];
                //加入主游戏树的节点集合
                while (true)
                {
                    nodes.AddRange(gameTree.Nodes);
                    ///加入主游戏树的第一个子游戏节点
                    if (gameTree.GameTrees.Count > 0)
                    {
                        gameTree = gameTree.GameTrees[0];
                    }
                    else
                    {
                        break;
                    }
                }
                //移除根节点
                nodes.Remove(_Games[0].Nodes[0]);
            }
            return nodes;
        }//end method

        #endregion//end 公共方法

        #region 私有方法

        /// <summary>
        /// 是否是主线游戏
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool IsMainGameTree(GameTree gameTree)
        {
            if (gameTree.Parent is SGF)
            {
                return true;
            }
            else if(gameTree.Parent is GameTree)
            {
                GameTree parent = gameTree.Parent as GameTree ;
                return parent.GameTrees.IndexOf(gameTree) == 0;
            }

            return false;
        }
        /// <summary>
        /// 解析游戏
        /// </summary>
        /// <param name="content"></param>
        private void ParseGames(string content)
        {
            Debug.Assert(content[0] == '(', "第一个字符应该是左括号。");
            ParseGameTree(this, content, 0);
            ParseNodes(this._Games[0]);

        }//end method

        /// <summary>
        /// 分析节点
        /// </summary>
        /// <param name="gameTree"></param>
        private void ParseNodes(GameTree gameTree)
        {
            string content = gameTree.Content;
            Regex regex = new Regex("[;](?<node>[^;]+)");
            MatchCollection matches = regex.Matches(content);
            int i = 0;
            foreach (Match m in matches)
            {
                if (!string.IsNullOrEmpty(m.Value))
                {
                    Node node = new Node();
                    node.Parent = gameTree;
                    node.Content = m.Value;
                    gameTree.Nodes.Add(node);
                    ParseProperty(node);
                }
                i++;
            }

            //if (gameTree.GameTrees.Count > 0)
            //{
            //    foreach (GameTree game in gameTree.GameTrees)
            //    {
            //        ParseNodes(game);  
            //    }
            //}
        }

        /// <summary>
        /// 解析节点的属性
        /// </summary>
        /// <param name="node">要解析的节点</param>
        /// <remarks>如果一个属性有多个值，先把除第一个值以外的其他值作为属性解析，然后再合并</remarks>
        private void ParseProperty(Node node)
        {
            //断言，确保节点和内容不为空
            Debug.Assert(node != null && !string.IsNullOrEmpty(node.Content), "节点对象不应该为空并且内容不为空。");


            //这段正则有问题，不能略过转义的\[，所以暂时用替换的方式来解决，以后有时间研究一下
            Regex regex = new Regex("[;]?(?<Ident>[^[]*)[[](?<value>[^]]+)[]]");
            string temp = node.Content.Replace("\\]" , "quote;") ;
            MatchCollection matches = regex.Matches(temp);
            foreach (Match m in matches)
            {
                if (m.Success)
                {
                    string ident = Regex.Replace(m.Groups["Ident"].Value , "\\s" , "");
                    //如果属性标识不为空，添加新属性
                    if (!string.IsNullOrEmpty(ident))
                    {
                        string value = m.Groups["value"].Value.Replace("quote;", "\\]");
                        Property p = new Property(ident, SGF.SGFDecode(value));
                        p.Parent = node;
                        node.Properties.Add(p);
                    }
                    else //否则该属性为上一个属性的值
                    {
                        Property lastProperty = node.Properties.Last();
                        if (lastProperty != null)
                        {
                            lastProperty.Values.Add( SGF.SGFDecode(m.Groups["value"].Value));
                        }
                    }
                }
            }
        }//end method

        /// <summary>
        /// 解析游戏树
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="content"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int ParseGameTree(object parent, string content, int pos)
        {
            Debug.Assert(content[pos] == '(', "第一个字符应该是左括号。");
            //if (pos >= content.Length - 1)
            //{
            //    return pos + 1;
            //}
            //int i = pos;
            GameTree gameTree = new GameTree();
            gameTree.Parent = parent;
            _UniqueIndex++;
            gameTree.UniqueIndex = _UniqueIndex;
                _Games.Add(gameTree);
                //gameTree.EndIndex = content.Length - 1;
                //gameTree.Content = content;
            //else
            //{
            //    GameTree parentGame = parent as GameTree;

            //    parentGame.GameTrees.Add(gameTree);
            //    if (parentGame.Nodes.Count > 0)
            //    {
            //        parentGame.Nodes.Last().ReferenceGame = gameTree;
            //    }
            //}
            int i = pos + 1;
            while (true)
            {
                char ch = content[i];
                switch (ch)
                {
                    case '[':   //重点处理‘[’号，因为其中可能包含转义字符或者左右括号
                        //首先查找对应的']'，注意要略过转义的\]
                        int index = 0;
                        int j = i;
                        while (true)
                        {
                            index = content.IndexOf(']', j);
                            if (content[index - 1] == '\\')
                            {
                                j = index + 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        string str = content.Substring(i, index - i + 1);

                        gameTree.Content += str;
                        i += str.Length;
                        break;
                    case '(': //创建新game

                        ParseNodes(gameTree);
                        //if (gameTree.Nodes.Count == 0)
                        //{
                        //    Console.WriteLine("wrong");
                        //}
                        //i += gameTree.Content.Length;
                        gameTree.Content = string.Empty;
                        //i = this.ParseGameTree(gameTree, content, i);
                        GameTree gameTree1 = new GameTree();
                        gameTree1.Parent = gameTree;
                        _UniqueIndex++;
                        gameTree1.UniqueIndex = _UniqueIndex;
                        gameTree.GameTrees.Add(gameTree1);
                        if (gameTree.Nodes.Count > 0)
                        {
                            gameTree.Nodes.Last().ReferenceGames.Add(gameTree1);
                        }
                        

                        i++;
                        gameTree = gameTree1;

                        break;
                    case ')':
                        ParseNodes(gameTree);
                        gameTree.Content = string.Empty;
                        if (gameTree.Parent is GameTree)
                        {
                            gameTree = gameTree.Parent as GameTree;
                        }
                        i++;
                        break;
                    case '\\':
                        i++;
                        gameTree.Content += content[i];
                        break;
                    default:
                        gameTree.Content += ch;
                        i++;
                        break;
                }

                if (i >= content.Length )
                {
                    break;
                }
            }

            return i;
        }

        #endregion//end 私有方法

        #region 静态方法

        /// <summary>
        /// 编码，转义关键字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string SGFEncode(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            char[] keywords = new char[] {'(',')',';',']','\r','\n' };
            StringBuilder builder = new StringBuilder("");
            foreach (char ch in source)
            {
                if (keywords.Contains(ch))
                {
                    builder.Append("\\");
                }
                builder.Append(ch);

            }
            return builder.ToString();
        }//end method

        /// <summary>
        /// 反编码，去除转义关键字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string SGFDecode(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            char[] keywords = new char[] { '(', ')', ';', ']', '\r', '\n' };
            StringBuilder builder = new StringBuilder("");
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '\\')
                {
                    if (i < source.Length - 1 && keywords.Contains(source[i + 1]))
                    {
                        builder.Append(source[i + 1]);
                        i++;
                    }
                    else
                    {
                        builder.Append(source[i]);
                    }
                }
                else
                {
                    builder.Append(source[i]);
                }

            }

            return builder.ToString();
        }//end method
        #endregion//end 静态方法

        #region 内部类

        /// <summary>
        /// 属性
        /// </summary>
        public class Property : ChildStruct
        {
            /// <summary>
            /// 属性名称
            /// </summary>
            string _Ident;

            /// <summary>
            /// 设置或获得属性标示
            /// </summary>
            public string Ident
            {
                get { return _Ident; }
                set { _Ident = value; }
            }

            /// <summary>
            /// 属性值集合
            /// </summary>
            List<string> _Values;

            /// <summary>
            /// 设置或获得值集合
            /// </summary>
            public List<string> Values
            {
                get { return _Values; }
            }

            /// <summary>
            /// 设置或获得值
            /// </summary>
            public string Value
            {
                get
                {
                    if (_Values.Count > 0)
                    {
                        return _Values[0];
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                set
                {
                    if (_Values.Count == 0)
                    {
                        _Values.Add(value);
                    }
                    else
                    {
                        _Values[0] = value;
                    }
                }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public Property()
            {
                _Ident = string.Empty;
                _Values = new List<string>();
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="ident">标识符</param>
            /// <param name="value">值</param>
            public Property(string ident, string value)
            {
                _Ident = ident;
                _Values = new List<string>();
                _Values.Add(value);
            }


        }//end class

        /// <summary>
        /// 属性值类型
        /// </summary>
        public enum PropertyValueType
        {
            None,
            Number,
            Real,
            Double,
            Color,
            SimpleText,
            Text,
            Point,
            Move,
            Stone
        }//end enum 

        /// <summary>
        /// 节点结构
        /// </summary>
        public class Node : ChildStruct
        {
            /// <summary>
            /// 关联游戏
            /// </summary>
            List<GameTree> _ReferenceGames = new List<GameTree>();

            /// <summary>
            /// 设置或获得关联游戏
            /// </summary>
            public List<GameTree> ReferenceGames
            {
                get { return _ReferenceGames; }
            }

            /// <summary>
            /// 获得同级下一个节点
            /// </summary>
            /// <remarks>如果没有下一节点返回null</remarks>
            public Node NextNode
            {
                get
                {
                    //断言
                    Debug.Assert(Parent != null && Parent is GameTree, "父对象应该是GameTree并且不能为空。");
                    GameTree parent = Parent as GameTree;
                    int index = parent.Nodes.IndexOf(this);
                    if (index < parent.Nodes.Count - 1)
                    {
                        return parent.Nodes[index + 1];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// 获得同级上一个节点
            /// </summary>
            /// <remarks>如果没有上一节点返回null</remarks>
            public Node PrevNode
            {
                get
                {
                    //断言
                    Debug.Assert(Parent != null && Parent is GameTree, "父对象应该是GameTree并且不能为空。");
                    GameTree parent = Parent as GameTree;
                    int index = parent.Nodes.IndexOf(this);
                    if (index > 0)
                    {
                        return parent.Nodes[index - 1];
                    }
                    else
                    {
                        return null;
                    }
                }
            }


            /// <summary>
            /// 属性集合
            /// </summary>
            List<Property> _Properties = new List<Property>();

            /// <summary>
            /// 属性集合
            /// </summary>
            public List<Property> Properties
            {
                get { return _Properties; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public Node()
            {
                _Properties = new List<Property>();
            }


        }//end class

        /// <summary>
        /// 游戏类
        /// </summary>
        public class GameTree : ChildStruct
        {


            /// <summary>
            /// 子游戏树
            /// </summary>
            List<GameTree> _GameTrees;

            /// <summary>
            /// 获得游戏树
            /// </summary>
            public List<GameTree> GameTrees
            {
                get { return _GameTrees; }
            }

            /// <summary>
            /// 节点集合
            /// </summary>
            List<Node> _Nodes;

            /// <summary>
            /// 获得节点集合
            /// </summary>
            public List<Node> Nodes
            {
                get { return _Nodes; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public GameTree()
            {
                _GameTrees = new List<GameTree>();
                _Nodes = new List<Node>();
            }
        }//end class

        /// <summary>
        /// 子结构抽象类
        /// </summary>
        public abstract class ChildStruct
        {
            object _Parent = null;
            public object Parent
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

            int _UniqueIndex;

            public int UniqueIndex
            {
                get { return _UniqueIndex; }
                set { _UniqueIndex = value; }
            }

            string _Content = string.Empty;

            public string Content
            {
                get { return _Content; }
                set { _Content = value; }
            }

        }//end class
        #endregion//end 内部类

    }//end class
}//end namespace
