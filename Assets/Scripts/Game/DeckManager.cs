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
            deck.Add(new CardData(nextCardId++, color, CardType.Number, 0));

            for (int copy = 0; copy < 2; copy++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    deck.Add(new CardData(nextCardId++, color, CardType.Number, number));
                }

                deck.Add(new CardData(nextCardId++, color, CardType.Block));
                deck.Add(new CardData(nextCardId++, color, CardType.Reverse));
                deck.Add(new CardData(nextCardId++, color, CardType.DrawTwo));
            }
        }

        for (int i = 0; i < 4; i++)
        {
            deck.Add(new CardData(nextCardId++, CardColor.Wild, CardType.ChangeColor));
            deck.Add(new CardData(nextCardId++, CardColor.Wild, CardType.DrawFour));
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

    public void AddCards(List<CardData> cards)
    {
        if (cards == null || cards.Count == 0)
        {
            return;
        }

        deck.AddRange(cards);
    }

    public List<CardData> GetDeckCards()
    {
        return deck;
    }

    public void SetDeckCards(List<CardData> newDeck)
    {
        deck = newDeck ?? new List<CardData>();
    }
}
