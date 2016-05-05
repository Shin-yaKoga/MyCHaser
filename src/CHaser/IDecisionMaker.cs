/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// IDecisionMaker.cs: 次に実行するコマンドを導出するインタフェース
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Jan. 16, 2015
// Last update: Jan. 16, 2015
/////////////////////////////////////////////////////////////////////////

namespace CHaser
{
public interface IDecisionMaker {
    // 次に実行するアクションの決定
    Command    DecideNextAction(FloorMap map);
}
}

//
// End of File
//
