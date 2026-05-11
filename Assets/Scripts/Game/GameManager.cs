using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class GameManager : MonoBehaviourPunCallbacks
{
    public int playerCount = 4;
    public int startCardCount = 1;

    private DeckManager deckManager = new DeckManager();
    private List<PlayerData> players = new List<PlayerData>();

    private CardData topDiscardCard;
    private int currentPlayerIndex = 0;
    private int direction = 1;
    private bool isGameOver = false;
    private string lastMessage = "";
    private string winnerName = "";
    private void Awake()
    {
        StartOfflineGame();
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PublishGameState();
            }
            else
            {
                TryApplyRoomState();
            }
        }
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha1))
    //     {
    //         PlayCard(0);
    //     }

    //     if (Input.GetKeyDown(KeyCode.Alpha2))
    //     {
    //         PlayCard(1);
    //     }

    //     if (Input.GetKeyDown(KeyCode.Alpha3))
    //     {
    //         PlayCard(2);
    //     }

    //     if (Input.GetKeyDown(KeyCode.D))
    //     {
    //         DrawCard();
    //     }
    // }

    public void StartOfflineGame()
    {
        players.Clear();
        isGameOver = false;
        currentPlayerIndex = 0;
        direction = 1;
        lastMessage = "Game started!";
        winnerName = "";
        if (PhotonNetwork.InRoom)
        {
            playerCount = PhotonNetwork.PlayerList.Length;

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                string playerName = PhotonNetwork.PlayerList[i].NickName;

                if (string.IsNullOrEmpty(playerName))
                {
                    playerName = "Player " + (i + 1);
                }

                players.Add(new PlayerData(i, playerName));
            }
        }
        else
        {
            for (int i = 0; i < playerCount; i++)
            {
                players.Add(new PlayerData(i, $"Player {i + 1}"));
            }
        }

        deckManager.CreateDeck();
        deckManager.Shuffle();

        DealCards();

        topDiscardCard = deckManager.DrawCard();

        Debug.Log("Game started!");
        Debug.Log("Top card: " + topDiscardCard.GetDisplayName());
        PrintCurrentPlayer();
        PrintAllHands();
        PublishGameState();
    }

    private void DealCards()
    {
        for (int i = 0; i < startCardCount; i++)
        {
            foreach (PlayerData player in players)
            {
                CardData card = deckManager.DrawCard();

                if (card != null)
                {
                    player.handCards.Add(card);
                }
            }
        }
    }

    public void PlayCard(int handCardIndex)
    {
        if (isGameOver)
        {
            Debug.Log("Game is already over.");
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];

        if (handCardIndex < 0 || handCardIndex >= currentPlayer.handCards.Count)
        {
            Debug.LogWarning("Invalid hand card index.");
            return;
        }

        CardData selectedCard = currentPlayer.handCards[handCardIndex];

        if (!RuleChecker.IsValidMove(selectedCard, topDiscardCard))
        {
            lastMessage = $"{currentPlayer.playerName} cannot play {selectedCard.GetDisplayName()}";
            Debug.Log(lastMessage);

            PublishGameState();

            return;
        }
        currentPlayer.handCards.RemoveAt(handCardIndex);
        topDiscardCard = selectedCard;

        lastMessage = $"{currentPlayer.playerName} played {selectedCard.GetDisplayName()}";
        Debug.Log(lastMessage);
        if (currentPlayer.handCards.Count == 0)
        {
            isGameOver = true;
            winnerName = currentPlayer.playerName;
            lastMessage = $"{currentPlayer.playerName} wins!";
            Debug.Log(lastMessage);

            PublishGameState();

            return;
        }

        ApplyCardEffect(selectedCard);
        PrintCurrentPlayer();
        PrintAllHands();
        PublishGameState();
    }

    public void DrawCard()
    {
        if (isGameOver)
        {
            Debug.Log("Game is already over.");
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        CardData card = deckManager.DrawCard();

        if (card != null)
        {
            currentPlayer.handCards.Add(card);
            lastMessage = $"{currentPlayer.playerName} drew a card.";
            Debug.Log(lastMessage);
        }
        MoveToNextPlayer();
        PrintCurrentPlayer();
        PrintAllHands();

        PublishGameState();
    }

    private void ApplyCardEffect(CardData card)
    {
        if (card.type == CardType.Skip)
        {
            MoveToNextPlayer();
            MoveToNextPlayer();
            Debug.Log("Skip effect activated.");
        }
        else if (card.type == CardType.Reverse)
        {
            direction *= -1;
            MoveToNextPlayer();
            Debug.Log("Reverse effect activated.");
        }
        else if (card.type == CardType.DrawTwo)
        {
            MoveToNextPlayer();

            PlayerData targetPlayer = players[currentPlayerIndex];

            for (int i = 0; i < 2; i++)
            {
                CardData drawnCard = deckManager.DrawCard();

                if (drawnCard != null)
                {
                    targetPlayer.handCards.Add(drawnCard);
                }
            }

            Debug.Log($"{targetPlayer.playerName} draws 2 cards.");

            MoveToNextPlayer();
        }
        else
        {
            MoveToNextPlayer();
        }
    }

    private void MoveToNextPlayer()
    {
        currentPlayerIndex += direction;

        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
        else if (currentPlayerIndex < 0)
        {
            currentPlayerIndex = players.Count - 1;
        }
    }

    private void PrintCurrentPlayer()
    {
        Debug.Log($"Current turn: {players[currentPlayerIndex].playerName}");
    }

    private void PrintAllHands()
    {
        foreach (PlayerData player in players)
        {
            string hand = "";

            for (int i = 0; i < player.handCards.Count; i++)
            {
                hand += $"[{i}] {player.handCards[i].GetDisplayName()}  ";
            }

            Debug.Log($"{player.playerName}: {hand}");
        }
    }
    public List<CardData> GetCurrentPlayerHand()
    {
        if (players == null || players.Count == 0)
        {
            return new List<CardData>();
        }

        if (PhotonNetwork.InRoom)
        {
            int localIndex = GetLocalPlayerIndex();

            if (localIndex >= 0 && localIndex < players.Count)
            {
                return players[localIndex].handCards;
            }
        }

        return players[currentPlayerIndex].handCards;
    }

    public CardData GetTopDiscardCard()
    {
        return topDiscardCard;
    }

    public string GetCurrentPlayerName()
    {
        return players[currentPlayerIndex].playerName;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
    public string GetLastMessage()
    {
        return lastMessage;
    }
    public List<PlayerData> GetPlayers()
    {
        return players;
    }
    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }
        public string GetWinnerName()
    {
        return winnerName;
    }

    public bool IsLocalPlayerTurn()
    {
        if (!PhotonNetwork.InRoom)
        {
            return true;
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= PhotonNetwork.PlayerList.Length)
        {
            return false;
        }

        Player currentPhotonPlayer = PhotonNetwork.PlayerList[currentPlayerIndex];

        return currentPhotonPlayer == PhotonNetwork.LocalPlayer;
    }
    private GameStateData CreateStateData()
    {
        GameStateData state = new GameStateData();

        state.players = players;
        state.deckCards = deckManager.GetDeckCards();
        state.topDiscardCard = topDiscardCard;

        state.currentPlayerIndex = currentPlayerIndex;
        state.direction = direction;

        state.isGameOver = isGameOver;
        state.lastMessage = lastMessage;
        state.winnerName = winnerName;

        return state;
    }

    private void ApplyStateData(GameStateData state)
    {
        if (state == null)
        {
            return;
        }

        players = state.players;
        deckManager.SetDeckCards(state.deckCards);

        topDiscardCard = state.topDiscardCard;
        currentPlayerIndex = state.currentPlayerIndex;
        direction = state.direction;

        isGameOver = state.isGameOver;
        lastMessage = state.lastMessage;
        winnerName = state.winnerName;
    }

    private void PublishGameState()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        GameStateData state = CreateStateData();
        string json = JsonUtility.ToJson(state);

        Hashtable props = new Hashtable();
        props["gameState"] = json;

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void TryApplyRoomState()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gameState"))
        {
            string json = PhotonNetwork.CurrentRoom.CustomProperties["gameState"].ToString();
            GameStateData state = JsonUtility.FromJson<GameStateData>(json);
            ApplyStateData(state);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("gameState"))
        {
            string json = propertiesThatChanged["gameState"].ToString();
            GameStateData state = JsonUtility.FromJson<GameStateData>(json);

            ApplyStateData(state);

            GameUIManager uiManager = FindAnyObjectByType<GameUIManager>();

            if (uiManager != null)
            {
                uiManager.RefreshUI();
            }
        }
    }
        public int GetLocalPlayerIndex()
    {
        if (!PhotonNetwork.InRoom)
        {
            return currentPlayerIndex;
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                return i;
            }
        }

        return -1;
    }
}