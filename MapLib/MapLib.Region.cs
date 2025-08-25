using System;
using System.Collections.Generic;
using System.Linq;

namespace MapLib
{
    static partial class Map
    {
        #region 判断点是否在多边形区域内

        /// <summary>
        /// 判断经纬度点是否在多边形区域内（射线法）
        /// </summary>
        /// <param name="lng">点的经度</param>
        /// <param name="lat">点的纬度</param>
        /// <param name="paths">多边形边界点集合（按顺时针或逆时针顺序排列）</param>
        /// <returns>true：点在区域内；false：点在区域外或多边形无效（点数量不足3）</returns>
        public static bool IsInRegion(double lng, double lat, IList<LngLatTag> paths)
        {
            if (paths.Count < 3) return false; // 少于3个点无法构成多边形

            int crossCount = 0; // 射线与多边形边界的交点数量
            int pathCount = paths.Count;
            for (int i = 0; i < pathCount; i++)
            {
                int nextIndex = (i == pathCount - 1) ? 0 : i + 1; // 闭合多边形（最后一点连接第一点）
                double lngStart = paths[i].lng, latStart = paths[i].lat;
                double lngEnd = paths[nextIndex].lng, latEnd = paths[nextIndex].lat;

                // 判断点的纬度是否在当前边的纬度范围内（可能相交）
                if ((lat >= latStart && lat < latEnd) || (lat >= latEnd && lat < latStart))
                {
                    if (Math.Abs(latStart - latEnd) <= 0) continue; // 跳过水平边（避免除零）

                    // 计算射线与边的交点经度
                    double intersectLng = lngStart - ((lngStart - lngEnd) * (latStart - lat)) / (latStart - latEnd);
                    // 若交点在点的右侧，计数加1
                    if (intersectLng < lng) crossCount++;
                }
            }
            // 交点数量为奇数则在区域内
            return (crossCount % 2) != 0;
        }

        /// <summary>
        /// 判断经纬度点是否在多边形区域内（射线法）
        /// </summary>
        /// <param name="lng">点的经度</param>
        /// <param name="lat">点的纬度</param>
        /// <param name="paths">多边形边界点集合（按顺序排列）</param>
        /// <returns>true：点在区域内；false：点在区域外或多边形无效</returns>
        public static bool IsInRegion(double lng, double lat, IList<LngLat> paths)
        {
            if (paths.Count < 3) return false; // 少于3个点无法构成多边形

            int crossCount = 0; // 射线与多边形边界的交点数量
            int pathCount = paths.Count;
            for (int i = 0; i < pathCount; i++)
            {
                int nextIndex = (i == pathCount - 1) ? 0 : i + 1; // 闭合多边形（最后一点连接第一点）
                double lngStart = paths[i].lng, latStart = paths[i].lat;
                double lngEnd = paths[nextIndex].lng, latEnd = paths[nextIndex].lat;

                // 判断点的纬度是否在当前边的纬度范围内（可能相交）
                if ((lat >= latStart && lat < latEnd) || (lat >= latEnd && lat < latStart))
                {
                    if (Math.Abs(latStart - latEnd) <= 0) continue; // 跳过水平边（避免除零）

                    // 计算射线与边的交点经度
                    double intersectLng = lngStart - ((lngStart - lngEnd) * (latStart - lat)) / (latStart - latEnd);
                    // 若交点在点的右侧，计数加1
                    if (intersectLng < lng) crossCount++;
                }
            }
            // 交点数量为奇数则在区域内
            return (crossCount % 2) != 0;
        }

        /// <summary>
        /// 判断经纬度点是否在多边形区域内（射线法）
        /// </summary>
        /// <param name="lng">点的经度</param>
        /// <param name="lat">点的纬度</param>
        /// <param name="paths">多边形边界点集合（按顺序排列）</param>
        /// <returns>true：点在区域内；false：点在区域外或多边形无效</returns>
        public static bool IsInRegion(double lng, double lat, IList<double[]> paths)
        {
            if (paths.Count < 3) return false; // 少于3个点无法构成多边形

            int crossCount = 0; // 射线与多边形边界的交点数量
            int pathCount = paths.Count;
            for (int i = 0; i < pathCount; i++)
            {
                int nextIndex = (i == pathCount - 1) ? 0 : i + 1; // 闭合多边形（最后一点连接第一点）
                double lngStart = paths[i][0], latStart = paths[i][1];
                double lngEnd = paths[nextIndex][0], latEnd = paths[nextIndex][1];

                // 判断点的纬度是否在当前边的纬度范围内（可能相交）
                if ((lat >= latStart && lat < latEnd) || (lat >= latEnd && lat < latStart))
                {
                    if (Math.Abs(latStart - latEnd) <= 0) continue; // 跳过水平边（避免除零）

                    // 计算射线与边的交点经度
                    double intersectLng = lngStart - ((lngStart - lngEnd) * (latStart - lat)) / (latStart - latEnd);
                    // 若交点在点的右侧，计数加1
                    if (intersectLng < lng) crossCount++;
                }
            }
            // 交点数量为奇数则在区域内
            return (crossCount % 2) != 0;
        }

