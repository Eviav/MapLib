namespace MapLib
{
    /// <summary>
    /// 经纬度坐标模型
    /// </summary>
    public class LngLat
    {
        /// <summary>
        /// 初始化经纬度坐标
        /// </summary>
        /// <param name="_lng">经度（度）</param>
        /// <param name="_lat">纬度（度）</param>
        public LngLat(double _lng, double _lat)
        {
            lng = _lng;
            lat = _lat;
        }

        /// <summary>
        /// 从数组初始化经纬度坐标
        /// </summary>
        /// <param name="lnglat">经纬度数组（[经度, 纬度]）</param>
        public LngLat(double[] lnglat)
        {
            lng = lnglat[0];
            lat = lnglat[1];
        }

        /// <summary>
        /// 复制经纬度坐标
        /// </summary>
        /// <param name="lnglat">待复制的经纬度对象</param>
        public LngLat(LngLat lnglat)
        {
            lng = lnglat.lng;
            lat = lnglat.lat;
        }

        /// <summary>
        /// 经度（单位：度，范围-180~180）
        /// </summary>
        public double lng { get; set; }

        /// <summary>
        /// 纬度（单位：度，范围-90~90）
        /// </summary>
        public double lat { get; set; }

        /// <summary>
        /// 转换为"经度,纬度"字符串
        /// </summary>
        /// <returns>格式化字符串</returns>
        public override string ToString() => lng + "," + lat;

        /// <summary>
        /// 转换为经纬度数组（[经度, 纬度]）
        /// </summary>
        /// <returns>双精度数组</returns>
        public double[] ToDouble() => new double[] { lng, lat };
    }

    /// <summary>
    /// 带桩号和附加信息的经纬度坐标
    /// </summary>
    public class LngLatTag : LngLat
    {
        /// <summary>
        /// 初始化带桩号的经纬度坐标
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <param name="_m">桩号（单位：米）</param>
        /// <param name="_tag">附加信息（自定义数据）</param>
        public LngLatTag(double lng, double lat, int _m, object _tag) : base(lng, lat)
        {
            m = _m;
            tag = _tag;
        }

        /// <summary>
        /// 从已有经纬度初始化带桩号坐标
        /// </summary>
        /// <param name="lnglat">基础经纬度</param>
        /// <param name="_m">桩号</param>
        /// <param name="_tag">附加信息</param>
        public LngLatTag(LngLat lnglat, int _m, object _tag) : base(lnglat)
        {
            m = _m;
            tag = _tag;
        }

        /// <summary>
        /// 桩号（路线上的里程标记，单位：米）
        /// </summary>
        public int m { get; set; }

        /// <summary>
        /// 附加信息（可存储任意自定义数据）
        /// </summary>
        public object tag { get; set; }
    }

    /// <summary>
    /// 道路桩号模型（包含经纬度和桩号）
    /// </summary>
    public class RoadStation
    {
        /// <summary>
        /// 初始化空道路桩号
        /// </summary>
        public RoadStation() { }

        /// <summary>
        /// 从数组初始化道路桩号
        /// </summary>
        /// <param name="_lnglat">经纬度数组（[经度, 纬度]）</param>
        /// <param name="_m">桩号</param>
        public RoadStation(double[] _lnglat, int _m)
        {
            lng = _lnglat[0];
            lat = _lnglat[1];
            m = _m;
        }

        /// <summary>
        /// 从经纬度初始化道路桩号
        /// </summary>
        /// <param name="_lnglat">经纬度对象</param>
        /// <param name="_m">桩号</param>
        public RoadStation(LngLat _lnglat, int _m)
        {
            lng = _lnglat.lng;
            lat = _lnglat.lat;
            m = _m;
        }

        /// <summary>
        /// 复制道路桩号
        /// </summary>
        /// <param name="_lnglat">待复制的桩号对象</param>
        /// <param name="_m">新桩号值</param>
        public RoadStation(RoadStation _lnglat, int _m)
        {
            lng = _lnglat.lng;
            lat = _lnglat.lat;
            m = _m;
        }

        /// <summary>
        /// 直接初始化道路桩号
        /// </summary>
        /// <param name="_lng">经度</param>
        /// <param name="_lat">纬度</param>
        /// <param name="_m">桩号</param>
        public RoadStation(double _lng, double _lat, int _m)
        {
            lng = _lng;
            lat = _lat;
            m = _m;
        }

        /// <summary>
        /// 经度
        /// </summary>
        public double lng { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double lat { get; set; }

        /// <summary>
        /// 桩号（单位：米）
        /// </summary>
        public int m { get; set; }
    }
}