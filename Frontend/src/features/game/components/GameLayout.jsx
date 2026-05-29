import { Suspense, useMemo, useState } from "react";
import TableScene from "./TableScene";
import HandPanel from "./HandPanel";
import PlayersSidebar from "./PlayersSidebar";
import ColorPicker from "./ColorPicker";
import VictoryScreen from "./VictoryScreen";
import { useRoomState } from "../hooks/useRoomState";
import { UNO_COLORS } from "../../../shared/constants/assets";

export default function GameLayout({ session, onLeave }) {
  const { roomState, isConnected, pendingWildCardId, actions } = useRoomState(session);
  const [draggedCardId, setDraggedCardId] = useState(null);
  const [dropActive, setDropActive] = useState(false);

  const selfPlayer = useMemo(
    () => roomState?.players?.find((player) => player.isSelf) ?? null,
    [roomState],
  );

  const isWinner = roomState?.winnerId != null;
  const isWaiting = roomState?.gamePhase === "waiting";
  const canCallUno = selfPlayer?.handCount <= 2
    && selfPlayer?.handCount > 0
    && roomState?.gamePhase === "playing"
    && roomState?.unoCallerId !== selfPlayer?.id;
  const canChallenge = roomState?.canChallenge;
  const hasDrawn = roomState?.hasDrawnThisTurn;
  const mustDraw = roomState?.pendingDraws > 0;

  function handleDropZoneDragOver(event) {
    event.preventDefault();
    setDropActive(true);
  }

  function handleDropZoneDrop(event) {
    event.preventDefault();
    setDropActive(false);

    const cardId = event.dataTransfer.getData("text/plain") || draggedCardId;

    if (cardId) {
      actions.playCard(cardId);
    }

    setDraggedCardId(null);
  }

  return (
    <main className="game-shell">
      <section className="table-stage">
        <div className="hud-top">
          <div className="hud-copy">
            <p className="eyebrow">Room {session.roomCode}</p>
            <h2>UNO AR</h2>
            <p className="hud-subtitle">Tập trung vào bàn chơi, kéo bài vào giữa bàn để đánh.</p>
          </div>
          <div className="status-row">
            <span className={isConnected ? "status-pill online" : "status-pill offline"}>
              {isConnected ? "Online" : "Offline"}
            </span>
            {roomState?.currentColor && (
              <span
                className="current-color-pill"
                style={{ background: UNO_COLORS[roomState.currentColor]?.hex || "#fff" }}
              >
                {UNO_COLORS[roomState.currentColor]?.label || roomState.currentColor}
              </span>
            )}
            {roomState?.pendingDraws > 0 && (
              <span className="pending-draws-pill">+{roomState.pendingDraws}</span>
            )}
            <button type="button" className="ghost-button" onClick={onLeave}>
              Thoát
            </button>
          </div>
        </div>

        <div className="table-wrapper">
          <Suspense fallback={null}>
            <TableScene roomState={roomState} roomCode={session.roomCode} />
          </Suspense>

          <div
            className={dropActive ? "play-dropzone active" : "play-dropzone"}
            onDragOver={handleDropZoneDragOver}
            onDragLeave={() => setDropActive(false)}
            onDrop={handleDropZoneDrop}
          >
            <span>Kéo bài vào đây để đánh</span>
          </div>

          {roomState?.lastAction && (
            <div className="last-action-overlay">
              <p>{roomState.lastAction}</p>
            </div>
          )}
        </div>

        <div className="bottom-bar">
          <div className="turn-info">
            <strong>{selfPlayer?.name || session.playerName}</strong>
            <span>
              {isWaiting
                ? "Đang chờ người chơi..."
                : isWinner
                  ? "Game kết thúc!"
                  : canChallenge
                    ? "Đối thủ đánh +4! Bạn có muốn Challenge?"
                    : mustDraw
                      ? `Bạn phải rút ${roomState.pendingDraws} lá!`
                      : roomState?.canAct
                        ? hasDrawn
                          ? "Đã rút bài. Đánh bài hoặc kết thúc lượt"
                          : "Đến lượt bạn!"
                        : "Chờ người chơi khác..."}
            </span>
          </div>
        </div>
      </section>

      <aside className="side-panel">
        <PlayersSidebar
          players={roomState?.players || []}
          currentTurnPlayerId={roomState?.currentTurnPlayerId}
          deckCount={roomState?.deckCount || 0}
          direction={roomState?.direction}
          currentColor={roomState?.currentColor}
          pendingDraws={roomState?.pendingDraws}
          unoCallerId={roomState?.unoCallerId}
          gamePhase={roomState?.gamePhase}
          onCatchUno={actions.catchUno}
        />

        <div className="panel-card">
          <h3>Hoạt động</h3>
          <p>{roomState?.lastAction || "Phòng vừa được tạo. Chờ người chơi..."}</p>
        </div>
      </aside>

      <HandPanel
        cards={roomState?.hand || []}
        disabled={!roomState?.canAct}
        playableCardIds={roomState?.playableCardIds || []}
        canAct={roomState?.canAct}
        canCallUno={canCallUno}
        canChallenge={canChallenge}
        mustDraw={mustDraw}
        hasDrawn={hasDrawn}
        pendingDraws={roomState?.pendingDraws || 0}
        onPlayCard={actions.playCard}
        onDrawCard={actions.drawCard}
        onEndTurn={actions.endTurn}
        onCallUno={actions.callUno}
        onChallenge={actions.challengeWildFour}
        onDragStart={setDraggedCardId}
        onDragEnd={() => setDraggedCardId(null)}
        topDiscard={roomState?.topDiscard}
      />

      {pendingWildCardId && (
        <ColorPicker
          onSelect={actions.confirmWildColor}
          onCancel={actions.cancelWildColor}
        />
      )}

      {isWinner && (
        <VictoryScreen
          roomState={roomState}
          onRestart={actions.restart}
          onLeave={onLeave}
        />
      )}
    </main>
  );
}
