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
    new double[] { 116.4074, 39.9042 },
    new double[] { 116.4174, 39.9142 },
    new double[] { 116.4274, 39.9242 },
    new double[] { 116.4374, 39.9342 }
};

// 设备轨迹点
var deviceTrack = new List<double[]>
{
    new double[] { 116.4075, 39.9043 },
    new double[] { 116.4071, 39.9043 },
    new double[] { 116.4273, 39.9242 }
};

// 计算覆盖率，设置阈值为10米
double coverage = MapLib.Map.CalculateCoverage(roadPoints, deviceTrack, 15);

Console.WriteLine($"路段覆盖率: {coverage:F2}%");