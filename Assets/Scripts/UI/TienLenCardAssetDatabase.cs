using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TienLenCardAssetDatabase", menuName = "Card Game/Tien Len Card Asset Database")]
public class TienLenCardAssetDatabase : ScriptableObject
{
    public Texture2D cardBack;
    public List<TienLenCardTextureEntry> cards = new List<TienLenCardTextureEntry>();

    public Texture2D GetTexture(PlayingCardData card)
    {
        if (card == null)
        {
            return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            TienLenCardTextureEntry entry = cards[i];
            if (entry != null && entry.suit == card.suit && entry.rank == card.rank)
            {
                return entry.texture;
            }
        }

        return null;
    }
}

[Serializable]
public class TienLenCardTextureEntry
{
    public PlayingCardSuit suit;
    public int rank;
    public Texture2D texture;
}
