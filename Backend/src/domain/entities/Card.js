function createCard({ id, family, color, label, points, assetPath, kind }) {
  return {
    id,
    family,
    color,
    label,
    points,
    assetPath,
    kind,
  };
}

module.exports = { createCard };
