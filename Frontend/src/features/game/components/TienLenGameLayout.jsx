import { Suspense, useMemo } from "react";
import TableScene from "./TableScene";
import TienLenHandPanel from "./TienLenHandPanel";
import PlayersSidebar from "./PlayersSidebar";
import VictoryScreen from "./VictoryScreen";
import { useTienLenRoomState } from "../hooks/useTienLenRoomState";

export default function TienLenGameLayout({ session, gameType, onLeave, onBackToHub }) {
  const { roomState, isConnected, selectedCardIds, comboInfo, actions } = useTienLenRoomState(session);

  const selfPlayer = useMemo(
    () => roomState?.players?.find((p) => p.isSelf) ?? null,
    [roomState],
  );

  const isWinner = roomState?.winnerId != null;
  const isWaiting = roomState?.gamePhase === "waiting";

  const lastComboDisplay = useMemo(() => {
    if (!roomState?.lastPlayedCombo) return null;
    const NAMES = { single: "Lá đơn", pair: "Đôi", triple: "Ba", quad: "Tứ quý", straight: "Sảnh", doubleStraight: "Sảnh đôi" };
    return NAMES[roomState.lastPlayedCombo.type] || roomState.lastPlayedCombo.type;
  }, [roomState?.lastPlayedCombo]);

  return (
    <main className="game-shell">
      <section className="table-stage">
        <div className="hud-top">
          <div>
            <p className="eyebrow">Room {session.roomCode}</p>
            <h2>🀄 Tiến Lên</h2>
          </div>
          <div className="status-row">
            <span className={isConnected ? "status-pill online" : "status-pill offline"}>
              {isConnected ? "Online" : "Offline"}
            </span>
            {lastComboDisplay && (
              <span className="current-color-pill" style={{ background: "rgba(243,198,109,0.3)", color: "#f3c66d" }}>
                {lastComboDisplay}
              </span>
            )}
            {roomState?.isNewRound && roomState?.gamePhase === "playing" && (
              <span className="status-pill" style={{ background: "rgba(46,204,113,0.18)", color: "#9ee3b3" }}>
                Vòng mới
              </span>
            )}
            <button type="button" className="ghost-button" onClick={onBackToHub}>Thoát</button>
          </div>
        </div>

        <div className="table-wrapper">
          <Suspense fallback={null}>
            <TableScene roomState={roomState} roomCode={session.roomCode} gameType="tien-len" />
          </Suspense>

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
                  : roomState?.canAct
                    ? roomState?.isNewRound
                      ? "Vòng mới — Đánh bài bất kỳ!"
                      : "Đến lượt bạn! Chọn bài và đánh"
                    : "Chờ người chơi khác..."}
            </span>
          </div>
        </div>
      </section>

      <aside className="side-panel">
        <PlayersSidebar
          players={roomState?.players || []}
          currentTurnPlayerId={roomState?.currentTurnPlayerId}
          deckCount={0}
          direction={1}
          currentColor={null}
          pendingDraws={0}
          unoCallerId={null}
          gamePhase={roomState?.gamePhase}
        />
        <div className="panel-card">
          <h3>Hoạt động</h3>
          <p>{roomState?.lastAction || "Phòng vừa được tạo. Chờ người chơi..."}</p>
        </div>
      </aside>

      <TienLenHandPanel
        cards={roomState?.hand || []}
        disabled={!roomState?.canAct}
        selectedCardIds={selectedCardIds}
        comboInfo={comboInfo}
        onToggleCard={actions.toggleCard}
        onPlayCards={actions.playCards}
        onPass={actions.pass}
        canPass={roomState?.canPass}
      />

      {isWinner && (
        <VictoryScreen
          roomState={roomState}
          onRestart={actions.restart}
          onLeave={onBackToHub}
        />
      )}
    </main>
  );
}
