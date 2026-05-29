import { useMemo, useState } from "react";
import { gameSocket } from "../../../core/socket/gameSocket";
import { avatarOptions, backgroundAsset, avatarFrameAsset } from "../../../shared/constants/assets";

const TIEN_LEN_RULES = [
  "Mỗi người 13 lá bài (4 người chơi)",
  "Thứ tự: 3 < 4 < ... < K < A < 2",
  "Chất: ♠ < ♣ < ♦ < ♥",
  "Combo: Đơn, Đôi, Sảnh, Tứ quý, Sảnh đôi",
  "Tứ quý / 3 đôi thông chặn được 2",
  "Người có 3♠ đi trước (ván đầu)",
  "Hết bài đầu tiên = Thắng!",
];

const UNO_RULES = [
  "Khởi đầu mỗi người 7 lá bài",
  "Đánh bài trùng màu hoặc trùng số",
  "Lá Skip: Bỏ lượt người tiếp theo",
  "Lá Reverse: Đổi chiều đánh",
  "Lá +2: Người tiếp rút 2 lá",
  "Lá Wild: Chọn màu bất kỳ",
  "Lá Wild +4: Chọn màu + người tiếp rút 4",
  'Gọi "UNO!" khi còn 1 lá',
];

export default function LobbyScreen({ gameType, userProfile, onJoin, onBack }) {
  const [roomCode, setRoomCode] = useState("ROOM01");
  const [errorMessage, setErrorMessage] = useState("");
  const [isJoining, setIsJoining] = useState(false);

  const playerName = userProfile?.name || "";
  const avatarKey = userProfile?.avatarKey || avatarOptions[0].key;

  const selectedAvatar = useMemo(
    () => avatarOptions.find((a) => a.key === avatarKey) ?? avatarOptions[0],
    [avatarKey],
  );

  const isUno = gameType === "uno";
  const rules = isUno ? UNO_RULES : TIEN_LEN_RULES;
  const gameTitle = isUno ? "UNO" : "Tiến Lên Miền Nam";
  const gameIcon = isUno ? "🃏" : "🀄";
  const joinEvent = isUno ? "room:join" : "tien-len:join";

  function handleJoin() {
    if (!roomCode.trim()) {
      setErrorMessage("Nhập mã phòng.");
      return;
    }

    setIsJoining(true);
    setErrorMessage("");

    gameSocket.emit(joinEvent, { roomCode: roomCode.trim().toUpperCase(), playerName, avatarKey }, (response) => {
      setIsJoining(false);
      if (!response?.ok) {
        setErrorMessage(response?.error?.message || "Không thể vào phòng.");
        return;
      }
      onJoin({
        playerName,
        roomCode: roomCode.trim().toUpperCase(),
        avatarKey,
        gameType,
        initialState: response.state ?? null,
      });
    });
  }

  function handleAddBot() {
    gameSocket.emit("room:add-bot", { roomCode: roomCode.trim().toUpperCase(), gameType }, (response) => {
      if (!response?.ok) {
        setErrorMessage(response?.error?.message || "Không thể thêm bot.");
      }
    });
  }

  return (
    <main className="lobby-shell" style={{
      backgroundImage: `linear-gradient(rgba(9, 14, 24, 0.65), rgba(9, 14, 24, 0.88)), url(${backgroundAsset})`,
    }}>
      <section className="lobby-panel">
        <div className="lobby-copy">
          <button type="button" className="ghost-button lobby-back-btn" onClick={onBack}>← Quay lại</button>
          <p className="eyebrow">{gameIcon} AR Card Game</p>
          <h1>{gameTitle}</h1>
          <p className="lead">
            {isUno
              ? "Chơi UNO cùng bạn bè trong không gian 3D. Đánh bài, gọi UNO, và chiến thắng!"
              : "Game bài dân gian Việt Nam. Đánh combo, chặn heo, tiến lên!"}
          </p>
          <div className="lobby-rules">
            <h4>Luật chơi {gameTitle}:</h4>
            <ul>
              {rules.map((rule, i) => <li key={i}>{rule}</li>)}
            </ul>
          </div>
        </div>

        <div className="lobby-grid">
          <div className="avatar-preview">
            <div className="avatar-frame" style={{ backgroundImage: `url(${avatarFrameAsset})` }}>
              <img src={selectedAvatar.assetPath} alt={selectedAvatar.name} />
            </div>
            <div>
              <strong>{playerName}</strong>
              <p style={{ margin: 0, fontSize: "0.82rem", color: "rgba(244,247,251,0.6)" }}>{selectedAvatar.name}</p>
            </div>
          </div>

          <div className="field-group">
            <label htmlFor="room-code">Mã phòng</label>
            <input
              id="room-code"
              value={roomCode}
              onChange={(e) => setRoomCode(e.target.value.toUpperCase())}
              placeholder="ROOM01"
              maxLength={12}
            />
          </div>

          {errorMessage && <p className="inline-error">{errorMessage}</p>}

          <button type="button" className="primary-button" onClick={handleJoin} disabled={isJoining}>
            {isJoining ? "Đang vào..." : "Vào phòng"}
          </button>

          <button type="button" className="secondary-button" onClick={handleAddBot}>
            🤖 Thêm Bot
          </button>
        </div>
      </section>
    </main>
  );
}
