import { Canvas } from "@react-three/fiber";
import { Float, Text, useTexture } from "@react-three/drei";
import { useMemo, useState, useEffect } from "react";
import * as THREE from "three";
import Room3D from "./3d/Room3D";
import GameTable3D from "./3d/GameTable3D";
import FirstPersonCamera from "./3d/FirstPersonCamera";
import CharacterModel3D from "./3d/CharacterModel3D";
import UnoEffects from "./3d/UnoEffects";
import TienLenEffects from "./3d/TienLenEffects";
import { cardBackAsset, UNO_COLORS } from "../../../shared/constants/assets";
import TableCardMesh from "./TableCardMesh";
import { AnimatedCard } from "./3d/CardAnimationSystem";
import PlayingCard3D from "./3d/PlayingCard3D";

const SEAT_POSITIONS_3D = [
  { x: 0, y: 0, z: -2.6, ry: 0 },
  { x: -2.8, y: 0, z: 0, ry: Math.PI / 2 },
  { x: 2.8, y: 0, z: 0, ry: -Math.PI / 2 },
  { x: 0, y: 0, z: 2.6, ry: Math.PI },
];
const DECK_POSITION = [-1.8, 0.12, 0];
const DISCARD_POSITION = [0.38, 0.03, 0];
const SELF_DRAW_TARGET = [0.15, 0.55, 2.05];

function layoutTableCards(cards) {
  const maxColumns = 5;
  const rowCount = Math.ceil(cards.length / maxColumns);

  return cards.map((card, index) => {
    const row = Math.floor(index / maxColumns);
    const column = index % maxColumns;
    const cardsInRow = Math.min(maxColumns, cards.length - row * maxColumns);
    const centeredColumn = column - (cardsInRow - 1) / 2;
    const centeredRow = row - (rowCount - 1) / 2;

    return {
      ...card,
      placement: {
        ...card.placement,
        x: Number((centeredColumn * 0.42).toFixed(2)),
        y: Number((0.03 + index * 0.006).toFixed(3)),
        z: Number((centeredRow * 0.34).toFixed(2)),
        rotation: Number((((column % 2) - 0.5) * 0.08).toFixed(2)),
      },
    };
  });
}

