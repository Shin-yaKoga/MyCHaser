/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014-2015 Something Precious, Inc.
//
// FloorMap.cs: �T���ςݗ̈�ɑ΂���}�b�v��\�������N���X
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Dec. 07, 2014
// Last update: Jul. 29, 2015
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;


namespace CHaser
{
public class FloorMap {
// �񋓒l�^
    // �}�X�̏��
    public enum CellStat {
        UNKNOWN       = -1,  // ���m
        NONE          = 0,   // �����Ȃ�
        CHARACTER     = 1,   // �L�����N�^
        BLOCK         = 2,   // �u���b�N
        ITEM          = 3,   // �A�C�e��
            // �ȑO�ɒT���ς݂������ݏ�Ԃ͕s��
        WAS_NONE      = 10,  // �����Ȃ�����
        WAS_CHARACTER = 11,  // �L�����N�^�����݂���
        WAS_ITEM      = 13,  // �A�C�e�������݂���
    }

    // �ړ���T���̌���
    public enum Direction { Right, Left, Up, Down }

// ���J���\�b�h
    // �R���X�g���N�^
    public FloorMap() {
        mCurrPosX = mCurrPosY = 0;
        mWorld        = new PartialPlane<CellStat>(CellStat.UNKNOWN);
        mKnownRegions = new List<RectRegion>();
        mFootsteps    = new Footsteps();

        // �v���C���[�̏����ʒu�̃}�X�̏�Ԃ��Z�b�g
        mWorld.SetValueAt(0, 0, CellStat.NONE);
        mFootsteps.MarkVisit(0, 0);
    }

    // ����
    public int    CurrWidth { get { return mWorld.Width; } }
    public int    CurrHeight { get { return mWorld.Height; } }
    public int    DistanceToLeftEdge { get { return (mCurrPosX - mWorld.LeftBottomX); } }
    public int    DistanceToBottomEdge { get { return (mCurrPosY - mWorld.LeftBottomY); } }
    public int    DistanceToRightEdge { get { return (mWorld.LeftBottomX + mWorld.Width - mCurrPosX); } }
    public int    DistanceToTopEdge { get { return (mWorld.LeftBottomY + mWorld.Height - mCurrPosY); } }
    public int    CurrPosX { get { return mCurrPosX; } }
    public int    CurrPosY { get { return mCurrPosY; } }

