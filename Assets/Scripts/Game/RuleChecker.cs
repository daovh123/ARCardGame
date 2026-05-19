using System.Collections.Generic;

public static class RuleChecker
{
    public static bool IsValidMove(CardData selectedCard, CardData topCard)
    {
        CardColor currentColor = topCard != null ? topCard.color : CardColor.Wild;
        return IsValidMove(selectedCard, topCard, currentColor, null);
    }

    public static bool IsValidMove(CardData selectedCard, CardData topCard, CardColor currentColor, List<CardData> currentHand)
    {
        if (selectedCard == null || topCard == null)
        {
            return false;
        }

        if (selectedCard.type == CardType.ChangeColor)
        {
            return true;
        }

        if (selectedCard.type == CardType.DrawFour)
        {
            return true;
        }

        if (currentColor != CardColor.Wild && selectedCard.color == currentColor)
        {
            return true;
        }

        if (selectedCard.type == CardType.Number &&
            topCard.type == CardType.Number &&
            selectedCard.number == topCard.number)
        {
            return true;
        }

        if (selectedCard.type != CardType.Number &&
            selectedCard.type == topCard.type)
        {
            return true;
        }

        return false;
    }

    public static bool HasPlayableCard(List<CardData> hand, CardData topCard, CardColor currentColor)
    {
        if (hand == null)
        {
            return false;
        }

        foreach (CardData card in hand)
        {
            if (IsValidMove(card, topCard, currentColor, hand))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasCardMatchingColor(List<CardData> hand, CardColor currentColor)
    {
        if (hand == null || currentColor == CardColor.Wild)
        {
            return false;
        }

        foreach (CardData card in hand)
        {
            if (card != null && card.color == currentColor)
            {
                return true;
            }
        }

        return false;
    }
}
