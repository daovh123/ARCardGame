const { GameError } = require("../errors/GameError");

const RANK_ORDER = ["3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2"];
const SUIT_ORDER = ["spade", "club", "diamond", "heart"];

function getCardValue(card) {
  return RANK_ORDER.indexOf(card.label);
}

function getSuitValue(card) {
  return SUIT_ORDER.indexOf(card.suit);
}

function getAbsoluteValue(card) {
  return getCardValue(card) * 4 + getSuitValue(card);
}

function sortCards(cards) {
  return [...cards].sort((a, b) => getAbsoluteValue(a) - getAbsoluteValue(b));
}

function classifyCombo(cards) {
  if (!cards || cards.length === 0) return null;

  const sorted = sortCards(cards);
  const count = sorted.length;

  if (count === 1) {
    return { type: "single", cards: sorted, highCard: sorted[0] };
  }

  if (count === 2) {
    if (sorted[0].label === sorted[1].label) {
      return { type: "pair", cards: sorted, highCard: sorted[1] };
    }
    return null;
  }

  if (count === 3) {
    if (sorted[0].label === sorted[1].label && sorted[1].label === sorted[2].label) {
      return { type: "triple", cards: sorted, highCard: sorted[2] };
    }
    if (isValidStraight(sorted)) {
      return { type: "straight", cards: sorted, highCard: sorted[count - 1] };
    }
    return null;
  }

  if (count === 4) {
    if (sorted.every((c) => c.label === sorted[0].label)) {
      return { type: "quad", cards: sorted, highCard: sorted[3] };
    }
    if (isValidStraight(sorted)) {
      return { type: "straight", cards: sorted, highCard: sorted[count - 1] };
    }
    return null;
  }

  if (count >= 6 && count % 2 === 0 && isValidDoubleStraight(sorted)) {
    return { type: "doubleStraight", cards: sorted, highCard: sorted[count - 1] };
  }

  if (count >= 3 && isValidStraight(sorted)) {
    return { type: "straight", cards: sorted, highCard: sorted[count - 1] };
  }

  return null;
}

function isValidStraight(sorted) {
  if (sorted.some((c) => c.label === "2")) return false;
  for (let i = 1; i < sorted.length; i++) {
    if (getCardValue(sorted[i]) !== getCardValue(sorted[i - 1]) + 1) return false;
  }
  return true;
}

function isValidDoubleStraight(sorted) {
  if (sorted.length < 6 || sorted.length % 2 !== 0) return false;
  if (sorted.some((c) => c.label === "2")) return false;

  const pairs = [];
  for (let i = 0; i < sorted.length; i += 2) {
    if (sorted[i].label !== sorted[i + 1].label) return false;
    pairs.push(sorted[i]);
  }

  for (let i = 1; i < pairs.length; i++) {
    if (getCardValue(pairs[i]) !== getCardValue(pairs[i - 1]) + 1) return false;
  }
  return true;
}

function canBeat(lastCombo, newCombo) {
  if (!lastCombo) return true;
  if (!newCombo) return false;

  if (lastCombo.type === "single" && lastCombo.highCard.label === "2") {
    if (newCombo.type === "quad") return true;
    if (newCombo.type === "doubleStraight" && newCombo.cards.length >= 6) return true;
  }

  if (lastCombo.type === "pair" && lastCombo.highCard.label === "2") {
    if (newCombo.type === "doubleStraight" && newCombo.cards.length >= 8) return true;
  }

  if (newCombo.type !== lastCombo.type) return false;
  if (newCombo.cards.length !== lastCombo.cards.length) return false;

  return getAbsoluteValue(newCombo.highCard) > getAbsoluteValue(lastCombo.highCard);
}

function findStartingPlayer(players) {
  for (const player of players) {
    const has3Spade = player.hand.some((c) => c.label === "3" && c.suit === "spade");
    if (has3Spade) return player;
  }
  return players[0];
}

function mustIncludeThreeOfSpades(cards, isFirstMove) {
  if (!isFirstMove) return true;
  return cards.some((c) => c.label === "3" && c.suit === "spade");
}

