import { useRef, useMemo } from "react";
import { useFrame } from "@react-three/fiber";
import { Text } from "@react-three/drei";
import * as THREE from "three";

export function QuadEffect({ position = [0, 1.2, 0], onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  const particles = useMemo(() =>
    Array.from({ length: 16 }).map(() => ({
      dir: new THREE.Vector3((Math.random() - 0.5) * 2, Math.random() * 2, (Math.random() - 0.5) * 2).normalize(),
      speed: 1 + Math.random() * 2,
    })), []);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      ref.current.children.forEach((child, i) => {
        if (child.isMesh && particles[i]) {
          child.position.addScaledVector(particles[i].dir, delta * particles[i].speed);
          child.material.opacity = Math.max(0, 1 - elapsed.current / 1.5);
          child.scale.multiplyScalar(0.98);
        }
      });
      if (elapsed.current > 1.5 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={position}>
      {particles.map((_, i) => (
        <mesh key={i}>
          <sphereGeometry args={[0.04, 6, 6]} />
          <meshStandardMaterial color="#f3c66d" emissive="#f3c66d" emissiveIntensity={2} transparent opacity={1} />
        </mesh>
      ))}
      <Text fontSize={0.35} color="#f3c66d" outlineWidth={0.02} outlineColor="#000" anchorX="center">
        TỨ QUÝ!
      </Text>
    </group>
  );
}

export function ChopTwoEffect({ position = [0, 1, 0], onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      const opacity = Math.max(0, 1 - elapsed.current / 2);
      ref.current.children.forEach((c) => { if (c.material) c.material.opacity = opacity; });
      if (elapsed.current > 2 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={position}>
      <Text fontSize={0.3} color="#00bfff" outlineWidth={0.015} outlineColor="#000" anchorX="center">
        ⚡ CHẶN HEO! ⚡
      </Text>
      <pointLight intensity={5} color="#00bfff" distance={3} />
    </group>
  );
}

export function VictoryEffect({ position = [0, 2.5, 0], onComplete }) {
  const ref = useRef();
  const elapsed = useRef(0);

  const confetti = useMemo(() =>
    Array.from({ length: 30 }).map(() => ({
      pos: new THREE.Vector3((Math.random() - 0.5) * 4, Math.random() * 3, (Math.random() - 0.5) * 4),
      color: ["#e74c3c", "#3498db", "#f1c40f", "#2ecc71", "#9b59b6"][Math.floor(Math.random() * 5)],
      speed: 0.5 + Math.random(),
      rotSpeed: Math.random() * 5,
    })), []);

  useFrame((_, delta) => {
    elapsed.current += delta;
    if (ref.current) {
      ref.current.children.forEach((child, i) => {
        if (child.isMesh && confetti[i]) {
          child.position.y -= delta * confetti[i].speed;
          child.rotation.x += delta * confetti[i].rotSpeed;
          child.rotation.z += delta * confetti[i].rotSpeed * 0.5;
        }
      });
      if (elapsed.current > 4 && onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={position}>
      {confetti.map((c, i) => (
        <mesh key={i} position={c.pos}>
          <planeGeometry args={[0.06, 0.1]} />
          <meshStandardMaterial color={c.color} emissive={c.color} emissiveIntensity={0.5} side={THREE.DoubleSide} />
        </mesh>
      ))}
    </group>
  );
}

export default function TienLenEffects({ activeEffect }) {
  if (!activeEffect) return null;

  switch (activeEffect.type) {
    case "quad":
      return <QuadEffect position={activeEffect.position} onComplete={activeEffect.onComplete} />;
    case "chop":
      return <ChopTwoEffect position={activeEffect.position} onComplete={activeEffect.onComplete} />;
    case "victory":
      return <VictoryEffect position={activeEffect.position} onComplete={activeEffect.onComplete} />;
    default:
      return null;
  }
}
