import { avatarOptions, avatarFrameAsset, cardBackAsset, UNO_COLORS } from "../../../shared/constants/assets";

function resolveAvatar(key) {
  return avatarOptions.find((avatar) => avatar.key === key)?.assetPath || avatarOptions[0].assetPath;
}

export default function PlayersSidebar({ players, currentTurnPlayerId, deckCount, direction, currentColor, pendingDraws, unoCallerId, gamePhase, onCatchUno }) {
  const currentTurnPlayer = players.find((p) => p.id === currentTurnPlayerId);

  return (
    <div className="panel-card">
      <div className="panel-header">
        <h3>Người chơi</h3>
        <span>{deckCount} lá bài</span>
      </div>

      {currentTurnPlayer && gamePhase === "playing" && (
        <div className="turn-highlight-bar">
          Đến lượt: <strong>{currentTurnPlayer.name}</strong>
        </div>
      )}

      {currentColor && (
        <div className="current-color-bar">
          <span>Màu hiện tại:</span>
          <div
            className="current-color-dot"
            style={{ background: UNO_COLORS[currentColor]?.hex || "#ffffff" }}
          />
          <span>{UNO_COLORS[currentColor]?.label || currentColor}</span>
        </div>
      )}

      {direction && (
        <div className="direction-indicator">
          <span>{direction === 1 ? "Chiều thuận" : "Chiều ngược"}</span>
        </div>
      )}

      {pendingDraws > 0 && (
        <div className="pending-draws-badge">
          +{pendingDraws} lá bài chờ rút
        </div>
      )}

      <div className="player-list">
        {players.map((player) => {
          const canCatch = !player.isSelf
            && player.handCount <= 2
            && unoCallerId !== player.id
            && gamePhase === "playing";

          return (
            <div
              key={player.id}
              className={
                player.id === currentTurnPlayerId
                  ? "player-card active"
                  : "player-card"
              }
            >
              {player.id === currentTurnPlayerId && (
                <div className="active-turn-ring" />
              )}

              <div className="player-avatar" style={{ backgroundImage: "url(" + avatarFrameAsset + ")" }}>
                <img src={resolveAvatar(player.avatarKey)} alt={player.name} />
              </div>

              <div className="player-meta">
                <strong>
                  {player.name}
                  {player.isSelf ? " (Bạn)" : ""}
                </strong>
                <span>{player.handCount} lá bài</span>
                <span className="player-score">{player.score} điểm</span>
              </div>

              <div className="player-hand-mini">
                <img src={cardBackAsset} alt="card back" />
                <small>{player.handCount}</small>
                {unoCallerId === player.id && (
                  <span className="uno-badge">UNO!</span>
                )}
                {canCatch && (
                  <button
                    type="button"
                    className="catch-uno-button"
                    onClick={() => onCatchUno(player.id)}
                  >
                    Bắt UNO
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
