public static class RuleChecker
{
    public static bool IsValidMove(CardData selectedCard, CardData topCard)
    {
        if (selectedCard == null || topCard == null)
        {
            return false;
        }

        // Lá đổi màu và +4 được đánh bất cứ lúc nào
        if (selectedCard.type == CardType.ChangeColor ||
            selectedCard.type == CardType.DrawFour)
        {
            return true;
        }

        // Cùng màu
        if (selectedCard.color == topCard.color)
        {
            return true;
        }

        // Cùng số
        if (selectedCard.type == CardType.Number &&
            topCard.type == CardType.Number &&
            selectedCard.number == topCard.number)
        {
            return true;
        }

        // Cùng loại lá đặc biệt
        if (selectedCard.type != CardType.Number &&
            selectedCard.type == topCard.type)
        {
            return true;
        }

        return false;
    }
}