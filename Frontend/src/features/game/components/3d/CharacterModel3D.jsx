import { useMemo, useRef } from "react";
import { useTexture, Text } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { avatarOptions } from "../../../../shared/constants/assets";

const BODY_COLORS = [
  "#c0392b", "#2980b9", "#27ae60", "#8e44ad",
  "#d35400", "#16a085", "#2c3e50", "#f39c12",
  "#1abc9c", "#e74c3c", "#3498db", "#9b59b6", "#e67e22",
];

function resolveAvatarPath(key) {
  return avatarOptions.find((a) => a.key === key)?.assetPath || avatarOptions[0].assetPath;
}

export default function CharacterModel3D({ player, seatPosition, isCurrentTurn, isPlayingCard }) {
  const avatarPath = useMemo(() => resolveAvatarPath(player.avatarKey), [player.avatarKey]);
  const avatarTex = useTexture(avatarPath);
  const bodyColor = useMemo(() => BODY_COLORS[player.seatIndex % BODY_COLORS.length], [player.seatIndex]);

  const groupRef = useRef();
  const headRef = useRef();
  const rightArmRef = useRef();
  const breathRef = useRef(0);

  useFrame((_, delta) => {
    breathRef.current += delta * 1.4;

    if (groupRef.current) {
      groupRef.current.position.y = (seatPosition.y || 0) + Math.sin(breathRef.current) * 0.008;
    }

    if (headRef.current && isCurrentTurn) {
      headRef.current.rotation.z = Math.sin(breathRef.current * 0.8) * 0.05;
    } else if (headRef.current) {
      headRef.current.rotation.z *= 0.94;
    }

    if (rightArmRef.current) {
      const targetRot = isPlayingCard ? -0.45 : 0.5;
      rightArmRef.current.rotation.x += (targetRot - rightArmRef.current.rotation.x) * delta * 4;
    }
  });

  const glowColor = isCurrentTurn ? "#f3c66d" : "transparent";
  const nameColor = isCurrentTurn ? "#f8f0cc" : "#8899aa";

  return (
    <group ref={groupRef} position={[seatPosition.x, seatPosition.y || 0, seatPosition.z]} rotation={[0, seatPosition.ry, 0]}>
      {isCurrentTurn && (
        <mesh position={[0, 0.01, 0]} rotation={[-Math.PI / 2, 0, 0]}>
          <circleGeometry args={[0.54, 32]} />
          <meshStandardMaterial color={glowColor} emissive={glowColor} emissiveIntensity={0.5} transparent opacity={0.22} />
        </mesh>
      )}

      <mesh position={[0, 0.2, -0.02]} castShadow receiveShadow>
        <boxGeometry args={[0.58, 0.08, 0.58]} />
        <meshStandardMaterial color="#52382b" roughness={0.86} />
      </mesh>
      <mesh position={[0, 0.56, -0.24]} castShadow receiveShadow>
        <boxGeometry args={[0.58, 0.72, 0.08]} />
        <meshStandardMaterial color="#4a3126" roughness={0.86} />
      </mesh>
      {[
        [-0.22, -0.02, 0.18],
        [0.22, -0.02, 0.18],
        [-0.22, -0.02, -0.16],
        [0.22, -0.02, -0.16],
      ].map((pos, index) => (
        <mesh key={index} position={pos} castShadow>
          <boxGeometry args={[0.06, 0.38, 0.06]} />
          <meshStandardMaterial color="#3b2419" roughness={0.88} />
        </mesh>
      ))}

      <mesh position={[0, 0.5, 0.03]} castShadow>
        <cylinderGeometry args={[0.17, 0.22, 0.44, 8]} />
        <meshStandardMaterial color={bodyColor} roughness={0.7} metalness={0.05} />
      </mesh>
      <mesh position={[0, 0.29, 0.05]} castShadow>
        <boxGeometry args={[0.34, 0.14, 0.28]} />
        <meshStandardMaterial color={bodyColor} roughness={0.72} />
      </mesh>
      <mesh position={[0, 0.73, 0.02]}>
        <cylinderGeometry args={[0.06, 0.08, 0.1, 8]} />
        <meshStandardMaterial color="#e8c9a0" roughness={0.8} />
      </mesh>

      <group ref={headRef} position={[0, 0.94, 0.02]}>
        <mesh castShadow>
          <sphereGeometry args={[0.2, 16, 16]} />
          <meshStandardMaterial color="#e8c9a0" roughness={0.8} />
        </mesh>
        <mesh position={[0, 0, 0.205]}>
          <circleGeometry args={[0.16, 24]} />
          <meshBasicMaterial map={avatarTex} transparent />
        </mesh>
        <mesh position={[0, 0.1, -0.02]}>
          <sphereGeometry args={[0.21, 16, 8, 0, Math.PI * 2, 0, Math.PI * 0.5]} />
          <meshStandardMaterial color={bodyColor} roughness={0.6} />
        </mesh>
        {isCurrentTurn && (
          <pointLight position={[0, 0.3, 0.2]} intensity={2.5} color="#f3c66d" distance={2} />
        )}
      </group>

      <mesh position={[-0.28, 0.5, 0.02]} rotation={[0.45, 0, 0.32]}>
        <cylinderGeometry args={[0.05, 0.04, 0.4, 6]} />
        <meshStandardMaterial color={bodyColor} roughness={0.7} />
      </mesh>
      <mesh ref={rightArmRef} position={[0.28, 0.5, 0.02]} rotation={[0.5, 0, -0.32]}>
        <cylinderGeometry args={[0.05, 0.04, 0.4, 6]} />
        <meshStandardMaterial color={bodyColor} roughness={0.7} />
      </mesh>
      <mesh position={[-0.1, 0.12, 0.14]} rotation={[Math.PI / 2, 0, 0]} castShadow>
        <cylinderGeometry args={[0.05, 0.05, 0.34, 6]} />
        <meshStandardMaterial color="#1f2937" roughness={0.82} />
      </mesh>
      <mesh position={[0.1, 0.12, 0.14]} rotation={[Math.PI / 2, 0, 0]} castShadow>
        <cylinderGeometry args={[0.05, 0.05, 0.34, 6]} />
        <meshStandardMaterial color="#1f2937" roughness={0.82} />
      </mesh>

      <Text position={[0, 1.26, 0]} fontSize={0.12} color={nameColor} anchorX="center" anchorY="middle" outlineWidth={0.008} outlineColor="#000">
        {player.name}
      </Text>
      <Text position={[0, 0.12, 0]} rotation={[-Math.PI / 2, 0, 0]} fontSize={0.1} color="#6688aa" anchorX="center">
        {player.isSelf ? "" : `${player.handCount} lÃ¡`}
      </Text>
      {player.isBot && (
        <Text position={[0, 1.4, 0]} fontSize={0.08} color="#5fb4ff" anchorX="center">
          BOT
        </Text>
      )}
    </group>
  );
}
