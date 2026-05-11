using UnityEngine;
using System;

public enum CardColor
{
    Red,
    Blue,
    Green,
    Yellow
}

public enum CardType
{
    Number,
    Skip,
    Reverse,
    DrawTwo
}

[Serializable]
public class CardData
{
    public int cardId;
    public CardColor color;
    public CardType type;
    public int number;

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

        return $"{color} {type}";
    }
}