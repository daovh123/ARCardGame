using System.Collections.Generic;
using UnityEngine;

public class TienLenGameManager : MonoBehaviour
{
    private readonly List<TienLenPlayerData> players = new List<TienLenPlayerData>();
    private readonly bool[] passed = new bool[4];
    private readonly List<PlayingCardData> tableCards = new List<PlayingCardData>();

    private TienLenCombination tableCombination;
    private int currentPlayerIndex;
    private int tableOwnerIndex = -1;
    private int finishOrder;
    private bool tableOwnerFinished;
    private bool isGameOver;
    private string lastMessage = "";
    private int feedbackVersion;
    private TienLenFeedbackSnapshot lastFeedback = new TienLenFeedbackSnapshot();

    public void StartGame()
    {
        players.Clear();
        tableCards.Clear();
        tableCombination = null;
        tableOwnerIndex = -1;
        tableOwnerFinished = false;
        finishOrder = 0;
        isGameOver = false;
        lastMessage = "Player with 3 of Spades starts. They may lead any valid set.";

        for (int i = 0; i < 4; i++)
        {
            players.Add(new TienLenPlayerData(i, "Player " + (i + 1)));
            passed[i] = false;
        }

        List<PlayingCardData> deck = CreateDeck();
        Shuffle(deck);
        Deal(deck);
        currentPlayerIndex = FindPlayerWithThreeSpades();
        RecordFeedback(
            TienLenFeedbackKind.GameStart,
            currentPlayerIndex,
            currentPlayerIndex,
            "3 OF SPADES STARTS",
            players[currentPlayerIndex].playerName + " leads first. Any valid set may be played.",
            null,
            0);

        if (TryApplyInstantWin())
        {
            return;
        }
    }

    public bool PlayCards(List<int> selectedHandIndices)
    {
        if (isGameOver || selectedHandIndices == null || selectedHandIndices.Count == 0)
        {
            lastMessage = "Select at least one card.";
            RecordFeedback(TienLenFeedbackKind.Invalid, currentPlayerIndex, currentPlayerIndex, "INVALID", lastMessage, null, 0);
            return false;
        }

        TienLenPlayerData player = players[currentPlayerIndex];
        List<PlayingCardData> selectedCards = GetSelectedCards(player, selectedHandIndices);
        TienLenCombination challenger = TienLenRuleChecker.Evaluate(selectedCards);

        if (!challenger.IsValid)
        {
            lastMessage = "Invalid set. Use single, pair, triple, straight, four-kind, or consecutive pairs.";
            RecordFeedback(TienLenFeedbackKind.Invalid, currentPlayerIndex, currentPlayerIndex, "INVALID SET", lastMessage, selectedCards, 0);
            return false;
        }

        if (!TienLenRuleChecker.CanBeat(challenger, tableCombination))
        {
            lastMessage = challenger.GetLabel() + " cannot beat " + tableCombination.GetLabel() + ".";
            RecordFeedback(TienLenFeedbackKind.Invalid, currentPlayerIndex, currentPlayerIndex, "CANNOT BEAT", lastMessage, selectedCards, 0);
            return false;
        }

        TienLenCombination previousCombination = tableCombination;
        RemoveSelectedCards(player, selectedCards);
        tableCards.Clear();
        tableCards.AddRange(challenger.cards);
        tableCombination = challenger;
        tableOwnerIndex = currentPlayerIndex;
        tableOwnerFinished = false;
        passed[currentPlayerIndex] = false;

        lastMessage = player.playerName + " played " + challenger.GetLabel() + ": " + FormatCards(challenger.cards) + ".";
        RecordPlayFeedback(player.playerIndex, challenger, previousCombination);

        if (player.handCards.Count == 0)
        {
            FinishPlayer(player);
            tableOwnerFinished = true;
        }

        AdvanceTurnAfterAction();
        AttachCurrentPlayerToLatestFeedback();
        return true;
    }

