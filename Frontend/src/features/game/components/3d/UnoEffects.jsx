import { useRef } from "react";
import { useFrame } from "@react-three/fiber";
import { Text } from "@react-three/drei";
import * as THREE from "three";

export function SkipEffect({ position = [0, 1.5, 0], onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      ref.current.rotation.z += delta * 3;
      const opacity = Math.max(0, 1 - elapsed.current / 1.5);
      ref.current.children.forEach((c) => {
        if (c.material) { c.material.opacity = opacity; }
      });
      if (elapsed.current > 1.5 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={position}>
      <mesh>
        <ringGeometry args={[0.3, 0.38, 32]} />
        <meshStandardMaterial color="#e74c3c" emissive="#e74c3c" emissiveIntensity={1.5} transparent opacity={1} side={THREE.DoubleSide} />
      </mesh>
    </group>
  );
}

export function ReverseEffect({ direction = 1, onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      ref.current.rotation.y += delta * 4 * direction;
      const scale = 1 + Math.sin(elapsed.current * 4) * 0.2;
      ref.current.scale.setScalar(scale);
      if (elapsed.current > 2 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={[0, 0.8, 0]}>
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[1.2, 0, 0]}>
        <coneGeometry args={[0.15, 0.35, 3]} />
        <meshStandardMaterial color="#3498db" emissive="#3498db" emissiveIntensity={1} transparent opacity={0.8} />
      </mesh>
    </group>
  );
}

export function DrawEffect({ count = 2, position = [0, 1.5, 0], onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      ref.current.position.y += delta * 0.5;
      const opacity = Math.max(0, 1 - elapsed.current / 1.5);
      ref.current.children.forEach((c) => { if (c.material) c.material.opacity = opacity; });
      if (elapsed.current > 1.5 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={position}>
      <Text fontSize={0.3} color="#ff6b6b" outlineWidth={0.015} outlineColor="#000" anchorX="center">
        +{count}
      </Text>
    </group>
  );
}

export function WildColorEffect({ color = "#ffffff", onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      const scale = 1 + elapsed.current * 2;
      ref.current.scale.setScalar(scale);
      const opacity = Math.max(0, 1 - elapsed.current / 1.2);
      ref.current.children.forEach((c) => { if (c.material) c.material.opacity = opacity; });
      if (elapsed.current > 1.2 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={[0, 0.3, 0]} rotation={[-Math.PI / 2, 0, 0]}>
      <mesh>
        <ringGeometry args={[0.3, 0.5, 32]} />
        <meshStandardMaterial color={color} emissive={color} emissiveIntensity={2} transparent opacity={1} side={THREE.DoubleSide} />
      </mesh>
    </group>
  );
}

export function UnoCallEffect({ position = [0, 2, 0], onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      const scale = 1 + Math.sin(elapsed.current * 6) * 0.3;
      ref.current.scale.setScalar(scale);
      if (elapsed.current > 2 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={position}>
      <Text fontSize={0.5} color="#e74c3c" outlineWidth={0.025} outlineColor="#fff" anchorX="center" fontWeight="bold">
        UNO!
      </Text>
    </group>
  );
}

export default function UnoEffects({ activeEffect }) {
  if (!activeEffect) return null;

  switch (activeEffect.type) {
    case "skip":
      return <SkipEffect position={activeEffect.position} onComplete={activeEffect.onComplete} />;
    case "reverse":
      return <ReverseEffect direction={activeEffect.direction} onComplete={activeEffect.onComplete} />;
    case "draw":
      return <DrawEffect count={activeEffect.count} position={activeEffect.position} onComplete={activeEffect.onComplete} />;
    case "wild":
      return <WildColorEffect color={activeEffect.color} onComplete={activeEffect.onComplete} />;
    case "uno-call":
      return <UnoCallEffect position={activeEffect.position} onComplete={activeEffect.onComplete} />;
    default:
      return null;
  }
}
