﻿/* created by: SWT-P_SS_20_Dixit */

using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A timer that can be run in multiple modes, different events are triggered based on what mode it's in.
/// Only a single timer can run at once.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class CountdownTimer : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] timerTextFields;

    /// <summary>
    /// A Unity Event for the timeout in the giving answer phase.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public UnityEvent OnTimeoutGiveAnswer = new UnityEvent();

    /// <summary>
    /// A Unity Event for the timeout in the select answer phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public UnityEvent OnTimeoutSelectAnswer = new UnityEvent();

    /// <summary>
    /// A Unity Event for the timeout of the timer in the score screen.
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
    private void PassSecond()
    {
        RpcUpdateTimerTextfield(_timer);
        if (_timer <= 0)
        {
            CancelInvoke(nameof(PassSecond));
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
            RpcUpdateTimerTextfield(_timer);
            if (isServer)
            {
                InvokeRepeating(nameof(PassSecond), 0f, 1f);
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
        foreach (var textField in timerTextFields)
        {
            textField.text = timer + "s";
        }
    }
}
