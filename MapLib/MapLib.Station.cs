using System;
using System.Collections.Generic;

namespace MapLib
{
    static partial class Map
    {
        /// <summary>
        /// 沿折线按指定间隔生成桩号（支持自定义桩号位置）
        /// </summary>
        /// <param name="lines">折线点集合（[经度, 纬度]数组）</param>
        /// <param name="m">桩号间隔（单位：米）</param>
        /// <param name="sm">起始桩号（单位：米，默认0）</param>
        /// <param name="station_div">自定义桩号位置（优先使用这些点分割线段）</param>
        /// <param name="direction">是否反向生成（从终点向起点）</param>
        /// <returns>生成的桩号集合（包含经纬度和桩号值）</returns>
        public static List<RoadStation> Station(this double[][] lines, int m, int sm = 0, List<RoadStation>? station_div = null, bool direction = false)
        {
            double totalDistance = lines.Distance(); // 计算折线总长度
            // 生成高密度采样点（用于桩号插值）
            List<double[]> denseLine = LineDense(lines, totalDistance);

            // 处理自定义桩号分割
            if (station_div != null && station_div.Count > 1)
            {
                var segments = new List<RoadStations>(station_div.Count);
                int startStation = sm;
                int startIndex = 0;

                // 按自定义桩号分割线段
                foreach (var divPoint in station_div)
                {
                    // 查找自定义点在高密度线中的最近点
                    int endIndex = FindNearest(denseLine, divPoint.lng, divPoint.lat, startIndex);
                    if (startIndex == endIndex) continue; // 跳过重复点

                    // 截取线段并添加到分段列表
                    var segmentLine = denseLine.LineSubstring(startIndex, endIndex);
                    if (segmentLine.Count > 2)
                        segments.Add(new RoadStations(segmentLine, startStation, divPoint.m));

                    startStation = divPoint.m;
                    startIndex = endIndex - 1;
                }

                // 自动生成桩号
                return StationAuto(segments, totalDistance, m, sm);
            }
            else
            {
                // 无自定义分割：直接按总长度生成
                if (direction && sm > totalDistance)
                    return StationAuto(
                        new List<RoadStations> { new RoadStations(denseLine, sm, sm - (int)Math.Ceiling(totalDistance)) },
                        totalDistance, m, sm);
                else
                    return StationAuto(
                        new List<RoadStations> { new RoadStations(denseLine, sm, sm + (int)Math.Ceiling(totalDistance)) },
                        totalDistance, m, sm);
            }
        }

        /// <summary>
        /// 在折线中查找距离目标点最近的点的索引
        /// </summary>
        /// <param name="line">折线点集合</param>
        /// <param name="lng">目标点经度</param>
        /// <param name="lat">目标点纬度</param>
        /// <param name="start">搜索起始索引（优化性能）</param>
        /// <returns>最近点的索引</returns>
        static int FindNearest(this List<double[]> line, double lng, double lat, int start)
        {
            var distanceList = new List<MaxDistance>(line.Count - start);
            // 计算目标点到各点的距离
            for (int i = start; i < line.Count; i++) distanceList.Add(new MaxDistance(i, Distance(lng, lat, line[i][0], line[i][1])));
            // 按距离排序，取最近点
            distanceList.Sort((x, y) => x.distance.CompareTo(y.distance));
            return distanceList[0].index;
        }

