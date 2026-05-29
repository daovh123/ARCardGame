const {
  ensurePlayerTurn,
  ensureRoomExists,
  findPlayer,
  drawFromDeck,
  rotateTurn,
  canPlayCard,
} = require("../../domain/services/RoomRules");
const { GameError } = require("../../domain/errors/GameError");

function createDrawCardUseCase({ roomRepository }) {
  return ({ roomCode, playerId }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);
    ensurePlayerTurn(room, playerId);

    const player = findPlayer(room, playerId);

    if (room.pendingChallenge) {
      throw new GameError("You must wait for the +4 challenge to resolve.", "PENDING_CHALLENGE");
    }

    if (room.pendingDraws > 0) {
      const drawAmount = room.pendingDraws;
      room.pendingDraws = 0;

      const drawnCards = drawFromDeck(room, drawAmount);
      player.hand.push(...drawnCards);

      room.lastAction = `${player.name} drew ${drawnCards.length} card(s) (forced)`;

      rotateTurn(room);
      const nextPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
      room.lastAction += ` -> ${nextPlayer?.name || "?"}'s turn`;

      room.unoCallerId = null;
      room.hasDrawnThisTurn = false;

      return roomRepository.save(room);
    }

    if (room.hasDrawnThisTurn) {
      throw new GameError("You already drew this turn. Play a card or end your turn.", "ALREADY_DREW");
    }

    if (player.hand.some((card) => canPlayCard(room, card, player))) {
      throw new GameError("You already have playable cards.", "PLAYABLE_CARDS_AVAILABLE");
    }

    const drawnCards = [];
    let canPlayAfterDraw = false;

    while (!canPlayAfterDraw) {
      const nextDraw = drawFromDeck(room, 1);
      const drawnCard = nextDraw[0];
      if (!drawnCard) break;

      drawnCards.push(drawnCard);
      player.hand.push(drawnCard);

      if (canPlayCard(room, drawnCard, player)) {
        canPlayAfterDraw = true;
      }
    }

    room.hasDrawnThisTurn = canPlayAfterDraw;

    if (drawnCards.length === 0) {
      room.lastAction = `${player.name} tried to draw, but the deck is empty`;
      rotateTurn(room);
      const nextPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
      room.lastAction += ` -> ${nextPlayer?.name || "?"}'s turn`;
      room.hasDrawnThisTurn = false;
    } else if (canPlayAfterDraw) {
      room.lastAction = `${player.name} drew ${drawnCards.length} card(s) and can now play or end turn`;
    } else {
      room.lastAction = `${player.name} drew ${drawnCards.length} card(s) but still cannot play`;
      rotateTurn(room);
      const nextPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
      room.lastAction += ` -> ${nextPlayer?.name || "?"}'s turn`;
      room.hasDrawnThisTurn = false;
    }

    room.unoCallerId = null;

    return roomRepository.save(room);
  };
}

module.exports = { createDrawCardUseCase };
