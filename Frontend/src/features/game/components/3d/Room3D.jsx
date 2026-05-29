import { useMemo } from "react";
import * as THREE from "three";
import { useTexture } from "@react-three/drei";

export default function Room3D() {
  const floorColor = useMemo(() => new THREE.Color("#3a2518"), []);
  const wallColor = useMemo(() => new THREE.Color("#2b2333"), []);
  const ceilingColor = useMemo(() => new THREE.Color("#1b1722"), []);

  return (
    <group>
      {/* Floor */}
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, -0.5, 0]} receiveShadow>
        <planeGeometry args={[14, 14]} />
        <meshStandardMaterial color={floorColor} roughness={0.92} metalness={0.05} />
      </mesh>

      {/* Ceiling */}
      <mesh rotation={[Math.PI / 2, 0, 0]} position={[0, 5, 0]}>
        <planeGeometry args={[14, 14]} />
        <meshStandardMaterial color={ceilingColor} roughness={1} />
      </mesh>

      {/* Back wall (North) */}
      <mesh position={[0, 2.25, -7]} receiveShadow>
        <planeGeometry args={[14, 5.5]} />
        <meshStandardMaterial color={wallColor} roughness={0.95} />
      </mesh>

      {/* Front wall (South) - behind player */}
      <mesh position={[0, 2.25, 7]} rotation={[0, Math.PI, 0]}>
        <planeGeometry args={[14, 5.5]} />
        <meshStandardMaterial color={wallColor} roughness={0.95} />
      </mesh>

      {/* Left wall */}
      <mesh position={[-7, 2.25, 0]} rotation={[0, Math.PI / 2, 0]}>
        <planeGeometry args={[14, 5.5]} />
        <meshStandardMaterial color={wallColor} roughness={0.95} />
      </mesh>

      {/* Right wall */}
      <mesh position={[7, 2.25, 0]} rotation={[0, -Math.PI / 2, 0]}>
        <planeGeometry args={[14, 5.5]} />
        <meshStandardMaterial color={wallColor} roughness={0.95} />
      </mesh>

      {/* Wall decorations - Picture frames */}
      <WallFrame position={[-2, 2.8, -6.95]} />
      <WallFrame position={[2, 2.8, -6.95]} />
      <WallFrame position={[-6.95, 2.8, -2]} rotation={[0, Math.PI / 2, 0]} />

      {/* Ceiling lamp */}
      <CeilingLamp position={[0, 4.5, 0]} />

      {/* Wall sconce lights */}
      <WallLight position={[-6.9, 3.2, 0]} />
      <WallLight position={[6.9, 3.2, 0]} />

      {/* Corner shelf */}
      <mesh position={[6.2, 1.5, -6.2]}>
        <boxGeometry args={[1.2, 0.06, 1.2]} />
        <meshStandardMaterial color="#3a2519" roughness={0.8} />
      </mesh>

      {/* Ambient fill */}
      <ambientLight intensity={0.4} color="#d6c3a5" />
    </group>
  );
}

function WallFrame({ position, rotation = [0, 0, 0] }) {
  return (
    <group position={position} rotation={rotation}>
      <mesh>
        <boxGeometry args={[1.2, 0.9, 0.04]} />
        <meshStandardMaterial color="#4a3828" roughness={0.7} metalness={0.1} />
      </mesh>
      <mesh position={[0, 0, 0.025]}>
        <planeGeometry args={[1.0, 0.7]} />
        <meshStandardMaterial color="#2a2035" roughness={0.9} />
      </mesh>
    </group>
  );
}

function CeilingLamp({ position }) {
  return (
    <group position={position}>
      {/* Lamp shade */}
      <mesh>
        <coneGeometry args={[0.8, 0.4, 8, 1, true]} />
        <meshStandardMaterial color="#3a3028" roughness={0.6} side={THREE.DoubleSide} />
      </mesh>
      {/* Lamp rod */}
      <mesh position={[0, 0.35, 0]}>
        <cylinderGeometry args={[0.02, 0.02, 0.7]} />
        <meshStandardMaterial color="#555" metalness={0.8} roughness={0.2} />
      </mesh>
      {/* Main overhead light */}
      <pointLight position={[0, -0.3, 0]} intensity={22} color="#f5deb3" distance={12} castShadow shadow-mapSize-width={1024} shadow-mapSize-height={1024} />
      <spotLight position={[0, -0.2, 0]} angle={0.9} penumbra={0.55} intensity={12} color="#fff0cf" distance={10} target-position={[0, -5, 0]} />
    </group>
  );
}

function WallLight({ position }) {
  return (
    <group position={position}>
      <mesh>
        <boxGeometry args={[0.15, 0.25, 0.1]} />
        <meshStandardMaterial color="#8B7355" roughness={0.4} metalness={0.3} emissive="#f5deb3" emissiveIntensity={0.3} />
      </mesh>
      <pointLight intensity={4.5} color="#f5deb3" distance={6} />
    </group>
  );
}
