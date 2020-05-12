using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RandomizePosition : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(randPos),0f,2f);   
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            transform.rotation = transform.rotation*Quaternion.Euler(0f,1.0f,0f);
        }
    }

    void randPos()
    {
        if (isLocalPlayer)
        {
            transform.position = new Vector3(Random.Range(-4f,4f),Random.Range(-4f,4f),0f);
        }
    }
}
