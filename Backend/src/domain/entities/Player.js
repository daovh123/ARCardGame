function createPlayer({ id, name, avatarKey }) {
  return {
    id,
    name,
    avatarKey,
    hand: [],
    score: 0,
    seatIndex: -1,
    connectedAt: Date.now(),
  };
}

module.exports = { createPlayer };
