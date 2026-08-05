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

            foreach (var gps in roadPoints)
            {
                double distance = gps.PointToPointLine(gpsPoints);
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

        /// <summary>
        /// 计算GPS点集合(带 Tag)对道路的覆盖度,直接返回落在阈值内的GPS点(保留 Tag 便于调用方回查时间戳/速度等)。
        /// 实现走 SpatialGrid 空间索引:把 GPS 装箱后,沿道路段逐段按"段包围盒 + 1 格缓冲"查候选 GPS,再用 PointToPointLine 对单段做精判。
        /// 复杂度 O(N + M×K),N 为 GPS 数,M 为道路段数,K 为常数(每段邻域内的候选数),不再有 N×M 的暴力循环。
        /// prog:被 GPS 轨迹覆盖到的道路段数 / 道路段数,严格落在 0~1 区间。
        /// </summary>
        /// <typeparam name="T">Tag 字段的类型(避免 object boxing)</typeparam>
        /// <param name="roadPoints">道路折线点集合</param>
        /// <param name="gpsPoints">GPS点集合(PointT&lt;T&gt;,带 Tag)</param>
        /// <param name="distanceThreshold">距离阈值(米)</param>
        /// <param name="covered">落在阈值内的GPS点(无命中或参数无效时为空集合)</param>
        /// <returns>覆盖比例(0~1);参数无效时返回-1</returns>
        public static double CalculateCoverage<T>(IList<double[]> roadPoints, IList<LngLat<T>> gpsPoints, double distanceThreshold, out List<LngLat<T>> covered)
        {
            if (roadPoints == null || roadPoints.Count < 2 || gpsPoints == null || gpsPoints.Count == 0)
            {
                covered = new List<LngLat<T>>(0);
                return -1;
            }
            int totalSegments = roadPoints.Count - 1;

            // 1) GPS 包围盒 + 网格单元大小(同时满足"密度自适应"和"阈值内候选不漏"两约束)
            double minLng = double.MaxValue, maxLng = double.MinValue;
            double minLat = double.MaxValue, maxLat = double.MinValue;
            for (int i = 0; i < gpsPoints.Count; i++)
            {
                var g = gpsPoints[i];
                if (g.lng < minLng) minLng = g.lng;
                if (g.lng > maxLng) maxLng = g.lng;
                if (g.lat < minLat) minLat = g.lat;
                if (g.lat > maxLat) maxLat = g.lat;
            }
            double bboxLng = Math.Max(maxLng - minLng, 1e-9), bboxLat = Math.Max(maxLat - minLat, 1e-9);
            double centerLat = (minLat + maxLat) * 0.5;
            double cosLat = Math.Max(Math.Cos(centerLat * PI180), 0.1);
            int sqrtN = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(gpsPoints.Count)));
            double autoDegLng = bboxLng / sqrtN, autoDegLat = bboxLat / sqrtN;
            double thresholdDegLng = 3 * distanceThreshold / (111320.0 * cosLat), thresholdDegLat = 3 * distanceThreshold / 111320.0;
            double cellSizeLng = Math.Max(autoDegLng, thresholdDegLng), cellSizeLat = Math.Max(autoDegLat, thresholdDegLat);

            // 2) 把 GPS 装箱到 SpatialGrid(一次性 O(N) 工作量)
            var grid = new SpatialGrid<LngLat<T>>(cellSizeLng, cellSizeLat);
            for (int i = 0; i < gpsPoints.Count; i++)
            {
                var g = gpsPoints[i];
                grid.Insert(i, g, g.lng, g.lat);
            }

            // 3) 沿道路段逐段查询附近 GPS,对每段做精判
            var gpsMatched = new bool[gpsPoints.Count];
            covered = new List<LngLat<T>>();
            var segmentCovered = new bool[totalSegments];
            int coveredCount = 0;

            // 复用的 2-元素 LngLat 缓冲区(供 PointToPointLine 单段距离精判)
            var segLngLat = new LngLat[] { new LngLat(0, 0), new LngLat(0, 0) };

            for (int si = 0; si < totalSegments; si++)
            {
                var a = roadPoints[si];
                var b = roadPoints[si + 1];
                segLngLat[0].lng = a[0]; segLngLat[0].lat = a[1];
                segLngLat[1].lng = b[0]; segLngLat[1].lat = b[1];

                double segMinLng = Math.Min(a[0], b[0]), segMaxLng = Math.Max(a[0], b[0]);
                double segMinLat = Math.Min(a[1], b[1]), segMaxLat = Math.Max(a[1], b[1]);

                foreach (var entry in grid.QueryBoundingBox(segMinLng, segMaxLng, segMinLat, segMaxLat))
                {
                    double d = entry.Item.PointToPointLine(segLngLat);
                    if (d > distanceThreshold) continue;

                    if (!segmentCovered[si])
                    {
                        segmentCovered[si] = true;
                        coveredCount++;
                    }
                    if (!gpsMatched[entry.Index])
                    {
                        gpsMatched[entry.Index] = true;
                        covered.Add(entry.Item);
                    }
                }
            }

            return coveredCount * 1.0 / totalSegments;
        }

        /// <summary>
        /// 便捷包装:仅返回覆盖比例,不输出覆盖点集合。
        /// </summary>
        public static double CalculateCoverage<T>(IList<double[]> roadPoints, IList<LngLat<T>> gpsPoints, double distanceThreshold = 10) => CalculateCoverage(roadPoints, gpsPoints, distanceThreshold, out _);

        #endregion
    }
}