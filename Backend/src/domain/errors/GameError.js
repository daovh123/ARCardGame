class GameError extends Error {
  constructor(message, code = "GAME_ERROR") {
    super(message);
    this.name = "GameError";
    this.code = code;
  }
}

module.exports = { GameError };
