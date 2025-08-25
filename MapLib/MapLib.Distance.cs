using System;
using System.Collections.Generic;

namespace MapLib
{
    static partial class Map
    {
        /// <summary>
        /// 计算两个经纬度点之间的地表距离（基于WGS84椭球模型）
        /// </summary>
        /// <param name="lnglat">第一个点坐标</param>
        /// <param name="lnglat2">第二个点坐标</param>
        /// <returns>两点间距离（单位：米）</returns>
        public static double Distance(this LngLat lnglat, LngLat lnglat2) => Distance(lnglat.lng, lnglat.lat, lnglat2.lng, lnglat2.lat);

        /// <summary>
        /// 计算两个经纬度点（数组形式）之间的地表距离
        /// </summary>
        /// <param name="lnglat">第一个点坐标（[经度, 纬度]）</param>
        /// <param name="lnglat2">第二个点坐标（[经度, 纬度]）</param>
        /// <returns>两点间距离（单位：米）</returns>
        public static double Distance(this double[] lnglat, double[] lnglat2) => Distance(lnglat[0], lnglat[1], lnglat2[0], lnglat2[1]);

        /// <summary>
        /// 计算两个经纬度点之间的地表距离（Haversine公式）
        /// </summary>
        /// <param name="lng">第一个点经度</param>
        /// <param name="lat">第一个点纬度</param>
        /// <param name="lng2">第二个点经度</param>
        /// <param name="lat2">第二个点纬度</param>
        /// <returns>两点间距离（单位：米）；计算失败返回0</returns>
        public static double Distance(double lng, double lat, double lng2, double lat2)
        {
            try
            {
                double c = Math.Sin((lat2 - lat) * PI180 / 2), d = Math.Sin((lng2 - lng) * PI180 / 2), a = c * c + d * d * Math.Cos(lat2 * PI180) * Math.Cos(lat2 * PI180);
                // 地球直径（WGS84地球半径≈6378km，直径≈12756km）
                return 12756274 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            }
            catch
            {
                return 0; // 异常时返回0（无效距离）
            }
        }

        /// <summary>
        /// 计算多点连成的折线总长度
        /// </summary>
        /// <param name="lines">折线点集合（按顺序排列）</param>
        /// <returns>折线总长度（单位：米）；点数量不足时返回0</returns>
        public static double Distance(this IList<double[]> lines)
        {
            if (lines.Count > 0)
            {
                double total = 0;
                double[] previousPoint = lines[0];
                // 累加相邻点之间的距离
                for (int i = 1; i < lines.Count; i++)
                {
                    total += Distance(previousPoint, lines[i]);
                    previousPoint = lines[i];
                }
                return total;
            }
            return 0;
        }

        /// <summary>
        /// 查找点到带桩号线段的最近点及对应桩号
        /// </summary>
        /// <param name="current">当前点坐标</param>
        /// <param name="line">带桩号的线段点集合（LngLatTag包含桩号信息）</param>
        /// <param name="station">输出参数：最近点对应的桩号（可能为null）</param>
        /// <returns>最近的带桩号点；查找失败返回null</returns>
        public static LngLatTag? Distance(this LngLat current, IList<LngLatTag> line, out int? station)
        {
            try
            {
                // 计算当前点到每个带桩号点的距离并排序
                var distanceList = new List<LngLatM>(line.Count);
                foreach (var item in line) distanceList.Add(new LngLatM(current.Distance(item), item));
                distanceList.Sort((x, y) => x.m.CompareTo(y.m)); // 按距离升序排序

                // 取距离最近的两个点计算桩号
                LngLatM nearest = distanceList[0], secondNearest = distanceList[1];
                // 根据桩号方向计算当前点对应的桩号
                station = (int)Math.Round(nearest.lnglat.m > secondNearest.lnglat.m ? nearest.lnglat.m - nearest.m : nearest.lnglat.m + nearest.m);
                return nearest.lnglat;
            }
            catch
            {
                station = null;
                return null;
            }
        }

        /// <summary>
        /// 辅助类：存储点到带桩号点的距离
        /// </summary>
        class LngLatM
        {
            /// <summary>
            /// 初始化距离和对应带桩号点
            /// </summary>
            /// <param name="_m">点到带桩号点的距离</param>
            /// <param name="_lnglat">带桩号点</param>
            public LngLatM(double _m, LngLatTag _lnglat)
            {
                m = _m;
                lnglat = _lnglat;
            }

            /// <summary>
            /// 距离（单位：米）
            /// </summary>
            public double m { get; set; }

            /// <summary>
            /// 带桩号的点
            /// </summary>
            public LngLatTag lnglat { get; set; }
        }
    }
}