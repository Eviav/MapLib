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
        /// 判断经纬度点是否在多边形区域内（射线法，重载：普通LngLat点）
        /// </summary>
        /// <param name="lng">点的经度</param>
        /// <param name="lat">点的纬度</param>
        /// <param name="paths">多边形边界点集合（按顺序排列）</param>
        /// <returns>true：点在区域内；false：点在区域外或多边形无效</returns>
        public static bool IsInRegion(double lng, double lat, IList<LngLat> paths)
        {
            if (paths.Count < 3) return false;

            int crossCount = 0;
            int pathCount = paths.Count;
            for (int i = 0; i < pathCount; i++)
            {
                int nextIndex = (i == pathCount - 1) ? 0 : i + 1;
                double lngStart = paths[i].lng, latStart = paths[i].lat;
                double lngEnd = paths[nextIndex].lng, latEnd = paths[nextIndex].lat;

                if ((lat >= latStart && lat < latEnd) || (lat >= latEnd && lat < latStart))
                {
                    if (Math.Abs(latStart - latEnd) <= 0) continue;

                    double intersectLng = lngStart - ((lngStart - lngEnd) * (latStart - lat)) / (latStart - latEnd);
                    if (intersectLng < lng) crossCount++;
                }
            }
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
    }
}