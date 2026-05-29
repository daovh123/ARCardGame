const { GameError } = require("../../../domain/errors/GameError");
const { handlePass } = require("../../../domain/services/TienLenRules");

function createPassTienLenTurnUseCase({ roomRepository }) {
  return ({ roomCode, playerId }) => {
    const room = roomRepository.findByCode(roomCode);
    if (!room) throw new GameError("Room not found.", "ROOM_NOT_FOUND");
    if (room.gamePhase !== "playing") throw new GameError("Game not active.", "GAME_NOT_STARTED");

    handlePass(room, playerId);
    return roomRepository.save(room);
  };
}

module.exports = { createPassTienLenTurnUseCase };
