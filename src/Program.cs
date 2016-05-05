/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// Program.cs: MyCHaser �̃��C�����[�`��
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

			// �v���C���[���n��
			if (! player.Start(args[0], "C" == args[1])) {
				Console.Out.WriteLine("failed to start the player.");
				ShowUsage();
			}

			// �v���C���[�̃X���b�h����~����܂ő҂�
			while (player.IsThreadAlive) {
				Thread.Sleep(500);
			}
		}
	}

// ����J���\�b�h
	private static void ShowUsage()
	{
		Console.Out.WriteLine("Usage: MyCHaser <server addr> {C,H}");
	}
}
}

//
// End of File
//
