/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// BasicDecisionMaker.cs: DecisionMaker インタフェースの基本実装
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Jan. 12, 2015
// Last update: Jul. 31, 2015
/////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;


namespace CHaser
{
public class BasicDecisionMaker : DecisionMaker {
    // コンストラクタ
    public BasicDecisionMaker() {}

// 非公開メソッド
    // DecisionMaker インタフェースの実装
    protected override List<IActionSelector> CreateActionSelectors()
    {
        List<IActionSelector>    selectors = new List<IActionSelector>(3);

        selectors.Add(new AdjacentCharacter());
        selectors.Add(new AdjacentBlock());
        selectors.Add(new AdjacentItem());
        selectors.Add(new PreferFirstTime());

        return selectors;
    }

    // 基底クラスの振る舞いのカスタマイズ
        // 最終候補の選択
    protected override void SelectFinalCandidate(
        List<Command> ioCandidates, FloorMap map)
    {
        if (null != mLastAction) {
            // 前回実行したアクションが非移動の場合、今回も同じかチェック
            if (!mLastAction.IsWalk && mLastAction == ioCandidates[0]) {
                // 同じ場合、移動アクションを選択可能なら選択
                foreach (Command c in ioCandidates) {
                    if (c.IsWalk) {
                        ioCandidates.Clear();
                        ioCandidates.Add(c);
                        return;  // 見つかった
                    }
                }
            } else if (mLastAction.IsWalk && !ioCandidates[0].IsWalk
                && !ioCandidates[0].IsPut) {
                // 前回実行したアクションが移動で、今回は探索が候補の場合は調整
                foreach (Command c in ioCandidates) {
                    if (mLastAction.GetDir() == c.GetDir()) {
                        ioCandidates.Clear();
                        ioCandidates.Add(c);
                        return;  // 見つかった
                    }
                }
            }
        }

        // デフォルトでは基底クラスと同じ振る舞い
        base.SelectFinalCandidate(ioCandidates, map);
    }
}
}

//
// End of File
//
