import { useRef } from "react";
import { useFrame } from "@react-three/fiber";
import * as THREE from "three";
import PlayingCard3D from "./PlayingCard3D";

export function AnimatedCard({
  frontTexturePath,
  startPos,
  endPos,
  startRot,
  endRot,
  duration = 0.5,
  delay = 0,
  arcHeight = 0,
  onComplete,
}) {
  const ref = useRef();
  const elapsed = useRef(0);
  const finished = useRef(false);

  useFrame((_, delta) => {
    if (finished.current) return;
    
    elapsed.current += delta;
    if (elapsed.current < delay) return;

    const progress = Math.min((elapsed.current - delay) / duration, 1);
    
    // Smooth ease-out cubic interpolation
    const t = 1 - Math.pow(1 - progress, 3);
    
    if (ref.current) {
      // Interpolate position
      ref.current.position.lerpVectors(
        new THREE.Vector3(...startPos),
        new THREE.Vector3(...endPos),
        t
      );
      if (arcHeight > 0) {
        ref.current.position.y += Math.sin(progress * Math.PI) * arcHeight;
      }
      
      // Interpolate rotation
      const sRot = new THREE.Euler(...startRot);
      const eRot = new THREE.Euler(...endRot);
      const qStart = new THREE.Quaternion().setFromEuler(sRot);
      const qEnd = new THREE.Quaternion().setFromEuler(eRot);
      ref.current.quaternion.slerpQuaternions(qStart, qEnd, t);
    }
    
    if (progress >= 1 && !finished.current) {
      finished.current = true;
      if (onComplete) onComplete();
    }
  });

  return (
    <group ref={ref} position={startPos}>
      <PlayingCard3D frontTexturePath={frontTexturePath} />
    </group>
  );
}