    // �T���ςݗ̈�̍X�V
    public void    AddAround(byte[] cellStats, bool addToKnwonRegion)
    {
        // assertion
        if (null == cellStats || 9 != cellStats.Length) {
            System.Console.Out.WriteLine("illegal argument for AddAround().");
            return;
        }

        CellStat[] cStats = new CellStat[9];
        RectRegion reg = new RectRegion(mCurrPosX - 1, mCurrPosY - 1, 3, 3);

        for (int row = reg.height - 1, idx = 0; 0 <= row; --row) {
            int srcOffset = reg.width * row;

            for (int col = 0; col < reg.width; ++col) {
                cStats[idx++] = ToCellStat(cellStats[srcOffset + col]);
            }
        }
        cStats[4] = CellStat.NONE;  // �v���C���[�͏��O
        if (addToKnwonRegion) {
            this.AddRegion(reg, cStats);
        } else {
            this.ApplyRegionInfo(reg, cStats);
        }
    }
    public void AddAsLookResult(byte[] cellStats, Direction dir)
    {
        // assertion
        if (null == cellStats || 9 != cellStats.Length) {
            Console.Out.WriteLine("illegal argument for AddAround().");
            return;
        }

        CellStat[] cStats = new CellStat[9];
        RectRegion reg;
        int left, bottom;

        switch (dir) {
        case Direction.Right:
            left   = mCurrPosX + 1;
            bottom = mCurrPosY - 1;
            break;
        case Direction.Left:
            left   = mCurrPosX - 3;
            bottom = mCurrPosY - 1;
            break;
        case Direction.Up:
            left   = mCurrPosX - 1;
            bottom = mCurrPosY + 1;
            break;
        case Direction.Down:
            left   = mCurrPosX - 1;
            bottom = mCurrPosY - 3;
            break;
        default:
            Console.Out.WriteLine("unexpected Direction value: {0}", dir);
            throw (new Exception());
        }
        reg = new RectRegion(left, bottom, 3, 3);

        for (int row = reg.height - 1, idx = 0; 0 <= row; --row) {
            int srcOffset = reg.width * row;

            for (int col = 0; col < reg.width; ++col) {
                cStats[idx++] = ToCellStat(cellStats[srcOffset + col]);
            }
        }
        this.AddRegion(reg, cStats);
    }
    public void AddAsSearchResult(byte[] cellStats, Direction dir)
    {
        // assertion
        if (null == cellStats || 9 != cellStats.Length) {
            Console.Out.WriteLine("illegal argument for AddAround().");
            return;
        }

        CellStat[] cStats = new CellStat[9];
        RectRegion reg;

        switch (dir) {
        case Direction.Right:
            reg = new RectRegion(mCurrPosX + 1, mCurrPosY, 9, 1);
            for (int i = 0; i < reg.width; ++i) {
                cStats[i] = ToCellStat(cellStats[i]);
            }
            break;
        case Direction.Left:
            reg = new RectRegion(mCurrPosX - 9, mCurrPosY, 9, 1);
            for (int col = reg.width - 1, idx = 0; 0 <= col; --col) {
                cStats[idx++] = ToCellStat(cellStats[col]);
            }
            break;
        case Direction.Up:
            reg = new RectRegion(mCurrPosX, mCurrPosY + 1, 1, 9);
            for (int i = 0; i < reg.height; ++i) {
                cStats[i] = ToCellStat(cellStats[i]);
            }
            break;
        case Direction.Down:
            reg = new RectRegion(mCurrPosX, mCurrPosY - 9, 1, 9);
            for (int row = reg.height - 1, idx = 0; 0 <= row; --row) {
                cStats[idx++] = ToCellStat(cellStats[row]);
            }
            break;
        default:
            Console.Out.WriteLine("unexpected Direction value: {0}", dir);
            throw (new Exception());
        }
        this.AddRegion(reg, cStats);
    }

    // �v���C���[�̌��݈ʒu�ړ�
    public void    WalkTo(Direction dir)
    {
        int diffX = 0;
        int diffY = 0;

        // �K�v�Ȃ�T���ςݗ̈���g��
        switch (dir) {
        case Direction.Left:
            if (0 == this.DistanceToLeftEdge) {
                mWorld.ExpandHorizontal(1, false);
                break;
            }
            diffX = -1;
            break;
        case Direction.Right:
            if (1 == this.DistanceToRightEdge) {
                mWorld.ExpandHorizontal(1, true);
                break;
            }
            diffX = 1;
            break;
        case Direction.Down:
            if (0 == this.DistanceToBottomEdge) {
                mWorld.ExpandVertical(1, false);
                break;
            }
            diffY = -1;
            break;
        case Direction.Up:
            if (0 == this.DistanceToTopEdge) {
                mWorld.ExpandVertical(1, true);
                break;
            }
            diffY = 1;
            break;
        }

        // �v���C���[�̌��݈ʒu���W���X�V
        mCurrPosX += diffX;
        mCurrPosY += diffY;

        // �v���C���[�̈ړ��������X�V
        mFootsteps.MarkVisit(mCurrPosX, mCurrPosY);
    }

