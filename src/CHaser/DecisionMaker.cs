/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2015 Something Precious, Inc.
//
// DecisionMaker.cs: 次に実行するコマンドの導出用
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 23, 2014
// Last update: Jul. 31, 2015
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace CHaser
{
public abstract class DecisionMaker : IDecisionMaker {
    // コンストラクタ
    public DecisionMaker()
    {
        mActionSelectors = this.CreateActionSelectors();
        mLastAction      = null;
        mCommandList     = new List<Command>(16);

        // assertion
        if (null == mActionSelectors) {
            Console.Out.WriteLine("illegal status at DecisionMaker().");
            return;
        }
    }

    // IDecisionMaker インタフェースの実装
    public Command    DecideNextAction(FloorMap map)
    {
        // 初期候補列を作成
        mCommandList.Clear();
        this.MakeInitialCandidates(mCommandList);

        // 候補を絞り込む
        this.SelectActionCandidate(mCommandList, map);
        if (1 < mCommandList.Count) {
            // 一つに決まらない場合は最終選択
            this.SelectFinalCandidate(mCommandList, map);
        }

        // 選択されたアクションを記録
        mLastAction = mCommandList[0];

        // 選択されたアクションを返す
        return mLastAction;
    }

// 非公開メソッド
    // コマンド選択オブジェクト列の生成 [abstract]
    protected abstract List<IActionSelector>  CreateActionSelectors();

    // 初期候補アクション列の生成 [virtual]
    protected virtual void MakeInitialCandidates(List<Command> outCandidates)
    {
        outCandidates.Add(Command.WalkRight);
        outCandidates.Add(Command.WalkLeft);
        outCandidates.Add(Command.WalkUp);
        outCandidates.Add(Command.WalkDown);
        outCandidates.Add(Command.LookRight);
        outCandidates.Add(Command.LookLeft);
        outCandidates.Add(Command.LookUp);
        outCandidates.Add(Command.LookDown);
        outCandidates.Add(Command.SearchRight);
        outCandidates.Add(Command.SearchLeft);
        outCandidates.Add(Command.SearchUp);
        outCandidates.Add(Command.SearchDown);
        outCandidates.Add(Command.PutRight);
        outCandidates.Add(Command.PutLeft);
        outCandidates.Add(Command.PutDown);
        outCandidates.Add(Command.PutDown);
    }

    // 候補アクションの絞り込み [virtual]
    protected virtual void SelectActionCandidate(
        List<Command> ioCandidates, FloorMap map)
    {
        // 候補が一つに絞られるまで繰り返し
        for (int i = 0; i < mActionSelectors.Count; ++i) {
            mActionSelectors[i].Select(mCommandList, map);
            if (1 == mCommandList.Count) {
                break;  // 候補が決まった
            }
        }

        // assertion
        if (ioCandidates.Count < 1) {
            Console.Out.WriteLine("illegal status at SelectActionCandidate().");
            return;
        }
    }

    // 最終候補の選択 [virtual]
    protected virtual void  SelectFinalCandidate(
        List<Command> ioCandidates, FloorMap map)
    {
        // 先頭の候補を選択
        ioCandidates.RemoveRange(1, ioCandidates.Count - 1);
    }

// データメンバ
    protected List<IActionSelector>  mActionSelectors; // コマンド選択オブジェクト列
    protected Command                mLastAction;      // 最後に選択したアクション
    private List<Command>            mCommandList;     // 候補アクション列のコンテナ
}
}
//
// End of File
//
