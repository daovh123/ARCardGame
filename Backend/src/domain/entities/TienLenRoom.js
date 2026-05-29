function createTienLenRoom({ code, createdAt = Date.now() }) {
  return {
    code,
    gameType: "tien-len",
    players: [],
    currentTurnPlayerId: null,
    lastPlayedCombo: null,
    passCount: 0,
    winnerId: null,
    gamePhase: "waiting",
    roundStarter: null,
    isFirstGame: true,
    tableCards: [],
    rankings: [],
    lastAction: null,
    createdAt,
  };
}

module.exports = { createTienLenRoom };
