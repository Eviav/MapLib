using System;
using System.Collections.Generic;

namespace MapLib
{
    /// <summary>
    /// 基于等距矩形网格的二维空间索引(经纬度度数坐标)。
    /// 用于在道路段/GPS 点等大规模数据上做"邻域候选查询",把 O(N×M) 的暴力循环压到 O(N + M×K) 其中 K 是常数。
    /// </summary>
    /// <typeparam name="T">条目类型(一般为 PointT&lt;T&gt; 或 LngLat)</typeparam>
    public class SpatialGrid<T>
    {
        private readonly double _cellSizeLng;
        private readonly double _cellSizeLat;
        private readonly Dictionary<long, List<Entry>> _cells;

        /// <summary>
        /// 构建网格。cellSize 单位是度数;为了保证阈值 d 内的候选 GPS 在 QueryBoundingBox 输出内,
        /// 调用方应确保 cellSize ≥ 2 × (d 的度数换算),以便 ±1 cell 缓冲能覆盖阈值。
        /// </summary>
        public SpatialGrid(double cellSizeLng, double cellSizeLat)
        {
            _cellSizeLng = cellSizeLng;
            _cellSizeLat = cellSizeLat;
            _cells = new Dictionary<long, List<Entry>>();
        }

        /// <summary>
        /// 单条网格条目:保留插入时的索引和原始值。
        /// </summary>
        public readonly struct Entry
        {
            public Entry(int index, T item)
            {
                Index = index;
                Item = item;
            }
            public int Index { get; }
            public T Item { get; }
        }

        /// <summary>当前网格占用的格子数(去重后)</summary>
        public int CellCount => _cells.Count;

        /// <summary>插入的条目总数(用于面积/均匀度统计)</summary>
        public int PointCount { get; private set; }

        /// <summary>把条目按 (lng, lat) 落到对应格子里。</summary>
        public void Insert(int index, T item, double lng, double lat)
        {
            long key = Key(lng, lat);
            if (!_cells.TryGetValue(key, out var list))
            {
                list = new List<Entry>();
                _cells[key] = list;
            }
            list.Add(new Entry(index, item));
            PointCount++;
        }

        /// <summary>
        /// 返回位于(以度数计的)矩形包围盒 [minLng..maxLng] × [minLat..maxLat] 内或紧邻(±1 cell 缓冲)的所有条目。
        /// 调用方只需传入待查询区段的端点,本方法会自动枚举覆盖到的格子。
        /// </summary>
        public IEnumerable<Entry> QueryBoundingBox(double minLng, double maxLng, double minLat, double maxLat)
        {
            int minCx = (int)Math.Floor(minLng / _cellSizeLng) - 1;
            int maxCx = (int)Math.Floor(maxLng / _cellSizeLng) + 1;
            int minCy = (int)Math.Floor(minLat / _cellSizeLat) - 1;
            int maxCy = (int)Math.Floor(maxLat / _cellSizeLat) + 1;

            for (int cx = minCx; cx <= maxCx; cx++)
            {
                for (int cy = minCy; cy <= maxCy; cy++)
                {
                    long key = ((long)cx << 32) | (uint)cy;
                    if (_cells.TryGetValue(key, out var list))
                    {
                        foreach (var e in list)
                            yield return e;
                    }
                }
            }
        }

        private long Key(double lng, double lat)
        {
            int cx = (int)Math.Floor(lng / _cellSizeLng);
            int cy = (int)Math.Floor(lat / _cellSizeLat);
            return ((long)cx << 32) | (uint)cy;
        }
    }
}