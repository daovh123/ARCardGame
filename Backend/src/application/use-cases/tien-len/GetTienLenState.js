const { classifyCombo, canBeat, sortCards } = require("../../../domain/services/TienLenRules");

function createGetTienLenStateUseCase({ roomRepository }) {
  function serializePlayer(player, viewerId) {
    return {
      id: player.id,
      name: player.name,
      avatarKey: player.avatarKey,
      score: player.score,
      seatIndex: player.seatIndex,
      handCount: player.hand.length,
      isSelf: player.id === viewerId,
      isBot: player.isBot || false,
    };
  }

  function serializeTableCard(card) {
    return {
      id: card.id,
      label: card.label,
      color: card.color,
      suit: card.suit,
      assetPath: card.assetPath,
      placement: card.placement,
      playedBy: card.playedBy,
    };
  }

  return ({ roomCode, viewerId }) => {
    const room = roomRepository.findByCode(roomCode);
    if (!room) return null;

    const viewer = room.players.find((p) => p.id === viewerId);
    const isViewerTurn = room.currentTurnPlayerId === viewerId;
    const isNewRound = !room.lastPlayedCombo;

    return {
      gameType: "tien-len",
      roomCode: room.code,
      currentTurnPlayerId: room.currentTurnPlayerId,
      lastAction: room.lastAction,
      winnerId: room.winnerId,
      gamePhase: room.gamePhase,
      rankings: room.rankings,
      passCount: room.passCount,
      lastPlayedCombo: room.lastPlayedCombo ? {
        type: room.lastPlayedCombo.type,
        cards: room.lastPlayedCombo.cards.map((c) => ({
          id: c.id, label: c.label, color: c.color, suit: c.suit, assetPath: c.assetPath,
        })),
        playerId: room.lastPlayedCombo.playerId,
      } : null,
      isNewRound,
      players: room.players.map((p) => serializePlayer(p, viewerId)),
      tableCards: room.tableCards.slice(-20).map(serializeTableCard),
      hand: viewer ? sortCards(viewer.hand).map((c) => ({
        id: c.id, label: c.label, color: c.color, suit: c.suit,
        assetPath: c.assetPath, points: c.points,
      })) : [],
      canAct: isViewerTurn && room.gamePhase === "playing" && !room.winnerId,
      canPass: isViewerTurn && room.gamePhase === "playing" && !room.winnerId && !isNewRound,
    };
  };
}

module.exports = { createGetTienLenStateUseCase };
