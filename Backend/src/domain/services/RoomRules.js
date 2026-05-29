const { GameError } = require("../errors/GameError");
const { reshuffleDiscardPile } = require("./DeckFactory");

function ensureRoomExists(room) {
  if (!room) {
    throw new GameError("Room not found.", "ROOM_NOT_FOUND");
  }
}

function ensureGameActive(room) {
  ensureRoomExists(room);
  if (room.winnerId) {
    throw new GameError("Game is already finished.", "GAME_FINISHED");
  }
  if (room.gamePhase !== "playing") {
    throw new GameError("Game has not started yet.", "GAME_NOT_STARTED");
  }
}

function ensurePlayerTurn(room, playerId) {
  ensureGameActive(room);

  if (room.currentTurnPlayerId !== playerId) {
    throw new GameError("It is not your turn.", "NOT_YOUR_TURN");
  }
}

function findPlayer(room, playerId) {
  ensureRoomExists(room);
  const player = room.players.find((entry) => entry.id === playerId);

  if (!player) {
    throw new GameError("Player not found in room.", "PLAYER_NOT_FOUND");
  }

  return player;
}

function drawFromDeck(room, amount = 1) {
  ensureRoomExists(room);
  const drawn = [];

  for (let index = 0; index < amount; index += 1) {
    if (room.deck.length === 0) {
      reshuffleDiscardPile(room);
    }
    const nextCard = room.deck.shift();

    if (!nextCard) {
      break;
    }

    drawn.push(nextCard);
  }

  return drawn;
}

function getTopDiscard(room) {
  if (room.discardPile.length === 0) return null;
  return room.discardPile[room.discardPile.length - 1];
}

function hasColorInHand(player, color) {
  return player.hand.some((card) => card.color === color);
}

function canPlayCard(room, card, player) {
  if (room.gamePhase !== "playing") return false;

  if (room.pendingChallenge) return false;

  if (room.pendingDraws > 0) {
    return canStackDrawTwo(room, card);
  }

  const topCard = getTopDiscard(room);

  if (!topCard) return true;

  if (card.kind === "wild") return true;

  if (card.kind === "wild-draw-four") {
    return !hasColorInHand(player, room.currentColor);
  }

  if (card.color === room.currentColor) return true;

  if (card.label === topCard.label) return true;

  if (card.color === topCard.color) return true;

  return false;
}

function rotateTurn(room, skipCount = 0) {
  ensureRoomExists(room);

  if (room.players.length === 0) {
    room.currentTurnPlayerId = null;
    return room;
  }

  const currentIndex = room.players.findIndex((entry) => entry.id === room.currentTurnPlayerId);
  let steps = 1 + skipCount;

  const nextIndex = ((currentIndex + room.direction * steps) % room.players.length + room.players.length) % room.players.length;
  room.currentTurnPlayerId = room.players[nextIndex].id;
  room.hasDrawnThisTurn = false;
  return room;
}

function createTablePlacement(existingCardsCount) {
  const spread = 1.8;
  const angle = existingCardsCount * 0.55;

  return {
    x: Number((Math.cos(angle) * (0.35 + (existingCardsCount % 4) * 0.16)).toFixed(2)),
    z: Number((Math.sin(angle) * 0.5).toFixed(2)),
    rotation: Number((((existingCardsCount % 5) - 2) * 0.12).toFixed(2)),
    y: Number((0.02 + existingCardsCount * 0.002).toFixed(3)),
    spread,
  };
}

function applyCardEffect(room, card) {
  const effectLog = [];

  if (card.kind === "skip" || card.label === "block") {
    effectLog.push(`Skip next player`);
  } else if (card.kind === "reverse" || card.label === "inverse") {
    if (room.players.length === 2) {
      effectLog.push(`Skip (reverse with 2 players)`);
    } else {
      room.direction *= -1;
      effectLog.push(`Direction reversed`);
    }
  } else if (card.kind === "draw-two") {
    room.pendingDraws += 2;
    effectLog.push(`+2 draws pending`);
  } else if (card.kind === "wild-draw-four") {
    room.pendingDraws += 4;
    effectLog.push(`+4 draws pending`);
  }

  return effectLog;
}

