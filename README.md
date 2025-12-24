# MapLib
实用的地图空间计算/桩号/车道级

📣点是**零维**的，线是**一维**的，区域是**二维**的

> 因行业所需，开发了这份开源库，已经经过现实与时间测试

## 🌴 支持

#### 坐标转换

火星坐标/百度坐标/世界坐标
> GCJ-02/BD-09/WGS84

#### 计算里程

计算两点之间的距离，计算线的距离

#### 判断是否在区域内

判断点是否在区域内

#### 方向角/目标点

计算两点的方向角，通过当前点和方向角计算某个距离的目标点

#### 桩号/自定义

使用 `方向角/目标点` 能获取未来点位置的特性，我们可以将杂乱无章的线坐标轻松转换成井然有序的桩号点
>结合现实中 `人工测绘误差` 的实际桩号 `系统内称为自定义桩号` 通过平均里程的方式，达到与现实1:1的桩号分部

## 📖 文档

### 坐标转换工具类

#### WGS84（世界坐标）转GCJ02（火星坐标）

```csharp
// 定义WGS84坐标（示例：北京某位置）
var wgs84LngLat = new LngLat(116.39748, 39.90882);
// 转换为GCJ02坐标
var gcj02LngLat = wgs84LngLat.WGS84_To_GCJ02();
// 输出结果（示例）：Lng=116.40382, Lat=39.91465
Console.WriteLine($"转换后GCJ02坐标：Lng={gcj02LngLat.lng:0.000000}, Lat={gcj02LngLat.lat:0.000000}");
```

```csharp
// 定义WGS84坐标数组（示例：上海某位置）
double[] wgs84Array = new double[] { 121.47370, 31.23041 };
// 转换为GCJ02坐标
var gcj02LngLat = wgs84Array.WGS84_To_GCJ02();
// 输出结果（示例）：Lng=121.48015, Lat=31.23576
Console.WriteLine($"转换后GCJ02坐标：Lng={gcj02LngLat.lng:0.000000}, Lat={gcj02LngLat.lat:0.000000}");
```

```csharp
// 定义WGS84经纬度（示例：广州某位置）
double wgs84Lng = 113.26443;
double wgs84Lat = 23.12911;
// 转换为GCJ02坐标
var gcj02LngLat = Map.WGS84_To_GCJ02(wgs84Lng, wgs84Lat);
// 输出结果（示例）：Lng=113.27088, Lat=23.13456
Console.WriteLine($"转换后GCJ02坐标：Lng={gcj02LngLat.lng:0.000000}, Lat={gcj02LngLat.lat:0.000000}");
```

#### GCJ02（火星坐标）转WGS84（世界坐标）

```csharp
// 定义GCJ02坐标（示例：北京某位置）
var gcj02LngLat = new LngLat(116.40382, 39.91465);
// 转换为WGS84坐标
var wgs84LngLat = gcj02LngLat.GCJ02_To_WGS84();
// 输出结果（示例）：Lng=116.39748, Lat=39.90882
Console.WriteLine($"转换后WGS84坐标：Lng={wgs84LngLat.lng:0.000000}, Lat={wgs84LngLat.lat:0.000000}");
```

```csharp
// 定义GCJ02坐标数组（示例：上海某位置）
double[] gcj02Array = new double[] { 121.48015, 31.23576 };
// 转换为WGS84坐标
var wgs84LngLat = gcj02Array.GCJ02_To_WGS84();
// 输出结果（示例）：Lng=121.47370, Lat=31.23041
Console.WriteLine($"转换后WGS84坐标：Lng={wgs84LngLat.lng:0.000000}, Lat={wgs84LngLat.lat:0.000000}");
```

