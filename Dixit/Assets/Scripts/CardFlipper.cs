using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlipper : MonoBehaviour
{
    [SerializeField] private Animator anim;

    public void Flip()
    {
        anim.Play("FlipCardChild");
    }
    public void Unflip()
    {
        anim.Play("UnflipCardChild");
    }
}
