const {
  ensurePlayerTurn,
  ensureRoomExists,
  findPlayer,
  canPlayCard,
  applyCardEffect,
  applyPendingDrawsToNextPlayer,
  checkWinCondition,
  rotateTurn,
  createTablePlacement,
  hasColorInHand,
} = require("../../domain/services/RoomRules");
const { GameError } = require("../../domain/errors/GameError");

function createPlayCardUseCase({ roomRepository }) {
  return ({ roomCode, playerId, cardId, chosenColor }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);
    ensurePlayerTurn(room, playerId);

    const player = findPlayer(room, playerId);
    const handIndex = player.hand.findIndex((card) => card.id === cardId);

    if (handIndex < 0) {
      throw new GameError("Card not found in your hand.", "CARD_NOT_IN_HAND");
    }

    const card = player.hand[handIndex];

    if (!canPlayCard(room, card, player)) {
      if (room.pendingDraws > 0) {
        throw new GameError(
          `You must draw ${room.pendingDraws} cards first. You cannot play right now.`,
          "MUST_DRAW_FIRST",
        );
      }
      throw new GameError(
        `Cannot play ${card.color} ${card.label}. Must match color (${room.currentColor}) or label.`,
        "INVALID_PLAY",
      );
    }

    if ((card.kind === "wild" || card.kind === "wild-draw-four") && !chosenColor) {
      throw new GameError("You must choose a color for wild cards.", "COLOR_REQUIRED");
    }

    if (chosenColor && !["red", "blue", "green", "yellow"].includes(chosenColor)) {
      throw new GameError("Invalid color choice.", "INVALID_COLOR");
    }

    player.hand.splice(handIndex, 1);

    const placement = createTablePlacement(room.tableCards.length);
    room.tableCards.push({
      ...card,
      placement,
      playedBy: player.id,
    });

    room.discardPile.push(card);

    if (card.kind === "wild" || card.kind === "wild-draw-four") {
      room.currentColor = chosenColor;
    } else {
      room.currentColor = card.color;
    }

    room.unoCallerId = null;
    room.hasDrawnThisTurn = false;

    const effects = applyCardEffect(room, card);
    const won = checkWinCondition(room, player);

    if (card.kind === "wild-draw-four") {
      const nextIndex = ((room.players.findIndex((p) => p.id === playerId) + room.direction) % room.players.length + room.players.length) % room.players.length;
      const nextPlayerId = room.players[nextIndex].id;

      room.pendingChallenge = {
        challengerId: nextPlayerId,
        wildFourPlayerId: playerId,
        cardId: card.id,
        chosenColor: chosenColor,
      };
    }

    let actionMsg = `${player.name} played ${card.color} ${card.label}`;
    if (effects.length > 0) actionMsg += ` [${effects.join(", ")}]`;

    if (!won) {
      const isSkip = card.kind === "skip" || card.label === "block";
      const isReverseAsSkip = card.kind === "reverse" && room.players.length === 2;

      if (isSkip || isReverseAsSkip) {
        rotateTurn(room, 1);
      } else if (card.kind !== "reverse") {
        rotateTurn(room);
      } else {
        const currentIndex = room.players.findIndex((p) => p.id === playerId);
        const nextIndex = ((currentIndex + room.direction) % room.players.length + room.players.length) % room.players.length;
        room.currentTurnPlayerId = room.players[nextIndex].id;
        room.hasDrawnThisTurn = false;
      }

      const nextPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
      actionMsg += ` → ${nextPlayer?.name || "?"}'s turn`;
    } else {
      actionMsg += ` 🏆 ${player.name} WINS!`;
    }

    room.lastAction = actionMsg;

    return roomRepository.save(room);
  };
}

module.exports = { createPlayCardUseCase };