    // �}�X�̏�Ԃ̎擾
    public CellStat    GetCellStat(int relPosX, int relPosY)
    {
        return mWorld.GetValueAt(relPosX + mCurrPosX, relPosY + mCurrPosY);
    }
    public bool FindNearestTarget(CellStat target, out int relPosX, out int relPosY)
    {
        bool isFound = false;
        int left    = mCurrPosX - 1;
        int bottom  = mCurrPosY - 1;
        int size    = 2 + 1;

        // �o�͈�����������
        relPosX = relPosY = 0;

        // �v���C���[�̎��͂���O���Ɍ������Č���
        for (;;) {
            // �T���ςݗ̈悩�犮�S�Ɉ�E������I��
            if (! this.IsInsideOfWorld(left, bottom, size)) {
                 break;
            }

            // �v���C���[�̍����}�X���玞�v���ɑ���
            if (this.SearchOutskirts(target,
                left, bottom, size, ref relPosX, ref relPosY)) {
                isFound = true;  // ��������
                break;
            }

            // ���[�v�ϐ����X�V
            --left;
            --bottom;
            size += 2;
        }

        return isFound;
    }
    public int GetVisitCount(int relPosX, int relPosY)
    {
        return mFootsteps.GetVisitCount(relPosX + mCurrPosX, relPosY + mCurrPosY);
    }

    // ���̃^�[���̏���
    public void PrepareNextTurn()
    {
        // ���m�̈���X�V�^����������
        for (int i = 0; i < mKnownRegions.Count; ++i) {
            RectRegion  reg = mKnownRegions[i];

            if (this.UpdateRegion(ref reg)) {
                mKnownRegions.RemoveAt(i--);
            } else {
                mKnownRegions[i] = reg;
            }
        //
        // ���ӁFmKnownRegions �́AList �Ȃ̂ŁA[] ���Z�q�ɑ΂���
        //       ref �ŎQ�Ƃ����o�����Ƃ��ł��Ȃ��B���̂��߁A
        //       ��U���[�J���ϐ��l�ɃR�s�[���āA���e�ύX��ɏ���
        //       �߂��K�v������B('15. 1/24, koga)
        }
    }

    // �v���C���[���΍��W����}�b�v���W�ւ̕ϊ�
    public void ConvertToMapCoordinate(int relPosX, int relPosY,
        out int posX, out int posY)
    {
        posX = relPosX + mCurrPosX;
        posY = relPosY + mCurrPosY;
    }

// ����J���\�b�h
    private static CellStat ToCellStat(byte rawVal)
    {
        return (CellStat)(rawVal - (byte)'0');
    }

    private CellStat NextStat(CellStat currStat)
    {
        CellStat nextStat = currStat;

        switch (currStat) {
        case CellStat.NONE:
            nextStat = CellStat.WAS_NONE;
            break;
        case CellStat.CHARACTER:
            nextStat = CellStat.WAS_CHARACTER;
            break;
        case CellStat.ITEM:
            nextStat = CellStat.WAS_ITEM;
            break;
        }

        return nextStat;
    }

    private bool    UpdateRegion(ref RectRegion reg)
    {
        // �̈�̏�[�����
        if (this.OutdateRow(ref reg, reg.bottom + reg.height - 1)) {
            return true;
        }

        // �̈�̉��[�����
        if (this.OutdateRow(ref reg, reg.bottom++)) {
            return true;
        }

        // �̈�̉E�[�����
        if (this.OutdateCol(ref reg, reg.left + reg.width - 1)) {
            return true;
        }

        // �̈�̍��[�����
        if (this.OutdateCol(ref reg, reg.left++)) {
            return true;
        }

        return false;
    }
    private bool    OutdateRow(ref RectRegion reg, int y)
    {
        for (int x = reg.left, n = x + reg.width; x < n; ++x) {
            mWorld.SetValueAt(x, y, NextStat(mWorld.GetValueAt(x, y)));
        }

        return (0 == --(reg.height));
    }
    private bool    OutdateCol(ref RectRegion reg, int x)
    {
        for (int y = reg.bottom, n = y + reg.height; y < n; ++y) {
            mWorld.SetValueAt(x, y, NextStat(mWorld.GetValueAt(x, y)));
        }

        return (0 == --(reg.width));
    }

