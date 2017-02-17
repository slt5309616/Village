using UnityEngine;
using System;
using System.Collections.Generic;


public class PathSearch  {
    Dictionary<int, Point> pointDic = new Dictionary<int, Point>();
    List<int> path = new List<int>();
    int cols = 0;
    int rows = 0;

    //开启列表
    List<Point> openList = new List<Point>();
    public PathSearch(Dictionary<int,MapData> mapDic,int rows,int cols) {

        this.cols = cols;
        this.rows = rows;
        var itr = mapDic.GetEnumerator();
        while (itr.MoveNext()) {
            //通过地图信息生成搜索表
            var point = new Point();
            MapUtil.GetInstance().CaculatePointIndex(itr.Current.Key, out point.y, out point.x);
            point.isVisited = false;
            point.isActive = itr.Current.Value.isActive;
            pointDic.Add(itr.Current.Key, point);
        }
    }
    public List<int> GeneratePath(int beginIndex,int endIndex) {
        var startPoint = pointDic[beginIndex];
        var endPoint = pointDic[endIndex];
        this.path = FindWay(startPoint, endPoint);
        return this.path;
    }

    //从开启列表查找F值最小的节点
    private Point GetMinFFromOpenList() {
        Point Pmin = null;
        foreach (Point p in openList)
            if (Pmin == null || Pmin.G + Pmin.H > p.G + p.H)
                Pmin = p;
        return Pmin;
    }

    //判断一个点是否为障碍物
    private bool IsBar(Point p) {
        if (pointDic[MapUtil.GetInstance().GetIndex(p.y,p.x)].isActive) return true;
        return false;
    }

    private bool IsVisited(int x, int y) {
        return pointDic[MapUtil.GetInstance().GetIndex(y, x)].isVisited;
    }

    //判断开启列表是否包含一个坐标的点
    private bool IsInOpenList(int x, int y) {
        return pointDic[MapUtil.GetInstance().GetIndex(y, x)].isInOpenList;
    }
    //从开启列表返回对应坐标的点
    private Point GetPointFromOpenList(int x, int y) {
        foreach (Point p in openList)
            if (p.x == x && p.y == y)
                return p;
        return null;
    }

    public Point GetPointFromIndex(int index) {
        return pointDic[index];
    }

    //计算某个点的G值
    private int GetG(Point p) {
        if (p.father == null)
            return 0;
        if (p.x == p.father.x || p.y == p.father.y)
            return p.father.G + 10;
        else return p.father.G + 14;
    }

    //计算某个点的H值
    private int GetH(Point p, Point pb) {
        return Math.Abs(p.x - pb.x) + Math.Abs(p.y - pb.y);
    }

    
    //检查当前节点附近的节点
    private void CheckP8(Point p0, Point pa, ref Point pb) {
        for (int xt = p0.x - 1; xt <= p0.x + 1; xt++) {
            for (int yt = p0.y - 1; yt <= p0.y + 1; yt++) {
                //排除超过边界和等于自身的点
                if ((xt >= 0 && xt < cols && yt >= 0 && yt < rows) && !(xt == p0.x && yt == p0.y)) {
                    //排除障碍点和已经访问过的点
                    var index = MapUtil.GetInstance().GetIndex(yt, xt);
                    if (pointDic.ContainsKey(index)&& pointDic[index].isActive&& !IsVisited(xt, yt)) {
                        Point pt = pointDic[index];
                        if (IsInOpenList(xt, yt)) {
                            //Point pt = pointDic[MapUtil.GetInstance().GetIndex(yt, xt)];
                            int G_new = 0;
                            if (p0.x == pt.x || p0.y == pt.y) {
                                G_new = p0.G + 10;
                            } else {
                                G_new = p0.G + 14;
                            }
                            if (G_new < pt.G) {
                                pt.father = p0;
                                pt.G = G_new;
                                openList.Add(pt);
                                pt.isInOpenList = true;
                            }
                        } else {
                            //不在开启列表中
                            //Point pt = pointDic[MapUtil.GetInstance().GetIndex(yt, xt)];
                            pt.father = p0;
                            pt.G = GetG(pt);
                            pt.H = GetH(pt, pb);
                            openList.Add(pt);
                            pt.isInOpenList = true;
                        }
                    }
                }
            }
        }
    }



    public List<int> FindWay(Point pa, Point pb) {
        List<int> resultPath = new List<int>();
        openList.Add(pa);
        while (!(IsInOpenList(pb.x, pb.y) || openList.Count == 0)) {
            Point p0 = GetMinFFromOpenList();
            if (p0 == null) return resultPath;
            openList.Remove(p0);
            p0.isInOpenList = false;
            pointDic[MapUtil.GetInstance().GetIndex(p0.y, p0.x)].isVisited = true;
            if (p0 == pb) {
                break;
            }
            CheckP8(p0,pa, ref pb);
        }

        //将结果以倒序加入result
        Point p = GetPointFromOpenList(pb.x, pb.y);
        while (p.father != null) {
            p = p.father;
            resultPath.Add(MapUtil.GetInstance().GetIndex(p.y, p.x));
        }
        //第一个点会出现坐标误差，删除
        resultPath.RemoveAt(resultPath.Count-1 );
        return resultPath;
    }


}










