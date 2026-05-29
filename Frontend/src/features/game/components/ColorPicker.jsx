import { UNO_COLORS } from "../../../shared/constants/assets";

export default function ColorPicker({ onSelect, onCancel }) {
  return (
    <div className="color-picker-overlay" onClick={onCancel}>
      <div className="color-picker-modal" onClick={(e) => e.stopPropagation()}>
        <h3>Chọn màu</h3>
        <p>Chọn màu cho lá bài Wild</p>
        <div className="color-grid">
          {Object.entries(UNO_COLORS).map(([key, { hex, label }]) => (
            <button
              key={key}
              type="button"
              className="color-swatch"
              style={{ background: hex }}
              onClick={() => onSelect(key)}
            >
              {label}
            </button>
          ))}
        </div>
        <button type="button" className="ghost-button color-cancel" onClick={onCancel}>
          Hủy
        </button>
      </div>
    </div>
  );
}
