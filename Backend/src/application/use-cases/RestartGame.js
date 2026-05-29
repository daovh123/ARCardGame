const { ensureRoomExists, resetGameForNewRound } = require("../../domain/services/RoomRules");
const { GameError } = require("../../domain/errors/GameError");

function createRestartGameUseCase({ roomRepository }) {
  return ({ roomCode }) => {
    const room = roomRepository.findByCode(roomCode);
    ensureRoomExists(room);

    if (room.players.length < 2) {
      throw new GameError("Need at least 2 players to restart.", "NOT_ENOUGH_PLAYERS");
    }

    resetGameForNewRound(room);
    room.lastAction = "New round started!";

    return roomRepository.save(room);
  };
}

module.exports = { createRestartGameUseCase };
