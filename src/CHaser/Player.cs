/////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Something Precious, Inc.
//
// Player.cs: CHaser �N���C�A���g�̃��C���N���X
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
    // �������Ɖ��
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
        // �X���b�h�����쒆�Ȃ��~
        if (this.IsRunning) {
            this.Stop();
        }
    }

    // ����
    public bool    IsRunning { get { return (null != mThread); } }
    public bool    IsThreadAlive { get {
        return (null != mThread && mThread.IsAlive); } }

    // �N���C�A���g����X���b�h�̊J�n�ƒ�~
    public bool    Start(string svrAddr, bool isCool)
    {
        // assertion
        if (this.IsRunning) {
            Console.Out.WriteLine("illegal argument for Start().");
            return false;
        }

        // �T�[�o�Ƃ̒ʐM�I�u�W�F�N�g�𐶐����ăT�[�o�ɐڑ�
        mServer = new ServerProxy(
            svrAddr, (isCool ? 40000 : 50000), mTeamName);
        if (! mServer.IsValid) {
            Console.Out.WriteLine("failed to connecting to the server.");
            mServer = null;
            return false;
        }

        // �N���C�A���g������s�p�̃I�u�W�F�N�g�𐶐�
        mDecisionMaker = this.CreateDecisionMaker();
        mFloorMap = new FloorMap();

        // �N���C�A���g����X���b�h�𐶐��E�n��
        mQuitRequested = false;
        mThread = new Thread(new ThreadStart(this.ThreadProc));
        mThread.Start();

        return true;
    }
    public void    Stop()
    {
        if (this.IsRunning) {
            // �N���C�A���g����X���b�h�ɏI���ʒm
            mQuitRequested = true;

            // �X���b�h�̏I����҂�
            mThread.Join();

            // �N���C�A���g������s�p�̃I�u�W�F�N�g�����
            mServer.Dispose();
            mServer        = null;
            mDecisionMaker = null;
            mFloorMap      = null;
            mThread        = null;  // �X���b�h�̒�~���L�^
        }
    }

// ����J���\�b�h
    // �h���N���X�ł̃J�X�^�}�C�Y�p [virtual]
    protected virtual IDecisionMaker CreateDecisionMaker()
    {
        return (new BasicDecisionMaker());
    }

    // �N���C�A���g�X���b�h����
    private void    ThreadProc()
    {
        byte[]  respBuf = new byte[9];
        byte    respCode;

        // �I���ʒm�����܂ŌJ��Ԃ�
        while (! mQuitRequested) {
            Command nextAction;

            // �^�[���̎��s����
            if (! mServer.BeginOneTurn(out respCode, respBuf)) {
                Console.Out.WriteLine("failed to BeginOneTurn().");
                break;  // �ʐM�G���[
            } else if ('0' == respCode) {
                break;  // �T�[�o����I���ʒm���ꂽ
            }

            // �󂯎�������͏����}�b�v�ɔ��f
            mFloorMap.AddAround(respBuf, true);

            // ���s����A�N�V����������
            nextAction = mDecisionMaker.DecideNextAction(mFloorMap);
            if (null == nextAction) {
                Console.Out.WriteLine("failed to DecideNextAction().");
                break;
            }

            // �^�[�����s������
            if (! mServer.EndOneTurn(nextAction.Method, out respCode, respBuf)) {
                Console.Out.WriteLine("failed to EndOneTurn().");
                break;  // �ʐM�G���[
            } else if ('0' == respCode) {
                break;  // �T�[�o����I���ʒm���ꂽ
            }

            // walk �̏ꍇ�� WalkTo() �Ń}�b�v��̈ʒu���X�V
            if (nextAction.IsWalk) {
                mFloorMap.WalkTo(nextAction.GetWalkDir());
            }

            // ���s�����A�N�V�����̌��ʂ��}�b�v�ɔ��f
            if (nextAction.IsLook) {
                mFloorMap.AddAsLookResult(respBuf, nextAction.GetDir());
            } else if (nextAction.IsSearch) {
                mFloorMap.AddAsSearchResult(respBuf, nextAction.GetDir());
            } else {
                mFloorMap.AddAround(respBuf, false);
            //
            // ���ӁF�A�N�V�������s��Ɏ󂯎�������͗̈�́A����ƑS�������̈��
            //       ����̃^�[�����s�O�Ɏ󂯎��B�]���āA���m�̈�Ƃ��ă}�b�v��
            //       �o�^������K�v���Ȃ��B���̂��߁AAddAround() �̑�������
            //       false �ɂ���B('15. 1/24, koga@stprec.co.jp)
            }

            // ���̃^�[���̏����i���m�̈���X�V�^�����j
            mFloorMap.PrepareNextTurn();
        }

        // �T�[�o�Ƃ̐ڑ������
        mServer.Dispose();
    }

// �f�[�^�����o
    private byte[]         mTeamName;      // �`�[����
    private bool           mQuitRequested; // �I���v�����ꂽ��
    private Thread         mThread;        // �X���b�h
    private ServerProxy    mServer;        // �T�[�o�Ƃ̒ʐM�I�u�W�F�N�g
    private IDecisionMaker mDecisionMaker; // �A�N�V��������I�u�W�F�N�g
    private FloorMap       mFloorMap;      // �}�b�v
}
}

//
// End of File
//
