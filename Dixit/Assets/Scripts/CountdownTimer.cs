﻿using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountdownTimer : NetworkBehaviour
{
    [SerializeField] private Text timerTextField;
    public UnityEvent OnTimeoutGiveAnswer = new UnityEvent();
    public UnityEvent OnTimeoutSelectAnswer = new UnityEvent();
    public UnityEvent OnTimeoutScoreScreen = new UnityEvent();
    public UnityEvent OnTimeoutDefault = new UnityEvent();
    private int _timer;

    public enum timerModes
    {
        giveAnswer,
        selectAnswer,
        scoreScreen,
        defaultMode
    }

    private timerModes timerMode;

    [Server]
    private void passSecond()
    {
        timerTextField.text = _timer + "s";
        RpcUpdateTimerTextfield(_timer);
        if (_timer <= 0)
        {
            CancelInvoke(nameof(passSecond));
            switch (timerMode)
            {
                case timerModes.giveAnswer:
                    OnTimeoutGiveAnswer.Invoke();
                    break;
                case timerModes.selectAnswer:
                    OnTimeoutSelectAnswer.Invoke();
                    break;
                case timerModes.scoreScreen:
                    OnTimeoutScoreScreen.Invoke();
                    break;
                case timerModes.defaultMode:
                    OnTimeoutDefault.Invoke();
                    break;
            }
        }
        else
        {
            _timer--;
        }
    }

    [Server]
    public void StartTimer(int startingTime, timerModes timerMode = timerModes.defaultMode)
    {
        if (_timer == 0)
        {
            this.timerMode = timerMode;
            _timer = startingTime;
            timerTextField.text = _timer + "s";
            if (isServer)
            {
                InvokeRepeating(nameof(passSecond), 0f, 1f);
            }
        }
    }
    [Server]
    public void StopTimer()
    {
        _timer = 0;
    }

    [ClientRpc]
    private void RpcUpdateTimerTextfield(int timer)
    {
        timerTextField.text = timer + "s";
    }
}