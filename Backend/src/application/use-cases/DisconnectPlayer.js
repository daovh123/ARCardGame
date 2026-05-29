const { rotateTurn } = require("../../domain/services/RoomRules");

function createDisconnectPlayerUseCase({ roomRepository }) {
  return ({ playerId }) => {
    for (const room of roomRepository.all()) {
      const playerIndex = room.players.findIndex((player) => player.id === playerId);

      if (playerIndex < 0) {
        continue;
      }

      const [player] = room.players.splice(playerIndex, 1);

      if (room.currentTurnPlayerId === playerId) {
        rotateTurn(room);
      }

      if (room.unoCallerId === playerId) {
        room.unoCallerId = null;
      }

      room.lastAction = `${player.name} disconnected`;

      if (room.players.length === 0) {
        roomRepository.remove(room.code);
        return room.code;
      }

      if (room.players.length === 1 && room.gamePhase === "playing") {
        const remaining = room.players[0];
        room.winnerId = remaining.id;
        room.gamePhase = "finished";
        room.lastAction = `${player.name} disconnected. ${remaining.name} wins!`;
      }

      roomRepository.save(room);
      return room.code;
    }

    return null;
  };
}

module.exports = { createDisconnectPlayerUseCase };