    private void    AddRegion(RectRegion region, CellStat[] cellStats)
    {
        // �̈�̓��e���}�b�v�ɔ��f
        this.ApplyRegionInfo(region, cellStats);

        // �̈�����m�̈惊�X�g�ɒǉ�
        mKnownRegions.Add(region);
    }
    private void    ApplyRegionInfo(RectRegion region, CellStat[] cellStats)
    {
        int regRight = region.left + region.width;
        int regTop   = region.bottom + region.height;

        // �K�v�Ȃ琅�������Ɋg��
        if (region.left < mWorld.LeftBottomX) {
            mWorld.ExpandHorizontal(mWorld.LeftBottomX - region.left, false);
        }
        if (mWorld.LeftBottomX + mWorld.Width < regRight) {
            mWorld.ExpandHorizontal(
                regRight - (mWorld.LeftBottomX + mWorld.Width), true);
        }

        // �K�v�Ȃ琂�������Ɋg��
        if (region.bottom < mWorld.LeftBottomY) {
            mWorld.ExpandVertical(mWorld.LeftBottomY - region.bottom, false);
        }
        if (mWorld.LeftBottomY + mWorld.Height < regTop) {
            mWorld.ExpandVertical(
                regTop - (mWorld.LeftBottomY + mWorld.Height), true);
        }

        // �}�X�̏�Ԃ��X�V
        for (int row = 0, idx = 0; row < region.height; ++row) {
            for (int col = 0; col < region.width; ++col) {
                mWorld.SetValueAt(region.left + col, region.bottom + row,
                    cellStats[idx++]);
            }
        }
    }

    private bool    IsInsideOfWorld(int left, int bottom, int size)
    {
        // assertion
        if ((left + size <= mWorld.LeftBottomX)
        || (mWorld.LeftBottomX + mWorld.Width <= left)
        || (bottom + size <= mWorld.LeftBottomY)
        || (mWorld.LeftBottomY + mWorld.Height <= bottom)) {
            Console.Out.WriteLine("illegal argument for IsInsideOfWorld().");
            return false;
        }

        return ((left >= mWorld.LeftBottomX)
            || (mWorld.LeftBottomX + mWorld.Width >= left + size)
            || (bottom >= mWorld.LeftBottomY)
            || (mWorld.LeftBottomY + mWorld.Height >= bottom + size));
    }
    private bool    SearchOutskirts(CellStat target,
        int left, int bottom, int size, ref int relPosX, ref int relPosY)
    {
        int x, y, n;

        // �O���̍��ӂ𑖍�
        x = left;
        for (y = bottom, n = y + size; y < n; ++y) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // ��������
            }
        }

        // �O���̏�ӂ𑖍�
        y = bottom + size - 1;
        for (x = left + 1, n = left + size; x < n; ++x) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // ��������
            }
        }

        // �O���̉E�ӂ𑖍�
        x = left + size - 1;
        for (y = bottom + size - 1; bottom <= y; --y) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // ��������
            }
        }

        // �O���̉��ӂ𑖍�
        y = bottom;
        for (x = left + size - 1; left <= x; --x) {
            if (mWorld.GetValueAt(x, y) == target) {
                this.ToRelPos(x, y, ref relPosX, ref relPosY);
                return true;  // ��������
            }
        }

        return false;
    }
    private void    ToRelPos(int x, int y, ref int relPosX, ref int relPosY)
    {
        relPosX = x - mCurrPosX;
        relPosY = y - mCurrPosY;
    }

// �����^
    // �����̈�
    struct RectRegion {
        public int    left;
        public int    bottom;
        public int    width;
        public int    height;

    // �R���X�g���N�^
        public RectRegion(int l, int b, int w, int h)
        {
            left   = l;
            bottom = b;
            width  = w;
            height = h;
        }
    }

// �f�[�^�����o
    private int                    mCurrPosX;     // �v���C���[�̌��݈ʒu���W�ix�����W�l�j
    private int                    mCurrPosY;     // �v���C���[�̌��݈ʒu���W�iy�����W�l�j
    private PartialPlane<CellStat> mWorld;        // �T���ςݗ̈�̊O�ڋ�`
    private List<RectRegion>       mKnownRegions; // �L���ȕ����T���̈�̗�
    private Footsteps              mFootsteps;    // �v���C���[�̈ړ�����
}
}

//
// End of File
//
