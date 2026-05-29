const { createTienLenRoom } = require("../../../domain/entities/TienLenRoom");
const { createPlayer } = require("../../../domain/entities/Player");
const { dealTienLenCards } = require("../../../domain/services/TienLenDeckFactory");
const { findStartingPlayer } = require("../../../domain/services/TienLenRules");
const { GameError } = require("../../../domain/errors/GameError");

function createJoinTienLenRoomUseCase({ roomRepository }) {
  return ({ socketId, roomCode, playerName, avatarKey }) => {
    const normalizedCode = String(roomCode || "").trim().toUpperCase();
    const normalizedName = String(playerName || "").trim();

    if (!normalizedCode) throw new GameError("Room code is required.", "ROOM_CODE_REQUIRED");
    if (!normalizedName) throw new GameError("Player name is required.", "PLAYER_NAME_REQUIRED");

    let room = roomRepository.findByCode(normalizedCode);

    if (!room) {
      room = createTienLenRoom({ code: normalizedCode });
    }

    if (room.gameType !== "tien-len") {
      throw new GameError("This room is not a Tiến Lên room.", "WRONG_GAME_TYPE");
    }

    const existing = room.players.find((p) => p.id === socketId);
    if (existing) return roomRepository.save(room);

    if (room.players.length >= 4) {
      throw new GameError("Room is full (max 4 players).", "ROOM_FULL");
    }

    const player = createPlayer({ id: socketId, name: normalizedName, avatarKey: avatarKey || "char_1" });
    player.seatIndex = room.players.length;
    room.players.push(player);

    if (room.players.length >= 2 && room.gamePhase === "waiting") {
      startTienLenGame(room);
    } else {
      room.lastAction = `${player.name} đã vào phòng. Chờ thêm người chơi...`;
    }

    return roomRepository.save(room);
  };
}

function startTienLenGame(room) {
  dealTienLenCards(room);
  room.gamePhase = "playing";
  room.rankings = [];
  room.winnerId = null;
  room.lastPlayedCombo = null;
  room.passCount = 0;
  room.tableCards = [];

  if (room.isFirstGame) {
    const starter = findStartingPlayer(room.players);
    room.currentTurnPlayerId = starter.id;
    room.roundStarter = starter.id;
    room.lastAction = `Game bắt đầu! ${starter.name} có 3♠ đi trước.`;
  } else {
    const prevWinner = room.rankings.length > 0 ? room.rankings[0] : room.players[0].id;
    room.currentTurnPlayerId = prevWinner;
    room.roundStarter = prevWinner;
    const winnerName = room.players.find((p) => p.id === prevWinner)?.name || "???";
    room.lastAction = `Ván mới! ${winnerName} đi trước.`;
  }
}

module.exports = { createJoinTienLenRoomUseCase, startTienLenGame };
