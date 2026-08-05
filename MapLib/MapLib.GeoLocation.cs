using System;

namespace MapLib
{
    /// <summary>
    /// 3D射线投射算法：像素→相机→机体→ENU→大地坐标
    /// </summary>
    static partial class Map
    {
        /// <summary>
        /// 3D 射线投射：像素 → 大地坐标（ENU 坐标系）
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <param name="altitude">相对高度</param>
        /// <param name="yaw">云台航向角</param>
        /// <param name="pitch">云台俯仰角</param>
        /// <param name="roll">云台横滚角</param>
        /// <param name="fov">水平视场角 (度)</param>
        /// <param name="imageWidth">图片宽度</param>
        /// <param name="imageHeight">图片高度</param>
        /// <param name="x">图标坐标x</param>
        /// <param name="y">图标坐标y</param>
        public static LngLat? PixelToGeo(double lng, double lat, double altitude, double yaw, double pitch, double roll, double fov, int imageWidth, int imageHeight, int x, int y)
        {
            // 计算等效焦距
            double hFovRad = fov * PI180;
            double f_pixel = (imageWidth / 2.0) / Math.Tan(hFovRad / 2.0);

            // 相机坐标系 (X右, Y下, Z前)，图像中心为原点
            double cx = x - (imageWidth / 2.0), cy = y - (imageHeight / 2.0);
            double[] v_cam = { cx, cy, f_pixel };

            // 转为弧度
            double pitchRad = pitch * PI180, rollRad = roll * PI180, yawRad = yaw * PI180;

            // 依次应用 Roll → Pitch → Yaw 旋转矩阵，转到世界 ENU 坐标系 (X东, Y北, Z下)
            double[] v1 = // Roll 绕 Z 轴
            {
                v_cam[0] * Math.Cos(rollRad) - v_cam[1] * Math.Sin(rollRad),
                v_cam[0] * Math.Sin(rollRad) + v_cam[1] * Math.Cos(rollRad),
                v_cam[2]
            };

            double[] v2 = // Pitch 绕 X 轴
            {
                v1[0],
                v1[1] * Math.Cos(pitchRad) - v1[2] * Math.Sin(pitchRad),
                v1[1] * Math.Sin(pitchRad) + v1[2] * Math.Cos(pitchRad)
            };

            double[] v3 = // Yaw 绕 Y 轴 → ENU
            {
                v2[0] * Math.Cos(yawRad) + v2[2] * Math.Sin(yawRad),  // East
                -v2[0] * Math.Sin(yawRad) + v2[2] * Math.Cos(yawRad), // North
                v2[1] // Depth (正=向下)
            };

            // 射线与地面 (Z=0) 求交
            if (v3[2] <= 0) return null; // 指向天空

            // 相似三角形缩放：高度 / 垂直深度
            double t = altitude / v3[2];
            double offsetEast = v3[0] * t, offsetNorth = v3[1] * t;

            // 转经纬度
            double latRad = lat * PI180;
            double targetLat = lat + (offsetNorth / EarthRadius) * (180 / PI);
            double targetLon = lng + (offsetEast / EarthRadius) * (180 / PI) / Math.Cos(latRad);

            return new LngLat(targetLon, targetLat);
        }

        /// <summary>
        /// 3D 射线投射：像素 → 大地坐标（ENU 坐标系）
        /// </summary>
        /// <param name="lnglat">经纬度</param>
        /// <param name="altitude">相对高度</param>
        /// <param name="yaw">云台航向角</param>
        /// <param name="pitch">云台俯仰角</param>
        /// <param name="roll">云台横滚角</param>
        /// <param name="fov">水平视场角 (度)</param>
        /// <param name="imageWidth">图片宽度</param>
        /// <param name="imageHeight">图片高度</param>
        /// <param name="x">图标坐标x</param>
        /// <param name="y">图标坐标y</param>
        public static LngLat? PixelToGeo(this LngLat lnglat, double altitude, double yaw, double pitch, double roll, double fov, int imageWidth, int imageHeight, int x, int y) => PixelToGeo(lnglat.lng, lnglat.lat, altitude, yaw, pitch, roll, fov, imageWidth, imageHeight, x, y);
    }
}