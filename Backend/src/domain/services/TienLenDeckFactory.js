const { randomUUID } = require("crypto");
const { createCard } = require("../entities/Card");

const SUITS = [
  { suit: "spade", name: "Spades" },
  { suit: "club", name: "Clubs" },
  { suit: "diamond", name: "Diamonds" },
  { suit: "heart", name: "Hearts" },
];

const RANKS = [
  { label: "3", points: 3 },
  { label: "4", points: 4 },
  { label: "5", points: 5 },
  { label: "6", points: 6 },
  { label: "7", points: 7 },
  { label: "8", points: 8 },
  { label: "9", points: 9 },
  { label: "10", points: 10 },
  { label: "J", points: 11 },
  { label: "Q", points: 12 },
  { label: "K", points: 13 },
  { label: "A", points: 14 },
  { label: "2", points: 15 },
];

function shuffle(items) {
  const deck = [...items];
  for (let i = deck.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [deck[i], deck[j]] = [deck[j], deck[i]];
  }
  return deck;
}

function createTienLenDeck() {
  const cards = [];

  for (const { suit, name } of SUITS) {
    for (const { label, points } of RANKS) {
      const fileName = `card${name}_${label}.png`;
      cards.push(
        createCard({
          id: randomUUID(),
          family: "standard",
          color: suit === "heart" || suit === "diamond" ? "red" : "black",
          label,
          points,
          assetPath: `/assets/standard/individual/${suit}/${fileName}`,
          kind: "number",
          suit,
        }),
      );
    }
  }

  return shuffle(cards);
}

function dealTienLenCards(room) {
  const deck = createTienLenDeck();
  const playerCount = room.players.length;
  const cardsPerPlayer = Math.floor(52 / playerCount);

  for (let i = 0; i < room.players.length; i++) {
    const start = i * cardsPerPlayer;
    room.players[i].hand = deck.slice(start, start + cardsPerPlayer);
    room.players[i].hand.sort((a, b) => {
      const { getAbsoluteValue } = require("./TienLenRules");
      return getAbsoluteValue(a) - getAbsoluteValue(b);
    });
  }

  return room;
}

module.exports = { createTienLenDeck, dealTienLenCards, shuffle };
