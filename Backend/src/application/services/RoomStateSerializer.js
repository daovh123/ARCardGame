const { canPlayCard, hasColorInHand } = require("../../domain/services/RoomRules");

function createRoomStateSerializer() {
  function serializePlayer(player, viewerId) {
    return {
      id: player.id,
      name: player.name,
      avatarKey: player.avatarKey,
      score: player.score,
      seatIndex: player.seatIndex,
      handCount: player.hand.length,
      isSelf: player.id === viewerId,
    };
  }

  function serializeTopCard(card) {
    if (!card) return null;
    return {
      id: card.id,
      label: card.label,
      color: card.color,
      points: card.points,
      assetPath: card.assetPath,
      kind: card.kind,
    };
  }

  function serializeTableCard(card) {
    return {
      id: card.id,
      label: card.label,
      color: card.color,
      points: card.points,
      assetPath: card.assetPath,
      kind: card.kind,
      placement: card.placement,
      playedBy: card.playedBy,
    };
  }

  function serialize(room, viewerId) {
    const viewer = room.players.find((player) => player.id === viewerId);
    const topDiscard = room.discardPile.length > 0
      ? room.discardPile[room.discardPile.length - 1]
      : null;

    const isViewerTurn = room.currentTurnPlayerId === viewerId;
    const canChallenge = room.pendingChallenge && room.pendingChallenge.challengerId === viewerId;

    return {
      roomCode: room.code,
      currentTurnPlayerId: room.currentTurnPlayerId,
      deckCount: room.deck.length,
      lastAction: room.lastAction,
      direction: room.direction,
      currentColor: room.currentColor,
      pendingDraws: room.pendingDraws,
      winnerId: room.winnerId,
      gamePhase: room.gamePhase,
      unoCallerId: room.unoCallerId,
      topDiscard: serializeTopCard(topDiscard),
      players: room.players.map((player) => serializePlayer(player, viewerId)),
      tableCards: room.tableCards.map(serializeTableCard),
      hand: viewer ? viewer.hand.map((card) => ({ ...card })) : [],
      canAct: isViewerTurn && room.gamePhase === "playing" && !room.winnerId && !room.pendingChallenge,
      canChallenge,
      pendingChallengeColor: canChallenge ? room.pendingChallenge?.chosenColor : null,
      hasDrawnThisTurn: isViewerTurn ? (room.hasDrawnThisTurn || false) : false,
      playableCardIds: viewer && room.gamePhase === "playing" && isViewerTurn && !room.pendingChallenge
        ? viewer.hand
            .filter((card) => canPlayCard(room, card, viewer))
            .map((card) => card.id)
        : [],
    };
  }

  return { serialize };
}

module.exports = { createRoomStateSerializer };
