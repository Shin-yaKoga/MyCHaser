/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2015 Something Precious, Inc.
//
// BasicIActionSelectors.cs: IActionSelector インタフェースの基本実装群
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 31, 2014
// Last update: Jul. 31, 2015
/////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;


namespace CHaser
{
// 隣に相手のキャラクタがいる場合のアクション
public class AdjacentCharacter : IActionSelector {
    // コンストラクタ
    public AdjacentCharacter() {}

    // IActionSelector インタフェースの実装
    public void Select(List<Command> candidates, FloorMap map)
    {
        Command theAction = null;

        // 隣に相手のキャラクタがいたら put を選択
        if (FloorMap.CellStat.CHARACTER == map.GetCellStat(-1, 0)) {
            theAction = Command.PutLeft;
        } else if (FloorMap.CellStat.CHARACTER == map.GetCellStat(0, 1)) {
            theAction = Command.PutUp;
        } else if (FloorMap.CellStat.CHARACTER == map.GetCellStat(1, 0)) {
            theAction = Command.PutRight;
        } else if (FloorMap.CellStat.CHARACTER == map.GetCellStat(0, -1)) {
            theAction = Command.PutDown;
        }
        if (null != theAction) {
            candidates.Clear();
            candidates.Add(theAction);
            return;  // put を選択
        }
    }
}

// 隣にブロックがある場合のアクション
public class AdjacentBlock : IActionSelector {
    // コンストラクタ
    public AdjacentBlock() {}

    // IActionSelector インタフェースの実装
    public void Select(List<Command> candidates, FloorMap map)
    {
        // 隣にブロックがある場合は、そちらには進まない
        if (FloorMap.CellStat.BLOCK == map.GetCellStat(-1, 0)) {
            candidates.Remove(Command.WalkLeft);
        }
        if (FloorMap.CellStat.BLOCK == map.GetCellStat(0, 1)) {
            candidates.Remove(Command.WalkUp);
        }
        if (FloorMap.CellStat.BLOCK == map.GetCellStat(1, 0)) {
            candidates.Remove(Command.WalkRight);
        }
        if (FloorMap.CellStat.BLOCK == map.GetCellStat(0, -1)) {
            candidates.Remove(Command.WalkDown);
        }
    }
}

// 隣にアイテムがある場合のアクション
public class AdjacentItem : IActionSelector {
    // コンストラクタ
    public AdjacentItem() {}

    // IActionSelector インタフェースの実装
    public void Select(List<Command> candidates, FloorMap map)
    {
        List<Command>   actions = null;

        // 隣にアイテムがある場合、そちらに進む
        if (FloorMap.CellStat.ITEM == map.GetCellStat(-1, 0)) {
            this.AddAction(Command.WalkLeft, ref actions);
        }
        if (FloorMap.CellStat.ITEM == map.GetCellStat(0, 1))
        {
            this.AddAction(Command.WalkUp, ref actions);
        }
        if (FloorMap.CellStat.ITEM == map.GetCellStat(1, 0))
        {
            this.AddAction(Command.WalkRight, ref actions);
        }
        if (FloorMap.CellStat.ITEM == map.GetCellStat(0, -1))
        {
            this.AddAction(Command.WalkDown, ref actions);
        }

        // アイテム上へ進む選択肢が見つかったら入力リストの内容を入れ替え
        if (null != actions) {
            // 進行先がブロックで囲まれている場合は選択肢から除外
            for (int i = 0; i < actions.Count; ++i) {
                if (this.IsNextPosDeadEnd(actions[i], map)) {
                    candidates.Remove(actions[i]);
                    actions.RemoveAt(i--);
                }
            }

            // アイテム上へ進む選択肢が残ったら入力リストの内容を入れ換え
            if (0 != actions.Count) {
                candidates.Clear();
                candidates.AddRange(actions);
            }
        }
    }

// 非公開メソッド
    // 進行先を候補に追加
    private void AddAction(
        Command candidate, ref List<Command> actions)
    {
        // 必要ならリストを生成
        if (null == actions) {
            actions = new List<Command>();
        }

        // リストに追加
        actions.Add(candidate);
    }

    // 進行先がブロックで囲まれていないか判定
    private bool    IsNextPosDeadEnd(Command c, FloorMap map)
    {
        Point[] around = new Point[3];
        int numSorroundBlock = 0;

        // 進行先を囲むセルの相対位置を導出
        if (0 == c.NextRelPosX) {  // Up or Down
            around[0].x = -1;
            around[1].x = 1;
            around[0].y = around[1].y = c.NextRelPosY;
            around[2].x = 0;
            around[2].y = c.NextRelPosY * 2;
        } else {  // Right or Left
            around[0].y = -1;
            around[1].y = 1;
            around[0].x = around[1].x = c.NextRelPosX;
            around[2].y = 0;
            around[2].x = c.NextRelPosX * 2;
        }

        // 進行先を囲むセル中のブロック数を算出
        for (int i = 0, n = 3; i < n; ++i) {
            if (FloorMap.CellStat.BLOCK ==
                    map.GetCellStat(around[i].x, around[i].y)) {
                ++numSorroundBlock;
            }
        }

        return (3 == numSorroundBlock);
    }
}

// 一度も訪れていない場所を移動先候補として優先するためのアクション
public class PreferFirstTime : IActionSelector
{
    // コンストラクタ
    public PreferFirstTime() {}

    // IActionSelector インタフェースの実装
    public void Select(List<Command> candidates, FloorMap map)
    {
        List<Command>    altActions = null;

        // 一度訪れた場所への移動は優先候補から除外
        for (int i = 0; i < candidates.Count; ++i) {
            if (! this.IsFirstTimeVisit(candidates[i], map)) {
                if (null == altActions) {
                    altActions = new List<Command>();
                }
                this.AddWithPriority(altActions, candidates[i], map);
                candidates.RemoveAt(i--);
            }
        }

        // 優先候補から外した移動先があれば、残った候補列の末尾に追加
        if (null != altActions) {
            candidates.AddRange(altActions);
        }
    }

// 非公開メソッド
    // 初めて訪れる移動先かどうかを判定
    private bool    IsFirstTimeVisit(Command c, FloorMap map)
    {
        // 非移動コマンドは無視
        if (0 == c.NextRelPosX && 0 == c.NextRelPosY) {
            return true;
        }

        return (0 == map.GetVisitCount(c.NextRelPosX, c.NextRelPosY));
    }

    // 訪問回数が多いものほど後ろへ配置
    private void    AddWithPriority(
        List<Command> altActions, Command c, FloorMap map)
    {
        int visitCount = map.GetVisitCount(c.NextRelPosX, c.NextRelPosY);

        for (int i = 0; i < altActions.Count; ++i) {
            Command c2 = altActions[i];

            if (visitCount <= map.GetVisitCount(c2.NextRelPosX, c2.NextRelPosY)) {
                altActions.Insert(i, c);
                return;
            }
        }

        altActions.Add(c);
    }
}
}

//
// End of File
//
