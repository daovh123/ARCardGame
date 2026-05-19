using UnityEngine;
using System;

public enum CardColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Wild
}

public enum CardType
{
    Number,
    Skip,
    Reverse,
    DrawTwo,
    Block,
    DrawFour,
    ChangeColor
}

[Serializable]
public class CardData
{
    public int cardId;
    public CardColor color;
    public CardType type;
    public int number;

    public CardData()
    {
    }

    public CardData(int cardId, CardColor color, CardType type, int number = -1)
    {
        this.cardId = cardId;
        this.color = color;
        this.type = type;
        this.number = number;
    }

    public string GetDisplayName()
    {
        if (type == CardType.Number)
        {
            return $"{color} {number}";
        }

        if (type == CardType.DrawFour)
        {
            return "+4";
        }

        if (type == CardType.ChangeColor)
        {
            return "Change Color";
        }

        if (type == CardType.Block)
        {
            return $"{color} Block";
        }

        return $"{color} {type}";
    }
}