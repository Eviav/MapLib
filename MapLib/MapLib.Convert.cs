using System;

namespace MapLib
{
    /// <summary>
    /// 地图坐标转换与处理工具类。
    /// </summary>
    static partial class Map
    {
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
        /// WGS84（世界坐标）转GCJ02（火星坐标）。境外坐标不做偏移，原样返回。
        /// </summary>
        /// <param name="lng">WGS84经度</param>
        /// <param name="lat">WGS84纬度</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat WGS84_To_GCJ02(double lng, double lat)
        {
            WGS84ToGCJ02Core(lng, lat, out double gcjLng, out double gcjLat);
            return new LngLat(gcjLng, gcjLat);
        }

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
        /// <param name="lnglat">GCJ02经纬度数组（[经度, 纬度]）</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat GCJ02_To_WGS84(this double[] lnglat) => GCJ02_To_WGS84(lnglat[0], lnglat[1]);

        /// <summary>
        /// GCJ02（火星坐标）转WGS84（世界坐标）。
        /// 一阶线性反解：以 GCJ02 为初值求正向偏移，再用 2*gcj - f(gcj) 抵消。
        /// </summary>
        /// <param name="lng">GCJ02经度</param>
        /// <param name="lat">GCJ02纬度</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat GCJ02_To_WGS84(double lng, double lat)
        {
            WGS84ToGCJ02Core(lng, lat, out double gcjLng, out double gcjLat);
            return new LngLat(lng * 2 - gcjLng, lat * 2 - gcjLat);
        }

        #endregion

        #region GCJ02（火星坐标）转BD09（百度坐标）

        /// <summary>
        /// GCJ02（火星坐标）转BD09（百度坐标）。与百度官方 Convertor / geoconv 服务结果一致。
        /// </summary>
        /// <param name="lnglat">GCJ02经纬度</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat GCJ02_To_BD09(this LngLat lnglat) => GCJ02_To_BD09(lnglat.lng, lnglat.lat);

        /// <summary>
        /// GCJ02（火星坐标）转BD09（百度坐标）
        /// </summary>
        /// <param name="lnglat">GCJ02经纬度数组（[经度, 纬度]）</param>
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
            GCJ02ToBD09Core(lng, lat, out double bdLng, out double bdLat);
            return new LngLat(bdLng, bdLat);
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
        /// <param name="lnglat">BD09经纬度数组（[经度, 纬度]）</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat BD09_To_GCJ02(this double[] lnglat) => BD09_To_GCJ02(lnglat[0], lnglat[1]);

        /// <summary>
        /// BD09（百度坐标）转GCJ02（火星坐标）
        /// 采用百度 JSAPI 的解析逆 pj()，为一阶近似，回代残差约 1e-6 度（约 10 厘米）。
        /// </summary>
        /// <param name="lng">BD09经度</param>
        /// <param name="lat">BD09纬度</param>
        /// <returns>GCJ02经纬度</returns>
        public static LngLat BD09_To_GCJ02(double lng, double lat)
        {
            BD09ToGCJ02Core(lng, lat, out double gcjLng, out double gcjLat);
            return new LngLat(gcjLng, gcjLat);
        }

        #endregion

        #region WGS84（世界坐标）转BD09（百度坐标）

        /// <summary>
        /// WGS84（世界坐标）转BD09（百度坐标）
        /// </summary>
        /// <param name="lnglat">WGS84经纬度</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat WGS84_To_BD09(this LngLat lnglat) => WGS84_To_BD09(lnglat.lng, lnglat.lat);

        /// <summary>
        /// WGS84（世界坐标）转BD09（百度坐标）
        /// </summary>
        /// <param name="lnglat">WGS84经纬度数组（[经度, 纬度]）</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat WGS84_To_BD09(this double[] lnglat) => WGS84_To_BD09(lnglat[0], lnglat[1]);

