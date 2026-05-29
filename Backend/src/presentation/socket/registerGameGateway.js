const { GameError } = require("../../domain/errors/GameError");

function registerGameGateway(io, dependencies) {
  const {
    joinRoom,
    drawCard,
    playCard,
    endTurn,
    disconnectPlayer,
    getRoomState,
    callUno,
    catchUno,
    challengeWildFour,
    restartGame,
    addBot,
    triggerBotTurn,
  } = dependencies;

  function emitRoomState(roomCode) {
    const room = io.sockets.adapter.rooms.get(roomCode);

    if (!room) {
      return;
    }

    for (const socketId of room) {
      const state = getRoomState({ roomCode, viewerId: socketId });

      if (state) {
        io.to(socketId).emit("room-state", state);
      }
    }
  }

  function handleUseCase(socket, action) {
    return (payload = {}, acknowledge) => {
      try {
        const room = action(payload);
        const state = room?.code ? getRoomState({ roomCode: room.code, viewerId: socket.id }) : null;

        if (room?.code) {
          emitRoomState(room.code);
        }

        if (acknowledge) {
          acknowledge({ ok: true, state });
        }
      } catch (error) {
        const normalizedError = error instanceof GameError
          ? { message: error.message, code: error.code }
          : { message: "Unexpected server error.", code: "UNEXPECTED_ERROR" };

        socket.emit("game:error", normalizedError);

        if (acknowledge) {
          acknowledge({ ok: false, error: normalizedError });
        }
      }
    };
  }

  io.on("connection", (socket) => {
    socket.on(
      "room:join",
      handleUseCase(socket, ({ roomCode, playerName, avatarKey }) => {
        const room = joinRoom({
          socketId: socket.id,
          roomCode,
          playerName,
          avatarKey,
        });

        socket.join(room.code);
        return room;
      }),
    );

    socket.on(
      "game:play-card",
      handleUseCase(socket, ({ roomCode, cardId, chosenColor }) =>
        playCard({ roomCode, playerId: socket.id, cardId, chosenColor })
      ),
    );

    socket.on(
      "game:draw-card",
      handleUseCase(socket, ({ roomCode }) => drawCard({ roomCode, playerId: socket.id })),
    );

    socket.on(
      "game:end-turn",
      handleUseCase(socket, ({ roomCode }) => endTurn({ roomCode, playerId: socket.id })),
    );

    socket.on(
      "game:call-uno",
      handleUseCase(socket, ({ roomCode }) => callUno({ roomCode, playerId: socket.id })),
    );

    socket.on(
      "game:catch-uno",
      handleUseCase(socket, ({ roomCode, targetId }) =>
        catchUno({ roomCode, catcherId: socket.id, targetId })
      ),
    );

    socket.on(
      "game:challenge-wild-four",
      handleUseCase(socket, ({ roomCode }) =>
        challengeWildFour({ roomCode, challengerId: socket.id })
      ),
    );

    socket.on(
      "game:restart",
      handleUseCase(socket, ({ roomCode }) => restartGame({ roomCode })),
    );

    socket.on(
      "room:add-bot",
      handleUseCase(socket, ({ roomCode, gameType }) =>
        addBot({ roomCode, gameType: gameType || "uno" })
      ),
    );

    socket.on("disconnect", () => {
      const roomCode = disconnectPlayer({ playerId: socket.id });

      if (roomCode) {
        emitRoomState(roomCode);
        if (triggerBotTurn) triggerBotTurn(roomCode, "uno");
      }
    });
  });
}

module.exports = { registerGameGateway };
