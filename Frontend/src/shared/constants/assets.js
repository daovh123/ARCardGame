export const avatarOptions = [
  { key: "char_1", name: "Nova", assetPath: "/assets/characters/char_1.png" },
  { key: "char_2", name: "Blaze", assetPath: "/assets/characters/char_2.png" },
  { key: "char_3", name: "Storm", assetPath: "/assets/characters/char_3.png" },
  { key: "char_4", name: "Iris", assetPath: "/assets/characters/char_4.png" },
  { key: "char_5", name: "Shadow", assetPath: "/assets/characters/char_5.png" },
  { key: "char_6", name: "Luna", assetPath: "/assets/characters/char_6.png" },
  { key: "char_7", name: "Rex", assetPath: "/assets/characters/char_7.png" },
  { key: "char_8", name: "Ember", assetPath: "/assets/characters/char_8.png" },
  { key: "char_9", name: "Frost", assetPath: "/assets/characters/char_9.png" },
  { key: "char_10", name: "Mika", assetPath: "/assets/characters/char_10.png" },
  { key: "char_11", name: "Ace", assetPath: "/assets/characters/char_11.png" },
  { key: "char_12", name: "Juno", assetPath: "/assets/characters/char_12.png" },
  { key: "char_13", name: "Kai", assetPath: "/assets/characters/char_13.png" },
];

export const cardBackAsset = "/assets/uno/individual/card back/card_back.png";
export const standardCardBackAsset = "/assets/standard/individual/card back/cardBackBlue.png";
export const tableTextureAsset = "/assets/tables/table_blue.png";
export const backgroundAsset = "/assets/backgrounds/background_1.png";
export const avatarFrameAsset = "/assets/characters/frame_circle.png";

export const UNO_COLORS = {
  red: { hex: "#e74c3c", label: "Đỏ" },
  blue: { hex: "#3498db", label: "Xanh dương" },
  green: { hex: "#2ecc71", label: "Xanh lá" },
  yellow: { hex: "#f1c40f", label: "Vàng" },
};

export const SEAT_POSITIONS = [
  { x: 0, z: -3.2, ry: 0 },
  { x: 3.6, z: -1.2, ry: -0.7 },
  { x: 3.6, z: 1.2, ry: -1.4 },
  { x: 0, z: 3.2, ry: Math.PI },
  { x: -3.6, z: 1.2, ry: 1.4 },
  { x: -3.6, z: -1.2, ry: 0.7 },
  { x: 1.8, z: -3.0, ry: -0.35 },
  { x: -1.8, z: -3.0, ry: 0.35 },
];

export const STANDARD_SUITS = ["spade", "club", "diamond", "heart"];
export const STANDARD_RANKS = ["3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2"];

export const SUIT_SYMBOLS = {
  spade: "♠",
  club: "♣",
  diamond: "♦",
  heart: "♥",
};

export const SUIT_COLORS = {
  spade: "#8899aa",
  club: "#8899aa",
  diamond: "#e74c3c",
  heart: "#e74c3c",
};

const SUIT_NAMES = {
  spade: "Spades",
  club: "Clubs",
  diamond: "Diamonds",
  heart: "Hearts",
};

export function getStandardCardPath(suit, rank) {
  return `/assets/standard/individual/${suit}/card${SUIT_NAMES[suit]}_${rank}.png`;
}
