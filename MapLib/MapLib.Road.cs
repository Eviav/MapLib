using System;
using System.Collections.Generic;

namespace MapLib
{
    static partial class Map
    {
        #region 判断是否偏离航线

        /// <summary>
        /// 计算点到航线的最短距离（判断是否偏离航线）
        /// </summary>
        /// <param name="lng">实时点的经度</param>
        /// <param name="lat">实时点的纬度</param>
        /// <param name="points">构成航线的有序点集合（线段序列）</param>
        /// <returns>点到航线的最短距离（单位：米）；若航线点数量不足，返回合理默认值</returns>
        public static double PointToPointLine(double lng, double lat, IList<LngLat> points)
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
                var foot = GetFootOfPerpendicular(lng, lat, a, b, c);
                if (foot == null) return -1; // 垂足计算失败，返回错误值

                // 计算点到垂足的直线距离
                double distance = Distance(lng, lat, foot.lng, foot.lat);

                // 判断垂足是否在线段上
                if (foot.lng >= rangeX[0] && foot.lng <= rangeX[1] && foot.lat >= rangeY[0] && foot.lat <= rangeY[1])
                {
                    // 垂足在线段上：更新最短距离（首次计算或当前距离更小）
                    if (minDistance == -1 || distance < minDistance) minDistance = distance;
                }
                else
                {
                    LngLat p1 = points[i], p2 = points[i + 1];
                    // 垂足在线段外：取点到线段两端点的最小距离
                    double distanceToStart = Distance(lng, lat, p1.lng, p1.lat), distanceToEnd = Distance(lng, lat, p2.lng, p2.lat);
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
                double start = Distance(lng, lat, startPoint.lng, startPoint.lat), end = Distance(lng, lat, endPoint.lng, endPoint.lat);
                return start <= end ? start : end;
            }

            return minDistance;
        }


        /// <summary>
        /// 计算点到航线的最短距离（判断是否偏离航线）
        /// </summary>
        /// <param name="point">实时点坐标</param>
        /// <param name="points">构成航线的有序点集合（线段序列）</param>
        /// <returns>点到航线的最短距离（单位：米）；若航线点数量不足，返回合理默认值</returns>
        public static double PointToPointLine(this LngLat point, IList<LngLat> points)
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
                var foot = GetFootOfPerpendicular(point.lng, point.lat, a, b, c);
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
        /// 计算点到航线的最短距离（判断是否偏离航线）
        /// </summary>
        /// <param name="point">实时点坐标</param>
        /// <param name="points">构成航线的有序点集合（线段序列）</param>
        /// <returns>点到航线的最短距离（单位：米）；若航线点数量不足，返回合理默认值</returns>
        public static double PointToPointLine(this double[] point, IList<double[]> points)
        {
            double minDistance = -1; // 初始化最短距离（-1表示未计算有效值）
            // 遍历航线中所有连续线段
            for (int i = 0; i < points.Count - 1; i++)
            {
                // 跳过重合点（避免无效线段）
                if (points[i][0] == points[i + 1][0] && points[i][1] == points[i + 1][1]) continue;

                // 计算线段的经度和纬度范围（用于判断垂足是否在线段上）
                double[] rangeX = new double[2], rangeY = new double[2];
                // 确定经度范围（左小右大）
                if (points[i][0] > points[i + 1][0])
                {
                    rangeX[0] = points[i + 1][0];
                    rangeX[1] = points[i][0];
                }
                else
                {
                    rangeX[0] = points[i][0];
                    rangeX[1] = points[i + 1][0];
                }
                // 确定纬度范围（下小上大）
                if (points[i][1] > points[i + 1][1])
                {
                    rangeY[0] = points[i + 1][1];
                    rangeY[1] = points[i][1];
                }
                else
                {
                    rangeY[0] = points[i][1];
                    rangeY[1] = points[i + 1][1];
                }

                // 计算线段的直线方程参数（AX + BY + C = 0）
                double a = points[i + 1][1] - points[i][1], b = points[i][0] - points[i + 1][0], c = points[i + 1][0] * points[i][1] - points[i][0] * points[i + 1][1];

                // 求点到直线的垂足及距离
                var foot = GetFootOfPerpendicular(point[0], point[1], a, b, c);
                if (foot == null) return -1; // 垂足计算失败，返回错误值

                // 计算点到垂足的直线距离
                double distance = Distance(point[0], point[1], foot.lng, foot.lat);

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
                double[] startPoint = points[0], endPoint = points[points.Count - 1];
                double start = Distance(point[0], point[1], startPoint[0], startPoint[1]), end = Distance(point[0], point[1], endPoint[0], endPoint[1]);
                return start <= end ? start : end;
            }

            return minDistance;
        }

