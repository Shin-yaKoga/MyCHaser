/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Something Precious, Inc.
//
// Footsteps.cs: マップ上でのプレイヤーの移動履歴
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 09, 2014
// Last update: Dec. 09, 2014
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace CHaser
{
// 座標点
public struct Point {
    public int x;
    public int y;

    // コンストラクタ
    public Point(int xval, int yval)
    {
        x = xval;
        y = yval;
    }
}

//
// Footsteps クラス
//
public class Footsteps {
    // コンストラクタ
    public Footsteps()
    {
        mVisitCounts = new Dictionary<Point, int>();
        mPath = new List<Point>();
    }

    // 属性
    public int PathCount { get { return mPath.Count; } }

    // 移動経路のイテレータ
    public List<Point>.Enumerator GetPathEnumerator()
    {
        return mPath.GetEnumerator();
    }

    // マスの訪問回数
    public int GetVisitCount(int x, int y)
    {
        Point p = new Point(x, y);
        int count;

        if (! mVisitCounts.TryGetValue(p, out count)) {
            count = 0;
        }

        return count;
    }

    // 訪問したマスを記録
    public void MarkVisit(int x, int y)
    {
        Point p = new Point(x, y);
        int count = 1;

        // 訪問回数を更新
        if (mVisitCounts.ContainsKey(p)) {
            count += mVisitCounts[p];
        }
        mVisitCounts[p] = count;

        // 移動経路も更新
        mPath.Add(p);
    }

// データメンバ
    List<Point>                    mPath;           // プレイヤーの移動経路
    private Dictionary<Point, int> mVisitCounts;    // 各マスの訪問回数
}
}


//
// End of File
//
