import { useMemo } from "react";
import { useTexture, RoundedBox, Text } from "@react-three/drei";
import { avatarOptions, avatarFrameAsset, cardBackAsset, UNO_COLORS } from "../../../shared/constants/assets";

function resolveAvatarPath(avatarKey) {
  return avatarOptions.find((a) => a.key === avatarKey)?.assetPath || avatarOptions[0].assetPath;
}

function PlayerHandPreview({ handCount, isCurrentTurn }) {
  const backTex = useTexture(cardBackAsset);
  const cards = useMemo(() => {
    const arr = [];
    const count = Math.min(handCount, 5);
    for (let i = 0; i < count; i++) {
      arr.push({ offset: (i - (count - 1) / 2) * 0.14, z: i * 0.01 });
    }
    return arr;
  }, [handCount]);

  return (
    <group position={[0, 0.02, 0.35]} rotation={[-Math.PI / 2, 0, 0]}>
      {cards.map((c, i) => (
        <mesh key={i} position={[c.offset, c.z, 0]}>
          <planeGeometry args={[0.18, 0.28]} />
          <meshStandardMaterial map={backTex} color="#ffffff" transparent opacity={0.9} />
        </mesh>
      ))}
      {handCount > 5 && (
        <Text position={[0.55, 0.01, 0]} fontSize={0.1} color="#f8f0cc">
          +{handCount - 5}
        </Text>
      )}
    </group>
  );
}

export default function PlayerAvatar3D({ player, position, isCurrentTurn, roomState }) {
  const avatarPath = useMemo(() => resolveAvatarPath(player.avatarKey), [player.avatarKey]);
  const avatarTex = useTexture(avatarPath);
  const frameTex = useTexture(avatarFrameAsset);

  const glowColor = isCurrentTurn ? "#f3c66d" : "#2a3a52";
  const nameColor = isCurrentTurn ? "#f8f0cc" : "#8899aa";

  return (
    <group position={[position.x, 0, position.z]} rotation={[0, position.ry, 0]}>
      <mesh position={[0, 0.01, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <circleGeometry args={[0.55, 32]} />
        <meshStandardMaterial
          color={glowColor}
          transparent
          opacity={isCurrentTurn ? 0.25 : 0.08}
        />
      </mesh>

      <group position={[0, 0.85, 0]}>
        <mesh>
          <planeGeometry args={[1.0, 1.0]} />
          <meshBasicMaterial map={frameTex} transparent />
        </mesh>
        <mesh position={[0, 0, 0.01]}>
          <planeGeometry args={[0.72, 0.72]} />
          <meshBasicMaterial map={avatarTex} transparent />
        </mesh>

        {isCurrentTurn && (
          <pointLight position={[0, 0, 0.5]} intensity={2} color="#f3c66d" distance={2} />
        )}
      </group>

      <Text
        position={[0, 0.22, 0]}
        rotation={[-Math.PI / 2, 0, 0]}
        fontSize={0.14}
        color={nameColor}
        anchorX="center"
        anchorY="middle"
      >
        {player.name}
      </Text>

      <Text
        position={[0, 0.12, 0]}
        rotation={[-Math.PI / 2, 0, 0]}
        fontSize={0.1}
        color="#6688aa"
        anchorX="center"
      >
        {player.isSelf ? "You" : `${player.handCount} cards`}
      </Text>

      {roomState?.currentColor && isCurrentTurn && (
        <mesh position={[0, 0.05, -0.4]} rotation={[-Math.PI / 2, 0, 0]}>
          <circleGeometry args={[0.12, 16]} />
          <meshStandardMaterial color={UNO_COLORS[roomState.currentColor]?.hex || "#ffffff"} emissive={UNO_COLORS[roomState.currentColor]?.hex || "#ffffff"} emissiveIntensity={0.5} />
        </mesh>
      )}

      <PlayerHandPreview handCount={player.handCount} isCurrentTurn={isCurrentTurn} />
    </group>
  );
}