        /// <summary>
        /// 计算点到直线的垂足坐标
        /// </summary>
        /// <param name="lng">目标点经度</param>
        /// <param name="lat">目标点纬度</param>
        /// <param name="A">直线方程参数A（AX + BY + C = 0）</param>
        /// <param name="B">直线方程参数B（AX + BY + C = 0）</param>
        /// <param name="C">直线方程参数C（AX + BY + C = 0）</param>
        /// <returns>垂足坐标；若直线无效（A和B均为0）或点在直线上，返回null或该点自身</returns>
        static LngLat? GetFootOfPerpendicular(double lng, double lat, double A, double B, double C)
        {
            if (A * A + B * B < 1e-13) return null;
            if (Math.Abs(A * lng + B * lat + C) < 1e-13) return new LngLat(lng, lat);
            else
            {
                double newX = (B * B * lng - A * B * lat - A * C) / (A * A + B * B), newY = (-A * B * lng + A * A * lat - B * C) / (A * A + B * B);
                return new LngLat(newX, newY);
            }
        }

        #endregion

        #region GPS点对道路的覆盖度计算

        /// <summary>
        /// 计算GPS点集合对道路的覆盖度
        /// </summary>
        /// <param name="roadPoints">道路折线点集合</param>
        /// <param name="gpsPoints">GPS点集合</param>
        /// <param name="distanceThreshold">距离阈值（米）</param>
        /// <returns>覆盖比例（0~1）；参数无效时返回-1</returns>
        public static double CalculateCoverage(IList<LngLat> roadPoints, IList<LngLat> gpsPoints, double distanceThreshold = 10)
        {
            if (roadPoints == null || roadPoints.Count < 2 || gpsPoints == null || gpsPoints.Count == 0) return -1;

            int coveredCount = 0, totalValidPoints = roadPoints.Count;

            foreach (var gps in gpsPoints)
            {
                double distance = gps.PointToPointLine(roadPoints);
                if (distance <= distanceThreshold) coveredCount++;
            }

            return coveredCount * 1.0 / totalValidPoints;
        }

        /// <summary>
        /// 计算GPS点集合对道路的覆盖度
        /// </summary>
        /// <param name="roadPoints">道路折线点集合</param>
        /// <param name="gpsPoints">GPS点集合</param>
        /// <param name="distanceThreshold">距离阈值（米）</param>
        /// <returns>覆盖比例（0~1）；参数无效时返回-1</returns>
        public static double CalculateCoverage(IList<double[]> roadPoints, IList<double[]> gpsPoints, double distanceThreshold = 10)
        {
            if (roadPoints == null || roadPoints.Count < 2 || gpsPoints == null || gpsPoints.Count == 0) return -1;

            int coveredCount = 0, totalValidPoints = roadPoints.Count;

            foreach (var gps in roadPoints)
            {
                double distance = gps.PointToPointLine(gpsPoints);
                if (distance <= distanceThreshold) coveredCount++;
            }

            return coveredCount * 1.0 / totalValidPoints;
        }

        #endregion
    }
}