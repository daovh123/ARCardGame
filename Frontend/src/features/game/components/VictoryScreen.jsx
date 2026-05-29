import { avatarOptions } from "../../../shared/constants/assets";

function resolveAvatar(avatarKey) {
  return avatarOptions.find((a) => a.key === avatarKey)?.assetPath || avatarOptions[0].assetPath;
}

export default function VictoryScreen({ roomState, onRestart, onLeave }) {
  const winner = roomState?.players?.find((p) => p.id === roomState.winnerId);
  if (!winner) return null;

  return (
    <div className="victory-overlay">
      <div className="victory-modal">
        <div className="victory-crown">&#x1F3C6;</div>
        <h2>Chiến thắng!</h2>
        <div className="victory-avatar">
          <img src={resolveAvatar(winner.avatarKey)} alt={winner.name} />
        </div>
        <h3>{winner.name}</h3>
        <p className="victory-score">Điểm: {winner.score}</p>

        <div className="victory-players">
          {roomState.players.map((player) => (
            <div key={player.id} className="victory-player-row">
              <img src={resolveAvatar(player.avatarKey)} alt={player.name} className="victory-mini-avatar" />
              <span>{player.name}</span>
              <span className="victory-player-score">{player.score} pts</span>
            </div>
          ))}
        </div>

        <div className="victory-actions">
          <button type="button" className="primary-button" onClick={onRestart}>
            Chơi lại
          </button>
          <button type="button" className="ghost-button" onClick={onLeave}>
            Thoát
          </button>
        </div>
      </div>
    </div>
  );
}
