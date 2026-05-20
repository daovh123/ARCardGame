using System.Collections.Generic;

public static class TienLenRuleChecker
{
    public static void SortHand(List<PlayingCardData> cards)
    {
        if (cards == null)
        {
            return;
        }

        cards.Sort((a, b) => a.SortValue.CompareTo(b.SortValue));
    }

    public static TienLenCombination Evaluate(List<PlayingCardData> selectedCards)
    {
        TienLenCombination combination = new TienLenCombination();
        combination.type = TienLenCombinationType.Invalid;

        if (selectedCards == null || selectedCards.Count == 0)
        {
            return combination;
        }

        combination.cards = new List<PlayingCardData>(selectedCards);
        SortHand(combination.cards);
        combination.highSortValue = combination.cards[combination.cards.Count - 1].SortValue;
        combination.mainRankValue = combination.cards[combination.cards.Count - 1].RankValue;

        int count = combination.cards.Count;

        if (count == 1)
        {
            combination.type = TienLenCombinationType.Single;
            return combination;
        }

        if (AllSameRank(combination.cards))
        {
            if (count == 2)
            {
                combination.type = TienLenCombinationType.Pair;
            }
            else if (count == 3)
            {
                combination.type = TienLenCombinationType.Triple;
            }
            else if (count == 4)
            {
                combination.type = TienLenCombinationType.FourKind;
            }

            return combination;
        }

        if (IsStraight(combination.cards))
        {
            combination.type = TienLenCombinationType.Straight;
            return combination;
        }

        if (IsConsecutivePairs(combination.cards))
        {
            combination.type = TienLenCombinationType.ConsecutivePairs;
            return combination;
        }

        return combination;
    }

    public static bool CanBeat(TienLenCombination challenger, TienLenCombination table)
    {
        if (challenger == null || !challenger.IsValid)
        {
            return false;
        }

        if (table == null || !table.IsValid)
        {
            return true;
        }

        if (CanBomb(challenger, table))
        {
            return true;
        }

        if (challenger.type != table.type || challenger.cards.Count != table.cards.Count)
        {
            return false;
        }

        if (challenger.mainRankValue != table.mainRankValue)
        {
            return challenger.mainRankValue > table.mainRankValue;
        }

        return challenger.highSortValue > table.highSortValue;
    }

    public static bool ContainsThreeSpades(List<PlayingCardData> cards)
    {
        if (cards == null)
        {
            return false;
        }

        foreach (PlayingCardData card in cards)
        {
            if (card.IsThreeSpades)
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanBomb(TienLenCombination challenger, TienLenCombination table)
    {
        bool tableSingleTwo = table.type == TienLenCombinationType.Single && table.ContainsTwo;
        bool tablePairTwo = table.type == TienLenCombinationType.Pair && table.ContainsTwo;

        if (challenger.type == TienLenCombinationType.FourKind)
        {
            if (tableSingleTwo || tablePairTwo)
            {
                return true;
            }

            if (table.type == TienLenCombinationType.FourKind)
            {
                return challenger.mainRankValue > table.mainRankValue;
            }
        }

        if (challenger.type == TienLenCombinationType.ConsecutivePairs)
        {
            if (challenger.PairRunLength == 3 && tableSingleTwo)
            {
                return true;
            }

            if (challenger.PairRunLength >= 4 && (tableSingleTwo || tablePairTwo || table.type == TienLenCombinationType.FourKind))
            {
                return true;
            }

            if (table.type == TienLenCombinationType.ConsecutivePairs &&
                challenger.PairRunLength == table.PairRunLength)
            {
                return challenger.mainRankValue > table.mainRankValue;
            }

            if (table.type == TienLenCombinationType.ConsecutivePairs)
            {
                return false;
            }
        }

        return false;
    }

    private static bool AllSameRank(List<PlayingCardData> cards)
    {
        int rank = cards[0].rank;

        foreach (PlayingCardData card in cards)
        {
            if (card.rank != rank)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsStraight(List<PlayingCardData> cards)
    {
        if (cards.Count < 3)
        {
            return false;
        }

        int previousRank = -1;

        foreach (PlayingCardData card in cards)
        {
            if (card.IsTwo)
            {
                return false;
            }

            if (previousRank >= 0 && card.rank != previousRank + 1)
            {
                return false;
            }

            previousRank = card.rank;
        }

        return true;
    }

    private static bool IsConsecutivePairs(List<PlayingCardData> cards)
    {
        if (cards.Count < 6 || cards.Count % 2 != 0)
        {
            return false;
        }

        Dictionary<int, int> rankCounts = new Dictionary<int, int>();

        foreach (PlayingCardData card in cards)
        {
            if (card.IsTwo)
            {
                return false;
            }

            if (!rankCounts.ContainsKey(card.rank))
            {
                rankCounts[card.rank] = 0;
            }

            rankCounts[card.rank]++;
        }

        List<int> ranks = new List<int>(rankCounts.Keys);
        ranks.Sort();

        if (ranks.Count != cards.Count / 2)
        {
            return false;
        }

        for (int i = 0; i < ranks.Count; i++)
        {
            if (rankCounts[ranks[i]] != 2)
            {
                return false;
            }

            if (i > 0 && ranks[i] != ranks[i - 1] + 1)
            {
                return false;
            }
        }

        return true;
    }
}
