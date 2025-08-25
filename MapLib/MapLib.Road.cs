using System;
using System.Collections.Generic;

namespace MapLib
{
    static partial class Map
    {
        /// <summary>
        /// 计算点到航线的最短距离（判断是否偏离航线的核心方法）
        /// </summary>
        /// <param name="point">实时点坐标</param>
        /// <param name="points">构成航线的有序点集合（线段序列）</param>
        /// <returns>点到航线的最短距离（单位：米）；若航线点数量不足，返回合理默认值</returns>
        public static double PointToPintLine(this LngLat point, IList<LngLat> points)
        {
            double minDistance = -1; // 初始化最短距离（-1表示未计算有效值）
            // 遍历航线中所有连续线段
            for (int i = 0; i < points.Count - 1; i++)
            {
                // 跳过重合点（避免无效线段）
                if (points[i].lng == points[i + 1].lng && points[i].lat == points[i + 1].lat) continue;

                // 计算线段的经度和纬度范围（用于判断垂足是否在线段上）
                double[] rangeX = new double[2], rangeY = new double[2];
                // 确定经度范围（左小右大）
                if (points[i].lng > points[i + 1].lng)
                {
                    rangeX[0] = points[i + 1].lng;
                    rangeX[1] = points[i].lng;
                }
                else
                {
                    rangeX[0] = points[i].lng;
                    rangeX[1] = points[i + 1].lng;
                }
                // 确定纬度范围（下小上大）
                if (points[i].lat > points[i + 1].lat)
                {
                    rangeY[0] = points[i + 1].lat;
                    rangeY[1] = points[i].lat;
                }
                else
                {
                    rangeY[0] = points[i].lat;
                    rangeY[1] = points[i + 1].lat;
                }

                // 计算线段的直线方程参数（AX + BY + C = 0）
                double a = points[i + 1].lat - points[i].lat, b = points[i].lng - points[i + 1].lng, c = points[i + 1].lng * points[i].lat - points[i].lng * points[i + 1].lat;

                // 求点到直线的垂足及距离
                var foot = getFootOfPerpendicular(point.lng, point.lat, a, b, c);
                if (foot == null) return -1; // 垂足计算失败，返回错误值

                // 计算点到垂足的直线距离
                double distance = Distance(point.lng, point.lat, foot.lng, foot.lat);

                // 判断垂足是否在线段上
                if (foot.lng >= rangeX[0] && foot.lng <= rangeX[1] && foot.lat >= rangeY[0] && foot.lat <= rangeY[1])
                {
                    // 垂足在线段上：更新最短距离（首次计算或当前距离更小）
                    if (minDistance == -1 || distance < minDistance) minDistance = distance;
                }
                else
                {
                    // 垂足在线段外：取点到线段两端点的最小距离
                    double distanceToStart = Distance(point, points[i]), distanceToEnd = Distance(point, points[i + 1]);
                    distance = Math.Min(distanceToStart, distanceToEnd);
                    // 更新最短距离
                    if (minDistance == -1 || distance < minDistance) minDistance = distance;
                }
            }

            // 若未计算到有效距离（如航线只有1个点），返回点到航线首尾点的最小距离
            // 如果是初始值则再次计算点到首末两点的距离，若均大于allowRange则认为偏离航线
            if (minDistance == -1)
            {
                LngLat startPoint = points[0], endPoint = points[points.Count - 1];
                double start = Distance(point.lng, point.lat, startPoint.lng, startPoint.lat), end = Distance(point.lng, point.lat, endPoint.lng, endPoint.lat);
                return start <= end ? start : end;
            }

            return minDistance;
        }

        /// <summary>
        /// 计算点到直线的垂足坐标
        /// </summary>
        /// <param name="x1">目标点经度</param>
        /// <param name="y1">目标点纬度</param>
        /// <param name="A">直线方程参数A（AX + BY + C = 0）</param>
        /// <param name="B">直线方程参数B（AX + BY + C = 0）</param>
        /// <param name="C">直线方程参数C（AX + BY + C = 0）</param>
        /// <returns>垂足坐标；若直线无效（A和B均为0）或点在直线上，返回null或该点自身</returns>
        static LngLat? getFootOfPerpendicular(double x1, double y1, double A, double B, double C)
        {
            if (A * A + B * B < 1e-13) return null;
            if (Math.Abs(A * x1 + B * y1 + C) < 1e-13) return new LngLat(x1, y1);
            else
            {
                double newX = (B * B * x1 - A * B * y1 - A * C) / (A * A + B * B), newY = (-A * B * x1 + A * A * y1 - B * C) / (A * A + B * B);
                return new LngLat(newX, newY);
            }
        }
    }
}