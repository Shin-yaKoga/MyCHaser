/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2015 Something Precious, Inc.
//
// FloorMap.cs: 探索済み領域に対するマップを表現したクラス
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 07, 2014
// Last update: Jul. 29, 2015
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace CHaser
{
public class FloorMap {
// 列挙値型
    // マスの状態
    public enum CellStat {
        UNKNOWN       = -1,  // 未知
        NONE          = 0,   // 何もない
        CHARACTER     = 1,   // キャラクタ
        BLOCK         = 2,   // ブロック
        ITEM          = 3,   // アイテム
            // 以前に探索済みだが現在状態は不明
        WAS_NONE      = 10,  // 何もなかった
        WAS_CHARACTER = 11,  // キャラクタが存在した
        WAS_ITEM      = 13,  // アイテムが存在した
    }

    // 移動や探索の向き
    public enum Direction { Right, Left, Up, Down }

// 公開メソッド
    // コンストラクタ
    public FloorMap() {
        mCurrPosX = mCurrPosY = 0;
        mWorld        = new PartialPlane<CellStat>(CellStat.UNKNOWN);
        mKnownRegions = new List<RectRegion>();
        mFootsteps    = new Footsteps();

        // プレイヤーの初期位置のマスの状態をセット
        mWorld.SetValueAt(0, 0, CellStat.NONE);
        mFootsteps.MarkVisit(0, 0);
    }

    // 属性
    public int    CurrWidth { get { return mWorld.Width; } }
    public int    CurrHeight { get { return mWorld.Height; } }
    public int    DistanceToLeftEdge { get { return (mCurrPosX - mWorld.LeftBottomX); } }
    public int    DistanceToBottomEdge { get { return (mCurrPosY - mWorld.LeftBottomY); } }
    public int    DistanceToRightEdge { get { return (mWorld.LeftBottomX + mWorld.Width - mCurrPosX); } }
    public int    DistanceToTopEdge { get { return (mWorld.LeftBottomY + mWorld.Height - mCurrPosY); } }
    public int    CurrPosX { get { return mCurrPosX; } }
    public int    CurrPosY { get { return mCurrPosY; } }

    // 探索済み領域の更新
    public void    AddAround(byte[] cellStats, bool addToKnwonRegion)
    {
        // assertion
        if (null == cellStats || 9 != cellStats.Length) {
            System.Console.Out.WriteLine("illegal argument for AddAround().");
            return;
        }

        CellStat[] cStats = new CellStat[9];
        RectRegion reg = new RectRegion(mCurrPosX - 1, mCurrPosY - 1, 3, 3);

        for (int row = reg.height - 1, idx = 0; 0 <= row; --row) {
            int srcOffset = reg.width * row;

            for (int col = 0; col < reg.width; ++col) {
                cStats[idx++] = ToCellStat(cellStats[srcOffset + col]);
            }
        }
        cStats[4] = CellStat.NONE;  // プレイヤーは除外
        if (addToKnwonRegion) {
            this.AddRegion(reg, cStats);
        } else {
            this.ApplyRegionInfo(reg, cStats);
        }
    }
    public void AddAsLookResult(byte[] cellStats, Direction dir)
    {
        // assertion
        if (null == cellStats || 9 != cellStats.Length) {
            Console.Out.WriteLine("illegal argument for AddAround().");
            return;
        }

        CellStat[] cStats = new CellStat[9];
        RectRegion reg;
        int left, bottom;

        switch (dir) {
        case Direction.Right:
            left   = mCurrPosX + 1;
            bottom = mCurrPosY - 1;
            break;
        case Direction.Left:
            left   = mCurrPosX - 3;
            bottom = mCurrPosY - 1;
            break;
        case Direction.Up:
            left   = mCurrPosX - 1;
            bottom = mCurrPosY + 1;
            break;
        case Direction.Down:
            left   = mCurrPosX - 1;
            bottom = mCurrPosY - 3;
            break;
        default:
            Console.Out.WriteLine("unexpected Direction value: {0}", dir);
            throw (new Exception());
        }
        reg = new RectRegion(left, bottom, 3, 3);

        for (int row = reg.height - 1, idx = 0; 0 <= row; --row) {
            int srcOffset = reg.width * row;

            for (int col = 0; col < reg.width; ++col) {
                cStats[idx++] = ToCellStat(cellStats[srcOffset + col]);
            }
        }
        this.AddRegion(reg, cStats);
    }
    public void AddAsSearchResult(byte[] cellStats, Direction dir)
    {
        // assertion
        if (null == cellStats || 9 != cellStats.Length) {
            Console.Out.WriteLine("illegal argument for AddAround().");
            return;
        }

        CellStat[] cStats = new CellStat[9];
        RectRegion reg;

        switch (dir) {
        case Direction.Right:
            reg = new RectRegion(mCurrPosX + 1, mCurrPosY, 9, 1);
            for (int i = 0; i < reg.width; ++i) {
                cStats[i] = ToCellStat(cellStats[i]);
            }
            break;
        case Direction.Left:
            reg = new RectRegion(mCurrPosX - 9, mCurrPosY, 9, 1);
            for (int col = reg.width - 1, idx = 0; 0 <= col; --col) {
                cStats[idx++] = ToCellStat(cellStats[col]);
            }
            break;
        case Direction.Up:
            reg = new RectRegion(mCurrPosX, mCurrPosY + 1, 1, 9);
            for (int i = 0; i < reg.height; ++i) {
                cStats[i] = ToCellStat(cellStats[i]);
            }
            break;
        case Direction.Down:
            reg = new RectRegion(mCurrPosX, mCurrPosY - 9, 1, 9);
            for (int row = reg.height - 1, idx = 0; 0 <= row; --row) {
                cStats[idx++] = ToCellStat(cellStats[row]);
            }
            break;
        default:
            Console.Out.WriteLine("unexpected Direction value: {0}", dir);
            throw (new Exception());
        }
        this.AddRegion(reg, cStats);
    }

    // プレイヤーの現在位置移動
    public void    WalkTo(Direction dir)
    {
        int diffX = 0;
        int diffY = 0;

        // 必要なら探索済み領域を拡張
        switch (dir) {
        case Direction.Left:
            if (0 == this.DistanceToLeftEdge) {
                mWorld.ExpandHorizontal(1, false);
                break;
            }
            diffX = -1;
            break;
        case Direction.Right:
            if (1 == this.DistanceToRightEdge) {
                mWorld.ExpandHorizontal(1, true);
                break;
            }
            diffX = 1;
            break;
        case Direction.Down:
            if (0 == this.DistanceToBottomEdge) {
                mWorld.ExpandVertical(1, false);
                break;
            }
            diffY = -1;
            break;
        case Direction.Up:
            if (0 == this.DistanceToTopEdge) {
                mWorld.ExpandVertical(1, true);
                break;
            }
            diffY = 1;
            break;
        }

        // プレイヤーの現在位置座標を更新
        mCurrPosX += diffX;
        mCurrPosY += diffY;

        // プレイヤーの移動履歴を更新
        mFootsteps.MarkVisit(mCurrPosX, mCurrPosY);
    }

    // マスの状態の取得
    public CellStat    GetCellStat(int relPosX, int relPosY)
    {
        return mWorld.GetValueAt(relPosX + mCurrPosX, relPosY + mCurrPosY);
    }
    public bool FindNearestTarget(CellStat target, out int relPosX, out int relPosY)
    {
        bool isFound = false;
        int left    = mCurrPosX - 1;
        int bottom  = mCurrPosY - 1;
        int size    = 2 + 1;

        // 出力引数を初期化
        relPosX = relPosY = 0;

        // プレイヤーの周囲から外側に向かって検索
        for (;;) {
            // 探索済み領域から完全に逸脱したら終了
            if (! this.IsInsideOfWorld(left, bottom, size)) {
                 break;
            }

            // プレイヤーの左下マスから時計回りに走査
            if (this.SearchOutskirts(target,
                left, bottom, size, ref relPosX, ref relPosY)) {
                isFound = true;  // 見つかった
                break;
            }

            // ループ変数を更新
            --left;
            --bottom;
            size += 2;
        }

        return isFound;
    }
    public int GetVisitCount(int relPosX, int relPosY)
    {
        return mFootsteps.GetVisitCount(relPosX + mCurrPosX, relPosY + mCurrPosY);
    }

    // 次のターンの準備
    public void PrepareNextTurn()
    {
        // 既知領域を更新／陳腐化させる
        for (int i = 0; i < mKnownRegions.Count; ++i) {
            RectRegion  reg = mKnownRegions[i];

            if (this.UpdateRegion(ref reg)) {
                mKnownRegions.RemoveAt(i--);
            } else {
                mKnownRegions[i] = reg;
            }
        //
        // 注意：mKnownRegions は、List なので、[] 演算子に対して
        //       ref で参照を取り出すことができない。このため、
        //       一旦ローカル変数値にコピーして、内容変更後に書き
        //       戻す必要がある。('15. 1/24, koga)
        }
    }

    // プレイヤー相対座標からマップ座標への変換
    public void ConvertToMapCoordinate(int relPosX, int relPosY,
        out int posX, out int posY)
    {
        posX = relPosX + mCurrPosX;
        posY = relPosY + mCurrPosY;
    }

// 非公開メソッド
    private static CellStat ToCellStat(byte rawVal)
    {
        return (CellStat)(rawVal - (byte)'0');
    }

    private CellStat NextStat(CellStat currStat)
    {
        CellStat nextStat = currStat;

        switch (currStat) {
        case CellStat.NONE:
            nextStat = CellStat.WAS_NONE;
            break;
        case CellStat.CHARACTER:
            nextStat = CellStat.WAS_CHARACTER;
            break;
        case CellStat.ITEM:
            nextStat = CellStat.WAS_ITEM;
            break;
        }

        return nextStat;
    }

    private bool    UpdateRegion(ref RectRegion reg)
    {
        // 領域の上端を陳腐化
        if (this.OutdateRow(ref reg, reg.bottom + reg.height - 1)) {
            return true;
        }

        // 領域の下端を陳腐化
        if (this.OutdateRow(ref reg, reg.bottom++)) {
            return true;
        }

        // 領域の右端を陳腐化
        if (this.OutdateCol(ref reg, reg.left + reg.width - 1)) {
            return true;
        }

        // 領域の左端を陳腐化
        if (this.OutdateCol(ref reg, reg.left++)) {
            return true;
        }

        return false;
    }
    private bool    OutdateRow(ref RectRegion reg, int y)
    {
        for (int x = reg.left, n = x + reg.width; x < n; ++x) {
            mWorld.SetValueAt(x, y, NextStat(mWorld.GetValueAt(x, y)));
        }

        return (0 == --(reg.height));
    }
    private bool    OutdateCol(ref RectRegion reg, int x)
    {
        for (int y = reg.bottom, n = y + reg.height; y < n; ++y) {
            mWorld.SetValueAt(x, y, NextStat(mWorld.GetValueAt(x, y)));
        }

        return (0 == --(reg.width));
    }

    private void    AddRegion(RectRegion region, CellStat[] cellStats)
    {
        // 領域の内容をマップに反映
        this.ApplyRegionInfo(region, cellStats);

        // 領域を既知領域リストに追加
        mKnownRegions.Add(region);
    }
    private void    ApplyRegionInfo(RectRegion region, CellStat[] cellStats)
    {
        int regRight = region.left + region.width;
        int regTop   = region.bottom + region.height;

        // 必要なら水平方向に拡張
        if (region.left < mWorld.LeftBottomX) {
            mWorld.ExpandHorizontal(mWorld.LeftBottomX - region.left, false);
        }
        if (mWorld.LeftBottomX + mWorld.Width < regRight) {
            mWorld.ExpandHorizontal(
                regRight - (mWorld.LeftBottomX + mWorld.Width), true);
        }

        // 必要なら垂直方向に拡張
        if (region.bottom < mWorld.LeftBottomY) {
            mWorld.ExpandVertical(mWorld.LeftBottomY - region.bottom, false);
        }
        if (mWorld.LeftBottomY + mWorld.Height < regTop) {
            mWorld.ExpandVertical(
                regTop - (mWorld.LeftBottomY + mWorld.Height), true);
        }

        // マスの状態を更新
        for (int row = 0, idx = 0; row < region.height; ++row) {
            for (int col = 0; col < region.width; ++col) {
                mWorld.SetValueAt(region.left + col, region.bottom + row,
                    cellStats[idx++]);
            }
        }
    }

    private bool    IsInsideOfWorld(int left, int bottom, int size)
    {
        // assertion
        if ((left + size <= mWorld.LeftBottomX)
        || (mWorld.LeftBottomX + mWorld.Width <= left)
        || (bottom + size <= mWorld.LeftBottomY)
        || (mWorld.LeftBottomY + mWorld.Height <= bottom)) {
            Console.Out.WriteLine("illegal argument for IsInsideOfWorld().");
            return false;
        }

        return ((left >= mWorld.LeftBottomX)
            || (mWorld.LeftBottomX + mWorld.Width >= left + size)
            || (bottom >= mWorld.LeftBottomY)
            || (mWorld.LeftBottomY + mWorld.Height >= bottom + size));
    }
    private bool    SearchOutskirts(CellStat target,
        int left, int bottom, int size, ref int relPosX, ref int relPosY)
    {
        int x, y, n;

        // 外周の左辺を走査
        x = left;
        for (y = bottom, n = y + size; y < n; ++y) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // 見つかった
            }
        }

        // 外周の上辺を走査
        y = bottom + size - 1;
        for (x = left + 1, n = left + size; x < n; ++x) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // 見つかった
            }
        }

        // 外周の右辺を走査
        x = left + size - 1;
        for (y = bottom + size - 1; bottom <= y; --y) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // 見つかった
            }
        }

        // 外周の下辺を走査
        y = bottom;
        for (x = left + size - 1; left <= x; --x) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // 見つかった
            }
        }

        return false;
    }
    private void    ToRelPos(int x, int y, ref int relPosX, ref int relPosY)
    {
        relPosX = x - mCurrPosX;
        relPosY = y - mCurrPosY;
    }

// 内部型
    // 部分領域
    struct RectRegion {
        public int    left;
        public int    bottom;
        public int    width;
        public int    height;

    // コンストラクタ
        public RectRegion(int l, int b, int w, int h)
        {
            left   = l;
            bottom = b;
            width  = w;
            height = h;
        }
    }

// データメンバ
    private int                    mCurrPosX;     // プレイヤーの現在位置座標（x軸座標値）
    private int                    mCurrPosY;     // プレイヤーの現在位置座標（y軸座標値）
    private PartialPlane<CellStat> mWorld;        // 探索済み領域の外接矩形
    private List<RectRegion>       mKnownRegions; // 有効な部分探索領域の列
    private Footsteps              mFootsteps;    // プレイヤーの移動履歴
}
}

//
// End of File
//