        /// <summary>
        /// 对线段进行高密度采样（确保桩号计算精度）
        /// </summary>
        /// <param name="lines">原始折线</param>
        /// <param name="totalDistance">折线总长度</param>
        /// <returns>高密度采样后的点集合</returns>
        static List<double[]> LineDense(double[][] lines, double totalDistance)
        {
            // 采样间隔：总长度的1/3，最小1米
            double interval = totalDistance / 3;
            if (interval > 1) interval = 1;
            else interval = Math.Round(interval, 4);

            var densePoints = new List<double[]>((int)Math.Ceiling(totalDistance / interval));
            if (lines.Length == 0) return densePoints;

            double[] previousPoint = lines[0];
            densePoints.Add(previousPoint);

            // 对每条线段进行插值采样
            for (int i = 1; i < lines.Length; i++)
            {
                double[] currentPoint = lines[i];
                double segmentDistance = Distance(previousPoint, currentPoint), accumulatedDistance = 0;
                double[] lastSampled = previousPoint;
                var sampledIds = new List<string>(); // 避免重复点

                // 沿线段按间隔采样
                while (accumulatedDistance < segmentDistance)
                {
                    // 计算采样点（基于方位角和距离）
                    double angle = Azimuth(new LngLat(lastSampled), new LngLat(currentPoint));
                    LngLat sampled = Destination(new LngLat(lastSampled), angle, interval);
                    string id = $"{sampled.lng},{sampled.lat}"; // 去重标识

                    if (!sampledIds.Contains(id))
                    {
                        sampledIds.Add(id);
                        densePoints.Add(new double[] { sampled.lng, sampled.lat });
                    }

                    // 更新累计距离
                    accumulatedDistance += Math.Round(Distance(lastSampled[0], lastSampled[1], sampled.lng, sampled.lat), 3);
                    lastSampled = new double[] { sampled.lng, sampled.lat };
                }

                // 添加线段终点
                densePoints.Add(currentPoint);
                previousPoint = currentPoint;
            }

            return densePoints;
        }

        /// <summary>
        /// 截取折线的子线段（从start到end-1）
        /// </summary>
        /// <param name="lines">原始折线</param>
        /// <param name="st">起始索引（包含）</param>
        /// <param name="et">结束索引（不包含）</param>
        /// <returns>子线段点集合</returns>
        static List<double[]> LineSubstring(this List<double[]> lines, int st, int et)
        {
            var subLine = new List<double[]>(et - st);
            for (int i = st; i < et; i++) subLine.Add(lines[i]);
            return subLine;
        }

        #region 桩号自动生成

        /// <summary>
        /// 按分段自动生成桩号
        /// </summary>
        /// <param name="segments">线段分段集合</param>
        /// <param name="totalDistance">总长度</param>
        /// <param name="interval">桩号间隔</param>
        /// <param name="startStation">起始桩号</param>
        /// <returns>生成的桩号集合</returns>
        static List<RoadStation> StationAuto(List<RoadStations> segments, double totalDistance, int interval, int startStation)
        {
            var stations = new List<RoadStation>(segments.Count + 2)
            {
                // 添加起点桩号
                new RoadStation
                {
                    m = startStation,
                    lng = segments[0].lines[0][0],
                    lat = segments[0].lines[0][1]
                }
            };

            bool isIncrement = segments[0].length > 0; // 桩号是否递增

            // 为每个分段生成桩号
            foreach (var segment in segments)
            {
                stations.AddRange(StationAVG(segment, segment.length, interval, isIncrement));
                // 添加分段终点桩号
                stations.Add(new RoadStation
                {
                    m = segment.et,
                    lng = segment.lines[segment.lines.Count - 1][0],
                    lat = segment.lines[segment.lines.Count - 1][1]
                });
            }

            return stations;
        }

        /// <summary>
        /// 对单个线段按平均间隔生成桩号
        /// </summary>
        /// <param name="segment">线段信息</param>
        /// <param name="totalLength">线段长度</param>
        /// <param name="interval">桩号间隔</param>
        /// <param name="isIncrement">是否递增</param>
        /// <returns>线段内的桩号集合</returns>
        static List<RoadStation> StationAVG(RoadStations segment, int totalLength, int interval, bool isIncrement)
        {
            int currentStation = segment.st;
            var stations = new List<RoadStation>(segment.lines.Count);
            double accumulatedDistance = 0, segmentDistance = segment.lines.Distance();
            // 每段桩号数量对应的实际距离间隔
            double distancePerStation = segmentDistance / Math.Abs(totalLength * 1.0 / interval);

            double[] previousPoint = segment.lines[0];

            // 遍历线段点，累计距离达到间隔时生成桩号
            for (int i = 1; i < segment.lines.Count; i++)
            {
                double[] currentPoint = segment.lines[i];
                accumulatedDistance += Distance(previousPoint, currentPoint);

                // 达到间隔时生成桩号
                if (accumulatedDistance >= distancePerStation)
                {
                    accumulatedDistance = 0;
                    currentStation = isIncrement ? currentStation + interval : currentStation - interval;
                    stations.Add(new RoadStation(currentPoint, currentStation));
                }

                previousPoint = currentPoint;
            }

            return stations;
        }

