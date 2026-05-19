using UnityEngine;
using System.Collections.Generic;

public class DeckManager
{
    private List<CardData> deck = new List<CardData>();
    private int nextCardId = 0;

    public int DeckCount
    {
        get { return deck.Count; }
    }

    public void CreateDeck()
    {
        deck.Clear();
        nextCardId = 0;

        CardColor[] colors =
        {
            CardColor.Red,
            CardColor.Blue,
            CardColor.Green,
            CardColor.Yellow
        };

        foreach (CardColor color in colors)
        {
            for (int number = 1; number <= 5; number++)
            {
                deck.Add(new CardData(nextCardId++, color, CardType.Number, number));
            }

            deck.Add(new CardData(nextCardId++, color, CardType.Skip));
            deck.Add(new CardData(nextCardId++, color, CardType.Reverse));
            deck.Add(new CardData(nextCardId++, color, CardType.DrawTwo));
        }
    }

    public void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public CardData DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            return null;
        }

        CardData card = deck[0];
        deck.RemoveAt(0);
        return card;
    }
        public List<CardData> GetDeckCards()
    {
        return deck;
    }

    public void SetDeckCards(List<CardData> newDeck)
    {
        if (newDeck == null)
        {
            deck = new List<CardData>();
        }
        else
        {
            deck = newDeck;
        }
    }
}