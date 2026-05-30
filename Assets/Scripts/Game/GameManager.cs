using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    private const int MaxHandCardsBeforeLoss = 25;

    public int playerCount = 4;
    public int startCardCount = 7;

    private DeckManager deckManager = new DeckManager();
    private List<PlayerData> players = new List<PlayerData>();
    private List<CardData> discardPile = new List<CardData>();

    private CardData topDiscardCard;
    private CardColor currentColor = CardColor.Wild;
    private int currentPlayerIndex = 0;
    private int direction = 1;
    private bool hasDrawnThisTurn = false;
    private int drawnCardIndex = -1;
    private int pendingDrawPenalty = 0;
    private int unoDeclaredPlayerIndex = -1;
    private int finishOrder = 0;
    private bool isGameOver = false;
    private string lastMessage = "";
    private string winnerName = "";
    private int visualEventSequence;
    private string visualEventType = "";
    private int visualEventPlayerIndex = -1;
    private CardData visualEventCard;
    private string visualEventWinner = "";
    private int lastAppliedVisualEventSequence;

    private void Awake()
    {
        if (GameModeSelection.CurrentMode != GameMode.Uno)
        {
            return;
        }

        if (!PhotonNetwork.InRoom)
        {
            StartOfflineGame();
        }
    }

    private void Start()
    {
        if (GameModeSelection.CurrentMode != GameMode.Uno)
        {
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartOfflineGame();
                PublishGameState();
            }
            else
            {
                TryApplyRoomState();
            }
        }
    }

    public void StartOfflineGame()
    {
        players.Clear();
        discardPile.Clear();
        isGameOver = false;
        currentPlayerIndex = 0;
        direction = 1;
        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        pendingDrawPenalty = 0;
        unoDeclaredPlayerIndex = -1;
        finishOrder = 0;
        currentColor = CardColor.Wild;
        lastMessage = "Match color, number, symbol, or play a Wild.";
        winnerName = "";
        visualEventType = "";
        visualEventPlayerIndex = -1;
        visualEventCard = null;
        visualEventWinner = "";

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
                players.Add(new PlayerData(i, "Player " + (i + 1)));
            }
        }

        deckManager.CreateDeck();
        deckManager.Shuffle();

        DealCards();
        PrepareInitialDiscard();
        ClearVisualEventPayload();

        if (topDiscardCard != null)
        {
            Debug.Log("Top card: " + topDiscardCard.GetDisplayName() + " | Current color: " + currentColor);
        }

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
                CardData card = DrawFromDeck();

                if (card != null)
                {
                    player.handCards.Add(card);
                }
            }
        }
    }

    private void PrepareInitialDiscard()
    {
        topDiscardCard = DrawFromDeck();

        while (topDiscardCard != null && topDiscardCard.type == CardType.DrawFour)
        {
            deckManager.AddCards(new List<CardData> { topDiscardCard });
            deckManager.Shuffle();
            topDiscardCard = DrawFromDeck();
        }

        if (topDiscardCard == null)
        {
            return;
        }

        if (topDiscardCard.type == CardType.ChangeColor)
        {
            currentColor = ChooseBestColorForPlayer(players[currentPlayerIndex]);
            lastMessage = "Wild starts the discard pile. Current color is " + currentColor + ".";
            return;
        }

        currentColor = topDiscardCard.color;

        if (topDiscardCard.type == CardType.Block)
        {
            MoveToNextPlayer();
            lastMessage = "First card is Skip. Player 1 is skipped.";
        }
        else if (topDiscardCard.type == CardType.Reverse)
        {
            direction *= -1;

            if (GetActivePlayerCount() == 2)
            {
                MoveToNextPlayer();
                lastMessage = "First card is Reverse. Player 1 is skipped.";
            }
            else
            {
                currentPlayerIndex = players.Count - 1;
                GameEvents.TurnChanged(currentPlayerIndex);
                lastMessage = "First card is Reverse. Direction starts reversed.";
            }
        }
        else if (topDiscardCard.type == CardType.DrawTwo)
        {
            if (DrawCardsForPlayer(players[currentPlayerIndex], 2))
            {
                return;
            }

            MoveToNextPlayer();
            lastMessage = "First card is Draw Two. Player 1 draws 2 and is skipped.";
        }
    }

    private CardData DrawFromDeck()
    {
        CardData card = deckManager.DrawCard();

        if (card != null)
        {
            return card;
        }

        RecycleDiscardPileIntoDeck();
        return deckManager.DrawCard();
    }

    private void RecycleDiscardPileIntoDeck()
    {
        if (discardPile.Count == 0)
        {
            return;
        }

        deckManager.AddCards(discardPile);
        discardPile.Clear();
        deckManager.Shuffle();
        Debug.Log("Draw pile reshuffled from discard pile.");
    }

    private void MoveCurrentTopCardToDiscardPile()
    {
        if (topDiscardCard != null)
        {
            discardPile.Add(topDiscardCard);
        }
    }

    private bool DrawCardsForPlayer(PlayerData player, int count)
    {
        if (!IsPlayerActive(player))
        {
            return false;
        }

        for (int i = 0; i < count; i++)
        {
            CardData drawnCard = DrawFromDeck();

            if (drawnCard != null)
            {
                player.handCards.Add(drawnCard);
                NotifyCardDrawn(player.playerIndex);

                if (TryApplyCardLimitLoss(player))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void PlayCard(int handCardIndex)
    {
        PlayCard(handCardIndex, CardColor.Wild);
    }

    public void PlayCard(int handCardIndex, CardColor chosenColor)
    {
        EnsureCurrentPlayerIsActive();

        if (isGameOver || players.Count == 0)
        {
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return;
        }

        if (handCardIndex < 0 || handCardIndex >= currentPlayer.handCards.Count)
        {
            Debug.LogWarning("Invalid hand card index.");
            return;
        }

        CardData selectedCard = currentPlayer.handCards[handCardIndex];

        if (!CanPlayHandCard(handCardIndex))
        {
            lastMessage = currentPlayer.playerName + " cannot play " + selectedCard.GetDisplayName() + " on " + GetTopCardStatus() + ".";
            Debug.Log(lastMessage);
            PublishGameState();
            return;
        }

        if (RequiresColorChoice(selectedCard) && !IsValidChosenColor(chosenColor))
        {
            lastMessage = "Choose a color before playing a Wild card.";
            PublishGameState();
            return;
        }

        int actingPlayerIndex = currentPlayerIndex;
        bool shouldHaveCalledUno = currentPlayer.handCards.Count == 2;

        currentPlayer.handCards.RemoveAt(handCardIndex);
        MoveCurrentTopCardToDiscardPile();
        topDiscardCard = selectedCard;
        currentColor = RequiresColorChoice(selectedCard) ? chosenColor : selectedCard.color;
        hasDrawnThisTurn = false;
        drawnCardIndex = -1;

        lastMessage = currentPlayer.playerName + " played " + selectedCard.GetDisplayName();
        if (RequiresColorChoice(selectedCard))
        {
            lastMessage += " and changed the current color to " + currentColor + ".";
        }

        Debug.Log(lastMessage);
        NotifyCardPlayed(selectedCard, actingPlayerIndex);

        if (shouldHaveCalledUno && currentPlayer.handCards.Count == 1)
        {
            if (unoDeclaredPlayerIndex == actingPlayerIndex)
            {
                lastMessage += " UNO!";
            }
            else
            {
                if (DrawCardsForPlayer(currentPlayer, 2))
                {
                    PublishGameState();
                    return;
                }

                lastMessage += " " + currentPlayer.playerName + " forgot UNO and draws 2 cards.";
            }
        }

        unoDeclaredPlayerIndex = -1;

        ApplyCardEffect(selectedCard);

        if (currentPlayer.handCards.Count == 0)
        {
            FinishPlayer(currentPlayer);

            if (!isGameOver)
            {
                string finalPenaltyMessage = ResolveRoundEndDrawPenalty();
                AppendLastMessage(finalPenaltyMessage);
                EnsureCurrentPlayerIsActive();
            }

            PublishGameState();
            return;
        }

        PrintCurrentPlayer();
        PrintAllHands();
        PublishGameState();
    }

    private string ResolveRoundEndDrawPenalty()
    {
        if (pendingDrawPenalty <= 0 || players.Count == 0)
        {
            return "";
        }

        PlayerData targetPlayer = players[currentPlayerIndex];
        int cardsToDraw = pendingDrawPenalty;

        pendingDrawPenalty = 0;
        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        unoDeclaredPlayerIndex = -1;

        if (!IsPlayerActive(targetPlayer))
        {
            EnsureCurrentPlayerIsActive();
            return "";
        }

        AppendLastMessage(targetPlayer.playerName + " draws " + cardsToDraw + " final penalty cards.");
        DrawCardsForPlayer(targetPlayer, cardsToDraw);
        return "";
    }

    public void DrawCard()
    {
        EnsureCurrentPlayerIsActive();

        if (isGameOver || players.Count == 0)
        {
            return;
        }

        if (pendingDrawPenalty > 0)
        {
            ResolvePendingDrawPenalty();
            return;
        }

        if (hasDrawnThisTurn)
        {
            PassAfterDraw();
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return;
        }

        CardData card = DrawFromDeck();

        if (card != null)
        {
            currentPlayer.handCards.Add(card);
            NotifyCardDrawn(currentPlayerIndex);
            hasDrawnThisTurn = false;
            drawnCardIndex = -1;

            if (TryApplyCardLimitLoss(currentPlayer))
            {
                PublishGameState();
                return;
            }

            lastMessage = currentPlayer.playerName + " drew a card and lost their turn.";
            Debug.Log(lastMessage);
        }

        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        MoveToNextPlayer();
        PrintCurrentPlayer();
        PrintAllHands();
        PublishGameState();
    }

    public void DrawOneCardFromAR()
    {
        EnsureCurrentPlayerIsActive();

        if (isGameOver || players.Count == 0)
        {
            return;
        }

        if (pendingDrawPenalty <= 0)
        {
            DrawCard();
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return;
        }

        CardData card = DrawFromDeck();
        if (card != null)
        {
            currentPlayer.handCards.Add(card);
            pendingDrawPenalty = Mathf.Max(0, pendingDrawPenalty - 1);
            NotifyCardDrawn(currentPlayerIndex);

            if (TryApplyCardLimitLoss(currentPlayer))
            {
                PublishGameState();
                return;
            }
        }

        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        unoDeclaredPlayerIndex = -1;

        if (pendingDrawPenalty > 0)
        {
            lastMessage = currentPlayer.playerName + " drew a penalty card. Draw " + pendingDrawPenalty + " more.";
        }
        else
        {
            lastMessage = currentPlayer.playerName + " completed the draw penalty and lost their turn.";
            MoveToNextPlayer();
        }

        PrintCurrentPlayer();
        PrintAllHands();
        PublishGameState();
    }

    public void PassAfterDraw()
    {
        EnsureCurrentPlayerIsActive();

        if (isGameOver || !hasDrawnThisTurn)
        {
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return;
        }

        lastMessage = currentPlayer.playerName + " passed after drawing.";
        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        unoDeclaredPlayerIndex = -1;
        MoveToNextPlayer();
        PublishGameState();
    }

    public void CallUno()
    {
        EnsureCurrentPlayerIsActive();

        if (isGameOver || players.Count == 0)
        {
            return;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return;
        }

        if (currentPlayer.handCards.Count != 2)
        {
            lastMessage = "Call UNO when you are about to play your second-to-last card.";
            PublishGameState();
            return;
        }

        unoDeclaredPlayerIndex = currentPlayerIndex;
        lastMessage = currentPlayer.playerName + " called UNO!";
        PublishGameState();
    }

    private void ApplyCardEffect(CardData card)
    {
        if (card.type == CardType.Block)
        {
            MoveToNextPlayer();
            MoveToNextPlayer();
            lastMessage += " Next player is skipped.";
            Debug.Log("Skip effect activated.");
        }
        else if (card.type == CardType.Reverse)
        {
            direction *= -1;

            if (GetActivePlayerCount() == 2)
            {
                MoveToNextPlayer();
                MoveToNextPlayer();
                lastMessage += " Reverse acts like Skip in a 2-player game.";
            }
            else
            {
                MoveToNextPlayer();
                lastMessage += " Direction reversed.";
            }

            Debug.Log("Reverse effect activated.");
        }
        else if (card.type == CardType.DrawTwo)
        {
            pendingDrawPenalty += 2;
            MoveToNextPlayer();
            lastMessage += " Penalty is +" + pendingDrawPenalty + ". " + players[currentPlayerIndex].playerName + " must stack " + GetPendingDrawStackLabel() + " or draw " + pendingDrawPenalty + ".";
        }
        else if (card.type == CardType.DrawFour)
        {
            pendingDrawPenalty += 4;
            MoveToNextPlayer();
            lastMessage += " Penalty is +" + pendingDrawPenalty + ". " + players[currentPlayerIndex].playerName + " must stack " + GetPendingDrawStackLabel() + " or draw " + pendingDrawPenalty + ".";
        }
        else
        {
            MoveToNextPlayer();
        }

        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
    }

    private void ResolvePendingDrawPenalty()
    {
        if (pendingDrawPenalty <= 0 || players.Count == 0)
        {
            return;
        }

        PlayerData targetPlayer = players[currentPlayerIndex];
        int cardsToDraw = pendingDrawPenalty;

        pendingDrawPenalty = 0;
        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        unoDeclaredPlayerIndex = -1;

        if (!IsPlayerActive(targetPlayer))
        {
            EnsureCurrentPlayerIsActive();
            PublishGameState();
            return;
        }

        if (DrawCardsForPlayer(targetPlayer, cardsToDraw))
        {
            PublishGameState();
            return;
        }

        lastMessage = targetPlayer.playerName + " drew " + cardsToDraw + " penalty cards and lost their turn.";
        MoveToNextPlayer();
        PrintCurrentPlayer();
        PrintAllHands();
        PublishGameState();
    }

    private bool TryApplyCardLimitLoss(PlayerData player)
    {
        if (player == null || player.handCards.Count <= MaxHandCardsBeforeLoss)
        {
            return false;
        }

        EliminatePlayer(player, "more than " + MaxHandCardsBeforeLoss + " cards (" + player.handCards.Count + ")");
        return true;
    }

    private bool IsPlayerActive(PlayerData player)
    {
        return player != null && player.IsActive;
    }

    private int GetActivePlayerCount()
    {
        int count = 0;

        foreach (PlayerData player in players)
        {
            if (IsPlayerActive(player))
            {
                count++;
            }
        }

        return count;
    }

    private PlayerData GetOnlyActivePlayer()
    {
        foreach (PlayerData player in players)
        {
            if (IsPlayerActive(player))
            {
                return player;
            }
        }

        return null;
    }

    private void EnsureCurrentPlayerIsActive()
    {
        if (isGameOver || players.Count == 0)
        {
            return;
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            MoveToNextPlayer();
            return;
        }

        if (!IsPlayerActive(players[currentPlayerIndex]))
        {
            MoveToNextPlayer();
        }
    }

    private void FinishPlayer(PlayerData player)
    {
        if (!IsPlayerActive(player))
        {
            return;
        }

        finishOrder++;
        player.hasFinished = true;
        player.finishRank = finishOrder;
        player.handCards.Clear();

        AppendLastMessage(player.playerName + " finished #" + player.finishRank + ".");
        Debug.Log(lastMessage);

        if (!FinishRoundIfOnlyOneActivePlayer(true))
        {
            EnsureCurrentPlayerIsActive();
        }
    }

    private void EliminatePlayer(PlayerData player, string reason)
    {
        if (!IsPlayerActive(player))
        {
            return;
        }

        player.isEliminated = true;
        player.finishRank = players.Count;

        AppendLastMessage(player.playerName + " is out: " + reason + ".");
        Debug.Log(lastMessage);

        if (!FinishRoundIfOnlyOneActivePlayer(false))
        {
            EnsureCurrentPlayerIsActive();
        }
    }

    private bool FinishRoundIfOnlyOneActivePlayer(bool remainingPlayerIsLastPlace)
    {
        if (GetActivePlayerCount() > 1)
        {
            return false;
        }

        PlayerData finalPlayer = GetOnlyActivePlayer();
        if (finalPlayer != null)
        {
            if (remainingPlayerIsLastPlace)
            {
                finalPlayer.isLastPlace = true;
                finalPlayer.finishRank = players.Count;
                AppendLastMessage(finalPlayer.playerName + " is last place.");
            }
            else
            {
                finishOrder++;
                finalPlayer.hasFinished = true;
                finalPlayer.finishRank = finishOrder;
                AppendLastMessage(finalPlayer.playerName + " is the final remaining player.");
            }
        }

        pendingDrawPenalty = 0;
        hasDrawnThisTurn = false;
        drawnCardIndex = -1;
        unoDeclaredPlayerIndex = -1;
        isGameOver = true;
        winnerName = GetFirstPlaceName();
        AppendLastMessage("Round complete.");
        Debug.Log(lastMessage);
        NotifyGameOver(string.IsNullOrEmpty(winnerName) ? lastMessage : winnerName);
        return true;
    }

    private string GetFirstPlaceName()
    {
        foreach (PlayerData player in players)
        {
            if (player.hasFinished && player.finishRank == 1)
            {
                return player.playerName;
            }
        }

        return "";
    }

    private void AppendLastMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (string.IsNullOrEmpty(lastMessage))
        {
            lastMessage = message;
            return;
        }

        if (!lastMessage.EndsWith(".") && !lastMessage.EndsWith("!") && !lastMessage.EndsWith("?"))
        {
            lastMessage += ".";
        }

        lastMessage += " " + message;
    }

    private void MoveToNextPlayer()
    {
        if (players.Count == 0 || GetActivePlayerCount() == 0)
        {
            return;
        }

        int nextIndex = currentPlayerIndex;
        if (nextIndex < 0 || nextIndex >= players.Count)
        {
            nextIndex = direction >= 0 ? -1 : 0;
        }

        for (int i = 0; i < players.Count; i++)
        {
            nextIndex += direction;

            if (nextIndex >= players.Count)
            {
                nextIndex = 0;
            }
            else if (nextIndex < 0)
            {
                nextIndex = players.Count - 1;
            }

            if (IsPlayerActive(players[nextIndex]))
            {
                currentPlayerIndex = nextIndex;
                GameEvents.TurnChanged(currentPlayerIndex);
                return;
            }
        }
    }

    private void PrintCurrentPlayer()
    {
        if (players.Count > 0)
        {
            Debug.Log("Current turn: " + players[currentPlayerIndex].playerName);
        }
    }

    private void PrintAllHands()
    {
        foreach (PlayerData player in players)
        {
            string hand = "";

            for (int i = 0; i < player.handCards.Count; i++)
            {
                hand += "[" + i + "] " + player.handCards[i].GetDisplayName() + "  ";
            }

            Debug.Log(player.playerName + ": " + hand);
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

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return new List<CardData>();
        }

        return players[currentPlayerIndex].handCards;
    }

    public bool CanPlayHandCard(int handCardIndex)
    {
        if (isGameOver || players == null || players.Count == 0)
        {
            return false;
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return false;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return false;
        }

        if (handCardIndex < 0 || handCardIndex >= currentPlayer.handCards.Count)
        {
            return false;
        }

        if (hasDrawnThisTurn && handCardIndex != drawnCardIndex)
        {
            return false;
        }

        if (pendingDrawPenalty > 0)
        {
            return IsStackableDrawCard(currentPlayer.handCards[handCardIndex], currentPlayer.handCards);
        }

        return RuleChecker.IsValidMove(currentPlayer.handCards[handCardIndex], topDiscardCard, currentColor, currentPlayer.handCards);
    }

    public bool HasAnyPlayableCard()
    {
        if (players == null || players.Count == 0)
        {
            return false;
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return false;
        }

        PlayerData currentPlayer = players[currentPlayerIndex];
        if (!IsPlayerActive(currentPlayer))
        {
            return false;
        }

        if (pendingDrawPenalty > 0)
        {
            return HasStackableDrawCard(currentPlayer.handCards);
        }

        return RuleChecker.HasPlayableCard(currentPlayer.handCards, topDiscardCard, currentColor);
    }

    public bool IsWaitingForDrawDecision()
    {
        return hasDrawnThisTurn;
    }

    public bool HasPendingDrawPenalty()
    {
        return pendingDrawPenalty > 0;
    }

    public int GetPendingDrawPenalty()
    {
        return pendingDrawPenalty;
    }

    public string GetPendingDrawStackLabel()
    {
        return "+2/+4";
    }

    public CardColor GetCurrentColor()
    {
        return currentColor;
    }

    public CardData GetTopDiscardCard()
    {
        return topDiscardCard;
    }

    public string GetCurrentPlayerName()
    {
        if (players == null || players.Count == 0)
        {
            return "";
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return "";
        }

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
            return !isGameOver &&
                   currentPlayerIndex >= 0 &&
                   currentPlayerIndex < players.Count &&
                   IsPlayerActive(players[currentPlayerIndex]);
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= PhotonNetwork.PlayerList.Length)
        {
            return false;
        }

        if (currentPlayerIndex >= players.Count || !IsPlayerActive(players[currentPlayerIndex]))
        {
            return false;
        }

        Player currentPhotonPlayer = PhotonNetwork.PlayerList[currentPlayerIndex];
        return currentPhotonPlayer == PhotonNetwork.LocalPlayer;
    }

    public bool RequiresColorChoice(CardData card)
    {
        return card != null && (card.type == CardType.ChangeColor || card.type == CardType.DrawFour);
    }

    private bool IsValidChosenColor(CardColor color)
    {
        return color == CardColor.Red ||
               color == CardColor.Blue ||
               color == CardColor.Green ||
               color == CardColor.Yellow;
    }

    private bool IsStackableDrawCard(CardData card, List<CardData> hand)
    {
        if (card == null || topDiscardCard == null || pendingDrawPenalty <= 0)
        {
            return false;
        }

        return card.type == CardType.DrawTwo || card.type == CardType.DrawFour;
    }

    private bool HasStackableDrawCard(List<CardData> hand)
    {
        if (hand == null)
        {
            return false;
        }

        foreach (CardData card in hand)
        {
            if (IsStackableDrawCard(card, hand))
            {
                return true;
            }
        }

        return false;
    }

    private CardColor ChooseBestColorForPlayer(PlayerData player)
    {
        if (player == null)
        {
            return CardColor.Red;
        }

        int red = 0;
        int blue = 0;
        int green = 0;
        int yellow = 0;

        foreach (CardData card in player.handCards)
        {
            switch (card.color)
            {
                case CardColor.Red:
                    red++;
                    break;
                case CardColor.Blue:
                    blue++;
                    break;
                case CardColor.Green:
                    green++;
                    break;
                case CardColor.Yellow:
                    yellow++;
                    break;
            }
        }

        int best = red;
        CardColor bestColor = CardColor.Red;

        if (blue > best)
        {
            best = blue;
            bestColor = CardColor.Blue;
        }

        if (green > best)
        {
            best = green;
            bestColor = CardColor.Green;
        }

        if (yellow > best)
        {
            bestColor = CardColor.Yellow;
        }

        return bestColor;
    }

    private string GetTopCardStatus()
    {
        if (topDiscardCard == null)
        {
            return "empty discard";
        }

        return topDiscardCard.GetDisplayName() + " / " + currentColor;
    }



    private void ClearVisualEventPayload()
    {
        visualEventType = "";
        visualEventPlayerIndex = -1;
        visualEventCard = null;
        visualEventWinner = "";
        lastAppliedVisualEventSequence = visualEventSequence;
    }

    private void RegisterVisualEvent(string eventType, int playerIndex = -1, CardData card = null, string winner = "")
    {
        visualEventSequence++;
        visualEventType = eventType;
        visualEventPlayerIndex = playerIndex;
        visualEventCard = card;
        visualEventWinner = winner;
        lastAppliedVisualEventSequence = visualEventSequence;
    }

    private void NotifyCardPlayed(CardData card, int playerIndex)
    {
        RegisterVisualEvent("play", playerIndex, card);
        GameEvents.CardPlayed(card, playerIndex);
    }

    private void NotifyCardDrawn(int playerIndex)
    {
        RegisterVisualEvent("draw", playerIndex);
        GameEvents.CardDrawn(playerIndex);
    }

    private void NotifyGameOver(string winner)
    {
        RegisterVisualEvent("gameOver", -1, null, winner);
        GameEvents.GameOver(winner);
    }

    private void ReplayNetworkVisualEvent(GameStateData state)
    {
        if (state == null)
        {
            return;
        }

        bool hasNewVisualEvent = state.visualEventSequence > 0 && state.visualEventSequence > lastAppliedVisualEventSequence;
        if (hasNewVisualEvent)
        {
            lastAppliedVisualEventSequence = state.visualEventSequence;

            switch (state.visualEventType)
            {
                case "play":
                    if (state.visualEventCard != null && state.visualEventPlayerIndex >= 0)
                    {
                        GameEvents.CardPlayed(state.visualEventCard, state.visualEventPlayerIndex);
                    }
                    break;

                case "draw":
                    if (state.visualEventPlayerIndex >= 0)
                    {
                        GameEvents.CardDrawn(state.visualEventPlayerIndex);
                    }
                    break;

                case "gameOver":
                    GameEvents.GameOver(state.visualEventWinner);
                    break;
            }
        }

        GameEvents.TurnChanged(currentPlayerIndex);
    }

    private GameStateData CreateStateData()
    {
        GameStateData state = new GameStateData();

        state.players = players;
        state.deckCards = deckManager.GetDeckCards();
        state.discardPile = discardPile;
        state.topDiscardCard = topDiscardCard;
        state.currentColor = currentColor;
        state.currentPlayerIndex = currentPlayerIndex;
        state.direction = direction;
        state.hasDrawnThisTurn = hasDrawnThisTurn;
        state.drawnCardIndex = drawnCardIndex;
        state.pendingDrawPenalty = pendingDrawPenalty;
        state.unoDeclaredPlayerIndex = unoDeclaredPlayerIndex;
        state.finishOrder = finishOrder;
        state.isGameOver = isGameOver;
        state.lastMessage = lastMessage;
        state.winnerName = winnerName;
        state.visualEventSequence = visualEventSequence;
        state.visualEventType = visualEventType;
        state.visualEventPlayerIndex = visualEventPlayerIndex;
        state.visualEventCard = visualEventCard;
        state.visualEventWinner = visualEventWinner;

        return state;
    }

    private void ApplyStateData(GameStateData state)
    {
        if (state == null)
        {
            return;
        }

        players = state.players ?? new List<PlayerData>();
        deckManager.SetDeckCards(state.deckCards);
        discardPile = state.discardPile ?? new List<CardData>();
        topDiscardCard = state.topDiscardCard;
        currentColor = state.currentColor;
        currentPlayerIndex = state.currentPlayerIndex;
        direction = state.direction;
        hasDrawnThisTurn = state.hasDrawnThisTurn;
        drawnCardIndex = state.drawnCardIndex;
        pendingDrawPenalty = state.pendingDrawPenalty;
        unoDeclaredPlayerIndex = state.unoDeclaredPlayerIndex;
        finishOrder = state.finishOrder;
        isGameOver = state.isGameOver;
        lastMessage = state.lastMessage;
        winnerName = state.winnerName;
        visualEventSequence = state.visualEventSequence;
        visualEventType = state.visualEventType;
        visualEventPlayerIndex = state.visualEventPlayerIndex;
        visualEventCard = state.visualEventCard;
        visualEventWinner = state.visualEventWinner;
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
            lastAppliedVisualEventSequence = state.visualEventSequence;
            GameEvents.TurnChanged(currentPlayerIndex);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("gameState"))
        {
            string json = propertiesThatChanged["gameState"].ToString();
            GameStateData state = JsonUtility.FromJson<GameStateData>(json);
            ApplyStateData(state);
            ReplayNetworkVisualEvent(state);

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
