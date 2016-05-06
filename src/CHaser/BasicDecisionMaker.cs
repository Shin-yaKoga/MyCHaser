/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// BasicDecisionMaker.cs: DecisionMaker �C���^�t�F�[�X�̊�{����
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Jan. 12, 2015
// Last update: Jul. 31, 2015
/////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;


namespace CHaser
{
public class BasicDecisionMaker : DecisionMaker {
    // �R���X�g���N�^
    public BasicDecisionMaker() {}

// ����J���\�b�h
    // DecisionMaker �C���^�t�F�[�X�̎���
    protected override List<IActionSelector> CreateActionSelectors()
    {
        List<IActionSelector>    selectors = new List<IActionSelector>(3);

        selectors.Add(new AdjacentCharacter());
        selectors.Add(new AdjacentBlock());
        selectors.Add(new AdjacentItem());
        selectors.Add(new PreferFirstTime());

        return selectors;
    }

    // ���N���X�̐U�镑���̃J�X�^�}�C�Y
        // �ŏI���̑I��
    protected override void SelectFinalCandidate(
        List<Command> ioCandidates, FloorMap map)
    {
        if (null != mLastAction) {
            // �O����s�����A�N�V��������ړ��̏ꍇ�A������������`�F�b�N
            if (!mLastAction.IsWalk && mLastAction == ioCandidates[0]) {
                // �����ꍇ�A�ړ��A�N�V������I���\�Ȃ�I��
                foreach (Command c in ioCandidates) {
                    if (c.IsWalk) {
                        ioCandidates.Clear();
                        ioCandidates.Add(c);
                        return;  // ��������
                    }
                }
            } else if (mLastAction.IsWalk && !ioCandidates[0].IsWalk
                && !ioCandidates[0].IsPut) {
                // �O����s�����A�N�V�������ړ��ŁA����͒T�������̏ꍇ�͒���
                foreach (Command c in ioCandidates) {
                    if (mLastAction.GetDir() == c.GetDir()) {
                        ioCandidates.Clear();
                        ioCandidates.Add(c);
                        return;  // ��������
                    }
                }
            }
        }

        // �f�t�H���g�ł͊��N���X�Ɠ����U�镑��
        base.SelectFinalCandidate(ioCandidates, map);
    }
}
}

//
// End of File
//
