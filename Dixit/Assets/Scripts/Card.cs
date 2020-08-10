/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// Represents a card on the playing field.
/// A card could have the question written on it, or a possibly correct answer, or it could be the text field where players write their answers.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class Card : NetworkBehaviour
{
    /// <summary>
    /// The Animator playing the animations of the card
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [SerializeField] private Animator anim;

    /// <summary>
    /// The text written on this card
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [SyncVar, HideInInspector]
    public string text;

    /// <summary>
    /// If it is has a possible answer written on it, the <c>netId</c> of the player who gave it
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [SyncVar, HideInInspector]
    public UInt32 id;

    /// <summary>
    /// The Type of the Card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [SyncVar, HideInInspector]
    public CardType type;

    /// <summary>
    /// Type of a Card.
    /// A card can be a Question card where the current question is displayed,
    /// an answer card where a possible answer written on or an input card on which players write there own answers.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public enum CardType { Input, Question, Answer };

    /// <summary>
    /// The colour the card turns into when the answer it displayed was correct
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Material correctColour;

    /// <summary>
    /// The default colour of the card
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Material defalutColor;
    private Vector3 _slideVector;

    /// <summary>
    /// Defines the rotation of the card at beginning.
    /// The default value is false, so it starts frace up.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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

    /// <summary>
    /// On Client Start the Card will be identified and the needed Compentens will be activated, all other deactivated.
    /// If the card lies facedown at the beginnig, it will be fliped instantly.
    /// On question and answer cards the given text will be written.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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

    /// <summary>
    /// A instant flip facedown of the card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void InstantFlipFacedown()
    {
        anim.Play("InstantFlipFacedown");
    }

   /// <summary>
    /// A instant flip faceup of the card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void InstantFlipFaceup()
    {
        anim.Play("InstantFlipFaceup");
    }

    /// <summary>
    /// An animated flip facedown of the card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void FlipFacedown()
    {
        anim.Play("FlipFacedown");
    }

    /// <summary>
    /// An animated flip faceup of the card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void FlipFaceup()
    {
        anim.Play("FlipFaceup");
    }

    /// <summary>
    /// An animated flip facedown of the card.
    /// </summary>
    /// <param name="time"> Defines the delay until the animation starts <\param>
    /// \author SWT-P_SS_20_Dixit
    public void FlipFacedown(float time)
    {
        Invoke(nameof(FlipFacedown), time);
    }

    /// <summary>
    /// An animated flip faceup of the card.
    /// </summary>
    /// <param name="time"> Defines the delay until the animation starts <\param>
    /// \author SWT-P_SS_20_Dixit
    public void FlipFaceup(float time)
    {
        Invoke(nameof(FlipFaceup), time);
    }

    /// <summary>
    /// A flip of the card.
    /// </summary>
    /// <param name="toFacedown"> Defines the rotaion of the flip. If it is true the card flipes from faceup to facedown, else the other way around. <\param>
    /// <param name="instantly"> Defines wheater the flip in instantly or animated. <\param>
    /// <param name="time"> Defines the delay until the animation starts. <\param>
    /// \author SWT-P_SS_20_Dixit
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

    /// <summary>
    /// An animated slide of a card to a given postion.
    /// </summary>
    /// <param name="vector3"> Defines the postion slied to. <\param>
    /// \author SWT-P_SS_20_Dixit
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

    /// <summary>
    /// Highlights a card with the given material.
    /// </summary>
    /// <param name="highlighted"> Defines the material set onto the card. <\param>
    /// \author SWT-P_SS_20_Dixit
    public void HighlightCard(Material highlighted)
    {
        cardGo.GetComponent<MeshRenderer>().material = highlighted;
    }

    /// <summary>
    /// Highlights the correct answer card with the color defined in the <see cref="correctColour">correctColour</see>.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void HighlightCorrect()
    {
        HighlightCard(correctColour);
    }

    /// <summary>
    /// Resets the color of the card to the <see cref="defalutColor">defalutColor</see>.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void HighlightReset()
    {
        HighlightCard(defalutColor);
    }

    /// <summary>
    /// Disables the selction of a card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void DisableSelectInput()
    {
        selectAnswerBtn.SetActive(false);
    }
}