        #endregion

        /// <summary>
        /// 计算点到多边形的最短距离（基于边的垂直距离或端点距离）
        /// </summary>
        /// <param name="point">目标点</param>
        /// <param name="polygon">多边形边界点集合（闭合）</param>
        /// <returns>点到多边形的最短距离（单位：米）</returns>
        public static double DistanceFromPointToPolygon(LngLat point, IList<LngLat> polygon)
        {
            double minDistance = double.MaxValue; // 初始化最短距离为最大值

            // 遍历多边形的每条边（最后一条边连接最后一个点和第一个点）
            foreach (var edge in polygon.Zip(polygon.Skip(1).Concat(new[] { polygon.First() }), (p1, p2) => new { p1, p2 }))
            {
                // 计算边的方向向量
                var edgeVector = new LngLat(edge.p2.lng - edge.p1.lng, edge.p2.lat - edge.p1.lat);
                // 计算边的法向量（垂直于方向向量）
                var normalVector = new LngLat(-edgeVector.lat, edgeVector.lng);
                // 边的长度（避免除零）
                double edgeLength = Math.Sqrt(edgeVector.lng * edgeVector.lng + edgeVector.lat * edgeVector.lat);
                if (edgeLength <= 0) continue;

                // 计算点在边方向上的投影比例（t=0为起点，t=1为终点）
                var t = ((point.lng - edge.p1.lng) * edgeVector.lng + (point.lat - edge.p1.lat) * edgeVector.lat) / edgeLength;

                // 计算投影点（垂足）
                var intersectionPoint = new LngLat(edge.p1.lng + edgeVector.lng * t, edge.p1.lat + edgeVector.lat * t);

                // 投影点在边上：取点到投影点的距离
                if (t >= 0 && t <= 1)
                {
                    var distance = Math.Sqrt(Math.Pow(point.lng - intersectionPoint.lng, 2) + Math.Pow(point.lat - intersectionPoint.lat, 2));
                    if (distance < minDistance) minDistance = distance;
                }
                // 投影点在边的延长线上：取点到边的垂直距离
                else
                {
                    var distance = Math.Abs(edgeVector.lng * (point.lat - edge.p1.lat) - edgeVector.lat * (point.lng - edge.p1.lng)) / edgeLength;
                    if (distance < minDistance) minDistance = distance;
                }
            }
            return minDistance;
        }

        /// <summary>
        /// 将折线转换为多边形区域（沿折线两侧偏移指定距离）
        /// </summary>
        /// <param name="line">原始折线点集合（[经度, 纬度]数组）</param>
        /// <param name="range">偏移距离（单位：米，默认10米）</param>
        /// <returns>多边形区域点集合（闭合区域）</returns>
        public static double[][] LineToRegion(this double[][] line, double range = 10)
        {
            // 存储左侧和右侧偏移点
            List<double[]> leftPoints = new List<double[]>(line.Length), rightPoints = new List<double[]>(line.Length);
            double[]? lastPoint = null;

            // 遍历折线的每条线段
            for (int i = 1; i < line.Length; i++)
            {
                double[] startPoint = line[i - 1], endPoint = line[i];

                // 跳过重复点（避免方向计算错误）
                if (lastPoint != null && (lastPoint[0] == endPoint[0] && lastPoint[1] == endPoint[1])) continue;

                lastPoint = endPoint;
                LngLat start = new LngLat(startPoint), end = new LngLat(endPoint);

                // 计算线段方位角（前进方向）
                double angle = Azimuth(start, end);
                // 左侧偏移点（方位角-90度）
                leftPoints.Add(Destination(start, angle - 90, range).ToDouble());
                // 右侧偏移点（方位角+90度），逆序插入以保证闭合
                rightPoints.Insert(0, Destination(start, angle + 90, range).ToDouble());

                // 处理最后一条线段的终点偏移
                if (i == line.Length - 1)
                {
                    leftPoints.Add(Destination(end, angle - 90, range).ToDouble());
                    rightPoints.Insert(0, Destination(end, angle + 90, range).ToDouble());
                }
            }

            // 合并左侧和右侧点，形成闭合多边形
            var region = new List<double[]>(leftPoints.Count + rightPoints.Count);
            region.AddRange(leftPoints);
            region.AddRange(rightPoints);
            return region.ToArray();
        }
    }
}