import { avatarOptions, avatarFrameAsset } from "../../../shared/constants/assets";

export default function GameHubScreen({ userProfile, onSelectGame, onLogout }) {
  const avatar = avatarOptions.find((a) => a.key === userProfile.avatarKey) ?? avatarOptions[0];

  const games = [
    {
      id: "uno",
      title: "UNO",
      desc: "Chơi UNO cùng bạn bè trong không gian 3D. Đánh bài, gọi UNO, và chiến thắng!",
      players: "2-4 người chơi",
      gradient: "linear-gradient(135deg, #e74c3c 0%, #f1c40f 30%, #2ecc71 60%, #3498db 100%)",
      icon: "🃏",
    },
    {
      id: "tien-len",
      title: "Tiến Lên Miền Nam",
      desc: "Game bài dân gian Việt Nam. Đánh combo, chặn heo, tiến lên!",
      players: "2-4 người chơi",
      gradient: "linear-gradient(135deg, #c0392b 0%, #f39c12 50%, #d4a017 100%)",
      icon: "🀄",
    },
  ];

  return (
    <main className="hub-shell">
      <div className="hub-bg-glow" />

      <header className="hub-header">
        <div className="hub-user-info">
          <div className="hub-user-avatar" style={{ backgroundImage: `url(${avatarFrameAsset})` }}>
            <img src={avatar.assetPath} alt={avatar.name} />
          </div>
          <div>
            <p className="hub-welcome">Xin chào,</p>
            <h2 className="hub-username">{userProfile.name}</h2>
          </div>
        </div>
        <div className="hub-header-actions">
          <button type="button" className="ghost-button" onClick={onLogout}>Đăng xuất</button>
        </div>
      </header>

      <section className="hub-content">
        <div className="hub-title-section">
          <p className="eyebrow">✦ Chọn trò chơi ✦</p>
          <h1 className="hub-main-title">Game Bài 3D</h1>
        </div>

        <div className="hub-grid">
          {games.map((game) => (
            <button
              key={game.id}
              type="button"
              className={`game-card ${game.id}`}
              onClick={() => onSelectGame(game.id)}
            >
              <div className="game-card-bg" style={{ background: game.gradient }} />
              <div className="game-card-content">
                <span className="game-card-icon">{game.icon}</span>
                <h3 className="game-card-title">{game.title}</h3>
                <p className="game-card-desc">{game.desc}</p>
                <div className="game-card-footer">
                  <span className="game-card-badge">{game.players}</span>
                  <span className="game-card-play">Chơi ngay →</span>
                </div>
              </div>
            </button>
          ))}
        </div>
      </section>
    </main>
  );
}
