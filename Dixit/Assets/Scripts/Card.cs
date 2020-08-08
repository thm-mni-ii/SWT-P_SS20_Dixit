using System;
using System.Collections;
using UnityEngine;
using Mirror;

    /// <summary>
    /// Represents a card on the playing field.
    /// A card could have the question written on it, or a possibly correct answer, or it could be the text field where players write their answers.
    /// </summary>
public class Card : NetworkBehaviour
{
    [SerializeField] private Animator anim;

    /// <summary>
    /// The text written on this card
    /// </summary>
    [SyncVar, HideInInspector]
    public string text;
    /// <summary>
    /// If it is has a possible answer written on it, the <c>netId</c> of the player who gave it
    /// </summary>
    [SyncVar, HideInInspector]
    public UInt32 id;

    /// <summary>
    /// Type of a Card.
    /// A card can be a Question card where the current question is displayed,
    /// an answer card where a possible answer written on or an input card on which players write there own answers.
    /// </summary>
    [SyncVar, HideInInspector]
    public CardType type;
    public enum CardType { Input, Question, Answer };

    /// <summary>
    /// The colour the card turns into when the answer it displayed was correct
    /// </summary>
    public Material correctColour;
    /// <summary>
    /// The default colour of the card
    /// </summary>
    public Material defalutColor;
    private Vector3 _slideVector;

    [SyncVar, HideInInspector]
    public bool startFacedown = false;

    [SerializeField]
    private GameObject cardGo;
    [SerializeField]
    private GameObject writeAnswerGo;
    [SerializeField]
    private GameObject selectAnswerGo;
    [SerializeField]
    private TMPro.TMP_Text selectAnswerText;
    [SerializeField]
    private GameObject selectAnswerBtn;
    [SerializeField]
    private TMPro.TMP_Text questionText;

    public override void OnStartClient()
    {
        gameObject.tag = type + "Card";
        if (startFacedown)
        {
            InstantFlipFacedown();
        }

        switch (type)
        {
            case CardType.Input:
                {
                    writeAnswerGo.SetActive(true);
                    selectAnswerGo.SetActive(false);
                    break;
                }
            case CardType.Question:
                {
                    questionText.text = text;
                    break;
                }
            case CardType.Answer:
                {
                    writeAnswerGo.SetActive(false);
                    selectAnswerGo.SetActive(true);

                    selectAnswerText.text = text;
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
        Invoke(nameof(FlipFacedown), time);
    }

    public void FlipFaceup(float time)
    {
        Invoke(nameof(FlipFaceup), time);
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
            transform.position = Vector3.Lerp(transform.position, _slideVector, Time.deltaTime * 10);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void HighlightCard(Material highlighted)
    {
        cardGo.GetComponent<MeshRenderer>().material = highlighted;
    }

    public void HighlightCorrect()
    {
        HighlightCard(correctColour);
    }

    public void HighlightReset()
    {
        HighlightCard(defalutColor);
    }

    public void DisableSelectInput()
    {
        selectAnswerBtn.SetActive(false);
    }
}