function DeckStack3D({ count }) {
  const backTexture = useTexture(cardBackAsset);
  const layers = Math.min(Math.max(Math.ceil(count / 14), 3), 7);

  return (
    <group position={[-1.8, 0.1, 0]}>
      <mesh position={[0.04, -0.01, 0.08]} rotation={[-Math.PI / 2, 0, 0]}>
        <planeGeometry args={[0.92, 1.32]} />
        <meshStandardMaterial color="#0f1420" transparent opacity={0.28} />
      </mesh>

      <mesh position={[0, 0.015, 0]}>
        <boxGeometry args={[0.9, 0.04, 1.3]} />
        <meshStandardMaterial color="#f0eee8" roughness={0.92} metalness={0.01} />
      </mesh>

      {Array.from({ length: layers }).map((_, i) => (
        <group
          key={i}
          position={[
            ((i % 2) - 0.5) * 0.018,
            0.022 + i * 0.009,
            ((i % 3) - 1) * 0.012,
          ]}
          rotation={[0, (i - (layers - 1) / 2) * 0.02, 0]}
        >
          <PlayingCard3D frontTexturePath={cardBackAsset} />
        </group>
      ))}

      <mesh position={[0, 0.022 + layers * 0.009 + 0.002, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <planeGeometry args={[0.78, 1.16]} />
        <meshBasicMaterial map={backTexture} toneMapped={false} transparent alphaTest={0.05} />
      </mesh>

      <group position={[0.3, 0.12 + layers * 0.009, -0.38]}>
        <mesh>
          <cylinderGeometry args={[0.16, 0.16, 0.05, 24]} />
          <meshStandardMaterial color="#101722" metalness={0.08} roughness={0.7} />
        </mesh>
        <Text position={[0, 0.03, 0]} rotation={[-Math.PI / 2, 0, 0]} fontSize={0.12} color="#f8f0cc" anchorX="center" anchorY="middle">
          {count}
        </Text>
      </group>
    </group>
  );
}

function DiscardPile3D({ topCard, currentColor }) {
  const glowColor = UNO_COLORS[currentColor]?.hex || "#ffffff";

  return (
    <group position={[DISCARD_POSITION[0], 0.12, DISCARD_POSITION[2]]}>
      {currentColor && (
        <mesh position={[0, 0.005, 0]} rotation={[-Math.PI / 2, 0, 0]}>
          <ringGeometry args={[0.42, 0.5, 48]} />
          <meshStandardMaterial color={glowColor} emissive={glowColor} emissiveIntensity={0.8} transparent opacity={0.5} side={THREE.DoubleSide} />
        </mesh>
      )}
      {topCard && (
        <TableCardMesh
          key={topCard.id}
          card={{ ...topCard, placement: { x: 0, y: 0.02, z: 0, rotation: 0.08 } }}
          glowing
        />
      )}
    </group>
  );
}

function DirectionArrow3D({ direction }) {
  return (
    <group position={[1.8, 0.12, 0]}>
      <mesh rotation={[-Math.PI / 2, 0, direction === 1 ? 0 : Math.PI]}>
        <coneGeometry args={[0.12, 0.28, 3]} />
        <meshStandardMaterial color="#f3c66d" emissive="#f3c66d" emissiveIntensity={0.5} roughness={0.3} />
      </mesh>
    </group>
  );
}

export default function TableScene({ roomState, roomCode, gameType }) {
  const players = roomState?.players || [];
  const [activeEffect, setActiveEffect] = useState(null);

  const [initialized, setInitialized] = useState(false);
  const [animatedCardIds, setAnimatedCardIds] = useState(new Set());
  const [playingAnimations, setPlayingAnimations] = useState({}); // { cardId: animData }
  const [transientAnimations, setTransientAnimations] = useState([]);

  const selfPlayer = useMemo(() => players.find((p) => p.isSelf), [players]);
  const selfSeat = selfPlayer ? selfPlayer.seatIndex : 0;

  const opponents = useMemo(() => {
    const others = players.filter((p) => !p.isSelf);
    return others.map((player) => {
      let relativeSeat = (player.seatIndex - selfSeat + 4) % 4;
      // In a 2-player game, sit the opponent directly opposite (North)
      if (players.length === 2 && relativeSeat === 1) {
        relativeSeat = 2;
      }
      const index3D = [3, 1, 0, 2][relativeSeat];
      return {
        player,
        position: SEAT_POSITIONS_3D[index3D],
      };
    });
  }, [players, selfSeat]);

  const visibleTableCards = useMemo(() => {
    const cards = roomState?.tableCards || [];
    return cards.slice(-12);
  }, [roomState?.tableCards]);

  const isUno = gameType === "uno" || !gameType;
  const cardsForRender = useMemo(() => {
    if (isUno) {
      const latestCard = roomState?.tableCards?.slice(-1) || [];
      return latestCard.map((card) => ({
        ...card,
        placement: { x: DISCARD_POSITION[0], y: DISCARD_POSITION[1], z: DISCARD_POSITION[2], rotation: 0.08 },
      }));
    }

    return layoutTableCards(visibleTableCards);
  }, [isUno, roomState?.tableCards, visibleTableCards]);

  // Track new cards to play fly-in animations
  useEffect(() => {
    if (!roomState) return;
    const currentTableCardIds = cardsForRender.map((c) => c.id);

    if (!initialized) {
      // First load: initialize with existing table cards so they are static
      setAnimatedCardIds(new Set(currentTableCardIds));
      setInitialized(true);
    } else {
      // Subsequent loads: check for any new cards
      currentTableCardIds.forEach((id) => {
        if (!animatedCardIds.has(id)) {
          const cardObj = cardsForRender.find((c) => c.id === id);
          if (cardObj) {
            let startPos = [...DECK_POSITION];
            let startRot = [0, 0, 0];

            if (cardObj.playedBy) {
              const player = roomState.players.find((p) => p.id === cardObj.playedBy);
              if (player) {
                const relativeSeat = (player.seatIndex - selfSeat + 4) % 4;
                const index3D = [3, 1, 0, 2][relativeSeat];
                const pos = SEAT_POSITIONS_3D[index3D];
                startPos = [pos.x * 0.7, 0.8, pos.z * 0.7];
                startRot = [0, pos.ry || 0, 0];
              }
            }

            const endPos = [cardObj.placement?.x || 0, cardObj.placement?.y || 0.04, cardObj.placement?.z || 0];
            const endRot = [0, cardObj.placement?.rotation || 0, 0];

            setPlayingAnimations((prev) => ({
              ...prev,
              [id]: {
                frontTexturePath: cardObj.assetPath,
                startPos,
                endPos,
                startRot,
                endRot,
                arcHeight: 0.26,
              },
            }));

            setAnimatedCardIds((prev) => {
              const next = new Set(prev);
              next.add(id);
              return next;
            });
          }
        }
      });
    }
  }, [cardsForRender, initialized, roomState, selfSeat, animatedCardIds]);

  useEffect(() => {
    if (!roomState?.lastAction) return;

    const action = roomState.lastAction;
    const drawMatch = action.match(/drew (\d+) card/);
    if (!drawMatch) return;

    const drawingPlayer = roomState.players.find((p) => action.startsWith(p.name));
    if (!drawingPlayer) return;

    const totalCards = Math.min(Number(drawMatch[1]) || 1, 4);
    const relativeSeat = (drawingPlayer.seatIndex - selfSeat + 4) % 4;
    const index3D = [3, 1, 0, 2][relativeSeat];
    const pos = SEAT_POSITIONS_3D[index3D];
    const isSelfDraw = drawingPlayer.isSelf;
    const baseTarget = isSelfDraw ? SELF_DRAW_TARGET : [pos.x * 0.72, 0.7, pos.z * 0.72];

    const nextAnimations = Array.from({ length: totalCards }).map((_, index) => ({
      id: `draw-${drawingPlayer.id}-${roomState.lastAction}-${index}`,
      frontTexturePath: cardBackAsset,
      startPos: [DECK_POSITION[0], DECK_POSITION[1] + index * 0.01, DECK_POSITION[2]],
      endPos: [
        baseTarget[0] + (isSelfDraw ? 0.06 : 0.04) * (index - (totalCards - 1) / 2),
        baseTarget[1] + index * 0.01,
        baseTarget[2] + (isSelfDraw ? 0.04 : 0.02) * index,
      ],
      startRot: [0, 0, 0],
      endRot: [0, isSelfDraw ? 0.18 : pos.ry || 0, 0],
      duration: 0.48,
      delay: index * 0.08,
      arcHeight: 0.28,
    }));

    setTransientAnimations(nextAnimations);
  }, [roomState?.lastAction, roomState?.players, selfSeat]);

  const handleTransientAnimComplete = (cardId) => {
    setTransientAnimations((prev) => prev.filter((anim) => anim.id !== cardId));
  };

  const handleAnimComplete = (cardId) => {
    setPlayingAnimations((prev) => {
      const next = { ...prev };
      delete next[cardId];
      return next;
    });
  };

  // Effect Trigger logic from roomState.lastAction
  useEffect(() => {
    if (!roomState || !roomState.lastAction) return;

    const action = roomState.lastAction;
    
    // Find player who performed the action or who is target
    const triggeringPlayer = roomState.players.find((p) => action.startsWith(p.name));

    if (isUno) {
      if (action.includes("Skip next player") || action.includes("Skip (reverse with 2 players)")) {
        // Skip effect is shown at the skipped player's position (whose turn it is now)
        const skippedPlayer = roomState.players.find((p) => p.id === roomState.currentTurnPlayerId);
        if (skippedPlayer) {
          const relativeSeat = (skippedPlayer.seatIndex - selfSeat + 4) % 4;
          const index3D = [3, 1, 0, 2][relativeSeat];
          const pos = SEAT_POSITIONS_3D[index3D];
          setActiveEffect({
            type: "skip",
            position: [pos.x, 1.5, pos.z],
            onComplete: () => setActiveEffect(null),
          });
        }
      } else if (action.includes("Direction reversed")) {
        const direction = roomState.direction || 1;
        setActiveEffect({
          type: "reverse",
          direction,
          onComplete: () => setActiveEffect(null),
        });
      } else if (action.includes("penalty cards") || action.includes("forced")) {
        const drawingPlayer = roomState.players.find(
          (p) => action.includes(p.name) && (action.includes("drew") || action.includes("draws"))
        );
        if (drawingPlayer) {
          let count = 2;
          if (action.includes("4 penalty")) count = 4;
          else if (action.includes("6 penalty")) count = 6;
          else if (action.includes("drew 2")) count = 2;
          else if (action.includes("drew 4")) count = 4;

          const relativeSeat = (drawingPlayer.seatIndex - selfSeat + 4) % 4;
          const index3D = [3, 1, 0, 2][relativeSeat];
          const pos = SEAT_POSITIONS_3D[index3D];
          setActiveEffect({
            type: "draw",
            count,
            position: [pos.x, 1.8, pos.z],
            onComplete: () => setActiveEffect(null),
          });
        }
      } else if (action.includes("called UNO!")) {
        if (triggeringPlayer) {
          const relativeSeat = (triggeringPlayer.seatIndex - selfSeat + 4) % 4;
          const index3D = [3, 1, 0, 2][relativeSeat];
          const pos = SEAT_POSITIONS_3D[index3D];
          setActiveEffect({
            type: "uno-call",
            position: [pos.x, 1.6, pos.z],
            onComplete: () => setActiveEffect(null),
          });
        }
      } else if (action.includes("played wild")) {
        setActiveEffect({
          type: "wild",
          color:
            roomState.currentColor === "red"
              ? "#ff6b6b"
              : roomState.currentColor === "blue"
              ? "#3498db"
              : roomState.currentColor === "green"
              ? "#2ecc71"
              : roomState.currentColor === "yellow"
              ? "#f1c40f"
              : "#ffffff",
          onComplete: () => setActiveEffect(null),
        });
      }
    } else {
      // Tiến Lên
      if (action.includes("đánh tứ quý")) {
        if (triggeringPlayer) {
          const relativeSeat = (triggeringPlayer.seatIndex - selfSeat + 4) % 4;
          const index3D = [3, 1, 0, 2][relativeSeat];
          const pos = SEAT_POSITIONS_3D[index3D];
          setActiveEffect({
            type: "quad",
            position: [pos.x * 0.5, 1.0, pos.z * 0.5],
            onComplete: () => setActiveEffect(null),
          });
        }
      } else if (action.includes("chặn heo")) {
        if (triggeringPlayer) {
          const relativeSeat = (triggeringPlayer.seatIndex - selfSeat + 4) % 4;
          const index3D = [3, 1, 0, 2][relativeSeat];
          const pos = SEAT_POSITIONS_3D[index3D];
          setActiveEffect({
            type: "chop",
            position: [pos.x * 0.4, 0.8, pos.z * 0.4],
            onComplete: () => setActiveEffect(null),
          });
        }
      } else if (action.includes("đã thắng")) {
        if (triggeringPlayer) {
          const relativeSeat = (triggeringPlayer.seatIndex - selfSeat + 4) % 4;
          const index3D = [3, 1, 0, 2][relativeSeat];
          const pos = SEAT_POSITIONS_3D[index3D];
          setActiveEffect({
            type: "victory",
            position: [pos.x, 2.2, pos.z],
            onComplete: () => setActiveEffect(null),
          });
        }
      }
    }
  }, [roomState?.lastAction, isUno, selfSeat, roomState]);

  useEffect(() => {
    if (roomState?.gamePhase === "finished" && roomState?.winnerId) {
      const winner = roomState.players.find((p) => p.id === roomState.winnerId);
      if (winner) {
        const relativeSeat = (winner.seatIndex - selfSeat + 4) % 4;
        const index3D = [3, 1, 0, 2][relativeSeat];
        const pos = SEAT_POSITIONS_3D[index3D];
        setActiveEffect({
          type: isUno ? "uno-call" : "victory",
          position: [pos.x, 2.2, pos.z],
          color: "#f1c40f",
          onComplete: () => setActiveEffect(null),
        });
      }
    }
  }, [roomState?.gamePhase, roomState?.winnerId, isUno, selfSeat]);

  return (
    <Canvas
      camera={{ position: [0, 1.8, 4.2], fov: 55 }}
      shadows
      onCreated={({ gl }) => {
        gl.toneMappingExposure = 1.35;
      }}
    >
      <color attach="background" args={["#101522"]} />
      <fog attach="fog" args={["#101522", 10, 22]} />
      <hemisphereLight intensity={0.7} color="#d9e7ff" groundColor="#2c1d14" />
      <directionalLight
        position={[3.5, 5.5, 2.5]}
        intensity={1.1}
        color="#fff3d6"
        castShadow
        shadow-mapSize-width={1024}
        shadow-mapSize-height={1024}
      />

      {/* Room environment */}
      <Room3D />

      {/* Game table */}
      <GameTable3D />

      {/* First person camera controls */}
      <FirstPersonCamera />

      {/* UNO-specific elements */}
      {isUno && (
        <>
          <DeckStack3D count={roomState?.deckCount || 0} />
          <DiscardPile3D topCard={roomState?.topDiscard} currentColor={roomState?.currentColor} />
          <DirectionArrow3D direction={roomState?.direction || 1} />
        </>
      )}

      {/* Table cards */}
      {cardsForRender.map((card) => {
        if (playingAnimations[card.id]) {
          const anim = playingAnimations[card.id];
          return (
            <AnimatedCard
              key={card.id}
              frontTexturePath={anim.frontTexturePath}
              startPos={anim.startPos}
              endPos={anim.endPos}
              startRot={anim.startRot}
              endRot={anim.endRot}
              arcHeight={anim.arcHeight}
              onComplete={() => handleAnimComplete(card.id)}
            />
          );
        }

        if (isUno) {
          return null;
        }

        return <TableCardMesh key={card.id} card={card} />;
      })}

      {transientAnimations.map((anim) => (
        <AnimatedCard
          key={anim.id}
          frontTexturePath={anim.frontTexturePath}
          startPos={anim.startPos}
          endPos={anim.endPos}
          startRot={anim.startRot}
          endRot={anim.endRot}
          duration={anim.duration}
          delay={anim.delay}
          arcHeight={anim.arcHeight}
          onComplete={() => handleTransientAnimComplete(anim.id)}
        />
      ))}

      {/* Room code floating text */}
      <Float speed={1.4} rotationIntensity={0.05} floatIntensity={0.1}>
        <Text position={[0, 0.2, -2.5]} rotation={[-Math.PI / 2, 0, 0]} fontSize={0.18} color="#f8f0cc" outlineWidth={0.008} outlineColor="#000">
          {roomCode}
        </Text>
      </Float>

      {/* Opponent characters */}
      {opponents.map(({ player, position }) => (
        <CharacterModel3D
          key={player.id}
          player={player}
          seatPosition={position}
          isCurrentTurn={player.id === roomState?.currentTurnPlayerId}
          isPlayingCard={false}
        />
      ))}

      {/* Pending draws indicator */}
      {isUno && roomState?.pendingDraws > 0 && (
        <Float speed={2} floatIntensity={0.4}>
          <Text position={[0, 1.2, 0]} fontSize={0.35} color="#ff6b6b" outlineWidth={0.02} outlineColor="#000">
            +{roomState.pendingDraws}
          </Text>
        </Float>
      )}

      {/* Visual Effects */}
      {isUno ? (
        <UnoEffects activeEffect={activeEffect} />
      ) : (
        <TienLenEffects activeEffect={activeEffect} />
      )}
    </Canvas>
  );
}