```csharp
// 定义GCJ02经纬度（示例：广州某位置）
double gcj02Lng = 113.27088;
double gcj02Lat = 23.13456;
// 转换为WGS84坐标
var wgs84LngLat = Map.GCJ02_To_WGS84(gcj02Lng, gcj02Lat);
// 输出结果（示例）：Lng=113.26443, Lat=23.12911
Console.WriteLine($"转换后WGS84坐标：Lng={wgs84LngLat.lng:0.000000}, Lat={wgs84LngLat.lat:0.000000}");
```

#### GCJ02（火星坐标）转BD09（百度坐标）

```csharp
// 定义GCJ02坐标（示例：北京某位置）
var gcj02LngLat = new LngLat(116.40382, 39.91465);
// 转换为BD09坐标
var bd09LngLat = gcj02LngLat.GCJ02_To_BD09();
// 输出结果（示例）：Lng=116.41038, Lat=39.92125
Console.WriteLine($"转换后BD09坐标：Lng={bd09LngLat.lng:0.000000}, Lat={bd09LngLat.lat:0.000000}");
```

```csharp
// 定义GCJ02坐标数组（示例：上海某位置）
double[] gcj02Array = new double[] { 121.48015, 31.23576 };
// 转换为BD09坐标
var bd09LngLat = gcj02Array.GCJ02_To_BD09();
// 输出结果（示例）：Lng=121.48672, Lat=31.24232
Console.WriteLine($"转换后BD09坐标：Lng={bd09LngLat.lng:0.000000}, Lat={bd09LngLat.lat:0.000000}");
```

```csharp
// 定义GCJ02经纬度（示例：广州某位置）
double gcj02Lng = 113.27088;
double gcj02Lat = 23.13456;
// 转换为BD09坐标
var bd09LngLat = Map.GCJ02_To_BD09(gcj02Lng, gcj02Lat);
// 输出结果（示例）：Lng=113.27745, Lat=23.14112
Console.WriteLine($"转换后BD09坐标：Lng={bd09LngLat.lng:0.000000}, Lat={bd09LngLat.lat:0.000000}");
```

#### BD09（百度坐标）转GCJ02（火星坐标）

```csharp
// 定义BD09坐标（示例：北京某位置）
var bd09LngLat = new LngLat(116.41038, 39.92125);
// 转换为GCJ02坐标
var gcj02LngLat = bd09LngLat.BD09_To_GCJ02();
// 输出结果（示例）：Lng=116.40382, Lat=39.91465
Console.WriteLine($"转换后GCJ02坐标：Lng={gcj02LngLat.lng:0.000000}, Lat={gcj02LngLat.lat:0.000000}");
```

#### Distance

> 计算两点间的直线距离（单位：米），支持基于经纬度的地理距离计算

##### 参数

- `start`：起点经纬度（LngLat）
- `end`：终点经纬度（LngLat）

##### 返回值

double：两点间距离（单位：米）

##### 示例

```csharp
// 定义两个经纬度点（示例：北京天安门与北京故宫）
var start = new LngLat(116.39748, 39.90882);
var end = new LngLat(116.39144, 39.91358);
// 计算两点间距离
double distance = Map.Distance(start, end);
// 输出结果（示例）：1058.23（单位：米）
Console.WriteLine($"两点间距离：{distance:F2} 米");
```

### 坐标计算工具类（MapLib.Coordinate）

#### Azimuth

> 计算两点间的方位角（单位：度，从正北顺时针计算）。

##### 参数

- `start`：起点经纬度（LngLat）
- `end`：终点经纬度（LngLat）

##### 返回值

double：方位角（0-360度）

##### 示例

```csharp
var start = new LngLat(116.3, 39.9);
var end = new LngLat(116.4, 39.9);
double angle = Coordinate.Azimuth(start, end); 
// 示例结果：90.0（正东方向）
```

#### Destination

> 根据起点、方位角和距离，计算终点坐标。

##### 参数

- `start`：起点经纬度（LngLat）
- `azimuth`：方位角（度）
- `distance`：距离（米）

##### 返回值

LngLat：终点经纬度

##### 示例

