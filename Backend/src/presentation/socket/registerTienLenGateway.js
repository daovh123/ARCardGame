const { GameError } = require("../../domain/errors/GameError");

function registerTienLenGateway(io, dependencies) {
  const { joinTienLenRoom, playTienLenCards, passTienLenTurn, getTienLenState, restartTienLenGame, disconnectTienLenPlayer, triggerBotTurn } = dependencies;

  function emitTienLenState(roomCode) {
    const room = io.sockets.adapter.rooms.get(roomCode);
    if (!room) return;

    for (const socketId of room) {
      const state = getTienLenState({ roomCode, viewerId: socketId });
      if (state) {
        io.to(socketId).emit("tien-len:room-state", state);
      }
    }

    if (triggerBotTurn) triggerBotTurn(roomCode, "tien-len");
  }

  function handleUseCase(socket, action) {
    return (payload = {}, acknowledge) => {
      try {
        const room = action(payload);
        const state = room?.code ? getTienLenState({ roomCode: room.code, viewerId: socket.id }) : null;

        if (room?.code) emitTienLenState(room.code);
        if (acknowledge) acknowledge({ ok: true, state });
      } catch (error) {
        const normalized = error instanceof GameError
          ? { message: error.message, code: error.code }
          : { message: "Unexpected server error.", code: "UNEXPECTED_ERROR" };

        socket.emit("game:error", normalized);
        if (acknowledge) acknowledge({ ok: false, error: normalized });
      }
    };
  }

  io.on("connection", (socket) => {
    socket.on("tien-len:join", handleUseCase(socket, ({ roomCode, playerName, avatarKey }) => {
      const room = joinTienLenRoom({ socketId: socket.id, roomCode, playerName, avatarKey });
      socket.join(room.code);
      return room;
    }));

    socket.on("tien-len:play-cards", handleUseCase(socket, ({ roomCode, cardIds }) =>
      playTienLenCards({ roomCode, playerId: socket.id, cardIds })
    ));

    socket.on("tien-len:pass", handleUseCase(socket, ({ roomCode }) =>
      passTienLenTurn({ roomCode, playerId: socket.id })
    ));

    socket.on("tien-len:restart", handleUseCase(socket, ({ roomCode }) =>
      restartTienLenGame({ roomCode })
    ));

    socket.on("disconnect", () => {
      const roomCode = disconnectTienLenPlayer({ playerId: socket.id });
      if (roomCode) emitTienLenState(roomCode);
    });
  });
}

module.exports = { registerTienLenGateway };
