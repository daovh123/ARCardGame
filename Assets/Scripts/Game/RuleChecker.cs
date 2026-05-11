using UnityEngine;
public static class RuleChecker
{
    public static bool IsValidMove(CardData selectedCard, CardData topCard)
    {
        if (selectedCard == null || topCard == null)
        {
            return false;
        }

        if (selectedCard.color == topCard.color)
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
}