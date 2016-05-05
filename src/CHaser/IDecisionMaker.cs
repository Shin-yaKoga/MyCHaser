/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// IDecisionMaker.cs: ���Ɏ��s����R�}���h�𓱏o����C���^�t�F�[�X
//
// Author:      Shin-ya Koga (koga@stprec.co.jp)
// Created:     Jan. 16, 2015
// Last update: Jan. 16, 2015
/////////////////////////////////////////////////////////////////////////

namespace CHaser
{
public interface IDecisionMaker {
    // ���Ɏ��s����A�N�V�����̌���
    Command    DecideNextAction(FloorMap map);
}
}

//
// End of File
//
