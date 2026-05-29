import { SUIT_SYMBOLS, SUIT_COLORS } from "../../../shared/constants/assets";
import ComboIndicator from "./ComboIndicator";

export default function TienLenHandPanel({ cards, disabled, selectedCardIds, comboInfo, onToggleCard, onPlayCards, onPass, canPass }) {
  return (
    <div className="hand-panel tien-len-hand">
      <div className="panel-header">
        <h3>Bài của bạn</h3>
        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
          {comboInfo && <ComboIndicator comboInfo={comboInfo} />}
          <span className="hand-count-badge">{cards.length} lá</span>
        </div>
      </div>

      <div className="hand-scroll">
        {cards.map((card) => {
          const isSelected = selectedCardIds.has(card.id);
          const suitSymbol = SUIT_SYMBOLS[card.suit] || "";
          const suitColor = SUIT_COLORS[card.suit] || "#fff";

          return (
            <div
              key={card.id}
              className={`hand-card tl-card ${isSelected ? "selected" : ""} ${disabled ? "disabled" : "playable"}`}
              onClick={() => !disabled && onToggleCard(card.id)}
            >
              <div className="hand-card-image-wrap" style={{ "--accent": suitColor }}>
                <img src={card.assetPath} alt={`${card.label}${suitSymbol}`} />
              </div>
              <span className="hand-card-label" style={{ color: suitColor }}>
                {card.label} {suitSymbol}
              </span>
            </div>
          );
        })}
      </div>

      <div className="tl-action-bar">
        <button
          type="button"
          className="tl-play-btn primary-button"
          onClick={onPlayCards}
          disabled={disabled || selectedCardIds.size === 0 || (comboInfo && !comboInfo.valid)}
        >
          Đánh ({selectedCardIds.size})
        </button>
        <button
          type="button"
          className="tl-pass-btn secondary-button"
          onClick={onPass}
          disabled={disabled || !canPass}
        >
          Bỏ lượt
        </button>
      </div>
    </div>
  );
}
