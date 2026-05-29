import { useTexture } from "@react-three/drei";
import * as THREE from "three";
import { tableTextureAsset } from "../../../../shared/constants/assets";

export default function GameTable3D({ tableColor }) {
  const tableTexture = useTexture(tableTextureAsset);

  return (
    <group position={[0, 0, 0]}>
      {/* Table top surface */}
      <mesh position={[0, 0, 0]} receiveShadow castShadow>
        <cylinderGeometry args={[2.72, 2.72, 0.08, 32]} />
        <meshStandardMaterial
          map={tableTexture}
          color="#5d9b62"
          emissive="#173019"
          emissiveIntensity={0.08}
          roughness={0.82}
          metalness={0.05}
        />
      </mesh>

      <mesh position={[0, -0.08, 0]} castShadow receiveShadow>
        <cylinderGeometry args={[1.05, 1.22, 0.18, 24]} />
        <meshStandardMaterial color="#352318" roughness={0.82} />
      </mesh>

      {/* Table legs */}
      {[
        [1.8, -0.55, 1.8],
        [-1.8, -0.55, 1.8],
        [1.8, -0.55, -1.8],
        [-1.8, -0.55, -1.8],
      ].map((pos, i) => (
        <mesh key={i} position={pos} castShadow>
          <cylinderGeometry args={[0.08, 0.1, 0.9, 8]} />
          <meshStandardMaterial color="#2a1810" roughness={0.7} metalness={0.1} />
        </mesh>
      ))}

      {/* Center glow - card placement area */}
      <mesh position={[0, 0.06, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <circleGeometry args={[0.7, 32]} />
        <meshStandardMaterial
          color="#f3c66d"
          emissive="#f3c66d"
          emissiveIntensity={0.15}
          transparent
          opacity={0.1}
        />
      </mesh>

      {/* Deck position marker */}
      <mesh position={[-1.8, 0.06, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <planeGeometry args={[0.9, 1.3]} />
        <meshStandardMaterial
          color="#f8f0cc"
          transparent
          opacity={0.04}
          roughness={1}
        />
      </mesh>
    </group>
  );
}
