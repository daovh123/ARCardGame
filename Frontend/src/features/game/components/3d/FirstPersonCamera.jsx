import { useRef, useCallback } from "react";
import { useThree, useFrame } from "@react-three/fiber";
import * as THREE from "three";

const TARGET = new THREE.Vector3(0, 0.5, 0);
const CAMERA_POS = new THREE.Vector3(0, 1.8, 4.2);
const LERP_SPEED = 5;
const MAX_YAW = Math.PI * 0.45;
const MIN_PITCH = -0.4;
const MAX_PITCH = 0.25;

export default function FirstPersonCamera() {
  const { camera } = useThree();
  const rotState = useRef({ yaw: 0, pitch: 0, targetYaw: 0, targetPitch: 0 });
  const isDragging = useRef(false);
  const lastPointer = useRef({ x: 0, y: 0 });

  const handlePointerDown = useCallback((e) => {
    isDragging.current = true;
    lastPointer.current = { x: e.clientX || e.touches?.[0]?.clientX || 0, y: e.clientY || e.touches?.[0]?.clientY || 0 };
  }, []);

  const handlePointerMove = useCallback((e) => {
    if (!isDragging.current) return;
    const x = e.clientX || e.touches?.[0]?.clientX || 0;
    const y = e.clientY || e.touches?.[0]?.clientY || 0;
    const dx = (x - lastPointer.current.x) * 0.003;
    const dy = (y - lastPointer.current.y) * 0.003;

    rotState.current.targetYaw = THREE.MathUtils.clamp(rotState.current.targetYaw - dx, -MAX_YAW, MAX_YAW);
    rotState.current.targetPitch = THREE.MathUtils.clamp(rotState.current.targetPitch - dy, MIN_PITCH, MAX_PITCH);

    lastPointer.current = { x, y };
  }, []);

  const handlePointerUp = useCallback(() => {
    isDragging.current = false;
  }, []);

  useFrame((_, delta) => {
    const st = rotState.current;
    st.yaw += (st.targetYaw - st.yaw) * Math.min(delta * LERP_SPEED, 1);
    st.pitch += (st.targetPitch - st.pitch) * Math.min(delta * LERP_SPEED, 1);

    camera.position.copy(CAMERA_POS);
    const lookTarget = TARGET.clone();
    lookTarget.x += Math.sin(st.yaw) * 5;
    lookTarget.y += st.pitch * 3;

    camera.lookAt(lookTarget);
  });

  return (
    <mesh
      position={[0, 2.5, 0]}
      visible={false}
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerUp}
      onPointerLeave={handlePointerUp}
    >
      <sphereGeometry args={[12, 8, 8]} />
      <meshBasicMaterial side={THREE.BackSide} transparent opacity={0} />
    </mesh>
  );
}
