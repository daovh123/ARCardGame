import PlayingCard3D from "./3d/PlayingCard3D";

export default function TableCardMesh({ card, glowing = false }) {
  return (
    <group
      position={[card.placement?.x || 0, card.placement?.y || 0.04, card.placement?.z || 0]}
      rotation={[0, card.placement?.rotation || 0, 0]}
    >
      <PlayingCard3D frontTexturePath={card.assetPath} glowing={glowing} />
    </group>
  );
}
