import { useState } from "react";
import { UNO_COLORS } from "../../../shared/constants/assets";

function formatCardLabel(card) {
  const kindMap = {
    number: card.label,
    skip: "Skip",
    reverse: "Đảo",
    "draw-two": "+2",
    wild: "Đổi màu",
    "wild-draw-four": "+4",
  };
  return kindMap[card.kind] || card.label;
}

function getCardAccentColor(card) {
  if (card.kind === "wild" || card.kind === "wild-draw-four") return "#ffffff";
  return UNO_COLORS[card.color]?.hex || "#ffffff";
}

export default function HandPanel({
  cards,
  disabled,
  playableCardIds,
  canAct,
  canCallUno,
  canChallenge,
  mustDraw,
  hasDrawn,
  pendingDraws,
  onPlayCard,
  onDrawCard,
  onEndTurn,
  onCallUno,
  onChallenge,
  onDragStart,
  onDragEnd,
  topDiscard,
}) {
  const [isCollapsed, setIsCollapsed] = useState(false);

  return (
    <section className={isCollapsed ? "hand-panel collapsed" : "hand-panel"}>
      <div className="panel-header">
        <div className="panel-header-left">
          <h3>Bài trên tay</h3>
          <span className="hand-count-badge">{cards.length} lá</span>
        </div>

        <div className="panel-actions">
          {topDiscard && (
            <div className="discard-preview-mini">
              <img src={topDiscard.assetPath} alt="top" />
            </div>
          )}

          {canChallenge ? (
            <>
              <button
                type="button"
                className="challenge-button"
                onClick={onChallenge}
              >
                Challenge +4
              </button>
              <button
                type="button"
                className="primary-button btn-sm hand-main-action"
                onClick={onDrawCard}
              >
                Rút {pendingDraws || 4} lá
              </button>
            </>
          ) : (
            <>
              <button
                type="button"
                className="primary-button btn-sm hand-main-action"
                onClick={onDrawCard}
                disabled={!canAct || hasDrawn}
              >
                {mustDraw
                  ? `Rút ${pendingDraws} lá`
                  : hasDrawn
                    ? "Đã rút"
                    : "Rút bài"}
              </button>

              <button
                type="button"
                className="secondary-button btn-sm"
                onClick={onEndTurn}
                disabled={!canAct || mustDraw}
              >
                Kết thúc
              </button>

              {canCallUno && (
                <button
                  type="button"
                  className="uno-button btn-sm"
                  onClick={onCallUno}
                >
                  UNO!
                </button>
              )}
            </>
          )}

          <button
            type="button"
            className="ghost-button hand-toggle-button"
            onClick={() => setIsCollapsed((prev) => !prev)}
          >
            {isCollapsed ? "Mở bài" : "Thu gọn"}
          </button>
        </div>
      </div>

      {!isCollapsed && (
        <div className="hand-scroll">
          {cards.map((card) => {
            const isPlayable = playableCardIds.includes(card.id);
            const accentColor = getCardAccentColor(card);

            return (
              <article
                key={card.id}
                className={
                  disabled
                    ? "hand-card disabled"
                    : isPlayable
                      ? "hand-card playable"
                      : "hand-card"
                }
                draggable={!disabled && isPlayable}
                onDragStart={(event) => {
                  event.dataTransfer.setData("text/plain", card.id);
                  onDragStart(card.id);
                }}
                onDragEnd={onDragEnd}
              >
                <div className="hand-card-image-wrap" style={{ "--accent": accentColor }}>
                  <img src={card.assetPath} alt={formatCardLabel(card)} />
                  {isPlayable && !disabled && (
                    <button
                      type="button"
                      className="hand-card-overlay-btn"
                      onClick={() => onPlayCard(card.id)}
                    >
                      Đánh
                    </button>
                  )}
                </div>
                <span className="hand-card-label" style={{ color: accentColor }}>
                  {formatCardLabel(card)}
                </span>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}
