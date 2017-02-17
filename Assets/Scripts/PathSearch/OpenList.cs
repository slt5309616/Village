using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
///   封装的OpenList，已经废弃
/// </summary>
public class OpenList  {
    public List<Point> data;
    public OpenList() {
        data = new List<Point>();
    }

    public void Add(Point p) {       
        data.Add(p);
        data.Sort((pa, pb) => { return (pa.G + pa.H).CompareTo(pb.H + pb.G); });
    }
    public void Remove(Point p) {
        data.Remove(p);
    }
    public Point GetMinPoint() {
        return data[0];
    }
}
