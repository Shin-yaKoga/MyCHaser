/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// ServerProxy.cs: CHaser サーバとの通信手順の実行用
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Jan. 15, 2015
// Last update: Sep. 06, 2015
/////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;


namespace CHaser
{
public class ServerProxy : IDisposable {
// 定数
    public static byte[]    GET_READY = new byte[2] { (byte)'g', (byte)'r' };
    public static byte[]    DELIMITER = new byte[2] { (byte)'\r', (byte)'\n' };

// 公開メソッド
    // 初期化と解放
    public ServerProxy(string svrAddr, int svrPort, byte[] teamName)
    {
        // assertion
        if (null == svrAddr || svrPort <= 0
        || (null == teamName || 8 != teamName.Length)) {
            Console.Out.WriteLine("illegal argment for ServerProxy().");
            return;
        }
        IPAddress   ipAddr;
        Socket  sock;

        try {
            // 指定されたサーバに接続
            ipAddr = IPAddress.Parse(svrAddr);
            sock   = new Socket(
                ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(ipAddr, svrPort);
            mConnection = new NetworkStream(sock, true);

            // チーム名を送信
            mConnection.Write(teamName, 0, teamName.Length);

            // その他のデータメンバを初期化
            mDelimBuf = new byte[2];
            mSendBuf  = new byte[4];
        } catch (Exception e) {
            Console.Out.WriteLine("caught an exception: {0}", e.Message);
            this.Dispose();
        }
    }
    public void    Dispose() {
        if (null != mConnection) {
            mConnection.Close();
            mConnection = null;
        }
    }

    // 属性
    public bool    IsValid { get { return (null != mConnection); } }

    // ターン動作
    public bool    BeginOneTurn(out byte outRespCode, byte[] outRespBody)
    {
        // 出力引数にデフォルト値をセット
        outRespCode = (byte)'1';

        // assertion
        if (null == outRespBody || 9 != outRespBody.Length) {
            Console.Out.WriteLine("illegal argment for BeginOneTurn().");
            return false;
        }

        try {
            // '@' を受信
            if (1 != mConnection.Read(outRespBody, 0, 1)
            || (byte)'@' != outRespBody[0]) {
                Console.Out.WriteLine("failed to recv '@'");
                return false;
            } else if (! this.RecvDelimiter()) {
                return false;
            }

            // "gr\r\n" を送信 (GetReady)
            GET_READY.CopyTo(mSendBuf, 0);
            DELIMITER.CopyTo(mSendBuf, 2);
            mConnection.Write(mSendBuf, 0, 4);

            // 制御情報・周囲情報を受信
            if (! this.RecvResponse(ref outRespCode, outRespBody)) {
                Console.Out.WriteLine("failed to RecvResponse()");
                return false;
            }
        } catch (Exception e) {
            Console.Out.WriteLine("caught an exception: {0}", e.Message);
            return false;
        }

        return true;
    }
    public bool    EndOneTurn(byte[] method,
        out byte outRespCode, byte[] outRespBody)
    {
        // 出力引数にデフォルト値をセット
        outRespCode = (byte)'1';

        // assertion
        if ((null == method || 2 != method.Length)
        || (null == outRespBody || 9 != outRespBody.Length)) {
            Console.Out.WriteLine("illegal argment for EndOneTurn().");
            return false;
        }

        try {
            // メソッド名と "\r\n" を送信
            method.CopyTo(mSendBuf, 0);
            DELIMITER.CopyTo(mSendBuf, 2);
            mConnection.Write(mSendBuf, 0, 4);

            // 制御情報・周囲情報を受信
            if (! this.RecvResponse(ref outRespCode, outRespBody)) {
                Console.Out.WriteLine("failed to RecvResponse().");
                return false;
            }

            // '#' と '\r\n' を送信
            mSendBuf[0] = (byte)'#';
            DELIMITER.CopyTo(mSendBuf, 1);
            mConnection.Write(mSendBuf, 0, 3);
        } catch (Exception e) {
            Console.Out.WriteLine("caught an exception: {0}", e.Message);
            Console.Out.WriteLine("at ServerProxy.EndOneTurn().");
            return false;
        }

        return true;
    }

// 非公開メソッド
    private bool    RecvResponse(ref byte outRespCode, byte[] outRespBody)
    {
        // 制御情報を受信
        if (1 != mConnection.Read(outRespBody, 0, 1)
        || ((byte)'0' != outRespBody[0] && (byte)'1' != outRespBody[0])) {
            Console.Out.WriteLine("failed to recv the control code.");
            return false;
        }
        outRespCode = outRespBody[0];

        // 周囲情報を受信
        if (! this.DoRead(outRespBody)) {
            Console.Out.WriteLine("failed to recv the response.");
            return false;
        } else if (!this.RecvDelimiter()) {
            return false;
        }

        return true;
    }

    private bool    RecvDelimiter()
    {
        // "\r\n" を受信
        if (! this.DoRead(mDelimBuf)) {
            Console.Out.WriteLine("failed to recv the delimiter.");
            return false;
        } else if ('\r' != mDelimBuf[0] || '\n' != mDelimBuf[1]) {
            Console.Out.WriteLine("unexpected delimiter.");
            return false;
        }

        return true;
    }

    private bool    DoRead(byte[] outData)
    {
        int offset = 0;

        // 要求されたバイト長のデータを全て受信するまで繰り返し
        while (offset < outData.Length) {
            int readBytes = mConnection.Read(
                outData, offset, outData.Length - offset);

            if (readBytes <= 0) {
                Console.Out.WriteLine("failed to read from the connection.");
                return false;
            }
            offset += readBytes;
        }

        return true;
    }

// データメンバ
    private NetworkStream    mConnection; // サーバとのコネクション
    private byte[]           mDelimBuf;   // "\r\n" の受信用
    private byte[]           mSendBuf;    // 送信用のバッファ
}
}

//
// End of File
//
