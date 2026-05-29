const { GameError } = require("../../../domain/errors/GameError");
const { startTienLenGame } = require("./JoinTienLenRoom");

function createRestartTienLenGameUseCase({ roomRepository }) {
  return ({ roomCode }) => {
    const room = roomRepository.findByCode(roomCode);
    if (!room) throw new GameError("Room not found.", "ROOM_NOT_FOUND");
    if (room.players.length < 2) throw new GameError("Not enough players.", "NOT_ENOUGH_PLAYERS");

    room.isFirstGame = false;
    startTienLenGame(room);
    return roomRepository.save(room);
  };
}

module.exports = { createRestartTienLenGameUseCase };
