using System;
using UnityEngine;
using System.Collections.Generic;
using Mirror;
using UnityEngine.UI;

public class SyncTimer : NetworkBehaviour
{
	[SerializeField][SyncVar]
	public int time = 0;
	void Start()
	{
		if (isServer)
		{
			InvokeRepeating(nameof(passSecond),0f,1f);
		}
	}

	void passSecond()
	{
		time++;
	}
	
}
