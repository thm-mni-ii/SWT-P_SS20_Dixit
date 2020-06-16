/* created by: SWT-P_SS_20_Dixit */
using System.Threading.Tasks;
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

    private enum Phase { WriteAnswer, ChoseAnswer, Evaluation }
    private Phase currentPhase;

    //Will be set by Game Host later on
    public string questionSetID = "0";
    public QuestionSet questionSet;

    private Task loadQuestionSet;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    public override void OnStartServer()
    {
        networkManager = NetworkManager.singleton;

        // Initializes QuestionSet from given ID
        loadQuestionSet = QuestionSet.RetrieveQuestionSet(questionSetID, GetComponent<DatabaseSetup>().db).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else
            {
                questionSet = task.Result;
                //How to get actual question text: questionSet.GetQuestion(0).ContinueWith(l => Debug.Log(l.Result.QuestionText));
               
            }

        }).ContinueWith( a => {
            if (a.IsFaulted)
            {
                Debug.LogException(a.Exception);
            }
        }); 
             
    }
    public void StartGame(){
        
        loadQuestionSet.ContinueWithOnMainThread(t => StartRound());
          
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

    private void WriteAnswerPhase(Question question)
    {
        //render question cards at client
        var cardGo = Instantiate(m_questionCardPrefab, new Vector3(0, 100, -1),  Quaternion.identity);
        var card = cardGo.GetComponent<Card>();
        card.text = question.QuestionText;
        card.type = Card.CardType.Question;
      
        NetworkServer.Spawn(cardGo);


        //render input card at client
        cardGo = Instantiate(m_cardPrefab, new Vector3( 0 , -100, -1), Quaternion.identity);
        card = cardGo.GetComponent<Card>();
        card.text = question.QuestionText;
        card.type = Card.CardType.Input;
       
        NetworkServer.Spawn(cardGo);

        //ToDo: start timer

        //wait for all players to send answer or get timeout

    }
    private void ChooseAnswerPhase()
    {  
        //delete input card at client
        Player.LocalPlayer.RpcDeleteInputCard();

        //Send answers to clients
        SendAnswers();

        //ToDo: start timer


    }

    private void EvaluationPhase (){
        // do stuff
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
                ChooseAnswerPhase();
                break;
            case Phase.ChoseAnswer:
                currentPhase = Phase.Evaluation;
                EvaluationPhase();
                break;
            case Phase.Evaluation:
                currentPhase = Phase.WriteAnswer;
                break;
            default:
                break;
        }
    }

    private void SendAnswers()
    {
        var answerTexts = answers.Values.ToArray();

        var startX = (answerTexts.Length * 125 + (answerTexts.Length -1) * 20) / 2;

        for(int i = 0; i < answerTexts.Length; i++)
        {
            var xPosition = startX -  62.5 - i * 145;

            var cardGo = Instantiate(m_cardPrefab, new Vector3((float) xPosition, -100, -2), Quaternion.Euler(0,0,0));
            var card = cardGo.GetComponent<Card>();
            card.text = answerTexts[i];
            card.choosen = i;
            card.type = Card.CardType.Answer;

            NetworkServer.Spawn(cardGo);
        }    
    }
}
