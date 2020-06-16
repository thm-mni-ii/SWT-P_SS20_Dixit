/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Firebase.Extensions;


/// <summary>
/// The GameManger keeps track of all given Answers and Changes the curren Phase of the Game.
/// It only exists on the Server.
/// </summary>
public class GameManager : NetworkBehaviour
{
    private Dictionary<NetworkIdentity, string> answers = new Dictionary<NetworkIdentity, string>();
    private Dictionary<NetworkIdentity, int> choices = new Dictionary<NetworkIdentity, int>();

    private NetworkManager networkManager;

    public GameObject m_cardPrefab;
    public GameObject m_questionCardPrefab;

    private enum Phase { WriteAnswer, ChoseAnswer }
    private Phase currentPhase;

    //Will be set by Game Host later on
    public string questionSetID = "0";
    public QuestionSet questionSet;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    public override void OnStartServer()
    {
        networkManager = NetworkManager.singleton;

        // Initializes QuestionSet from given ID
        QuestionSet.RetrieveQuestionSet(questionSetID, GetComponent<DatabaseSetup>().db).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else
            {
                questionSet = task.Result;
                //How to get actual question text: questionSet.GetQuestion(0).ContinueWith(l => Debug.Log(l.Result.QuestionText));
                StartRound();  
            }

        }).ContinueWith( a => {
            if (a.IsFaulted)
            {
                Debug.LogException(a.Exception);
            }
        }); 
             
    }

    private void StartRound(){

        currentPhase = Phase.WriteAnswer;
        
        questionSet.GetQuestion(0).ContinueWithOnMainThread(l => {
            Debug.Log(l.Result.QuestionText);
            WriteAnswerPhase(l.Result);  
        }).ContinueWith( a => {
            if (a.IsFaulted)
            {
                Debug.LogException(a.Exception);
            }
        });
       
    }

    private void WriteAnswerPhase(Question question){
        //render question cards at client
        var questionCard = (GameObject) Instantiate (m_questionCardPrefab, new Vector3(0, 100, -1),  Quaternion.identity);
        
        questionCard.GetComponentInChildren<Transform>().Find("Text (TMP)").GetComponent<TMPro.TMP_Text>().text = question.QuestionText;
        questionCard.gameObject.SetActive(true);
       
        NetworkServer.Spawn(questionCard);


        //render input card at client
        var card = (GameObject) Instantiate (m_cardPrefab, new Vector3( 0 , -100, -1), Quaternion.identity);

        var textTransform =  card.GetComponentInChildren<Transform>().Find("Card").GetComponentInChildren<Transform>();
        textTransform.Find("WriteAnswer").gameObject.SetActive(true);
        textTransform.Find("SelectAnswer").gameObject.SetActive(false);
            
        card.gameObject.SetActive(true);

        NetworkServer.Spawn(card);

        //ToDo: start timer

        //wait for all players to send answer or get timeout

    }

    /// <summary>
    /// Gets the list of Players in the current Game.
    /// <returns>The list of players.</returns>
    /// </summary>
    private List<Player> GetPlayers()
    {
        return NetworkServer.connections.Values.Select(c => c.identity.gameObject.GetComponent<Player>()).ToList();
    }


    /// <summary>
    /// Logs the given Answer of a Player during the WriteAnwer Phase.
    /// </summary>
    public void LogAnswer(NetworkIdentity player, string answer)
    {
        answers.Add(player, answer);
        Debug.Log(answer);

        if (answers.Count == networkManager.numPlayers)
        {
            ChangePhase();
        }
    }

    /// <summary>
    /// Logs the choosen Answer of a Player during the ChoseAnswer Phase.
    /// </summary>
    public void LogAnswer(NetworkIdentity player, int choice)
    {
        choices.Add(player, choice);
        if (answers.Count == networkManager.numPlayers)
        {
            ChangePhase();
        }
    }

    /// <summary>
    /// Changes Phase in the GameManager and calls the Corresponding RPC-Call
    /// </summary>
    private void ChangePhase()
    {
        switch (currentPhase)
        {
            case Phase.WriteAnswer:
                currentPhase = Phase.ChoseAnswer;
                break;
            case Phase.ChoseAnswer:
                currentPhase = Phase.WriteAnswer;
                break;
            default:
                break;
        }
        SendAnswers();
    }

    private void SendAnswers(){
        var answerTexts = answers.Values.ToArray();

        var startX = (answerTexts.Length * 125 + (answerTexts.Length -1) * 20) / 2;

        for(int i = 0; i < answerTexts.Length; i++)
        {
            var xPosition = startX -  62.5 - i * 145;

            var card = (GameObject) Instantiate (m_cardPrefab, new Vector3( (float)  xPosition , -100, -2), Quaternion.Euler(0,0,0));
            
            var textTransform =  card.GetComponentInChildren<Transform>().Find("Card").GetComponentInChildren<Transform>();
            textTransform.Find("WriteAnswer").gameObject.SetActive(false);
            textTransform.Find("SelectAnswer").gameObject.SetActive(true);


            textTransform.Find("SelectAnswer").gameObject.GetComponentInChildren<Transform>()
                .Find("Text (TMP)").GetComponent<TMPro.TMP_Text>().text = answerTexts[i];
                
            card.gameObject.SetActive(true);

            NetworkServer.Spawn(card);
        }
        
    }
}
