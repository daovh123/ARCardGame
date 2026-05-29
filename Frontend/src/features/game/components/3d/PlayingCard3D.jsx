import { useEffect } from "react";
import { useTexture } from "@react-three/drei";
import * as THREE from "three";
import { cardBackAsset } from "../../../../shared/constants/assets";

const CARD_WIDTH = 0.86;
const CARD_HEIGHT = 1.26;
const CARD_THICKNESS = 0.014;

export default function PlayingCard3D({ frontTexturePath, glowing = false }) {
  const frontTexture = useTexture(frontTexturePath);
  const backTexture = useTexture(cardBackAsset);
  const faceOffset = CARD_THICKNESS / 2 + 0.004;

  useEffect(() => {
    [frontTexture, backTexture].forEach((texture) => {
      if (!texture) return;
      texture.colorSpace = THREE.SRGBColorSpace;
      texture.anisotropy = 8;
      texture.needsUpdate = true;
    });
  }, [frontTexture, backTexture]);

  return (
    <group>
      <mesh castShadow receiveShadow>
        <boxGeometry args={[CARD_WIDTH, CARD_THICKNESS, CARD_HEIGHT]} />
        <meshStandardMaterial
          color={glowing ? "#fff4cf" : "#d9e0ea"}
          roughness={0.9}
          metalness={0.02}
          emissive={glowing ? "#f3c66d" : "#000000"}
          emissiveIntensity={glowing ? 0.12 : 0}
        />
      </mesh>

      <mesh position={[0, faceOffset, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <planeGeometry args={[CARD_WIDTH * 0.95, CARD_HEIGHT * 0.95]} />
        <meshBasicMaterial
          map={frontTexture}
          color="#ffffff"
          side={THREE.DoubleSide}
          transparent
          alphaTest={0.05}
          toneMapped={false}
          polygonOffset
          polygonOffsetFactor={-4}
        />
      </mesh>

      <mesh position={[0, -faceOffset, 0]} rotation={[Math.PI / 2, 0, Math.PI]}>
        <planeGeometry args={[CARD_WIDTH * 0.95, CARD_HEIGHT * 0.95]} />
        <meshBasicMaterial
          map={backTexture}
          color="#ffffff"
          side={THREE.DoubleSide}
          transparent
          alphaTest={0.05}
          toneMapped={false}
          polygonOffset
          polygonOffsetFactor={-4}
        />
      </mesh>
    </group>
  );
}
