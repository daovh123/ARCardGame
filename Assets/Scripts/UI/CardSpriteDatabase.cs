using UnityEngine;

[CreateAssetMenu(fileName = "CardSpriteDatabase", menuName = "Card Game/Card Sprite Database")]
public class CardSpriteDatabase : ScriptableObject
{
    [Header("Yellow")]
    public Sprite yellow_0;
    public Sprite yellow_1;
    public Sprite yellow_2;
    public Sprite yellow_3;
    public Sprite yellow_4;
    public Sprite yellow_5;
    public Sprite yellow_6;
    public Sprite yellow_7;
    public Sprite yellow_8;
    public Sprite yellow_9;
    public Sprite yellow_2plus;
    public Sprite yellow_block;
    public Sprite yellow_inverse;

    [Header("Blue")]
    public Sprite blue_0;
    public Sprite blue_1;
    public Sprite blue_2;
    public Sprite blue_3;
    public Sprite blue_4;
    public Sprite blue_5;
    public Sprite blue_6;
    public Sprite blue_7;
    public Sprite blue_8;
    public Sprite blue_9;
    public Sprite blue_2plus;
    public Sprite blue_block;
    public Sprite blue_inverse;

    [Header("Red")]
    public Sprite red_0;
    public Sprite red_1;
    public Sprite red_2;
    public Sprite red_3;
    public Sprite red_4;
    public Sprite red_5;
    public Sprite red_6;
    public Sprite red_7;
    public Sprite red_8;
    public Sprite red_9;
    public Sprite red_2plus;
    public Sprite red_block;
    public Sprite red_inverse;

    [Header("Green")]
    public Sprite green_0;
    public Sprite green_1;
    public Sprite green_2;
    public Sprite green_3;
    public Sprite green_4;
    public Sprite green_5;
    public Sprite green_6;
    public Sprite green_7;
    public Sprite green_8;
    public Sprite green_9;
    public Sprite green_2plus;
    public Sprite green_block;
    public Sprite green_inverse;

    [Header("Wild / Special")]
    public Sprite wild_plus4;
    public Sprite wild_change_color;
    public Sprite card_back;

    public Sprite GetSprite(CardData card)
    {
        if (card == null)
        {
            return card_back;
        }

        if (card.type == CardType.DrawFour)
        {
            return wild_plus4;
        }

        if (card.type == CardType.ChangeColor)
        {
            return wild_change_color;
        }

        switch (card.color)
        {
            case CardColor.Yellow:
                return GetYellowSprite(card);

            case CardColor.Blue:
                return GetBlueSprite(card);

            case CardColor.Red:
                return GetRedSprite(card);

            case CardColor.Green:
                return GetGreenSprite(card);

            default:
                return card_back;
        }
    }

    private Sprite GetYellowSprite(CardData card)
    {
        if (card.type == CardType.DrawTwo) return yellow_2plus;
        if (card.type == CardType.Block) return yellow_block;
        if (card.type == CardType.Reverse) return yellow_inverse;

        switch (card.number)
        {
            case 0: return yellow_0;
            case 1: return yellow_1;
            case 2: return yellow_2;
            case 3: return yellow_3;
            case 4: return yellow_4;
            case 5: return yellow_5;
            case 6: return yellow_6;
            case 7: return yellow_7;
            case 8: return yellow_8;
            case 9: return yellow_9;
            default: return yellow_0;
        }
    }

    private Sprite GetBlueSprite(CardData card)
    {
        if (card.type == CardType.DrawTwo) return blue_2plus;
        if (card.type == CardType.Block) return blue_block;
        if (card.type == CardType.Reverse) return blue_inverse;

        switch (card.number)
        {
            case 0: return blue_0;
            case 1: return blue_1;
            case 2: return blue_2;
            case 3: return blue_3;
            case 4: return blue_4;
            case 5: return blue_5;
            case 6: return blue_6;
            case 7: return blue_7;
            case 8: return blue_8;
            case 9: return blue_9;
            default: return blue_0;
        }
    }

    private Sprite GetRedSprite(CardData card)
    {
        if (card.type == CardType.DrawTwo) return red_2plus;
        if (card.type == CardType.Block) return red_block;
        if (card.type == CardType.Reverse) return red_inverse;

        switch (card.number)
        {
            case 0: return red_0;
            case 1: return red_1;
            case 2: return red_2;
            case 3: return red_3;
            case 4: return red_4;
            case 5: return red_5;
            case 6: return red_6;
            case 7: return red_7;
            case 8: return red_8;
            case 9: return red_9;
            default: return red_0;
        }
    }

    private Sprite GetGreenSprite(CardData card)
    {
        if (card.type == CardType.DrawTwo) return green_2plus;
        if (card.type == CardType.Block) return green_block;
        if (card.type == CardType.Reverse) return green_inverse;

        switch (card.number)
        {
            case 0: return green_0;
            case 1: return green_1;
            case 2: return green_2;
            case 3: return green_3;
            case 4: return green_4;
            case 5: return green_5;
            case 6: return green_6;
            case 7: return green_7;
            case 8: return green_8;
            case 9: return green_9;
            default: return green_0;
        }
    }
}