import { useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import { gameSocket } from "../../../core/socket/gameSocket";

export function useRoomState(session) {
  const [roomState, setRoomState] = useState(session.initialState);
  const [isConnected, setIsConnected] = useState(gameSocket.connected);
  const [pendingWildCardId, setPendingWildCardId] = useState(null);

  useEffect(() => {
    function handleConnect() {
      setIsConnected(true);
    }

    function handleDisconnect() {
      setIsConnected(false);
      toast.error("Mất kết nối đến server");
    }

    function handleRoomState(nextState) {
      setRoomState(nextState);
    }

    function handleGameError(error) {
      toast.error(error?.message || "Lỗi server");
    }

    gameSocket.on("connect", handleConnect);
    gameSocket.on("disconnect", handleDisconnect);
    gameSocket.on("room-state", handleRoomState);
    gameSocket.on("game:error", handleGameError);

    return () => {
      gameSocket.off("connect", handleConnect);
      gameSocket.off("disconnect", handleDisconnect);
      gameSocket.off("room-state", handleRoomState);
      gameSocket.off("game:error", handleGameError);
    };
  }, []);

  const actions = useMemo(
    () => ({
      playCard(cardId, chosenColor) {
        const card = roomState?.hand?.find((c) => c.id === cardId);
        if (card && (card.kind === "wild" || card.kind === "wild-draw-four") && !chosenColor) {
          setPendingWildCardId(cardId);
          return;
        }

        if (!roomState?.canAct) {
          toast.error("Chưa đến lượt bạn");
          return;
        }

        gameSocket.emit("game:play-card", { roomCode: session.roomCode, cardId, chosenColor }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể đánh bài");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
          }
        });
      },

      confirmWildColor(chosenColor) {
        if (!pendingWildCardId) return;
        const cardId = pendingWildCardId;
        setPendingWildCardId(null);

        gameSocket.emit("game:play-card", { roomCode: session.roomCode, cardId, chosenColor }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể đánh bài");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
          }
        });
      },

      cancelWildColor() {
        setPendingWildCardId(null);
      },

      drawCard() {
        if (!roomState?.canAct) {
          toast.error("Chưa đến lượt bạn");
          return;
        }

        gameSocket.emit("game:draw-card", { roomCode: session.roomCode }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể rút bài");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
          }
        });
      },

      endTurn() {
        if (!roomState?.canAct) {
          toast.error("Chưa đến lượt bạn");
          return;
        }

        gameSocket.emit("game:end-turn", { roomCode: session.roomCode }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể kết thúc lượt");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
          }
        });
      },

      callUno() {
        gameSocket.emit("game:call-uno", { roomCode: session.roomCode }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể gọi UNO");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
            toast.success("Bạn đã gọi UNO!");
          }
        });
      },

      catchUno(targetPlayerId) {
        gameSocket.emit("game:catch-uno", { roomCode: session.roomCode, targetId: targetPlayerId }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể bắt UNO");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
            toast.success("Bắt UNO thành công! Đối thủ rút 2 lá");
          }
        });
      },

      challengeWildFour() {
        gameSocket.emit("game:challenge-wild-four", { roomCode: session.roomCode }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể challenge");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
          }
        });
      },

      restart() {
        gameSocket.emit("game:restart", { roomCode: session.roomCode }, (response) => {
          if (!response?.ok) {
            toast.error(response?.error?.message || "Không thể bắt đầu lại");
            return;
          }
          if (response.state) {
            setRoomState(response.state);
            toast.success("Ván mới bắt đầu!");
          }
        });
      },
    }),
    [session.roomCode, roomState, pendingWildCardId],
  );

  return {
    roomState,
    isConnected,
    pendingWildCardId,
    actions,
  };
}
