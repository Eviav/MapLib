using System;

namespace MapLib
{
    /// <summary>
    /// 地图核心工具类（包含方位角、目标点计算等基础功能）
    /// </summary>
    public static partial class Map
    {
        /// <summary>
        /// 数学常量：圆周率π
        /// </summary>
        public const double PI = 3.1415926535897932384626;
        /// <summary>
        /// 角度转弧度系数（π/180）
        /// </summary>
        public const double PI180 = PI / 180;
        /// <summary>
        /// WGS84椭球长半轴（单位：米）
        /// </summary>
        public const double a = 6378245.0;
        /// <summary>
        /// WGS84椭球第一偏心率平方
        /// </summary>
        public const double ee = 0.00669342162296594323;

        /// <summary>
        /// 计算两点间的方位角（从起点到终点的前进方向）
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <returns>方位角（单位：度，范围0~360，顺时针从正北开始）</returns>
        public static double Azimuth(this LngLat start, LngLat end) => Azimuth(start.lng, start.lat, end.lng, end.lat);

        /// <summary>
        /// 计算两点间的方位角（数组重载）
        /// </summary>
        /// <param name="start">起点坐标数组（[经度, 纬度]）</param>
        /// <param name="end">终点坐标数组（[经度, 纬度]）</param>
        /// <returns>方位角（度）</returns>
        public static double Azimuth(this double[] start, double[] end) => Azimuth(start[0], start[1], end[0], end[1]);

        /// <summary>
        /// 计算两点间的方位角（经纬度参数）
        /// </summary>
        /// <param name="lng_start">起点经度</param>
        /// <param name="lat_start">起点纬度</param>
        /// <param name="lng_end">终点经度</param>
        /// <param name="lat_end">终点纬度</param>
        /// <returns>方位角（度）</returns>
        public static double Azimuth(double lng_start, double lat_start, double lng_end, double lat_end)
        {
            // 转换为弧度
            double lng_start_rad = lng_start * PI180, lat_start_rad = lat_start * PI180, lng_end_rad = lng_end * PI180, lat_end_rad = lat_end * PI180;

            // 计算方位角（基于球面三角公式）
            double y = Math.Sin(lng_end_rad - lng_start_rad) * Math.Cos(lat_end_rad), x = Math.Cos(lat_start_rad) * Math.Sin(lat_end_rad) - Math.Sin(lat_start_rad) * Math.Cos(lat_end_rad) * Math.Cos(lng_end_rad - lng_start_rad);
            var brng = Math.Atan2(y, x) * 180 / PI;
            return (brng + 360.0) % 360.0;
        }

        /// <summary>
        /// 根据起点、方位角和距离计算目标点坐标
        /// </summary>
        /// <param name="current">起点坐标</param>
        /// <param name="bearing">方位角（度，0~360）</param>
        /// <param name="m">距离（单位：米）</param>
        /// <returns>目标点坐标</returns>
        public static LngLat Destination(this LngLat current, double bearing, double m) => Destination(current.lng, current.lat, bearing, m);

        /// <summary>
        /// 计算目标点坐标（数组重载）
        /// </summary>
        /// <param name="current">起点坐标数组</param>
        /// <param name="bearing">方位角</param>
        /// <param name="m">距离（米）</param>
        /// <returns>目标点坐标</returns>
        public static LngLat Destination(this double[] current, double bearing, double m) => Destination(current[0], current[1], bearing, m);

        /// <summary>
        /// 计算目标点坐标（经纬度参数）
        /// </summary>
        /// <param name="lng">起点经度</param>
        /// <param name="lat">起点纬度</param>
        /// <param name="bearing">方位角（度）</param>
        /// <param name="m">距离（米）</param>
        /// <returns>目标点坐标</returns>
        public static LngLat Destination(double lng, double lat, double bearing, double m)
        {
            var radius = 6371e3;
            double lng_rad = lng * PI180, lat_rad = lat * PI180;
            double brng = bearing * PI180;
            double _lat = Math.Asin(Math.Sin(lat_rad) * Math.Cos(m / radius) + Math.Cos(lat_rad) * Math.Sin(m / radius) * Math.Cos(brng)), _lng = lng_rad + Math.Atan2(Math.Sin(brng) * Math.Sin(m / radius) * Math.Cos(lat_rad), Math.Cos(m / radius) - Math.Sin(lat_rad) * Math.Sin(_lat));
            return new LngLat(_lng * (180 / PI), _lat * (180 / PI));
        }
    }
}