    public bool Pass()
    {
        if (isGameOver)
        {
            return false;
        }

        if (tableCombination == null || !tableCombination.IsValid)
        {
            lastMessage = "You are leading this round. Play a valid set instead of passing.";
            RecordFeedback(TienLenFeedbackKind.Invalid, currentPlayerIndex, currentPlayerIndex, "LEAD REQUIRED", lastMessage, null, 0);
            return false;
        }

        if (currentPlayerIndex == tableOwnerIndex)
        {
            lastMessage = "You won the trick. Play the next set.";
            RecordFeedback(TienLenFeedbackKind.Invalid, currentPlayerIndex, currentPlayerIndex, "PLAY NEXT", lastMessage, null, 0);
            return false;
        }

        int actorIndex = currentPlayerIndex;
        passed[currentPlayerIndex] = true;
        lastMessage = players[currentPlayerIndex].playerName + " passed.";
        RecordFeedback(TienLenFeedbackKind.Pass, actorIndex, FindNextEligiblePlayer(actorIndex), "PASS", lastMessage, null, 0);
        RuntimeSfx.Play(RuntimeSfxType.Pass, 0.68f);
        AdvanceTurnAfterAction();
        AttachCurrentPlayerToLatestFeedback();
        return true;
    }

    public bool CanPlaySelection(List<int> selectedHandIndices)
    {
        if (isGameOver || selectedHandIndices == null || selectedHandIndices.Count == 0)
        {
            return false;
        }

        TienLenPlayerData player = players[currentPlayerIndex];
        List<PlayingCardData> selectedCards = GetSelectedCards(player, selectedHandIndices);
        TienLenCombination challenger = TienLenRuleChecker.Evaluate(selectedCards);

        if (!challenger.IsValid)
        {
            return false;
        }

        return TienLenRuleChecker.CanBeat(challenger, tableCombination);
    }

    public List<TienLenPlayerData> GetPlayers()
    {
        return players;
    }

    public List<PlayingCardData> GetCurrentHand()
    {
        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return new List<PlayingCardData>();
        }

