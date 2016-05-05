/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Something Precious, Inc.
//
// PartialPlane.cs: 有限の二次元矩形領域を表現したクラス
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 07, 2014
// Last update: Dec. 09, 2014
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace CHaser
{
//
// PartialPlaneのクラス定義
//
public class PartialPlane<T> {
    // コンストラクタ
    public PartialPlane(T defVal)
    {
        mDefVal = defVal;
        mLeftBottomX = mLeftBottomY = 0;
        mBody = new List<List<T>>();

        // プレイヤーの初期座標を追加
        List<T> origin = new List<T>();
        origin.Add(defVal);
        mBody.Add(origin);
    }

    // 属性
    public int    LeftBottomX { get { return mLeftBottomX; } }
    public int    LeftBottomY { get { return mLeftBottomY; } }
    public int    Width { get { return (mBody[0]).Count; } }
    public int    Height { get { return mBody.Count; } }

    // 単位領域の値の取得と設定
    public T    GetValueAt(int x, int y)
    {
        try {
            return (T)((mBody[y - mLeftBottomY])[x - mLeftBottomX]);
        } catch (ArgumentOutOfRangeException) {
            return mDefVal;
        }
    }
    public void    SetValueAt(int x, int y, T val)
    {
        if (! Contains(x, y)) {
            throw (new ArgumentOutOfRangeException());
        }
        (mBody[y - mLeftBottomY])[x - mLeftBottomX] = val;
    }

    // 領域の管理
    public bool    Contains(int x, int y)
    {
        return ((mLeftBottomX <= x && x < mLeftBottomX + Width)
            && (mLeftBottomY <= y && y < mLeftBottomY + Height));
    }
    public void    ExpandHorizontal(int diffWidth, bool toRight)
    {
        // assertion
        if (diffWidth <= 0) {
            Console.Out.WriteLine("illegal argument for ExpandHorizontal().");
            return;
        }

        int dstPos  = (toRight ? this.Width : 0);
        T[] defVals = new T[diffWidth];

        for (int i = 0; i < diffWidth; ++i) {
            defVals[i] = mDefVal;
        }
        for (int i = 0; i < this.Height; ++i) {
            (mBody[i]).InsertRange(dstPos, defVals);
        }
        if (! toRight) {
            mLeftBottomX -= diffWidth;
        }
    }
    public void ExpandVertical(int diffHeight, bool toTop)
    {
        // assertion
        if (diffHeight <= 0) {
            Console.Out.WriteLine("illegal argument for ExpandVertical().");
            return;
        }

        int dstPos  = (toTop ? Height : 0);
        int width   = this.Width;
        T[] defVals = new T[width];

        for (int i = 0; i < width; ++i) {
            defVals[i] = mDefVal;
        }
        for (int i = 0; i < diffHeight; ++i) {
            List<T> newArray = new List<T>(width);

            newArray.InsertRange(0, defVals);
            mBody.Insert(dstPos, newArray);
        }
        if (! toTop) {
            mLeftBottomY -= diffHeight;
        }
    }

    // データメンバ
    private T      mDefVal;         // 単位領域の初期値
    private int    mLeftBottomX;    // 領域の左下隅の座標（x軸座標値）
    private int    mLeftBottomY;    // 領域の左下隅の座標（y軸座標値）
    private List<List<T>> mBody;    // 領域実体を表す動的二次元配列
}
}

//
// End of File
//
