function createRoom({ code, deck, createdAt = Date.now() }) {
  return {
    code,
    players: [],
    deck,
    discardPile: [],
    tableCards: [],
    currentTurnPlayerId: null,
    direction: 1,
    currentColor: null,
    pendingDraws: 0,
    winnerId: null,
    gamePhase: "waiting",
    unoCallerId: null,
    createdAt,
    lastAction: null,
    pendingChallenge: null,
    lastPlayedCard: null,
    drewAndPlayed: false,
    roundScores: [],
    gameSettings: {},
  };
}

module.exports = { createRoom };