        /// <summary>
        /// WGS84（世界坐标）转BD09（百度坐标）。链路：WGS84 → GCJ02 → BD09。
        /// </summary>
        /// <param name="lng">WGS84经度</param>
        /// <param name="lat">WGS84纬度</param>
        /// <returns>BD09经纬度</returns>
        public static LngLat WGS84_To_BD09(double lng, double lat) => GCJ02_To_BD09(WGS84_To_GCJ02(lng, lat));

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
        /// <param name="lnglat">BD09经纬度数组（[经度, 纬度]）</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat BD09_To_WGS84(this double[] lnglat) => BD09_To_WGS84(lnglat[0], lnglat[1]);

        /// <summary>
        /// BD09（百度坐标）转WGS84（世界坐标）
        /// 链路：BD09 → GCJ02 → WGS84。
        /// 百度官方只提供「其他坐标系→BD09」的服务端转换，反向只能本地解算。
        /// </summary>
        /// <param name="lng">BD09经度</param>
        /// <param name="lat">BD09纬度</param>
        /// <returns>WGS84经纬度</returns>
        public static LngLat BD09_To_WGS84(double lng, double lat) => GCJ02_To_WGS84(BD09_To_GCJ02(lng, lat));

        #endregion

        /// <summary>
        /// 判断是否在中国境外（境外的坐标不做 GCJ02 偏移）
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <returns>true：在中国境外；false：可能在中国境内</returns>
        public static bool OutOfChina(double lng, double lat) => lng < 72.004 || lng > 137.8347 || lat < 0.8293 || lat > 55.8271;

        /// <summary>
        /// 判断经纬度是否在中国境内（粗略判断）
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <returns>true：可能在中国境内；false：不在</returns>
        public static bool IsInChina(double lng, double lat) => !OutOfChina(lng, lat);

        #region 私有核心算法

        /// <summary>GCJ02 加密的纬度偏移量</summary>
        /// <param name="x">经度 - 105</param>
        /// <param name="y">纬度 - 35</param>
        static double TransformLat(double x, double y)
        {
            var ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320.0 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>GCJ02 加密的经度偏移量</summary>
        /// <param name="x">经度 - 105</param>
        /// <param name="y">纬度 - 35</param>
        static double TransformLon(double x, double y)
        {
            var ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
            return ret;
        }

        /// <summary>
        /// WGS84 → GCJ02 正向加密核心（out 顺序：经度、纬度）。
        /// 境外坐标不做偏移，原样返回。
        /// </summary>
        static void WGS84ToGCJ02Core(double lng, double lat, out double gcjLng, out double gcjLat)
        {
            if (OutOfChina(lng, lat))
            {
                gcjLng = lng;
                gcjLat = lat;
                return;
            }
            var dLat = TransformLat(lng - 105.0, lat - 35.0);
            var dLon = TransformLon(lng - 105.0, lat - 35.0);
            var radLat = lat / 180.0 * PI;
            var magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            var sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * PI);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * PI);
            gcjLat = lat + dLat;
            gcjLng = lng + dLon;
        }

        /// <summary>
        /// GCJ02 → BD09 正向加密核心（百度 JSAPI oX，out 顺序：经度、纬度）
        /// </summary>
        static void GCJ02ToBD09Core(double lng, double lat, out double bdLng, out double bdLat)
        {
            var z = Math.Sqrt(lng * lng + lat * lat) + 0.00002 * Math.Sin(lat * X_PI);
            var theta = Math.Atan2(lat, lng) + 0.000003 * Math.Cos(lng * X_PI);
            bdLng = z * Math.Cos(theta) + 0.0065;
            bdLat = z * Math.Sin(theta) + 0.006;
        }

        /// <summary>
        /// BD09 → GCJ02 解析逆核心（百度 JSAPI pj，out 顺序：经度、纬度）
        /// </summary>
        static void BD09ToGCJ02Core(double lng, double lat, out double gcjLng, out double gcjLat)
        {
            var x = lng - 0.0065;
            var y = lat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * X_PI);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * X_PI);
            gcjLng = z * Math.Cos(theta);
            gcjLat = z * Math.Sin(theta);
        }

        #endregion

        #endregion
    }
}