```csharp
var start = new LngLat(116.3, 39.9);
var end = Coordinate.Destination(start, 90, 1000); 
// 从起点向正东移动1000米后的坐标
```

#### PointToPointLine

> 计算点到线段的最短距离及投影点。

##### 参数

- `point`：目标点（LngLat）
- `lineStart`：线段起点（LngLat）
- `lineEnd`：线段终点（LngLat）

##### 返回值

(double distance, LngLat projection)：距离（米）和投影点

##### 示例

```csharp
var point = new LngLat(116.3, 39.9);
var lineStart = new LngLat(116.2, 39.9);
var lineEnd = new LngLat(116.4, 39.9);
var (dist, proj) = Coordinate.PointToPointLine(point, lineStart, lineEnd);
```

### 区域分析工具类（MapLib.Region）

#### IsInRegion

> 判断点是否在多边形区域内（包含边界）。

##### 参数

- `point`：待判断点（LngLat）
- `polygon`：多边形顶点集合（LngLat[]）

##### 返回值

bool：`true`表示在区域内，`false`反之

##### 示例

```csharp
var point = new LngLat(116.3, 39.9);
var polygon = new[] { /* 多边形顶点数组 */ };
bool isIn = Region.IsInRegion(point, polygon);
```

#### DistanceFromPointToPolygon

> 计算点到多边形的最短距离（米）。

##### 参数

- `point`：目标点（LngLat）
- `polygon`：多边形顶点集合（LngLat[]）

##### 返回值

double：最短距离

##### 示例

```csharp
var point = new LngLat(116.3, 39.9);
var polygon = new[] { /* 多边形顶点数组 */ };
double dist = Region.DistanceFromPointToPolygon(point, polygon);
```

#### PolygonCenter

> 计算多边形的中心坐标（几何中心）。

##### 参数

- `polygon`：多边形顶点集合（LngLat[]）

##### 返回值

LngLat：中心坐标

##### 示例

```csharp
var polygon = new[] { /* 多边形顶点数组 */ };
var center = Region.PolygonCenter(polygon);
```

#### CalculateCoverage

> 计算多个点的覆盖范围（最小外接多边形）。

##### 参数

- `points`：点集合（LngLat[]）

##### 返回值

LngLat[]：覆盖范围多边形顶点

##### 示例

```csharp
var points = new[] { /* 点数组 */ };
var coverage = Region.CalculateCoverage(points);
```

### 桩号转换工具类（MapLib.Station）

#### StationToStr

> 将数字桩号（米）转换为格式化字符串（如`K1+234`）。

##### 参数

- `m`：桩号数字（int/double，单位：米）
- `join`：分隔符（默认"+"）

##### 返回值

string：格式化桩号

##### 示例

```csharp
string str1 = 1234.56.StationToStr(); // 输出：K1+235
string str2 = 2000.StationToStr("-"); // 输出：K2-000
```

**说明**：桩号转换逻辑为将米转换为千米后四舍五入保留3位小数，格式化为`K{整数部分}{分隔符}{小数部分（补0至3位）}`，例如1000米转换为`K1+000`，2500.12米转换为`K2+500`（四舍五入后2.500千米）。

#### StationToNum

> 将格式化桩号字符串转换为数字桩号（米）。

##### 参数

- `stationStr`：格式化桩号（如`K1+234`）
- `join`：分隔符（默认"+"）

##### 返回值

double：桩号数字（米）

##### 示例

```csharp
double num1 = Station.StationToNum("K1+235"); // 输出：1235.0
double num2 = Station.StationToNum("K2-000", "-"); // 输出：2000.0
```
> （注：文档部分内容由 AI 生成）

## ✍ 参考

[http://www.movable-type.co.uk/scripts/latlong.html](http://www.movable-type.co.uk/scripts/latlong.html)

![方向角](screenshot/Bearing.png?raw=true)

![目标点](screenshot/Destination.png?raw=true)