function applyPendingDrawsToNextPlayer(room) {
  if (room.pendingDraws > 0) {
    const nextPlayerId = room.currentTurnPlayerId;
    const nextPlayer = room.players.find((p) => p.id === nextPlayerId);
    if (nextPlayer) {
      const penaltyCards = drawFromDeck(room, room.pendingDraws);
      nextPlayer.hand.push(...penaltyCards);
    }
    room.pendingDraws = 0;
  }
}

function calculateHandPoints(player) {
  let total = 0;
  for (const card of player.hand) {
    total += card.points;
  }
  return total;
}

function checkWinCondition(room, player) {
  if (player.hand.length === 0) {
    room.winnerId = player.id;
    room.gamePhase = "finished";

    if (room.pendingDraws > 0) {
      applyPendingDrawsToNextPlayer(room);
    }

    let totalScore = 0;
    for (const p of room.players) {
      if (p.id !== player.id) {
        totalScore += calculateHandPoints(p);
      }
    }
    player.score += totalScore;
    return true;
  }
  return false;
}

function canStackDrawTwo(room, card) {
  if (room.pendingDraws <= 0) return false;
  if (room.pendingDraws % 2 !== 0) return false;
  return card.kind === "draw-two";
}

function forcePlayAfterDraw(room, player, drawnCard) {
  if (!drawnCard) return false;
  if (room.gamePhase !== "playing") return false;

  const topCard = getTopDiscard(room);
  if (!topCard) return true;

  if (drawnCard.kind === "wild") return true;
  if (drawnCard.kind === "wild-draw-four") return !hasColorInHand(player, room.currentColor);
  if (drawnCard.color === room.currentColor) return true;
  if (drawnCard.label === topCard.label) return true;
  if (drawnCard.color === topCard.color) return true;

  return false;
}

function getPlayableCards(room, player) {
  return player.hand.filter((card) => canPlayCard(room, card, player));
}

function resetGameForNewRound(room) {
  const { createUnoDeck } = require("./DeckFactory");
  room.deck = createUnoDeck();
  room.discardPile = [];
  room.tableCards = [];
  room.direction = 1;
  room.currentColor = null;
  room.pendingDraws = 0;
  room.winnerId = null;
  room.gamePhase = "playing";
  room.unoCallerId = null;
  room.pendingChallenge = null;
  room.lastPlayedCard = null;
  room.hasDrawnThisTurn = false;

  for (const player of room.players) {
    const drawn = drawFromDeck(room, 7);
    player.hand = [...drawn];
  }

  let firstCard = drawFromDeck(room, 1)[0];
  while (firstCard && (firstCard.kind === "wild" || firstCard.kind === "wild-draw-four")) {
    room.deck.push(firstCard);
    firstCard = drawFromDeck(room, 1)[0];
  }
  if (firstCard) {
    room.discardPile.push(firstCard);
    room.currentColor = firstCard.color;
    room.tableCards.push({
      ...firstCard,
      placement: createTablePlacement(0),
      playedBy: null,
    });

    if (firstCard.kind === "skip" || firstCard.label === "block") {
      room.currentTurnPlayerId = room.players.length > 1 ? room.players[1].id : room.players[0].id;
    } else if (firstCard.kind === "reverse" || firstCard.label === "inverse") {
      room.direction = -1;
      room.currentTurnPlayerId = room.players[room.players.length - 1].id;
    } else if (firstCard.kind === "draw-two") {
      room.pendingDraws = 2;
      room.currentTurnPlayerId = room.players[0].id;
    }
  }

  if (room.players.length > 0 && !room.currentTurnPlayerId) {
    room.currentTurnPlayerId = room.players[0].id;
  }

  return room;
}

module.exports = {
  ensurePlayerTurn,
  ensureRoomExists,
  ensureGameActive,
  findPlayer,
  drawFromDeck,
  rotateTurn,
  createTablePlacement,
  canPlayCard,
  canStackDrawTwo,
  forcePlayAfterDraw,
  getTopDiscard,
  hasColorInHand,
  applyCardEffect,
  applyPendingDrawsToNextPlayer,
  checkWinCondition,
  getPlayableCards,
  resetGameForNewRound,
};
