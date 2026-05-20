using System;

public enum PlayingCardSuit
{
    Spades,
    Clubs,
    Diamonds,
    Hearts
}

[Serializable]
public class PlayingCardData
{
    public int cardId;
    public int rank;
    public PlayingCardSuit suit;

    public PlayingCardData()
    {
    }

    public PlayingCardData(int cardId, int rank, PlayingCardSuit suit)
    {
        this.cardId = cardId;
        this.rank = rank;
        this.suit = suit;
    }

    public int RankValue
    {
        get { return rank == 2 ? 15 : rank; }
    }

    public int SuitValue
    {
        get { return (int)suit; }
    }

    public int SortValue
    {
        get { return RankValue * 10 + SuitValue; }
    }

    public bool IsThreeSpades
    {
        get { return rank == 3 && suit == PlayingCardSuit.Spades; }
    }

    public bool IsTwo
    {
        get { return rank == 2; }
    }

    public string RankLabel
    {
        get
        {
            switch (rank)
            {
                case 11:
                    return "J";

                case 12:
                    return "Q";

                case 13:
                    return "K";

                case 14:
                    return "A";

                default:
                    return rank.ToString();
            }
        }
    }

    public string SuitLabel
    {
        get
        {
            switch (suit)
            {
                case PlayingCardSuit.Spades:
                    return "♠";

                case PlayingCardSuit.Clubs:
                    return "♣";

                case PlayingCardSuit.Diamonds:
                    return "♦";

                case PlayingCardSuit.Hearts:
                    return "♥";

                default:
                    return "?";
            }
        }
    }

    public string GetDisplayName()
    {
        return RankLabel + SuitLabel;
    }
}