        return players[currentPlayerIndex].handCards;
    }

    public List<PlayingCardData> GetTableCards()
    {
        return tableCards;
    }

    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public string GetCurrentPlayerName()
    {
        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return "";
        }

        return players[currentPlayerIndex].playerName;
    }

    public string GetLastMessage()
    {
        return lastMessage;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public string GetTableLabel()
    {
        if (tableCombination == null || !tableCombination.IsValid)
        {
            return "Lead any valid set";
        }

        return tableCombination.GetLabel() + " by " + players[tableOwnerIndex].playerName;
    }

    public bool HasActiveTable()
    {
        return tableCombination != null && tableCombination.IsValid;
    }

    public bool HasPassed(int playerIndex)
    {
        return playerIndex >= 0 && playerIndex < passed.Length && passed[playerIndex];
    }

    public TienLenFeedbackSnapshot GetFeedbackSnapshot()
    {
        return lastFeedback.Clone();
    }

    private List<PlayingCardData> CreateDeck()
    {
        List<PlayingCardData> deck = new List<PlayingCardData>();
        int cardId = 0;

        for (int rank = 3; rank <= 14; rank++)
        {
            AddSuitSet(deck, cardId, rank);
            cardId += 4;
        }

        AddSuitSet(deck, cardId, 2);
        return deck;
    }

    private void AddSuitSet(List<PlayingCardData> deck, int startId, int rank)
    {
        deck.Add(new PlayingCardData(startId, rank, PlayingCardSuit.Spades));
        deck.Add(new PlayingCardData(startId + 1, rank, PlayingCardSuit.Clubs));
        deck.Add(new PlayingCardData(startId + 2, rank, PlayingCardSuit.Diamonds));
        deck.Add(new PlayingCardData(startId + 3, rank, PlayingCardSuit.Hearts));
    }

    private void Shuffle(List<PlayingCardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            PlayingCardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    private void Deal(List<PlayingCardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            players[i % players.Count].handCards.Add(deck[i]);
        }

        foreach (TienLenPlayerData player in players)
        {
            TienLenRuleChecker.SortHand(player.handCards);
        }
    }

    private int FindPlayerWithThreeSpades()
    {
        foreach (TienLenPlayerData player in players)
        {
            foreach (PlayingCardData card in player.handCards)
            {
                if (card.IsThreeSpades)
                {
                    return player.playerIndex;
                }
            }
        }

        return 0;
    }

    private bool TryApplyInstantWin()
    {
        foreach (TienLenPlayerData player in players)
        {
            string reason = GetInstantWinReason(player.handCards);

            if (string.IsNullOrEmpty(reason))
            {
                continue;
            }

            finishOrder++;
            player.hasFinished = true;
            player.finishRank = finishOrder;
            isGameOver = true;
            lastMessage = player.playerName + " wins instantly with " + reason + ".";
            RecordFeedback(TienLenFeedbackKind.InstantWin, player.playerIndex, -1, "INSTANT WIN", lastMessage, player.handCards, player.finishRank);
            RuntimeSfx.Play(RuntimeSfxType.Win, 0.90f);
            return true;
        }

        return false;
    }

    private string GetInstantWinReason(List<PlayingCardData> hand)
    {
        if (HasFourTwos(hand))
        {
            return "four 2s";
        }

        if (IsAllOneColor(hand))
        {
            return "all red/black cards";
        }

        if (IsDragonStraight(hand))
        {
            return "dragon straight A-2-3-...-K";
        }

        return "";
    }

    private bool HasFourTwos(List<PlayingCardData> hand)
    {
        int twoCount = 0;

        foreach (PlayingCardData card in hand)
        {
            if (card.IsTwo)
            {
                twoCount++;
            }
        }

        return twoCount == 4;
    }

    private bool IsAllOneColor(List<PlayingCardData> hand)
    {
        if (hand == null || hand.Count == 0)
        {
            return false;
        }

        bool firstIsRed = IsRedSuit(hand[0].suit);

        foreach (PlayingCardData card in hand)
        {
            if (IsRedSuit(card.suit) != firstIsRed)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsRedSuit(PlayingCardSuit suit)
    {
        return suit == PlayingCardSuit.Diamonds || suit == PlayingCardSuit.Hearts;
    }

    private bool IsDragonStraight(List<PlayingCardData> hand)
    {
        if (hand == null || hand.Count != 13)
        {
            return false;
        }

        bool[] ranks = new bool[16];

        foreach (PlayingCardData card in hand)
        {
            int rank = card.rank;

            if (rank < 2 || rank > 14 || ranks[rank])
            {
                return false;
            }

            ranks[rank] = true;
        }

        for (int rank = 2; rank <= 14; rank++)
        {
            if (!ranks[rank])
            {
                return false;
            }
        }

        return true;
    }

    private List<PlayingCardData> GetSelectedCards(TienLenPlayerData player, List<int> selectedHandIndices)
    {
        List<PlayingCardData> selectedCards = new List<PlayingCardData>();
        List<int> indices = new List<int>(selectedHandIndices);
        indices.Sort();

        for (int i = 0; i < indices.Count; i++)
        {
            int index = indices[i];
            if (index >= 0 && index < player.handCards.Count)
            {
                selectedCards.Add(player.handCards[index]);
            }
        }

        return selectedCards;
    }

    private void RemoveSelectedCards(TienLenPlayerData player, List<PlayingCardData> selectedCards)
    {
        foreach (PlayingCardData selectedCard in selectedCards)
        {
            player.handCards.Remove(selectedCard);
        }
    }

    private void FinishPlayer(TienLenPlayerData player)
    {
        if (player.hasFinished)
        {
            return;
        }

        finishOrder++;
        player.hasFinished = true;
        player.finishRank = finishOrder;
        lastMessage += " " + player.playerName + " finished #" + player.finishRank + ".";
        RecordFeedback(TienLenFeedbackKind.Finish, player.playerIndex, currentPlayerIndex, "FINISHED #" + player.finishRank, player.playerName + " finished #" + player.finishRank + ".", null, player.finishRank);
        RuntimeSfx.Play(RuntimeSfxType.Win, 0.78f);

        if (GetActivePlayerCount() <= 1)
        {
            TienLenPlayerData lastPlayer = GetOnlyActivePlayer();
            if (lastPlayer != null)
            {
                lastPlayer.hasFinished = true;
                lastPlayer.finishRank = players.Count;
                lastMessage += " " + lastPlayer.playerName + " is last place.";
            }

            isGameOver = true;
            lastMessage += " Round complete.";
            RecordFeedback(TienLenFeedbackKind.RoundComplete, player.playerIndex, -1, "ROUND COMPLETE", lastMessage, null, player.finishRank);
        }
    }

    private void AdvanceTurnAfterAction()
    {
        if (isGameOver)
        {
            return;
        }

        if (tableOwnerIndex >= 0 && AllOtherActivePlayersPassed())
        {
            int leader = players[tableOwnerIndex].IsActive ? tableOwnerIndex : FindNextActivePlayer(tableOwnerIndex);
            ClearTrick();
            currentPlayerIndex = leader;
            lastMessage += " " + players[currentPlayerIndex].playerName + " leads next.";
            RecordFeedback(TienLenFeedbackKind.NewTrick, leader, leader, "NEW TRICK", players[currentPlayerIndex].playerName + " leads next.", null, 0);
            return;
        }

        currentPlayerIndex = tableOwnerFinished
            ? FindNextEligiblePlayerFromOwner()
            : FindNextEligiblePlayer(currentPlayerIndex);
    }

    private void ClearTrick()
    {
        tableCombination = null;
        tableOwnerIndex = -1;
        tableOwnerFinished = false;
        tableCards.Clear();

        for (int i = 0; i < passed.Length; i++)
        {
            passed[i] = false;
        }
    }

    private bool AllOtherActivePlayersPassed()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i == tableOwnerIndex)
            {
                continue;
            }

            if (players[i].IsActive && !passed[i])
            {
                return false;
            }
        }

        return true;
    }

    private int FindNextEligiblePlayer(int fromIndex)
    {
        for (int step = 1; step <= players.Count; step++)
        {
            int nextIndex = (fromIndex + step) % players.Count;

            if (!players[nextIndex].IsActive)
            {
                continue;
            }

            if (tableCombination != null && tableCombination.IsValid && passed[nextIndex])
            {
                continue;
            }

            return nextIndex;
        }

        return FindNextActivePlayer(fromIndex);
    }

    private int FindNextEligiblePlayerFromOwner()
    {
        if (tableOwnerIndex < 0)
        {
            return FindNextEligiblePlayer(currentPlayerIndex);
        }

        return FindNextEligiblePlayer(tableOwnerIndex);
    }

    private int FindNextActivePlayer(int fromIndex)
    {
        for (int step = 1; step <= players.Count; step++)
        {
            int nextIndex = (fromIndex + step) % players.Count;

            if (players[nextIndex].IsActive)
            {
                return nextIndex;
            }
        }

        return fromIndex;
    }

    private int GetActivePlayerCount()
    {
        int count = 0;

        foreach (TienLenPlayerData player in players)
        {
            if (player.IsActive)
            {
                count++;
            }
        }

        return count;
    }

    private TienLenPlayerData GetOnlyActivePlayer()
    {
        foreach (TienLenPlayerData player in players)
        {
            if (player.IsActive)
            {
                return player;
            }
        }

        return null;
    }

    private string FormatCards(List<PlayingCardData> cards)
    {
        string text = "";

        for (int i = 0; i < cards.Count; i++)
        {
            if (i > 0)
            {
                text += " ";
            }

            text += cards[i].GetDisplayName();
        }

        return text;
    }

    private void RecordPlayFeedback(int actorIndex, TienLenCombination challenger, TienLenCombination previousCombination)
    {
        bool bomb = IsBombFeedback(challenger, previousCombination);
        TienLenFeedbackKind kind = bomb ? TienLenFeedbackKind.Bomb : TienLenFeedbackKind.Play;
        string title = GetCombinationFeedbackTitle(challenger, previousCombination, bomb);
        string message = players[actorIndex].playerName + " played " + challenger.GetLabel() + ".";
        RecordFeedback(kind, actorIndex, currentPlayerIndex, title, message, challenger.cards, 0, challenger);
    }

    private void RecordFeedback(
        TienLenFeedbackKind kind,
        int actorIndex,
        int nextPlayerIndex,
        string title,
        string message,
        List<PlayingCardData> cards,
        int finishRank,
        TienLenCombination combination = null)
    {
        feedbackVersion++;
        lastFeedback = new TienLenFeedbackSnapshot
        {
            version = feedbackVersion,
            kind = kind,
            actorIndex = actorIndex,
            nextPlayerIndex = nextPlayerIndex,
            finishRank = finishRank,
            title = title,
            message = message
        };

        if (cards != null)
        {
            lastFeedback.cards = new List<PlayingCardData>(cards);
        }

        if (combination != null)
        {
            lastFeedback.combinationType = combination.type;
            lastFeedback.pairRunLength = combination.PairRunLength;
            lastFeedback.containsTwo = combination.ContainsTwo;
        }
    }

    private void AttachCurrentPlayerToLatestFeedback()
    {
        if (isGameOver || lastFeedback == null)
        {
            return;
        }

        if (lastFeedback.kind == TienLenFeedbackKind.Play ||
            lastFeedback.kind == TienLenFeedbackKind.Bomb ||
            lastFeedback.kind == TienLenFeedbackKind.Pass ||
            lastFeedback.kind == TienLenFeedbackKind.Finish)
        {
            lastFeedback.nextPlayerIndex = currentPlayerIndex;
        }
    }

    private bool IsBombFeedback(TienLenCombination challenger, TienLenCombination previousCombination)
    {
        if (challenger == null || previousCombination == null || !previousCombination.IsValid)
        {
            return false;
        }

        bool previousSingleTwo = previousCombination.type == TienLenCombinationType.Single && previousCombination.ContainsTwo;
        bool previousPairTwo = previousCombination.type == TienLenCombinationType.Pair && previousCombination.ContainsTwo;

        if (challenger.type == TienLenCombinationType.FourKind)
        {
            return previousSingleTwo || previousPairTwo || previousCombination.type == TienLenCombinationType.FourKind;
        }

        if (challenger.type == TienLenCombinationType.ConsecutivePairs)
        {
            if (challenger.PairRunLength == 3 && previousSingleTwo)
            {
                return true;
            }

            return challenger.PairRunLength >= 4 &&
                (previousSingleTwo || previousPairTwo || previousCombination.type == TienLenCombinationType.FourKind ||
                 (previousCombination.type == TienLenCombinationType.ConsecutivePairs && previousCombination.PairRunLength == challenger.PairRunLength));
        }

        return false;
    }

    private string GetCombinationFeedbackTitle(TienLenCombination challenger, TienLenCombination previousCombination, bool bomb)
    {
        if (bomb && previousCombination != null && previousCombination.ContainsTwo)
        {
            return "CHAT HEO!";
        }

        if (challenger.type == TienLenCombinationType.FourKind)
        {
            return "TU QUY!";
        }

        if (challenger.type == TienLenCombinationType.ConsecutivePairs)
        {
            return challenger.PairRunLength + " DOI THONG!";
        }

        switch (challenger.type)
        {
            case TienLenCombinationType.Single:
                return "SINGLE";

            case TienLenCombinationType.Pair:
                return "PAIR";

            case TienLenCombinationType.Triple:
                return "TRIPLE";

            case TienLenCombinationType.Straight:
                return "STRAIGHT";

            default:
                return "PLAYED";
        }
    }
}
