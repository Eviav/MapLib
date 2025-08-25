using System;
using System.Collections.Generic;

namespace MapLib
{
    /// <summary>
    /// 地图坐标转换与处理工具类
    /// </summary>
    static partial class Map
    {
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
            List<double[]> region = new List<double[]>(leftPoints.Count + rightPoints.Count);
            region.AddRange(leftPoints);
            region.AddRange(rightPoints);
            return region.ToArray();
        }

        #region 坐标转换（不同坐标系之间的转换）

        #region WGS84（世界坐标）转GCJ02（火星坐标）

        /// <summary>
        /// WGS84（世界坐标）转GCJ02（火星坐标）
        /// </summary>
        /// <param name="lnglat">WGS84经纬度</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat WGS84_To_GCJ02(this LngLat lnglat) => WGS84_To_GCJ02(lnglat.lng, lnglat.lat);

        /// <summary>
        /// WGS84（世界坐标）转GCJ02（火星坐标）
        /// </summary>
        /// <param name="lnglat">WGS84经纬度数组（[经度, 纬度]）</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat WGS84_To_GCJ02(this double[] lnglat) => WGS84_To_GCJ02(lnglat[0], lnglat[1]);

        /// <summary>
        /// WGS84（世界坐标）转GCJ02（火星坐标）
        /// </summary>
        /// <param name="lng">WGS84经度</param>
        /// <param name="lat">WGS84纬度</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat WGS84_To_GCJ02(double lng, double lat) => transform(lng, lat);

        #endregion

        #region GCJ02（火星坐标）转WGS84（世界坐标）

        /// <summary>
        /// GCJ02（火星坐标）转WGS84（世界坐标）
        /// </summary>
        /// <param name="lnglat">GCJ02经纬度</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat GCJ02_To_WGS84(this LngLat lnglat) => GCJ02_To_WGS84(lnglat.lng, lnglat.lat);

        /// <summary>
        /// GCJ02（火星坐标）转WGS84（世界坐标）
        /// </summary>
        /// <param name="lnglat">GCJ02经纬度数组</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat GCJ02_To_WGS84(this double[] lnglat) => GCJ02_To_WGS84(lnglat[0], lnglat[1]);

        /// <summary>
        /// GCJ02（火星坐标）转WGS84（世界坐标）
        /// </summary>
        /// <param name="lng">GCJ02经度</param>
        /// <param name="lat">GCJ02纬度</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat GCJ02_To_WGS84(double lng, double lat)
        {
            // 先将GCJ02转为虚拟WGS84，再通过反向计算得到真实WGS84
            LngLat fakeWgs = transform(lng, lat);
            return new LngLat(lng * 2 - fakeWgs.lng, lat * 2 - fakeWgs.lat);
        }

        #endregion

        #region GCJ02（火星坐标）转BD09（百度坐标）

        /// <summary>
        /// GCJ02（火星坐标）转BD09（百度坐标）
        /// </summary>
        /// <param name="lnglat">GCJ02经纬度</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat GCJ02_To_BD09(this LngLat lnglat) => GCJ02_To_BD09(lnglat.lng, lnglat.lat);

        /// <summary>
        /// GCJ02（火星坐标）转BD09（百度坐标）
        /// </summary>
        /// <param name="lnglat">GCJ02经纬度数组</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat GCJ02_To_BD09(this double[] lnglat) => GCJ02_To_BD09(lnglat[0], lnglat[1]);

        /// <summary>
        /// GCJ02（火星坐标）转BD09（百度坐标）
        /// </summary>
        /// <param name="lng">GCJ02经度</param>
        /// <param name="lat">GCJ02纬度</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat GCJ02_To_BD09(double lng, double lat)
        {
            double x = lng, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * PI);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * PI);
            double bd_lon = z * Math.Cos(theta) + 0.0065;
            double bd_lat = z * Math.Sin(theta) + 0.006;
            return new LngLat(bd_lon, bd_lat);
        }

        #endregion

        #region BD09（百度坐标）转GCJ02（火星坐标）

        /// <summary>
        /// BD09（百度坐标）转GCJ02（火星坐标）
        /// </summary>
        /// <param name="lnglat">BD09经纬度</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat BD09_To_GCJ02(this LngLat lnglat) => BD09_To_GCJ02(lnglat.lng, lnglat.lat);

        /// <summary>
        /// BD09（百度坐标）转GCJ02（火星坐标）
        /// </summary>
        /// <param name="lnglat">BD09经纬度数组</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat BD09_To_GCJ02(this double[] lnglat) => BD09_To_GCJ02(lnglat[0], lnglat[1]);

        /// <summary>
        /// BD09（百度坐标）转GCJ02（火星坐标）
        /// </summary>
        /// <param name="lng">BD09经度</param>
        /// <param name="lat">BD09纬度</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat BD09_To_GCJ02(double lng, double lat)
        {
            // 百度解密算法（反向偏移）
            double x = lng - 0.0065, y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * PI);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * PI);
            double gg_lon = z * Math.Cos(theta), gg_lat = z * Math.Sin(theta);
            return new LngLat(gg_lon, gg_lat);
        }

        #endregion

        #region BD09（百度坐标）转WGS84（世界坐标）

        /// <summary>
        /// BD09（百度坐标）转WGS84（世界坐标）
        /// </summary>
        /// <param name="lnglat">BD09经纬度</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat BD09_To_WGS84(this LngLat lnglat) => BD09_To_WGS84(lnglat.lng, lnglat.lat);

        /// <summary>
        /// BD09（百度坐标）转WGS84（世界坐标）
        /// </summary>
        /// <param name="lnglat">BD09经纬度数组</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat BD09_To_WGS84(this double[] lnglat) => BD09_To_WGS84(lnglat[0], lnglat[1]);

        /// <summary>
        /// BD09（百度坐标）转WGS84（世界坐标）
        /// </summary>
        /// <param name="lng">BD09经度</param>
        /// <param name="lat">BD09纬度</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat BD09_To_WGS84(double lng, double lat) => GCJ02_To_WGS84(BD09_To_GCJ02(lng, lat));

        #endregion

        /// <summary>
        /// 判断经纬度是否在中国境内（粗略判断，用于坐标系转换优化）
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <returns>true：可能在中国境内；false：不在</returns>
        public static bool IsInChina(double lng, double lat)
        {
            // 中国大致经纬度范围：经度73°~135°，纬度18°~53°
            if ((lng < 72.004 || lng > 137.8347) || (lat < 0.8293 || lat > 55.8271)) return true;
            return false;
        }

        /// <summary>
        /// GCJ02加密核心算法（计算偏移量）
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <returns>加密后的虚拟WGS84坐标</returns>
        static LngLat transform(double lng, double lat)
        {
            double dLat = transformLat(lng - 105.0, lat - 35.0), dLon = transformLng(lng - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * PI, magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * PI);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * PI);
            double mgLat = lat + dLat, mgLon = lng + dLon;
            return new LngLat(mgLon, mgLat);
        }

        /// <summary>
        /// 计算纬度偏移量（GCJ02加密）
        /// </summary>
        static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// 计算经度偏移量（GCJ02加密）
        /// </summary>
        static double transformLng(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
            return ret;
        }

        #endregion
    }
}