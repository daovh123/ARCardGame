const COMBO_NAMES = {
  single: "Lá đơn",
  pair: "Đôi",
  triple: "Ba",
  quad: "Tứ quý",
  straight: "Sảnh",
  doubleStraight: "Sảnh đôi",
  invalid: "Không hợp lệ",
};

const COMBO_COLORS = {
  single: "#8899aa",
  pair: "#3498db",
  triple: "#9b59b6",
  quad: "#f39c12",
  straight: "#2ecc71",
  doubleStraight: "#e74c3c",
  invalid: "#e74c3c",
};

export default function ComboIndicator({ comboInfo }) {
  if (!comboInfo) return null;

  const name = COMBO_NAMES[comboInfo.type] || comboInfo.type;
  const color = COMBO_COLORS[comboInfo.type] || "#8899aa";
  const isValid = comboInfo.valid !== false;

  return (
    <div className={`combo-badge ${isValid ? "valid" : "invalid"}`} style={{ borderColor: color, color }}>
      <span className="combo-dot" style={{ background: color }} />
      {name}
    </div>
  );
}
