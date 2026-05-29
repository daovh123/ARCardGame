import { useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import { gameSocket } from "../../../core/socket/gameSocket";

export function useTienLenRoomState(session) {
  const [roomState, setRoomState] = useState(session.initialState);
  const [isConnected, setIsConnected] = useState(gameSocket.connected);
  const [selectedCardIds, setSelectedCardIds] = useState(new Set());

  useEffect(() => {
    function handleConnect() { setIsConnected(true); }
    function handleDisconnect() { setIsConnected(false); toast.error("Mất kết nối đến server"); }
    function handleRoomState(state) { setRoomState(state); }
    function handleError(error) { toast.error(error?.message || "Lỗi server"); }

    gameSocket.on("connect", handleConnect);
    gameSocket.on("disconnect", handleDisconnect);
    gameSocket.on("tien-len:room-state", handleRoomState);
    gameSocket.on("game:error", handleError);

    return () => {
      gameSocket.off("connect", handleConnect);
      gameSocket.off("disconnect", handleDisconnect);
      gameSocket.off("tien-len:room-state", handleRoomState);
      gameSocket.off("game:error", handleError);
    };
  }, []);

  const comboInfo = useMemo(() => {
    if (selectedCardIds.size === 0) return null;
    const cards = (roomState?.hand || []).filter((c) => selectedCardIds.has(c.id));
    return classifyComboClient(cards);
  }, [selectedCardIds, roomState?.hand]);

  const actions = useMemo(() => ({
    playCards() {
      if (selectedCardIds.size === 0) {
        toast.error("Chọn bài để đánh");
        return;
      }
      const cardIds = Array.from(selectedCardIds);
      gameSocket.emit("tien-len:play-cards", { roomCode: session.roomCode, cardIds }, (response) => {
        if (!response?.ok) {
          toast.error(response?.error?.message || "Không thể đánh bài");
          return;
        }
        setSelectedCardIds(new Set());
        if (response.state) setRoomState(response.state);
      });
    },

    pass() {
      gameSocket.emit("tien-len:pass", { roomCode: session.roomCode }, (response) => {
        if (!response?.ok) {
          toast.error(response?.error?.message || "Không thể bỏ lượt");
          return;
        }
        setSelectedCardIds(new Set());
        if (response.state) setRoomState(response.state);
      });
    },

    restart() {
      gameSocket.emit("tien-len:restart", { roomCode: session.roomCode }, (response) => {
        if (!response?.ok) {
          toast.error(response?.error?.message || "Không thể bắt đầu lại");
          return;
        }
        setSelectedCardIds(new Set());
        if (response.state) { setRoomState(response.state); toast.success("Ván mới bắt đầu!"); }
      });
    },

    toggleCard(cardId) {
      setSelectedCardIds((prev) => {
        const next = new Set(prev);
        if (next.has(cardId)) next.delete(cardId);
        else next.add(cardId);
        return next;
      });
    },

    clearSelection() {
      setSelectedCardIds(new Set());
    },
  }), [session.roomCode, selectedCardIds]);

  return { roomState, isConnected, selectedCardIds, comboInfo, actions };
}

function classifyComboClient(cards) {
  if (!cards || cards.length === 0) return null;
  const RANK_ORDER = ["3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2"];
  const sorted = [...cards].sort((a, b) => RANK_ORDER.indexOf(a.label) - RANK_ORDER.indexOf(b.label));
  const count = sorted.length;

  if (count === 1) return { type: "single", valid: true };
  if (count === 2 && sorted[0].label === sorted[1].label) return { type: "pair", valid: true };
  if (count === 3 && sorted.every((c) => c.label === sorted[0].label)) return { type: "triple", valid: true };
  if (count === 4 && sorted.every((c) => c.label === sorted[0].label)) return { type: "quad", valid: true };

  const hasTwos = sorted.some((c) => c.label === "2");
  if (!hasTwos && count >= 3) {
    let isStraight = true;
    for (let i = 1; i < count; i++) {
      if (RANK_ORDER.indexOf(sorted[i].label) !== RANK_ORDER.indexOf(sorted[i - 1].label) + 1) {
        isStraight = false;
        break;
      }
    }
    if (isStraight) return { type: "straight", valid: true };
  }

  if (!hasTwos && count >= 6 && count % 2 === 0) {
    let isDoubleStraight = true;
    for (let i = 0; i < count; i += 2) {
      if (sorted[i].label !== sorted[i + 1]?.label) { isDoubleStraight = false; break; }
      if (i > 0) {
        const prevRank = RANK_ORDER.indexOf(sorted[i - 2].label);
        const currRank = RANK_ORDER.indexOf(sorted[i].label);
        if (currRank !== prevRank + 1) { isDoubleStraight = false; break; }
      }
    }
    if (isDoubleStraight) return { type: "doubleStraight", valid: true };
  }

  return { type: "invalid", valid: false };
}
