import { useMemo, useState } from "react";
import { avatarOptions, backgroundAsset, avatarFrameAsset } from "../../../shared/constants/assets";

export default function LoginScreen({ onLogin }) {
  const [playerName, setPlayerName] = useState("");
  const [avatarKey, setAvatarKey] = useState(avatarOptions[0].key);
  const [error, setError] = useState("");

  const selectedAvatar = useMemo(
    () => avatarOptions.find((a) => a.key === avatarKey) ?? avatarOptions[0],
    [avatarKey],
  );

  function handleSubmit(e) {
    e.preventDefault();
    if (!playerName.trim()) {
      setError("Vui lòng nhập tên người chơi");
      return;
    }
    onLogin({ name: playerName.trim(), avatarKey });
  }

  return (
    <main className="login-shell" style={{ backgroundImage: `url(${backgroundAsset})` }}>
      <div className="login-particles">
        {Array.from({ length: 20 }).map((_, i) => (
          <span key={i} className="particle" style={{
            left: `${Math.random() * 100}%`,
            animationDelay: `${Math.random() * 8}s`,
            animationDuration: `${6 + Math.random() * 8}s`,
          }} />
        ))}
      </div>

      <form className="login-panel" onSubmit={handleSubmit}>
        <div className="login-left">
          <div className="login-avatar-big">
            <div className="avatar-frame-big" style={{ backgroundImage: `url(${avatarFrameAsset})` }}>
              <img src={selectedAvatar.assetPath} alt={selectedAvatar.name} />
            </div>
            <p className="avatar-name-big">{selectedAvatar.name}</p>
          </div>
        </div>

        <div className="login-right">
          <p className="eyebrow">✦ AR Board Game ✦</p>
          <h1 className="login-title">Bàn Chơi<br /><span>3D</span></h1>
          <p className="login-subtitle">Trải nghiệm game bài đỉnh cao trong không gian 3D sống động</p>

          <div className="field-group">
            <label htmlFor="login-name">Tên của bạn</label>
            <input
              id="login-name"
              value={playerName}
              onChange={(e) => { setPlayerName(e.target.value); setError(""); }}
              placeholder="Nhập tên hiển thị..."
              maxLength={20}
              autoFocus
            />
          </div>

          <div className="login-avatar-section">
            <label>Chọn nhân vật</label>
            <div className="login-avatar-grid">
              {avatarOptions.map((avatar) => (
                <button
                  key={avatar.key}
                  type="button"
                  className={avatarKey === avatar.key ? "avatar-option active" : "avatar-option"}
                  onClick={() => setAvatarKey(avatar.key)}
                  title={avatar.name}
                >
                  <img src={avatar.assetPath} alt={avatar.name} />
                </button>
              ))}
            </div>
          </div>

          {error && <p className="inline-error">{error}</p>}

          <button type="submit" className="primary-button login-btn">
            Tiếp tục →
          </button>
        </div>
      </form>
    </main>
  );
}
