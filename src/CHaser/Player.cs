/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// Player.cs: CHaser クライアントのメインクラス
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Jan. 16, 2015
// Last update: Jan. 19, 2015
/////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Threading;


namespace CHaser
{
public class Player : IDisposable {
    // 初期化と解放
    public Player(string teamName)
    {
        Encoding    utf8Enc = Encoding.UTF8;

        // assertion
        if (null == teamName
        || 8 < teamName.Length
        || utf8Enc.GetByteCount(teamName) != teamName.Length) {
            Console.Out.WriteLine("illegal argument for Player().");
            return;
        }

        mTeamName = new byte[8];
        utf8Enc.GetBytes(teamName, 0, teamName.Length, mTeamName, 0);
        for (int i = teamName.Length; i < 8; ++i) {
            mTeamName[i] = (byte)' ';
        }
    }
    public void    Dispose()
    {
        // スレッドが動作中なら停止
        if (this.IsRunning) {
            this.Stop();
        }
    }

    // 属性
    public bool    IsRunning { get { return (null != mThread); } }
    public bool    IsThreadAlive { get {
        return (null != mThread && mThread.IsAlive); } }

    // クライアント動作スレッドの開始と停止
    public bool    Start(string svrAddr, bool isCool)
    {
        // assertion
        if (this.IsRunning) {
            Console.Out.WriteLine("illegal argument for Start().");
            return false;
        }

        // サーバとの通信オブジェクトを生成してサーバに接続
        mServer = new ServerProxy(
            svrAddr, (isCool ? 40000 : 50000), mTeamName);
        if (! mServer.IsValid) {
            Console.Out.WriteLine("failed to connecting to the server.");
            mServer = null;
            return false;
        }

        // クライアント動作実行用のオブジェクトを生成
        mDecisionMaker = this.CreateDecisionMaker();
        mFloorMap = new FloorMap();

        // クライアント動作スレッドを生成・始動
        mQuitRequested = false;
        mThread = new Thread(new ThreadStart(this.ThreadProc));
        mThread.Start();

        return true;
    }
    public void    Stop()
    {
        if (this.IsRunning) {
            // クライアント動作スレッドに終了通知
            mQuitRequested = true;

            // スレッドの終了を待つ
            mThread.Join();

            // クライアント動作実行用のオブジェクトを解放
            mServer.Dispose();
            mServer        = null;
            mDecisionMaker = null;
            mFloorMap      = null;
            mThread        = null;  // スレッドの停止を記録
        }
    }

// 非公開メソッド
    // 派生クラスでのカスタマイズ用 [virtual]
    protected virtual IDecisionMaker CreateDecisionMaker()
    {
        return (new BasicDecisionMaker());
    }

    // クライアントスレッド動作
    private void    ThreadProc()
    {
        byte[]  respBuf = new byte[9];
        byte    respCode;

        // 終了通知されるまで繰り返し
        while (! mQuitRequested) {
            Command nextAction;

            // ターンの実行準備
            if (! mServer.BeginOneTurn(out respCode, respBuf)) {
                Console.Out.WriteLine("failed to BeginOneTurn().");
                break;  // 通信エラー
            } else if ('0' == respCode) {
                break;  // サーバから終了通知された
            }

            // 受け取った周囲情報をマップに反映
            mFloorMap.AddAround(respBuf, true);

            // 実行するアクションを決定
            nextAction = mDecisionMaker.DecideNextAction(mFloorMap);
            if (null == nextAction) {
                Console.Out.WriteLine("failed to DecideNextAction().");
                break;
            }

            // ターン実行を完了
            if (! mServer.EndOneTurn(nextAction.Method, out respCode, respBuf)) {
                Console.Out.WriteLine("failed to EndOneTurn().");
                break;  // 通信エラー
            } else if ('0' == respCode) {
                break;  // サーバから終了通知された
            }

            // walk の場合は WalkTo() でマップ上の位置を更新
            if (nextAction.IsWalk) {
                mFloorMap.WalkTo(nextAction.GetWalkDir());
            }

            // 実行したアクションの結果をマップに反映
            if (nextAction.IsLook) {
                mFloorMap.AddAsLookResult(respBuf, nextAction.GetDir());
            } else if (nextAction.IsSearch) {
                mFloorMap.AddAsSearchResult(respBuf, nextAction.GetDir());
            } else {
                mFloorMap.AddAround(respBuf, false);
            //
            // 注意：アクション実行後に受け取った周囲領域は、それと全く同じ領域を
            //       次回のターン実行前に受け取る。従って、既知領域としてマップに
            //       登録させる必要がない。そのため、AddAround() の第二引数を
            //       false にする。('15. 1/24, koga@stprec.co.jp)
            }

            // 次のターンの準備（既知領域を更新／陳腐化）
            mFloorMap.PrepareNextTurn();
        }

        // サーバとの接続を閉じる
        mServer.Dispose();
    }

// データメンバ
    private byte[]         mTeamName;      // チーム名
    private bool           mQuitRequested; // 終了要求されたか
    private Thread         mThread;        // スレッド
    private ServerProxy    mServer;        // サーバとの通信オブジェクト
    private IDecisionMaker mDecisionMaker; // アクション決定オブジェクト
    private FloorMap       mFloorMap;      // マップ
}
}

//
// End of File
//
