/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// Program.cs: MyCHaser のメインルーチン
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Aug. 09, 2015
// Last update: Aug. 09, 2015
/////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using CHaser;


namespace MyCHaser
{
class Program {
	static void Main(string[] args)
	{
		if (args.Length < 2) {
			ShowUsage();
		} else if (args[1] != "C" && args[1] != "H") {
			ShowUsage();
		} else {
			Player player = new Player("sapporo");

			// プレイヤーを始動
			if (! player.Start(args[0], "C" == args[1])) {
				Console.Out.WriteLine("failed to start the player.");
				ShowUsage();
			}

			// プレイヤーのスレッドが停止するまで待つ
			while (player.IsThreadAlive) {
				Thread.Sleep(500);
			}
		}
	}

// 非公開メソッド
	private static void ShowUsage()
	{
		Console.Out.WriteLine("Usage: MyCHaser <server addr> {C,H}");
	}
}
}

//
// End of File
//