function rotateTurn(room) {
  if (room.players.length === 0) {
    room.currentTurnPlayerId = null;
    return;
  }

  const activePlayers = room.players.filter((p) => p.hand.length > 0);
  if (activePlayers.length === 0) return;

  const currentIndex = activePlayers.findIndex((p) => p.id === room.currentTurnPlayerId);
  const nextIndex = (currentIndex + 1) % activePlayers.length;
  room.currentTurnPlayerId = activePlayers[nextIndex].id;
}

function handlePass(room, playerId) {
  const player = room.players.find((p) => p.id === playerId);
  if (!player) throw new GameError("Player not found.", "PLAYER_NOT_FOUND");

  if (room.currentTurnPlayerId !== playerId) {
    throw new GameError("It is not your turn.", "NOT_YOUR_TURN");
  }

  if (!room.lastPlayedCombo) {
    throw new GameError("Cannot pass when starting a new round.", "CANNOT_PASS_NEW_ROUND");
  }

  room.passCount += 1;
  room.lastAction = `${player.name} bỏ lượt`;

  const activePlayers = room.players.filter((p) => p.hand.length > 0);

  if (room.passCount >= activePlayers.length - 1) {
    room.lastPlayedCombo = null;
    room.passCount = 0;

    const lastPlayerId = room.tableCards.length > 0
      ? room.tableCards[room.tableCards.length - 1].playedBy
      : room.currentTurnPlayerId;

    const lastPlayer = room.players.find((p) => p.id === lastPlayerId);
    if (lastPlayer && lastPlayer.hand.length > 0) {
      room.currentTurnPlayerId = lastPlayerId;
    } else {
      rotateTurn(room);
    }

    room.lastAction += " — Vòng mới!";
  } else {
    rotateTurn(room);
  }
}

function checkWinCondition(room, player) {
  if (player.hand.length === 0) {
    room.rankings.push(player.id);

    const activePlayers = room.players.filter((p) => p.hand.length > 0);
    if (activePlayers.length <= 1) {
      if (activePlayers.length === 1) {
        room.rankings.push(activePlayers[0].id);
      }
      room.winnerId = room.rankings[0];
      room.gamePhase = "finished";
      return true;
    }

    if (room.currentTurnPlayerId === player.id) {
      rotateTurn(room);
    }
    room.lastPlayedCombo = null;
    room.passCount = 0;
  }
  return room.gamePhase === "finished";
}

function getPlayableCombos(hand, lastCombo, isNewRound) {
  if (isNewRound) return hand.length > 0;

  if (!lastCombo) return hand.length > 0;

  for (let i = 0; i < hand.length; i++) {
    const singleCombo = classifyCombo([hand[i]]);
    if (singleCombo && canBeat(lastCombo, singleCombo)) return true;
  }

  for (let i = 0; i < hand.length; i++) {
    for (let j = i + 1; j < hand.length; j++) {
      const pairCombo = classifyCombo([hand[i], hand[j]]);
      if (pairCombo && canBeat(lastCombo, pairCombo)) return true;
    }
  }

  return true;
}

function createTablePlacement(existingCount) {
  const angle = existingCount * 0.4;
  return {
    x: Number((Math.cos(angle) * (0.3 + (existingCount % 4) * 0.12)).toFixed(2)),
    z: Number((Math.sin(angle) * 0.4).toFixed(2)),
    rotation: Number((((existingCount % 5) - 2) * 0.1).toFixed(2)),
    y: Number((0.02 + existingCount * 0.002).toFixed(3)),
  };
}

module.exports = {
  RANK_ORDER,
  SUIT_ORDER,
  getCardValue,
  getSuitValue,
  getAbsoluteValue,
  sortCards,
  classifyCombo,
  isValidStraight,
  isValidDoubleStraight,
  canBeat,
  findStartingPlayer,
  mustIncludeThreeOfSpades,
  rotateTurn,
  handlePass,
  checkWinCondition,
  getPlayableCombos,
  createTablePlacement,
};
