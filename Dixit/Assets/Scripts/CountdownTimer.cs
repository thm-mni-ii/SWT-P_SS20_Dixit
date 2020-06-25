using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountdownTimer : NetworkBehaviour
{
    [SerializeField] private Text timerTextField;
    public UnityEvent OnTimeout = new UnityEvent();
    private int _timer;

    [Server]
    private void passSecond()
    {
        _timer--;
        timerTextField.text = _timer + "s";
        RpcUpdateTimerTextfield(_timer);
        if (_timer <= 0)
        {
            CancelInvoke(nameof(passSecond));
            OnTimeout.Invoke();
        }
    }

    [Server]
    public void StartTimer(int startingTime)
    {
        _timer = startingTime;
        timerTextField.text = _timer + "s";
        if (isServer)
        {
            InvokeRepeating(nameof(passSecond), 0f, 1f);
        }
    }

    [ClientRpc]
    private void RpcUpdateTimerTextfield(int timer)
    {
        timerTextField.text = timer + "s";
    }

}