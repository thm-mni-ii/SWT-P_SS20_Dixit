﻿/* created by: SWT-P_SS_20_Dixit */
using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// A timer that can be run in multiple modes, different events are triggered based on what mode it's in.
/// Only a single timer can run at once.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class CountdownTimer : NetworkBehaviour
{
    [SerializeField] private Text timerTextField;

    /// <summary>
    /// A Unity Event for the timeout in the giving anwer phase.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public UnityEvent OnTimeoutGiveAnswer = new UnityEvent();

    /// <summary>
    /// A Unity Event for the timeout in the select anwer phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public UnityEvent OnTimeoutSelectAnswer = new UnityEvent();

    /// <summary>
    /// A Unity Event for the timeout of the timer in the scroe screen.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public UnityEvent OnTimeoutScoreScreen = new UnityEvent();

    /// <summary>
    /// A Unity Event for the default timeout.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public UnityEvent OnTimeoutDefault = new UnityEvent();
    private int _timer;

    /// <summary>
    /// The Timer Modes.
    /// The differnt Timers are:  giveAnswer, selectAnswer, scoreScreen and a default timer.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public enum timerModes
    {
        giveAnswer,
        selectAnswer,
        scoreScreen,
        defaultMode
    }

    private timerModes timerMode;

    /// <summary>
    /// This method is called every second once the timer starts
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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

    /// <summary>
    /// This method starts the timer at <c>startingTime</c> in seconds in the mode <c>timerMode</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
    /// <summary>
    /// Sets the timer to 0 causing the timeout event to be triggered
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
