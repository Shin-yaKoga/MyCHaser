/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2016 Something Precious, Inc.
//
// PartialPlane.cs: �L���̓񎟌���`�̈��\�������N���X
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 07, 2014
// Last update: May. 07, 2016
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;


namespace CHaser
{
//
// PartialPlane�̃N���X��`
//
public class PartialPlane<T> {
    // �R���X�g���N�^
    public PartialPlane(T defVal)
    {
        mDefVal = defVal;
        mLeftBottomX = mLeftBottomY = 0;
        mBody = new List<List<T>>();

        // �v���C���[�̏������W��ǉ�
        List<T> origin = new List<T>();
        origin.Add(defVal);
        mBody.Add(origin);
    }

    // ����
    public int    LeftBottomX { get { return mLeftBottomX; } }
    public int    LeftBottomY { get { return mLeftBottomY; } }
    public int    Width { get { return (mBody[0]).Count; } }
    public int    Height { get { return mBody.Count; } }

    // �P�ʗ̈�̒l�̎擾�Ɛݒ�
    public T    GetValueAt(int x, int y)
    {
        try {
            return (T)((mBody[y - mLeftBottomY])[x - mLeftBottomX]);
        } catch (ArgumentOutOfRangeException) {
            return mDefVal;
        }
    }
    public void    SetValueAt(int x, int y, T val)
    {
        if (! Contains(x, y)) {
            throw (new ArgumentOutOfRangeException());
        }
        (mBody[y - mLeftBottomY])[x - mLeftBottomX] = val;
    }

    // �̈�̊Ǘ�
    public bool    Contains(int x, int y)
    {
        return ((mLeftBottomX <= x && x < mLeftBottomX + Width)
            && (mLeftBottomY <= y && y < mLeftBottomY + Height));
    }
    public void    ExpandHorizontal(int diffWidth, bool toRight)
    {
        // assertion
        if (diffWidth <= 0) {
            Console.Out.WriteLine("illegal argument for ExpandHorizontal().");
            return;
        }

        int dstPos  = (toRight ? this.Width : 0);
        T[] defVals = Enumerable.Repeat(mDefVal, diffWidth).ToArray();

        foreach (List<T> row in mBody) {
            row.InsertRange(dstPos, defVals);
        }
        if (! toRight) {
            mLeftBottomX -= diffWidth;
        }
    }
    public void ExpandVertical(int diffHeight, bool toTop)
    {
        // assertion
        if (diffHeight <= 0) {
            Console.Out.WriteLine("illegal argument for ExpandVertical().");
            return;
        }

        int dstPos  = (toTop ? Height : 0);
        int width   = this.Width;
        for (int i = 0; i < diffHeight; ++i) {
            mBody.Insert(dstPos, Enumerable.Repeat(mDefVal, width).ToList());
        }
        if (! toTop) {
            mLeftBottomY -= diffHeight;
        }
    }

    // �f�[�^�����o
    private T      mDefVal;         // �P�ʗ̈�̏����l
    private int    mLeftBottomX;    // �̈�̍������̍��W�ix�����W�l�j
    private int    mLeftBottomY;    // �̈�̍������̍��W�iy�����W�l�j
    private List<List<T>> mBody;    // �̈���̂�\�����I�񎟌��z��
}
}

//
// End of File
//
