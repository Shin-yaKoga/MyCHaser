/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2015 Something Precious, Inc.
//
// ActionSelector.cs: 次にとるべきアクションの候補選択用
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 23, 2014
// Last update: Jan. 24, 2015
/////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;


namespace CHaser
{
public interface IActionSelector {
    // 次にとるべきアクションの選択
    void    Select(List<Command> candidates, FloorMap map);
}
}

//
// End of File
//
