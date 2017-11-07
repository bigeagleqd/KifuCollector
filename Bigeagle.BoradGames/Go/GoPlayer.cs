//--------------------------------------------------------------------------
//
//  File:        GoPlayer.cs
//
//  Coder:       bigeagle@gmail.com
//
//  Date:        2013/1/24
//
//  Description: 围棋棋手类
//  
//  History:     2013/1/24 created by bigeagle  
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
    /// 围棋棋手类
    /// </summary>
    public class GoPlayer : Player
    {
        /// <summary>
        /// 棋手等级
        /// </summary>
        GoPlayerRank _Rank;

        /// <summary>
        /// 设置或获得棋手等级
        /// </summary>
        public GoPlayerRank Rank
        {
            get { return _Rank; }
            set { _Rank = value; }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public GoPlayer()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名字</param>
        /// <param name="side">对局方</param>
        /// <param name="rank">棋手等级</param>
        public GoPlayer(string name, Stone.Sides side, GoPlayerRank rank)
            : base(name, side)
        {
            _Rank = rank;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Name, _Rank);
        }
        public string ToChineseString()
        {
            return string.Format("{0} {1}", Name, _Rank.ToChineseString());
        }
        #region 围棋棋手等级
        /// <summary>
        /// 棋手等级结构
        /// </summary>
        public struct GoPlayerRank
        {
            /// <summary>
            /// 级别
            /// </summary>
            int _Rank;

            /// <summary>
            /// 设置或获得级别
            /// </summary>
            public int Rank
            {
                get { return _Rank; }
                set { _Rank = value; }
            }

            /// <summary>
            /// 级别类型
            /// </summary>
            RankType _Type;

            /// <summary>
            /// 设置或获得级别类型
            /// </summary>
            public RankType Type
            {
                get { return _Type; }
                set { _Type = value; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="rank">等级</param>
            /// <param name="type">等级类别</param>
            public GoPlayerRank(int rank , RankType type)
            {
                _Rank = rank;
                _Type = type;
            }//end method

            /// <summary>
            /// 重写ToString方法
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                
                return string.Format("{0}{1}", _Rank
                    , _Type != RankType.ProfessionalDan ? " " + _Type.ToString() : "P");
            }

            public string ToChineseString()
            {
                return string.Format("{0}{1}", _Rank
                    , _Type != RankType.Kyu ? "段" : "级");
                
            }

            
        }//end struct
        /// <summary>
        /// 围棋棋手等级类别枚举
        /// </summary>
        public enum RankType
        {
            /// <summary>
            /// 级
            /// </summary>
            Kyu ,

            /// <summary>
            /// 业余段位
            /// </summary>
            Dan ,

            /// <summary>
            /// 专业段位
            /// </summary>
            ProfessionalDan
        }//end enum

        #endregion//end 围棋棋手等级类别枚举

    }//end class
}//end namespace
