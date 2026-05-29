const { createRoom } = require("../../domain/entities/Room");
const { createPlayer } = require("../../domain/entities/Player");
const { createUnoDeck } = require("../../domain/services/DeckFactory");
const { drawFromDeck, createTablePlacement, resetGameForNewRound } = require("../../domain/services/RoomRules");
const { GameError } = require("../../domain/errors/GameError");

function createJoinRoomUseCase({ roomRepository }) {
  return ({ socketId, roomCode, playerName, avatarKey }) => {
    const normalizedRoomCode = String(roomCode || "").trim().toUpperCase();
    const normalizedName = String(playerName || "").trim();

    if (!normalizedRoomCode) {
      throw new GameError("Room code is required.", "ROOM_CODE_REQUIRED");
    }

    if (!normalizedName) {
      throw new GameError("Player name is required.", "PLAYER_NAME_REQUIRED");
    }

    let room = roomRepository.findByCode(normalizedRoomCode);

    if (!room) {
      room = createRoom({
        code: normalizedRoomCode,
        deck: createUnoDeck(),
      });
    }

    const existingPlayer = room.players.find((player) => player.id === socketId);

    if (existingPlayer) {
      return roomRepository.save(room);
    }

    if (room.players.length >= 8) {
      throw new GameError("Room is full (max 8 players).", "ROOM_FULL");
    }

    const player = createPlayer({
      id: socketId,
      name: normalizedName,
      avatarKey: avatarKey || "char_1",
    });

    player.seatIndex = room.players.length;
    room.players.push(player);

    if (room.players.length >= 2 && room.gamePhase === "waiting") {
      resetGameForNewRound(room);
      room.lastAction = `Game started! ${room.players[0].name} goes first.`;
    } else if (room.gamePhase === "waiting") {
      player.hand.push(...drawFromDeck(room, 7));
      room.lastAction = `${player.name} joined. Waiting for more players...`;
    } else {
      player.hand.push(...drawFromDeck(room, 7));
      room.lastAction = `${player.name} joined the game.`;
    }

    return roomRepository.save(room);
  };
}

module.exports = { createJoinRoomUseCase };
