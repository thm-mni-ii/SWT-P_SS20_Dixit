using System.Collections;
using UnityEngine;
using Mirror;

public class Card : NetworkBehaviour
{
    [SerializeField] private Animator anim;

    [SyncVar, HideInInspector]
    public string text;
    [SyncVar, HideInInspector]
    public NetworkIdentity choosen;

    [SyncVar, HideInInspector]
    public CardType type;
    public enum CardType {Input, Question, Answer};

    public Material correctColour;
    private Vector3 _slideVector;

    [SyncVar, HideInInspector]
    public bool startFacedown = false;

    public override void OnStartClient(){
        gameObject.tag = type + "Card";
        if (startFacedown)
        {
            InstantFlipFacedown();
        }

        switch (type)
        {
            case CardType.Input:
                {
                    var textTransform =  GetComponentInChildren<Transform>().Find("Card").GetComponentInChildren<Transform>();
                    textTransform.Find("WriteAnswer").gameObject.SetActive(true);
                    textTransform.Find("SelectAnswer").gameObject.SetActive(false);
                    break;
                }
            case CardType.Question:
                {
                    GetComponentInChildren<Transform>().Find("Text (TMP)").GetComponent<TMPro.TMP_Text>().text = text;
                    break;
                }
            case CardType.Answer:
                {
                    var textTransform = GetComponentInChildren<Transform>().Find("Card").GetComponentInChildren<Transform>();
                    textTransform.Find("WriteAnswer").gameObject.SetActive(false);
                    textTransform.Find("SelectAnswer").gameObject.SetActive(true);


                    textTransform.Find("SelectAnswer").gameObject.GetComponentInChildren<Transform>()
                        .Find("Text (TMP)").GetComponent<TMPro.TMP_Text>().text = text;
                    break;
                }
            default:
                break;
        }
    }

    public void InstantFlipFacedown()
    {
        anim.Play("InstantFlipFacedown");
    }

    public void InstantFlipFaceup()
    {
        anim.Play("InstantFlipFaceup");
    }
    
    public void FlipFacedown()
    {
        anim.Play("FlipFacedown");
    }

    public void FlipFaceup()
    {
        anim.Play("FlipFaceup");
    }
    
    public void FlipFacedown(float time)
    {
        Invoke(nameof(FlipFacedown),time);
    }

    public void FlipFaceup(float time)
    {
        Invoke(nameof(FlipFaceup),time);
    }
    
    [ClientRpc]
    public void RpcFlip(bool toFacedown, bool instantly, float time)
    {
        if (toFacedown)
        {
            if (instantly)
            {
                InstantFlipFacedown();
            }
            else
            {
                FlipFacedown(time);
            }
        }
        else
        {
            if (instantly)
            {
                InstantFlipFaceup();
            }
            else
            {
                FlipFaceup(time);
            }
        }
    }

    [ClientRpc]
    public void RpcSlideToPosition(Vector3 vector3)
    {
        _slideVector = vector3;
        StartCoroutine("SlideToPosition");
    }

    private IEnumerator SlideToPosition()
    {
        while (transform.position != _slideVector)
        {
            transform.position = Vector3.Lerp(transform.position, _slideVector,Time.deltaTime*10);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void HighlightCard(Material highlighted)
    {
        GetComponentInChildren<Transform>().Find("Card").GetComponent<MeshRenderer>().material = highlighted;
    }

    public void HighlightCorrect()
    {
        HighlightCard(correctColour);
    }
}
