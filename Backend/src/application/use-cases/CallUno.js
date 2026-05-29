const { ensureRoomExists, findPlayer, drawFromDeck, hasColorInHand, rotateTurn } = require("../../domain/services/RoomRules");
const { GameError } = require("../../domain/errors/GameError");

function createCallUnoUseCase({ roomRepository }) {
  return ({ roomCode, playerId }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);

    const player = findPlayer(room, playerId);

    if (player.hand.length > 2) {
      throw new GameError("You can only call UNO when you have 1-2 cards.", "INVALID_UNO_CALL");
    }

    room.unoCallerId = playerId;
    room.lastAction = `${player.name} called UNO!`;

    return roomRepository.save(room);
  };
}

function createCatchUnoUseCase({ roomRepository }) {
  return ({ roomCode, catcherId, targetId }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);

    const target = room.players.find((p) => p.id === targetId);
    if (!target) throw new GameError("Target player not found.", "PLAYER_NOT_FOUND");

    if (target.hand.length > 2) {
      throw new GameError("Target doesn't have UNO risk.", "INVALID_CATCH");
    }

    if (room.unoCallerId === targetId) {
      throw new GameError("That player already called UNO.", "ALREADY_CALLED");
    }

    const penaltyCards = drawFromDeck(room, 2);
    target.hand.push(...penaltyCards);

    const catcher = findPlayer(room, catcherId);
    room.lastAction = `${catcher.name} caught ${target.name} not calling UNO! +2 penalty cards.`;

    return roomRepository.save(room);
  };
}

function createChallengeWildFourUseCase({ roomRepository }) {
  return ({ roomCode, challengerId }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);

    if (!room.pendingChallenge) {
      throw new GameError("No pending +4 challenge.", "NO_PENDING_CHALLENGE");
    }

    if (room.pendingChallenge.challengerId !== challengerId) {
      throw new GameError("Only the affected player can challenge.", "NOT_CHALLENGER");
    }

    const wildFourPlayer = room.players.find((p) => p.id === room.pendingChallenge.wildFourPlayerId);
    if (!wildFourPlayer) {
      throw new GameError("Player not found.", "PLAYER_NOT_FOUND");
    }

    const challengeColor = room.pendingChallenge.chosenColor;
    const hadMatchingColor = hasColorInHand(wildFourPlayer, challengeColor);

    const challenger = findPlayer(room, challengerId);
    room.pendingChallenge = null;

    if (hadMatchingColor) {
      const penaltyCards = drawFromDeck(room, 4);
      wildFourPlayer.hand.push(...penaltyCards);

      room.lastAction = `Challenge SUCCESS! ${wildFourPlayer.name} had ${challengeColor} cards. ${wildFourPlayer.name} draws 4 penalty cards.`;

      rotateTurn(room);
      const nextPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
      room.lastAction += ` → ${nextPlayer?.name || "?"}'s turn`;

      return roomRepository.save(room);
    } else {
      const penaltyCards = drawFromDeck(room, 6);
      challenger.hand.push(...penaltyCards);

      room.lastAction = `Challenge FAILED! ${wildFourPlayer.name} had no ${challengeColor} cards. ${challenger.name} draws 6 penalty cards (4 + 2 penalty).`;

      room.pendingDraws = 0;

      rotateTurn(room);
      const nextPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
      room.lastAction += ` → ${nextPlayer?.name || "?"}'s turn`;

      return roomRepository.save(room);
    }
  };
}

module.exports = { createCallUnoUseCase, createCatchUnoUseCase, createChallengeWildFourUseCase };
