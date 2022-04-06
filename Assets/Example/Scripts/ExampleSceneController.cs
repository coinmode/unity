using CoinMode;
using CoinMode.NetApi;
using CoinMode.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleSceneController : MonoBehaviour
{
    [Space, Header("UI Panels")]
    [SerializeField]
    private GameObject titleLoginUi = null;

    [SerializeField]
    private GameObject gameUi = null;

    [Space, Header("Create & List Rounds")]
    [SerializeField]
    private InputField roundNameInput = null;

    [SerializeField]
    private InputField potContributionInput = null;

    [SerializeField]
    private GameButton createRoundButton = null;

    [SerializeField]
    private GameButton cmCreateRoundButton = null;

    [SerializeField]
    private GameRoundButton existingRoundTemplate = null;

    [SerializeField]
    private GameButton listRoundsButton = null;

    [SerializeField]
    private RectTransform roundsCountainer = null;

    [Space, Header("Round Information")]
    [SerializeField]
    private CanvasGroup roundInfoGroup = null;

    [SerializeField]
    private Text currentRoundText = null;

    [SerializeField]
    private GameButton getRoundInfoButton = null;

    [SerializeField]
    private RoundInfoParamDisplay roundInfoDisplayTemplate = null;

    [SerializeField]
    private RectTransform roundInfoContainer = null;

    [SerializeField]
    private GameButton invokePlayGameButton = null;

    [Space, Header("Session Loop")]
    [SerializeField]
    private CanvasGroup sessionLoopGroup = null;

    [SerializeField]
    private GameButton requestSessionButton = null;

    [SerializeField]
    private GameButton startSessionButton = null;

    [SerializeField]
    private InputField scoreInput = null;

    [SerializeField]
    private GameButton stopSessionButton = null;

    [SerializeField]
    private GameButton challengeSessionButton = null;

    [Space, Header("Round")]
    [SerializeField]
    private GameButton endRoundButton = null;

    [Space, Header("High Scores")]
    [SerializeField]
    private CanvasGroup highScoreGroup = null;

    [SerializeField]
    private GameButton viewHighScoreButton = null;

    [SerializeField]
    private RoundHighScoreDisplay highScoreTemplate = null;

    [SerializeField]
    private RectTransform highScoreContainer = null;

    [SerializeField]
    private GameButton invokeViewHighScoreButton = null;

    [SerializeField]
    private GameButton invokeViewGameOverButton = null;

    [Space, Header("Extras")]
    [SerializeField]
    private GameButton lobbyPlayersButton = null;

    [Space, Header("Info")]
    [SerializeField]
    private Text messageText = null;

    private PlayerComponent currentPlayer = null;
    private GameComponent currentGame = null;
    private RoundComponent currentRound = null;
    private SessionComponent currentSession = null;

    private List<GameRoundButton> listedRoundButtons = new List<GameRoundButton>();
    private List<RoundInfoParamDisplay> roundInfoParamDisplays = new List<RoundInfoParamDisplay>();
    private List<RoundHighScoreDisplay> highScoreDisplays = new List<RoundHighScoreDisplay>();

    private string currentRoundId
    {
        get { return _currentRoundId; }
        set
        {
            if(currentRound != null)
            {
                if (value != currentRound.roundId)
                {
                    highScoreGroup.interactable = false;
                    sessionLoopGroup.interactable = false;
                    currentRound = null;
                    ClearHighScores();
                    ClearRoundInfo();
                    ResetSessionLoop();
                }
                else
                {
                    highScoreGroup.interactable = true;
                    sessionLoopGroup.interactable = true;
                }
            }            
            if (!string.IsNullOrEmpty(value))
            {
                roundInfoGroup.interactable = true;
            }
            _currentRoundId = value;
        }
    }
    private string _currentRoundId = "";

    private string currentRoundName
    {
        get { return _currentRoundName; }
        set
        {
            _currentRoundName = value;
            string name = !string.IsNullOrEmpty(_currentRoundName) ? _currentRoundName : "None";
            currentRoundText.text = "Current Round: <b>" + name + "</b>";
        }
    }
    private string _currentRoundName = "";
    private string cachedCreatedRoundName = "";    

    private GamesRound.CreateRoundResponse createRoundResponse = null;
    private GamesRound.ListRoundsResponse listRoundsResponse = null;       

    private void Awake()
    {
        titleLoginUi.SetActive(true);
        gameUi.SetActive(false);
    }

    private void Start()
    {
        if(createRoundButton != null) createRoundButton.button.onClick.AddListener(RoundCreateAction);
        if (cmCreateRoundButton != null) cmCreateRoundButton.button.onClick.AddListener(CoinModeCreateRoundAction);
        if (listRoundsButton != null) listRoundsButton.button.onClick.AddListener(RoundsListAction);
        if (getRoundInfoButton != null) getRoundInfoButton.button.onClick.AddListener(GetRoundInfoAction);
        if (invokePlayGameButton != null) invokePlayGameButton.button.onClick.AddListener(InvokeCoinModeRoundInfoAction);
        if (requestSessionButton != null) requestSessionButton.button.onClick.AddListener(SessionRequestAction);
        if (startSessionButton != null) startSessionButton.button.onClick.AddListener(SessionStartAction);
        if (stopSessionButton != null) stopSessionButton.button.onClick.AddListener(SessionStopAction);
        if (challengeSessionButton != null) challengeSessionButton.button.onClick.AddListener(SessionChallengeAction);
        if (endRoundButton != null) endRoundButton.button.onClick.AddListener(EndRoundAction);
        if (viewHighScoreButton != null) viewHighScoreButton.button.onClick.AddListener(ViewHighScoresAction);
        if (invokeViewHighScoreButton != null) invokeViewHighScoreButton.button.onClick.AddListener(InvokeViewHighScoresAction);
        if (invokeViewGameOverButton != null) invokeViewGameOverButton.button.onClick.AddListener(InvokeViewGameOverAction);
        if (lobbyPlayersButton != null) lobbyPlayersButton.button.onClick.AddListener(InvokeViewLobbyPlayers);        

        if (existingRoundTemplate != null) existingRoundTemplate.gameObject.SetActive(false);
        if (highScoreTemplate != null) highScoreTemplate.gameObject.SetActive(false);
        if (roundInfoDisplayTemplate != null) roundInfoDisplayTemplate.gameObject.SetActive(false);

        roundInfoGroup.interactable = false;
        sessionLoopGroup.interactable = false;
        highScoreGroup.interactable = false;

        ResetSessionLoop();
    }

    public void OnTitleLogin(PlayerComponent player)
    {
        titleLoginUi.SetActive(false);
        gameUi.SetActive(true);
        this.currentPlayer = player;
        if(!CoinModeManager.titleComponent.TryGetGame("TestGame", out currentGame))
        {
            Debug.LogWarning("Example game Id not found in title games");
        }
    }

    // Create Round
    private void RoundCreateAction()
    {
        createRoundButton.SetButtonState(GameButton.ButtonState.Waiting);
        roundNameInput.interactable = false;
        potContributionInput.interactable = false;
        double potContribution = 0;
        double.TryParse(potContributionInput.text, out potContribution);
        cachedCreatedRoundName = roundNameInput.text;
        CoinModeManager.SendRequest(
            GamesRound.Create(currentPlayer.playToken, currentGame.gameId, null, null, null, true, potContribution, cachedCreatedRoundName, null, null, null, "", OnRoundCreateSuccess, OnRoundCreateFailure));
    }    

    private void OnRoundCreateSuccess(GamesRound.CreateRoundResponse response)
    {
        createRoundButton.SetButtonState(GameButton.ButtonState.Interatable);
        roundNameInput.interactable = true;
        potContributionInput.interactable = true;
        messageText.text = "Created round!";
        messageText.color = Color.green;
        createRoundResponse = response;
        currentRoundId = response.roundId;
        string name = string.IsNullOrEmpty(cachedCreatedRoundName) ? createRoundResponse.roundId : cachedCreatedRoundName;
        currentRoundName = name;
    }

    private void OnRoundCreateFailure(CoinModeErrorResponse response)
    {
        createRoundButton.SetButtonState(GameButton.ButtonState.Interatable); 
        roundNameInput.interactable = true;
        potContributionInput.interactable = true;
        messageText.text = response.userMessage;
        messageText.color = Color.red;
    }

    // Coin Mode Create Round
    private void CoinModeCreateRoundAction()
    {
        cmCreateRoundButton.SetButtonState(GameButton.ButtonState.Waiting);
        CoinModeMenu.OpenCreateRound(false, OnCoinModeRoundCreateSuccess, OnCoinModeRoundCreateFailure);
    }

    private void OnCoinModeRoundCreateSuccess(SessionComponent session)
    {
        cmCreateRoundButton.SetButtonState(GameButton.ButtonState.Interatable);

        messageText.text = "Created round!";
        messageText.color = Color.green;        
        
        currentRound = session.round;
        currentSession = session;

        currentRoundId = session.round.roundId;
        currentRoundName = session.round.roundId;

        OnGetRoundInfoSuccess(session.round);
        OnSessionRequestSuccess(session);
    }

    private void OnCoinModeRoundCreateFailure(SessionComponent session, CoinModeError error)
    {
        cmCreateRoundButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    // List Rounds
    private void RoundsListAction()
    {
        listRoundsButton.SetButtonState(GameButton.ButtonState.Waiting);
        CoinModeManager.SendRequest(
            GamesRound.List(currentGame.gameId, null, currentPlayer.playToken, true, true, false, null, null, OnRoundsListSuccess, OnRoundsListFailure));
    }

    private void OnRoundsListSuccess(GamesRound.ListRoundsResponse response)
    {
        listRoundsButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "Rounds Listed!";
        messageText.color = Color.green;
        listRoundsResponse = response;

        for(int i = 0; i < listedRoundButtons.Count; i++)
        {
            listedRoundButtons[i].transform.SetParent(null);
            Destroy(listedRoundButtons[i].gameObject);
        }
        listedRoundButtons.Clear();

        for(int i = 0; i < listRoundsResponse.rounds.Length; i++)
        {
            GameRoundButton button = Instantiate(existingRoundTemplate, roundsCountainer);
            button.gameObject.SetActive(true);
            button.SetInfo(SetRoundIdFromGameRoundButton, listRoundsResponse.rounds[i]);
            listedRoundButtons.Add(button);
        }
    }

    private void OnRoundsListFailure(CoinModeErrorResponse response)
    {
        listRoundsButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = response.userMessage;
        messageText.color = Color.red;
    }

    // Set Current Round Id
    private void SetRoundIdFromGameRoundButton(GameRoundButton button)
    {
        currentRoundId = button.roundInfo.roundId;
        messageText.text = "Selected Round Id!";
        messageText.color = Color.green;
        string name = string.IsNullOrEmpty(button.roundInfo.name) ? button.roundInfo.roundId : button.roundInfo.name;
        currentRoundName = name;
    }

    // Get Round Info
    private void GetRoundInfoAction()
    {
        getRoundInfoButton.SetButtonState(GameButton.ButtonState.Waiting);
        currentRound = currentGame.FindOrConstructRound(currentRoundId);
        currentRound.GetInfo(currentPlayer, OnGetRoundInfoSuccess, OnGetRoundInfoFailure);
    }

    private void OnGetRoundInfoSuccess(RoundComponent round)
    {
        getRoundInfoButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "Round Info Retrieved!";
        messageText.color = Color.green;

        if (round.roundId == currentRoundId)
        {
            highScoreGroup.interactable = true;
            sessionLoopGroup.interactable = true;
        }

        ClearRoundInfo();

        AddRoundInfoParamDisplay("Round ID", round.roundId);
        AddRoundInfoParamDisplay("Game", round.gameName);
        AddRoundInfoParamDisplay("Fee to Play", round.playFee.ToString());
        AddRoundInfoParamDisplay("Pot Contribution", round.potContribution.ToString());
        AddRoundInfoParamDisplay("Current Pot", round.winningPot.ToString());
        AddRoundInfoParamDisplay("Can Play?", round.playerCanPlay.ToString());
        AddRoundInfoParamDisplay("Status", round.status.ToString());
        if (round.epochStarted >= 0)
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(round.epochStarted);
            AddRoundInfoParamDisplay("Start Time", offset.LocalDateTime.ToString());
        }
        if (round.epochToFinish>= 0)
        {
            DateTimeOffset epochNow = new DateTimeOffset(DateTime.UtcNow);
            TimeSpan seconds = TimeSpan.FromSeconds(Mathf.Max(0.0F, round.epochToFinish - epochNow.ToUnixTimeSeconds()));
            AddRoundInfoParamDisplay("Time Remaining", string.Format("{0:mm\\:ss}", seconds));
        }
        if (round.epochEnded >= 0)
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(round.epochEnded);
            AddRoundInfoParamDisplay("Time Ended", offset.LocalDateTime.ToString());
        }
    }

    private void OnGetRoundInfoFailure(RoundComponent round, CoinModeError error)
    {
        getRoundInfoButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    private void AddRoundInfoParamDisplay(string paramName, string paramValue)
    {
        RoundInfoParamDisplay button = Instantiate(roundInfoDisplayTemplate, roundInfoContainer);
        button.gameObject.SetActive(true);
        button.ParamName.text = paramName;
        button.ParamValue.text = paramValue;
        roundInfoParamDisplays.Add(button);
    }

    private void ClearRoundInfo()
    {
        for (int i = 0; i < roundInfoParamDisplays.Count; i++)
        {
            roundInfoParamDisplays[i].transform.SetParent(null);
            Destroy(roundInfoParamDisplays[i].gameObject);
        }
        roundInfoParamDisplays.Clear();
    }

    // Coin Mode Plugin Get Round Info

    private void InvokeCoinModeRoundInfoAction()
    {
        invokePlayGameButton.SetButtonState(GameButton.ButtonState.Waiting);
        currentRound = currentGame.FindOrConstructRound(currentRoundId);
        CoinModeMenu.OpenPlayGame(currentRound, false, OnPlayGameSuccess, OnPlayGameFailure);
    }

    private void OnPlayGameSuccess(SessionComponent session)
    {
        invokePlayGameButton.SetButtonState(GameButton.ButtonState.Interatable);
        currentRound = session.round;
        currentSession = session;
        OnGetRoundInfoSuccess(session.round);
        OnSessionRequestSuccess(session);
    }

    private void OnPlayGameFailure(SessionComponent session, CoinModeError error)
    {
        invokePlayGameButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    // Session Loop
    private void ResetSessionLoop()
    {
        requestSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        startSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
        scoreInput.interactable = false;
        stopSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
        challengeSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
    }

    // Request Session
    private void SessionRequestAction()
    {
        requestSessionButton.SetButtonState(GameButton.ButtonState.Waiting);
        challengeSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
        currentRound.ConstructSession(out currentSession);
        currentPlayer.AssignSession(currentSession);
        currentSession.Request(currentPlayer, "", "", OnSessionRequestSuccess, OnSessionRequestFailure);
    }

    private void OnSessionRequestSuccess(SessionComponent session)
    {
        requestSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
        startSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "Requested session!";
        messageText.color = Color.green;
    }

    private void OnSessionRequestFailure(SessionComponent session, CoinModeError error)
    {
        requestSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    //Start Session
    private void SessionStartAction()
    {
        startSessionButton.SetButtonState(GameButton.ButtonState.Waiting);
        currentSession.Start(currentPlayer, OnSessionStartSuccess, OnSessionStartFailure);
    }

    private void OnSessionStartSuccess(SessionComponent session)
    {
        startSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
        scoreInput.interactable = true;
        stopSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "Session started!";
        messageText.color = Color.green;
    }

    private void OnSessionStartFailure(SessionComponent session, CoinModeError error)
    {
        startSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    // Stop Session
    private void SessionStopAction()
    {
        stopSessionButton.SetButtonState(GameButton.ButtonState.Waiting);
        scoreInput.interactable = false;
        double score = 0;
        double.TryParse(scoreInput.text, out score);
        currentSession.SetScore(score, scoreInput.text);
        currentSession.Stop(OnSessionStopSuccess, OnSessionStopFailure);
    }

    private void OnSessionStopSuccess(SessionComponent session)
    {
        stopSessionButton.SetButtonState(GameButton.ButtonState.Disabled);
        requestSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        challengeSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "Session stopped!";
        messageText.color = Color.green;
    }

    private void OnSessionStopFailure(SessionComponent session, CoinModeError error)
    {
        stopSessionButton.SetButtonState(GameButton.ButtonState.Interatable);
        scoreInput.interactable = true;
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    // Session Challenge
    private void SessionChallengeAction()
    {
#if UNITY_2019_3_OR_NEWER
        CoinModeMenu.OpenCreatePvpChallenge(CoinModeSettings.defaultGameAlias, currentSession, null, OnChallengeSuccess, OnChallengeFailure);
#endif
    }

    private void OnChallengeSuccess(SessionComponent session)
    {
        //CoinModeMenu.OpenShareScreen(session.round.roundId);        
    }

    private void OnChallengeFailure(SessionComponent session, CoinModeError error)
    {
        invokePlayGameButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    // End Round
    private void EndRoundAction()
    {
        endRoundButton.SetButtonState(GameButton.ButtonState.Waiting);
        currentRound.End("", true, OnEndRoundSuccess, OnEndRoundFailure);
    }

    private void OnEndRoundSuccess(RoundComponent round)
    {
        endRoundButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "End Round Successful!";
        messageText.color = Color.green;
    }

    private void OnEndRoundFailure(RoundComponent round, CoinModeError error)
    {
        endRoundButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    // High Scores
    private void ViewHighScoresAction()
    {
        viewHighScoreButton.SetButtonState(GameButton.ButtonState.Waiting);
        currentRound.GetHighscores(OnGetHighScoresSuccess, OnGetHighScoresFailure);
    }

    private void OnGetHighScoresSuccess(RoundComponent round)
    {
        viewHighScoreButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = "High Scores Retrieved!";
        messageText.color = Color.green;

        ClearHighScores();

        foreach (HighScore highScore in round.highScores)
        {
            RoundHighScoreDisplay display = Instantiate(highScoreTemplate, highScoreContainer);
            display.gameObject.SetActive(true);
            display.SetInfo(highScore);
            highScoreDisplays.Add(display);
        }
    }

    private void OnGetHighScoresFailure(RoundComponent round, CoinModeError error)
    {
        viewHighScoreButton.SetButtonState(GameButton.ButtonState.Interatable);
        messageText.text = error.userMessage;
        messageText.color = Color.red;
    }

    private void ClearHighScores()
    {
        for (int i = 0; i < highScoreDisplays.Count; i++)
        {
            highScoreDisplays[i].transform.SetParent(null);
            Destroy(highScoreDisplays[i].gameObject);
        }
        highScoreDisplays.Clear();
    }

    // Invoke view coin mode menu high scores
    private void InvokeViewHighScoresAction()
    {
        CoinModeMenu.OpenHighScores(currentRound);
    }

    // Invoke view coin mode game over
    private void InvokeViewGameOverAction()
    {
        CoinModeMenu.OpenRoundResults(currentRound);
    }

    // Invoke view coin mode lobby list
    private void InvokeViewLobbyPlayers()
    {
        CoinModeMenu.OpenLobbyPlayers();
    }
}
