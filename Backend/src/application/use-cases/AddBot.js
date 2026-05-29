const { createPlayer } = require("../../domain/entities/Player");
const { getRandomBotIdentity } = require("../../domain/services/BotPlayer");
const { GameError } = require("../../domain/errors/GameError");
const { drawFromDeck, resetGameForNewRound } = require("../../domain/services/RoomRules");
const { dealTienLenCards } = require("../../domain/services/TienLenDeckFactory");
const { findStartingPlayer } = require("../../domain/services/TienLenRules");
const { randomUUID } = require("crypto");

function createAddBotUseCase({ roomRepository }) {
  return ({ roomCode, gameType }) => {
    const room = roomRepository.findByCode(roomCode);
    if (!room) throw new GameError("Room not found.", "ROOM_NOT_FOUND");

    const maxPlayers = gameType === "tien-len" ? 4 : 8;
    if (room.players.length >= maxPlayers) {
      throw new GameError("Room is full.", "ROOM_FULL");
    }

    const existingNames = room.players.map((p) => p.name);
    const { name, avatarKey } = getRandomBotIdentity(existingNames);
    const botId = `bot_${randomUUID().slice(0, 8)}`;

    const bot = createPlayer({ id: botId, name, avatarKey });
    bot.isBot = true;
    bot.seatIndex = room.players.length;
    room.players.push(bot);

    if (gameType === "tien-len") {
      if (room.players.length >= 2 && room.gamePhase === "waiting") {
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
          room.lastAction = `Game bắt đầu! ${starter.name} có 3♠ đi trước.`;
        } else {
          room.currentTurnPlayerId = room.players[0].id;
          room.lastAction = `Ván mới bắt đầu!`;
        }
      }
    } else {
      if (room.players.length >= 2 && room.gamePhase === "waiting") {
        resetGameForNewRound(room);
        room.lastAction = `Game started! ${room.players[0].name} goes first.`;
      } else if (room.gamePhase === "waiting") {
        bot.hand.push(...drawFromDeck(room, 7));
      } else {
        bot.hand.push(...drawFromDeck(room, 7));
      }
    }

    room.lastAction = `🤖 ${name} đã vào phòng.`;
    return roomRepository.save(room);
  };
}

module.exports = { createAddBotUseCase };
