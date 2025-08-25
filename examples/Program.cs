Console.WriteLine("Hello, MapLib");
var valstr = MapLib.Map.StationToStr(1010);
Console.WriteLine("1010桩号：" + valstr);
int st2 = 1010 + 990;
var valstr2 = MapLib.Map.StationToStr(st2);
Console.WriteLine(st2 + "桩号：" + valstr2);
var val1 = MapLib.Map.Distance(102.570619, 24.964126, 102.575351, 24.960976);
Console.WriteLine("两点距离【云南省昆明市】：" + val1 + "米");
var val2 = MapLib.Map.Azimuth(new MapLib.LngLat(102.570619, 24.964126), new MapLib.LngLat(102.575351, 24.960976));
Console.WriteLine("方向角：" + val2);


// 路段定位点 (纬度, 经度)
var roadPoints = new List<double[]>
{
    new double[] { 39.9042, 116.4074 },   // 点1
    new double[] { 39.9142, 116.4174 },   // 点2
    new double[] { 39.9242, 116.4274 },   // 点3
    new double[] { 39.9342, 116.4374 }    // 点4
};

// 设备轨迹点
var deviceTrack = new List<double[]>
{
    new double[] { 39.9043, 116.4075 },   // 接近路段点1
    new double[] { 39.9243, 116.4275 }    // 接近路段点3
};

// 计算覆盖率，设置阈值为10米
double coverage = MapLib.Map.CalculateCoverage(roadPoints, deviceTrack, 10);

Console.WriteLine($"路段覆盖率: {coverage:F2}%");