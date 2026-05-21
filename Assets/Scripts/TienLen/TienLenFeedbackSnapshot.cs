using System.Collections.Generic;

public enum TienLenFeedbackKind
{
    None,
    GameStart,
    Play,
    Pass,
    Invalid,
    Bomb,
    NewTrick,
    Finish,
    RoundComplete,
    InstantWin
}

public class TienLenFeedbackSnapshot
{
    public int version;
    public TienLenFeedbackKind kind;
    public int actorIndex = -1;
    public int nextPlayerIndex = -1;
    public int finishRank;
    public string title = "";
    public string message = "";
    public TienLenCombinationType combinationType = TienLenCombinationType.Invalid;
    public int pairRunLength;
    public bool containsTwo;
    public List<PlayingCardData> cards = new List<PlayingCardData>();

    public TienLenFeedbackSnapshot Clone()
    {
        return new TienLenFeedbackSnapshot
        {
            version = version,
            kind = kind,
            actorIndex = actorIndex,
            nextPlayerIndex = nextPlayerIndex,
            finishRank = finishRank,
            title = title,
            message = message,
            combinationType = combinationType,
            pairRunLength = pairRunLength,
            containsTwo = containsTwo,
            cards = new List<PlayingCardData>(cards)
        };
    }
}