        /// <summary>
        /// 辅助类：存储点索引和距离
        /// </summary>
        class MaxDistance
        {
            public MaxDistance(int i, double d)
            {
                index = i;
                distance = d;
            }
            public int index { get; set; }
            public double distance { get; set; }
        }

        /// <summary>
        /// 辅助类：存储线段分段信息
        /// </summary>
        class RoadStations
        {
            /// <summary>
            /// 初始化线段分段
            /// </summary>
            /// <param name="Lines">分段内的点集合</param>
            /// <param name="startStation">起始桩号</param>
            /// <param name="endStation">结束桩号</param>
            public RoadStations(List<double[]> Lines, int startStation, int endStation)
            {
                lines = Lines;
                length = endStation - startStation;
                st = startStation;
                et = endStation;
            }

            public RoadStations(List<double[]> Lines, int Length, int startStation, int endStation)
            {
                lines = Lines;
                length = Length;
                st = startStation;
                et = endStation;
            }

            /// <summary>
            /// 分段内的点集合
            /// </summary>
            public List<double[]> lines { get; set; }

            /// <summary>
            /// 分段长度（桩号差值）
            /// </summary>
            public int length { get; set; }

            /// <summary>
            /// 起始桩号
            /// </summary>
            public int st { get; set; }

            /// <summary>
            /// 结束桩号
            /// </summary>
            public int et { get; set; }
        }

        #endregion

        #region 桩号格式转换

        /// <summary>
        /// 桩号字符串转数字（如"K1+234"转1234）
        /// </summary>
        /// <param name="m">桩号字符串（支持K前缀、+/-/.分隔符）</param>
        /// <param name="defaultValue">转换失败时的默认值（默认-1）</param>
        /// <returns>桩号数字（单位：米）</returns>
        public static int StationToNum(this string m, int defaultValue = -1)
        {
            string cleaned = m.TrimStart('K', 'k'); // 去除前缀K/k
            if (cleaned.Contains("+") || cleaned.Contains("-") || cleaned.Contains("."))
            {
                int index = -1;
                if (cleaned.Contains("+")) index = cleaned.LastIndexOf("+");
                else if (cleaned.Contains("-")) index = cleaned.LastIndexOf("-");
                else index = cleaned.LastIndexOf(".");
                if (int.TryParse(cleaned.Substring(0, index), out int _km_) && int.TryParse(cleaned.Substring(index + 1), out int _m_)) return _km_ * 1000 + _m_;
                else return defaultValue;
            }
            if (int.TryParse(cleaned, out int _value)) return _value;
            return defaultValue;
        }

        /// <summary>
        /// 数字桩号转格式化字符串（如1234转"K1+234"）
        /// </summary>
        /// <param name="m">桩号数字（单位：米）</param>
        /// <param name="join">分隔符（默认"+"）</param>
        /// <returns>格式化桩号字符串</returns>
        public static string StationToStr(this int m, string join = "+")
        {
            // 转换为千米+米格式（保留3位小数）
            string kmStr = Math.Round(m / 1000.0, 3).ToString();
            if (kmStr.Contains("."))
            {
                int dotIndex = kmStr.LastIndexOf(".");
                // 整数部分（千米）+ 分隔符 + 小数部分（米，补0至3位）
                return $"K{kmStr.Substring(0, dotIndex)}{join}{kmStr.Substring(dotIndex + 1).PadRight(3, '0')}";
            }
            // 整数千米（补000米）
            return $"K{kmStr}{join}000";
        }

        /// <summary>
        /// 数字桩号转格式化字符串（double重载）
        /// </summary>
        /// <param name="m">桩号数字（单位：米）</param>
        /// <param name="join">分隔符（默认"+"）</param>
        /// <returns>格式化桩号字符串</returns>
        public static string StationToStr(this double m, string join = "+")
        {
            string kmStr = Math.Round(m / 1000.0, 3).ToString();
            if (kmStr.Contains("."))
            {
                int dotIndex = kmStr.LastIndexOf(".");
                return $"K{kmStr.Substring(0, dotIndex)}{join}{kmStr.Substring(dotIndex + 1).PadRight(3, '0')}";
            }
            return $"K{kmStr}{join}000";
        }

        #endregion
    }
}