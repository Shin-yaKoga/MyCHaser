/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Something Precious, Inc.
//
// Footsteps.cs: �}�b�v��ł̃v���C���[�̈ړ�����
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 09, 2014
// Last update: Dec. 09, 2014
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace CHaser
{
// ���W�_
public struct Point {
    public int x;
    public int y;

    // �R���X�g���N�^
    public Point(int xval, int yval)
    {
        x = xval;
        y = yval;
    }
}

//
// Footsteps �N���X
//
public class Footsteps {
    // �R���X�g���N�^
    public Footsteps()
    {
        mVisitCounts = new Dictionary<Point, int>();
        mPath = new List<Point>();
    }

    // ����
    public int PathCount { get { return mPath.Count; } }

    // �ړ��o�H�̃C�e���[�^
    public List<Point>.Enumerator GetPathEnumerator()
    {
        return mPath.GetEnumerator();
    }

    // �}�X�̖K���
    public int GetVisitCount(int x, int y)
    {
        Point p = new Point(x, y);
        int count;

        if (! mVisitCounts.TryGetValue(p, out count)) {
            count = 0;
        }

        return count;
    }

    // �K�₵���}�X���L�^
    public void MarkVisit(int x, int y)
    {
        Point p = new Point(x, y);
        int count = 1;

        // �K��񐔂��X�V
        if (mVisitCounts.ContainsKey(p)) {
            count += mVisitCounts[p];
        }
        mVisitCounts[p] = count;

        // �ړ��o�H���X�V
        mPath.Add(p);
    }

// �f�[�^�����o
    List<Point>                    mPath;           // �v���C���[�̈ړ��o�H
    private Dictionary<Point, int> mVisitCounts;    // �e�}�X�̖K���
}
}


//
// End of File
//
