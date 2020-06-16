using UnityEngine;
using Mirror;

public class Card : NetworkBehaviour
{
    [SerializeField] private Animator anim;

    [SyncVar]
    public string text;
    [SyncVar]
    public NetworkIdentity choosen;

    [SyncVar]
    public CardType type;
    public enum CardType {Input, Question, Answer};

    public override void OnStartClient(){
        gameObject.tag = type + "Card";

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

    public void Flip()
    {
        anim.Play("FlipCardChild");
    }

    public void Unflip()
    {
        anim.Play("UnflipCardChild");
    }
}
