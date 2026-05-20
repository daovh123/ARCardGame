using System.Collections.Generic;

public enum TienLenCombinationType
{
    Invalid,
    Single,
    Pair,
    Triple,
    FourKind,
    Straight,
    ConsecutivePairs
}

public class TienLenCombination
{
    public TienLenCombinationType type;
    public List<PlayingCardData> cards = new List<PlayingCardData>();
    public int mainRankValue;
    public int highSortValue;

    public bool IsValid
    {
        get { return type != TienLenCombinationType.Invalid; }
    }

    public bool ContainsTwo
    {
        get
        {
            foreach (PlayingCardData card in cards)
            {
                if (card.IsTwo)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public int PairRunLength
    {
        get
        {
            if (type != TienLenCombinationType.ConsecutivePairs)
            {
                return 0;
            }

            return cards.Count / 2;
        }
    }

    public string GetLabel()
    {
        switch (type)
        {
            case TienLenCombinationType.Single:
                return "single";

            case TienLenCombinationType.Pair:
                return "pair";

            case TienLenCombinationType.Triple:
                return "triple";

            case TienLenCombinationType.FourKind:
                return "four of a kind";

            case TienLenCombinationType.Straight:
                return "straight";

            case TienLenCombinationType.ConsecutivePairs:
                return PairRunLength + " consecutive pairs";

            default:
                return "invalid";
        }
    }
}
