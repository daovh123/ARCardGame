import { useMemo, useState } from "react";
import LoginScreen from "./features/auth/components/LoginScreen";
import GameHubScreen from "./features/hub/components/GameHubScreen";
import LobbyScreen from "./features/lobby/components/LobbyScreen";
import GameLayout from "./features/game/components/GameLayout";
import TienLenGameLayout from "./features/game/components/TienLenGameLayout";

export default function App() {
  const [currentScreen, setCurrentScreen] = useState("login");
  const [userProfile, setUserProfile] = useState(null);
  const [selectedGame, setSelectedGame] = useState(null);
  const [session, setSession] = useState(null);

  const sessionKey = useMemo(
    () => `${session?.roomCode || ""}:${session?.playerName || ""}:${selectedGame || ""}`,
    [session, selectedGame],
  );

  function handleLogin(profile) {
    setUserProfile(profile);
    setCurrentScreen("hub");
  }

  function handleSelectGame(gameType) {
    setSelectedGame(gameType);
    setCurrentScreen("lobby");
  }

  function handleJoin(sessionData) {
    setSession(sessionData);
    setCurrentScreen("game");
  }

  function handleBackToHub() {
    setSelectedGame(null);
    setSession(null);
    setCurrentScreen("hub");
  }

  function handleLeaveGame() {
    setSession(null);
    setCurrentScreen("lobby");
  }

  function handleLogout() {
    setUserProfile(null);
    setSelectedGame(null);
    setSession(null);
    setCurrentScreen("login");
  }

  if (currentScreen === "login" || !userProfile) {
    return <LoginScreen onLogin={handleLogin} />;
  }

  if (currentScreen === "hub") {
    return (
      <GameHubScreen
        userProfile={userProfile}
        onSelectGame={handleSelectGame}
        onLogout={handleLogout}
      />
    );
  }

  if (currentScreen === "lobby") {
    return (
      <LobbyScreen
        gameType={selectedGame}
        userProfile={userProfile}
        onJoin={handleJoin}
        onBack={handleBackToHub}
      />
    );
  }

  if (currentScreen === "game" && session) {
    if (selectedGame === "tien-len") {
      return (
        <TienLenGameLayout
          key={sessionKey}
          session={session}
          gameType={selectedGame}
          onLeave={handleLeaveGame}
          onBackToHub={handleBackToHub}
        />
      );
    }

    return (
      <GameLayout
        key={sessionKey}
        session={session}
        gameType={selectedGame}
        onLeave={handleLeaveGame}
        onBackToHub={handleBackToHub}
      />
    );
  }

  return <LoginScreen onLogin={handleLogin} />;
}
