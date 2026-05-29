function createDisconnectTienLenPlayerUseCase({ roomRepository }) {
  return ({ playerId }) => {
    const rooms = roomRepository.all();

    for (const room of rooms) {
      if (room.gameType !== "tien-len") continue;

      const index = room.players.findIndex((p) => p.id === playerId);
      if (index === -1) continue;

      const player = room.players[index];
      room.lastAction = `${player.name} đã thoát.`;

      if (room.currentTurnPlayerId === playerId) {
        const activePlayers = room.players.filter((p) => p.id !== playerId && p.hand.length > 0);
        if (activePlayers.length > 0) {
          const currentIdx = room.players.findIndex((p) => p.id === playerId);
          let nextIdx = (currentIdx + 1) % room.players.length;
          while (room.players[nextIdx].id === playerId || room.players[nextIdx].hand.length === 0) {
            nextIdx = (nextIdx + 1) % room.players.length;
            if (nextIdx === currentIdx) break;
          }
          room.currentTurnPlayerId = room.players[nextIdx].id;
        }
      }

      room.players.splice(index, 1);

      if (room.players.length <= 1) {
        if (room.players.length === 1) {
          room.winnerId = room.players[0].id;
          room.gamePhase = "finished";
        } else {
          roomRepository.remove(room.code);
        }
      }

      roomRepository.save(room);
      return room.code;
    }

    return null;
  };
}

module.exports = { createDisconnectTienLenPlayerUseCase };
