const http = require("http");
const { Server } = require("socket.io");
const { createHttpApp } = require("./presentation/http/createApp");
const { registerGameGateway } = require("./presentation/socket/registerGameGateway");
const { registerTienLenGateway } = require("./presentation/socket/registerTienLenGateway");
const { InMemoryRoomRepository } = require("./infrastructure/repositories/InMemoryRoomRepository");
const { createRoomStateSerializer } = require("./application/services/RoomStateSerializer");
const { createJoinRoomUseCase } = require("./application/use-cases/JoinRoom");
const { createDrawCardUseCase } = require("./application/use-cases/DrawCard");
const { createPlayCardUseCase } = require("./application/use-cases/PlayCard");
const { createEndTurnUseCase } = require("./application/use-cases/EndTurn");
const { createDisconnectPlayerUseCase } = require("./application/use-cases/DisconnectPlayer");
const { createGetRoomStateUseCase } = require("./application/use-cases/GetRoomState");
const { createCallUnoUseCase, createCatchUnoUseCase, createChallengeWildFourUseCase } = require("./application/use-cases/CallUno");
const { createRestartGameUseCase } = require("./application/use-cases/RestartGame");
const { createAddBotUseCase } = require("./application/use-cases/AddBot");
const { createJoinTienLenRoomUseCase } = require("./application/use-cases/tien-len/JoinTienLenRoom");
const { createPlayTienLenCardsUseCase } = require("./application/use-cases/tien-len/PlayTienLenCards");
const { createPassTienLenTurnUseCase } = require("./application/use-cases/tien-len/PassTienLenTurn");
const { createGetTienLenStateUseCase } = require("./application/use-cases/tien-len/GetTienLenState");
const { createRestartTienLenGameUseCase } = require("./application/use-cases/tien-len/RestartTienLenGame");
const { createDisconnectTienLenPlayerUseCase } = require("./application/use-cases/tien-len/DisconnectTienLenPlayer");
const { getUnoBotAction, getTienLenBotAction, getBotDelay } = require("./domain/services/BotPlayer");

const PORT = Number(process.env.PORT || 3001);
const CLIENT_ORIGIN = process.env.CLIENT_ORIGIN || "http://localhost:5173";

const app = createHttpApp();
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: CLIENT_ORIGIN,
    methods: ["GET", "POST"],
  },
});

const roomRepository = new InMemoryRoomRepository();
const roomStateSerializer = createRoomStateSerializer();

// UNO use cases
const joinRoom = createJoinRoomUseCase({ roomRepository });
const drawCard = createDrawCardUseCase({ roomRepository });
const playCard = createPlayCardUseCase({ roomRepository });
const endTurn = createEndTurnUseCase({ roomRepository });
const disconnectPlayer = createDisconnectPlayerUseCase({ roomRepository });
const getRoomState = createGetRoomStateUseCase({ roomRepository, roomStateSerializer });
const callUno = createCallUnoUseCase({ roomRepository });
const catchUno = createCatchUnoUseCase({ roomRepository });
const challengeWildFour = createChallengeWildFourUseCase({ roomRepository });
const restartGame = createRestartGameUseCase({ roomRepository });
const addBot = createAddBotUseCase({ roomRepository });

// Tiến Lên use cases
const joinTienLenRoom = createJoinTienLenRoomUseCase({ roomRepository });
const playTienLenCards = createPlayTienLenCardsUseCase({ roomRepository });
const passTienLenTurn = createPassTienLenTurnUseCase({ roomRepository });
const getTienLenState = createGetTienLenStateUseCase({ roomRepository });
const restartTienLenGame = createRestartTienLenGameUseCase({ roomRepository });
const disconnectTienLenPlayer = createDisconnectTienLenPlayerUseCase({ roomRepository });

// Bot turn handler
function triggerBotTurn(roomCode, gameType) {
  const room = roomRepository.findByCode(roomCode);
  if (!room || room.gamePhase !== "playing" || room.winnerId) return;

  const currentPlayer = room.players.find((p) => p.id === room.currentTurnPlayerId);
  if (!currentPlayer || !currentPlayer.isBot) return;

  setTimeout(() => {
    try {
      if (gameType === "tien-len") {
        const action = getTienLenBotAction(room, currentPlayer);
        if (action.action === "play") {
          playTienLenCards({ roomCode, playerId: currentPlayer.id, cardIds: action.cardIds });
        } else {
          passTienLenTurn({ roomCode, playerId: currentPlayer.id });
        }
        emitTienLenStateAll(roomCode);
      } else {
        const action = getUnoBotAction(room, currentPlayer);
        if (action.action === "play") {
          playCard({ roomCode, playerId: currentPlayer.id, cardId: action.cardId, chosenColor: action.chosenColor });
          if (action.callUno) {
            try { callUno({ roomCode, playerId: currentPlayer.id }); } catch (_) {}
          }
        } else {
          drawCard({ roomCode, playerId: currentPlayer.id });
          try { endTurn({ roomCode, playerId: currentPlayer.id }); } catch (_) {}
        }
        emitUnoStateAll(roomCode);
      }
    } catch (err) {
      console.error("Bot error:", err.message);
    }
  }, getBotDelay());
}

function emitUnoStateAll(roomCode) {
  const sockRoom = io.sockets.adapter.rooms.get(roomCode);
  if (!sockRoom) return;
  for (const socketId of sockRoom) {
    const state = getRoomState({ roomCode, viewerId: socketId });
    if (state) io.to(socketId).emit("room-state", state);
  }
  triggerBotTurn(roomCode, "uno");
}

function emitTienLenStateAll(roomCode) {
  const sockRoom = io.sockets.adapter.rooms.get(roomCode);
  if (!sockRoom) return;
  for (const socketId of sockRoom) {
    const state = getTienLenState({ roomCode, viewerId: socketId });
    if (state) io.to(socketId).emit("tien-len:room-state", state);
  }
  triggerBotTurn(roomCode, "tien-len");
}

// Register UNO gateway
registerGameGateway(io, {
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
});

// Register Tiến Lên gateway
registerTienLenGateway(io, {
  joinTienLenRoom,
  playTienLenCards,
  passTienLenTurn,
  getTienLenState,
  restartTienLenGame,
  disconnectTienLenPlayer,
  triggerBotTurn,
});

server.listen(PORT, () => {
  console.log(`Backend listening on http://localhost:${PORT}`);
});
