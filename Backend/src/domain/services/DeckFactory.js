const { randomUUID } = require("crypto");
const { createCard } = require("../entities/Card");

const UNO_COLOR_DEFINITIONS = [
  {
    color: "red",
    numeric: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
    specials: [
      ["block", "block_red.png", 20, "skip"],
      ["inverse", "inverse_red.png", 20, "reverse"],
      ["plus2", "2plus_red.png", 20, "draw-two"],
    ],
  },
  {
    color: "blue",
    numeric: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
    specials: [
      ["block", "block_blue.png", 20, "skip"],
      ["inverse", "inverse_blue.png", 20, "reverse"],
      ["plus2", "2plus_blue.png", 20, "draw-two"],
    ],
  },
  {
    color: "green",
    numeric: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
    specials: [
      ["block", "block_green.png", 20, "skip"],
      ["inverse", "inverse_green.png", 20, "reverse"],
      ["plus2", "2plus_green.png", 20, "draw-two"],
    ],
  },
  {
    color: "yellow",
    numeric: ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"],
    specials: [
      ["block", "block_yellow.png", 20, "skip"],
      ["inverse", "inverse_yellow.png", 20, "reverse"],
      ["plus2", "2plus_yellow.png", 20, "draw-two"],
    ],
  },
];

const STANDARD_SUITS = [
  { suit: "club", color: "black" },
  { suit: "diamond", color: "red" },
  { suit: "heart", color: "red" },
  { suit: "spade", color: "black" },
];

const STANDARD_VALUES = [
  { label: "2", points: 2 },
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
];

function shuffle(items) {
  const deck = [...items];

  for (let index = deck.length - 1; index > 0; index -= 1) {
    const swapIndex = Math.floor(Math.random() * (index + 1));
    [deck[index], deck[swapIndex]] = [deck[swapIndex], deck[index]];
  }

  return deck;
}

function createUnoDeck() {
  const cards = [];

  for (let i = 0; i < 2; i++) {
    for (const definition of UNO_COLOR_DEFINITIONS) {
      for (const value of definition.numeric) {
        if (i === 1 && value === "0") continue;
        cards.push(
          createCard({
            id: randomUUID(),
            family: "uno",
            color: definition.color,
            label: value,
            points: Number(value),
            assetPath: `/assets/uno/individual/${definition.color}/${value}_${definition.color}.png`,
            kind: "number",
          }),
        );
      }

      for (const [label, fileName, points, kind] of definition.specials) {
        cards.push(
          createCard({
            id: randomUUID(),
            family: "uno",
            color: definition.color,
            label,
            points,
            assetPath: `/assets/uno/individual/${definition.color}/${fileName}`,
            kind,
          }),
        );
      }
    }

    cards.push(
      createCard({
        id: randomUUID(),
        family: "uno",
        color: "wild",
        label: "wild",
        points: 50,
        assetPath: "/assets/uno/individual/wild/wild_card.png",
        kind: "wild",
      }),
    );

    cards.push(
      createCard({
        id: randomUUID(),
        family: "uno",
        color: "wild",
        label: "plus4",
        points: 50,
        assetPath: "/assets/uno/individual/wild/4_plus.png",
        kind: "wild-draw-four",
      }),
    );
  }

  return shuffle(cards);
}

function createStandardDeck() {
  const cards = [];

  for (const { suit, color } of STANDARD_SUITS) {
    for (const { label, points } of STANDARD_VALUES) {
      const fileName = `${label.toLowerCase()}_${suit}.png`;
      cards.push(
        createCard({
          id: randomUUID(),
          family: "standard",
          color,
          suit,
          label,
          points,
          assetPath: `/assets/standard/individual/${suit}/${fileName}`,
          kind: "number",
        }),
      );
    }
  }

  return shuffle(cards);
}

function reshuffleDiscardPile(room) {
  if (room.discardPile.length <= 1) return;

  const topCard = room.discardPile.pop();
  const reshuffled = shuffle(room.discardPile);
  room.deck.push(...reshuffled);
  room.discardPile = [topCard];
}

module.exports = { createUnoDeck, createStandardDeck, shuffle, reshuffleDiscardPile };
