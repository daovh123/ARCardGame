const { ensurePlayerTurn, ensureRoomExists, rotateTurn } = require("../../domain/services/RoomRules");
const { GameError } = require("../../domain/errors/GameError");

function createEndTurnUseCase({ roomRepository }) {
  return ({ roomCode, playerId }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);
    ensurePlayerTurn(room, playerId);

    if (room.pendingChallenge) {
      throw new GameError("You must wait for the +4 challenge to resolve.", "PENDING_CHALLENGE");
    }

    const currentPlayer = room.players.find((player) => player.id === playerId);
    rotateTurn(room);
    const nextPlayer = room.players.find((player) => player.id === room.currentTurnPlayerId);
    room.lastAction = `${currentPlayer?.name || "Player"} ended turn. Next: ${nextPlayer?.name || "n/a"}`;

    return roomRepository.save(room);
  };
}

module.exports = { createEndTurnUseCase };
