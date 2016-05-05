/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2015 Something Precious, Inc.
//
// Command.cs: コマンドを表現したクラス
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 17, 2014
// Last update: Jul. 31, 2015
/////////////////////////////////////////////////////////////////////////

using System;


namespace CHaser
{
public class Command {
// 列挙値型
    public enum Kind {
        WALK_RIGHT = 0,
        WALK_LEFT,
        WALK_UP,
        WALK_DOWN,
        LOOK_RIGHT,
        LOOK_LEFT,
        LOOK_UP,
        LOOK_DOWN,
        SEARCH_RIGHT,
        SEARCH_LEFT,
        SEARCH_UP,
        SEARCH_DOWN,
        PUT_RIGHT,
        PUT_LEFT,
        PUT_UP,
        PUT_DOWN,
    }

// 公開メソッド
    // クラス属性（生成済みインスタンス）
    public static Command    WalkRight   {
        get { return sInstanceVec[(int)Kind.WALK_RIGHT];   } }
    public static Command    WalkLeft    {
        get { return sInstanceVec[(int)Kind.WALK_LEFT];    } }
    public static Command    WalkUp      {
        get { return sInstanceVec[(int)Kind.WALK_UP];      } }
    public static Command    WalkDown    {
        get { return sInstanceVec[(int)Kind.WALK_DOWN];    } }
    public static Command    LookRight   {
        get { return sInstanceVec[(int)Kind.LOOK_RIGHT];   } }
    public static Command    LookLeft    {
        get { return sInstanceVec[(int)Kind.LOOK_LEFT];    } }
    public static Command    LookUp      {
        get { return sInstanceVec[(int)Kind.LOOK_UP];      } }
    public static Command    LookDown    {
        get { return sInstanceVec[(int)Kind.LOOK_DOWN];    } }
    public static Command    SearchRight {
        get { return sInstanceVec[(int)Kind.SEARCH_RIGHT]; } }
    public static Command    SearchLeft  {
        get { return sInstanceVec[(int)Kind.SEARCH_LEFT];  } }
    public static Command    SearchUp    {
        get { return sInstanceVec[(int)Kind.SEARCH_UP];    } }
    public static Command    SearchDown  {
        get { return sInstanceVec[(int)Kind.SEARCH_DOWN];  } }
    public static Command    PutRight    {
        get { return sInstanceVec[(int)Kind.PUT_RIGHT];    } }
    public static Command    PutLeft     {
        get { return sInstanceVec[(int)Kind.PUT_LEFT];     } }
    public static Command    PutUp       {
        get { return sInstanceVec[(int)Kind.PUT_UP];       } }
    public static Command    PutDown     {
        get { return sInstanceVec[(int)Kind.PUT_DOWN];     } }

    // 属性
    public Kind   KindVal { get { return mKind; } }
    public byte[] Method { get { return mMethod; } }
    public int    NextRelPosX { get { return mNextRelPosX; } }
    public int    NextRelPosY { get { return mNextRelPosY; } }
    public bool   IsWalk { get {
        return (Kind.WALK_RIGHT <= mKind && mKind <= Kind.WALK_DOWN); } }
    public bool   IsLook { get {
        return (Kind.LOOK_RIGHT <= mKind && mKind <= Kind.LOOK_DOWN); } }
    public bool   IsSearch { get {
        return (Kind.SEARCH_RIGHT <= mKind && mKind <= Kind.SEARCH_DOWN); } }
    public bool   IsPut { get {
        return (Kind.PUT_RIGHT <= mKind && mKind <= Kind.PUT_DOWN); } }

    // 移動方向の取得
    public FloorMap.Direction   GetWalkDir() {
        // walk の場合は移動量から導出
        if (mNextRelPosX < 0) {
            return FloorMap.Direction.Left;
        } else if (0 < mNextRelPosX) {
            return FloorMap.Direction.Right;
        } else if (mNextRelPosY < 0) {
            return FloorMap.Direction.Down;
        } else {  // (0 < mNextRelPosY)
            return FloorMap.Direction.Up;
        }
    }

    // 方向の取得
    public FloorMap.Direction   GetDir() {
        switch (mKind) {
        case Kind.WALK_RIGHT:
        case Kind.LOOK_RIGHT:
        case Kind.SEARCH_RIGHT:
        case Kind.PUT_RIGHT:
            return FloorMap.Direction.Right;
        case Kind.WALK_LEFT:
        case Kind.LOOK_LEFT:
        case Kind.SEARCH_LEFT:
        case Kind.PUT_LEFT:
            return FloorMap.Direction.Left;
        case Kind.WALK_UP:
        case Kind.LOOK_UP:
        case Kind.SEARCH_UP:
        case Kind.PUT_UP:
            return FloorMap.Direction.Up;
        case Kind.WALK_DOWN:
        case Kind.LOOK_DOWN:
        case Kind.SEARCH_DOWN:
        case Kind.PUT_DOWN:
            return FloorMap.Direction.Down;
        default:
            throw (new Exception());
        }
    }

// 非公開メソッド
    // コンストラクタ
    private Command() {}

// データメンバ
    private Kind   mKind;        // コマンド種別
    private byte[] mMethod;      // メソッド名
    private int    mNextRelPosX; // 実行後のプレイヤー相対位置x座標
    private int    mNextRelPosY; // 実行後のプレイヤー相対位置y座標

// クラスデータ
    // 生成済みインスタンスの配列
    private static readonly Command[] sInstanceVec = new Command[16] {
        new Command { mKind = Kind.WALK_RIGHT,
            mMethod = new byte[2] { (byte)'w', (byte)'r' },
            mNextRelPosX = 1,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.WALK_LEFT,
            mMethod = new byte[2] { (byte)'w', (byte)'l' },
            mNextRelPosX = -1, mNextRelPosY = 0
        },
        new Command { mKind = Kind.WALK_UP,
            mMethod = new byte[2] { (byte)'w', (byte)'u' },
            mNextRelPosX = 0,  mNextRelPosY = 1
        },
        new Command { mKind = Kind.WALK_DOWN,
            mMethod = new byte[2] { (byte)'w', (byte)'d' },
            mNextRelPosX = 0,  mNextRelPosY = -1
        },
        new Command { mKind = Kind.LOOK_RIGHT,
            mMethod = new byte[2] { (byte)'l', (byte)'r' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.LOOK_LEFT,
            mMethod = new byte[2] { (byte)'l', (byte)'l' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.LOOK_UP,
            mMethod = new byte[2] { (byte)'l', (byte)'u' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.LOOK_DOWN,
            mMethod = new byte[2] { (byte)'l', (byte)'d' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.SEARCH_RIGHT,
            mMethod = new byte[2] { (byte)'s', (byte)'r' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.SEARCH_LEFT,
            mMethod = new byte[2] { (byte)'s', (byte)'l' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.SEARCH_UP,
            mMethod = new byte[2] { (byte)'s', (byte)'u' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.SEARCH_DOWN,
            mMethod = new byte[2] { (byte)'s', (byte)'d' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.PUT_RIGHT,
            mMethod = new byte[2] { (byte)'p', (byte)'r' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.PUT_LEFT,
            mMethod = new byte[2] { (byte)'p', (byte)'l' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.PUT_UP,
            mMethod = new byte[2] { (byte)'p', (byte)'u' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
        new Command { mKind = Kind.PUT_DOWN,
            mMethod = new byte[2] { (byte)'p', (byte)'d' },
            mNextRelPosX = 0,  mNextRelPosY = 0
        },
    };
}
}

//
// End of File
//
