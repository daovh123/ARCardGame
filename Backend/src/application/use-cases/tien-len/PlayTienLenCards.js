const { GameError } = require("../../../domain/errors/GameError");
const { classifyCombo, canBeat, mustIncludeThreeOfSpades, rotateTurn, checkWinCondition, createTablePlacement } = require("../../../domain/services/TienLenRules");

function createPlayTienLenCardsUseCase({ roomRepository }) {
  return ({ roomCode, playerId, cardIds }) => {
    const room = roomRepository.findByCode(roomCode);
    if (!room) throw new GameError("Room not found.", "ROOM_NOT_FOUND");
    if (room.gamePhase !== "playing") throw new GameError("Game not active.", "GAME_NOT_STARTED");
    if (room.currentTurnPlayerId !== playerId) throw new GameError("Not your turn.", "NOT_YOUR_TURN");

    const player = room.players.find((p) => p.id === playerId);
    if (!player) throw new GameError("Player not found.", "PLAYER_NOT_FOUND");

    if (!cardIds || cardIds.length === 0) {
      throw new GameError("No cards selected.", "NO_CARDS");
    }

    const selectedCards = [];
    for (const cid of cardIds) {
      const card = player.hand.find((c) => c.id === cid);
      if (!card) throw new GameError("Card not in hand.", "CARD_NOT_FOUND");
      selectedCards.push(card);
    }

    const combo = classifyCombo(selectedCards);
    if (!combo) {
      throw new GameError("Tổ hợp bài không hợp lệ.", "INVALID_COMBO");
    }

    const isNewRound = !room.lastPlayedCombo;
    const isFirstMove = room.isFirstGame && room.tableCards.length === 0;

    if (isFirstMove && !mustIncludeThreeOfSpades(selectedCards, true)) {
      throw new GameError("Ván đầu phải chơi 3♠.", "MUST_PLAY_THREE_SPADE");
    }

    if (!isNewRound && !canBeat(room.lastPlayedCombo, combo)) {
      throw new GameError("Bài không đủ lớn để đánh.", "CANNOT_BEAT");
    }

    for (const card of selectedCards) {
      const idx = player.hand.findIndex((c) => c.id === card.id);
      if (idx >= 0) player.hand.splice(idx, 1);
    }

    room.lastPlayedCombo = combo;
    room.passCount = 0;

    for (const card of selectedCards) {
      room.tableCards.push({
        ...card,
        placement: createTablePlacement(room.tableCards.length),
        playedBy: playerId,
      });
    }

    const comboNames = {
      single: "lá đơn",
      pair: "đôi",
      triple: "ba",
      quad: "tứ quý",
      straight: "sảnh",
      doubleStraight: "sảnh đôi",
    };

    let isChop = false;
    if (!isNewRound && room.lastPlayedCombo) {
      const lastType = room.lastPlayedCombo.type;
      const lastHigh = room.lastPlayedCombo.highCard.label;
      if (lastHigh === "2" || lastType === "quad" || lastType === "doubleStraight") {
        if (combo.type === "quad" || combo.type === "doubleStraight") {
          isChop = true;
        }
      }
    }

    room.lastAction = `${player.name} đánh ${comboNames[combo.type] || combo.type}`;
    if (isChop) {
      room.lastAction += " (chặn heo!)";
    }

    if (checkWinCondition(room, player)) {
      room.lastAction = `🏆 ${player.name} đã thắng!`;
      room.isFirstGame = false;
    } else {
      rotateTurn(room);
    }

    return roomRepository.save(room);
  };
}

module.exports = { createPlayTienLenCardsUseCase };
