const { classifyCombo, canBeat, sortCards, getAbsoluteValue } = require("./TienLenRules");
const { canPlayCard } = require("./RoomRules");

const BOT_NAMES = ["Bot Nova", "Bot Blaze", "Bot Storm", "Bot Iris", "Bot Shadow", "Bot Luna", "Bot Rex", "Bot Ember"];
const BOT_AVATARS = ["char_1", "char_2", "char_3", "char_4", "char_5", "char_6", "char_7", "char_8"];

function getRandomBotIdentity(existingNames) {
  const available = BOT_NAMES.filter((n) => !existingNames.includes(n));
  const name = available.length > 0 ? available[Math.floor(Math.random() * available.length)] : `Bot ${Date.now() % 1000}`;
  const avatar = BOT_AVATARS[Math.floor(Math.random() * BOT_AVATARS.length)];
  return { name, avatarKey: avatar };
}

function getUnoBotAction(room, botPlayer) {
  const playable = botPlayer.hand.filter((card) => canPlayCard(room, card, botPlayer));

  if (playable.length === 0) return { action: "draw" };

  const actionCards = playable.filter((c) => ["skip", "reverse", "draw-two"].includes(c.kind));
  const numberCards = playable.filter((c) => c.kind === "number");
  const wildCards = playable.filter((c) => c.kind === "wild" || c.kind === "wild-draw-four");

  let cardToPlay = null;
  if (actionCards.length > 0) {
    cardToPlay = actionCards[0];
  } else if (numberCards.length > 0) {
    cardToPlay = numberCards.sort((a, b) => b.points - a.points)[0];
  } else if (wildCards.length > 0) {
    cardToPlay = wildCards[0];
  }

  if (!cardToPlay) return { action: "draw" };

  let chosenColor = null;
  if (cardToPlay.kind === "wild" || cardToPlay.kind === "wild-draw-four") {
    const colorCounts = { red: 0, blue: 0, green: 0, yellow: 0 };
    for (const c of botPlayer.hand) {
      if (colorCounts[c.color] !== undefined) colorCounts[c.color]++;
    }
    chosenColor = Object.entries(colorCounts).sort((a, b) => b[1] - a[1])[0][0];
  }

  const shouldCallUno = botPlayer.hand.length <= 2;

  return { action: "play", cardId: cardToPlay.id, chosenColor, callUno: shouldCallUno };
}

function getTienLenBotAction(room, botPlayer) {
  const lastCombo = room.lastPlayedCombo;
  const hand = sortCards(botPlayer.hand);
  const isNewRound = !lastCombo;
  const isFirstMove = room.isFirstGame && room.tableCards.length === 0;

  if (isNewRound) {
    if (isFirstMove) {
      const threeSpade = hand.find((c) => c.label === "3" && c.suit === "spade");
      if (threeSpade) return { action: "play", cardIds: [threeSpade.id] };
    }
    return { action: "play", cardIds: [hand[0].id] };
  }

  const possiblePlays = findAllBeatingCombos(hand, lastCombo);
  if (possiblePlays.length === 0) return { action: "pass" };

  possiblePlays.sort((a, b) => {
    const aMax = Math.max(...a.map(getAbsoluteValue));
    const bMax = Math.max(...b.map(getAbsoluteValue));
    return aMax - bMax;
  });

  return { action: "play", cardIds: possiblePlays[0].map((c) => c.id) };
}

function findAllBeatingCombos(hand, lastCombo) {
  const results = [];

  if (lastCombo.type === "single") {
    for (const card of hand) {
      const combo = classifyCombo([card]);
      if (combo && canBeat(lastCombo, combo)) results.push([card]);
    }
  } else if (lastCombo.type === "pair") {
    for (let i = 0; i < hand.length; i++) {
      for (let j = i + 1; j < hand.length; j++) {
        if (hand[i].label === hand[j].label) {
          const combo = classifyCombo([hand[i], hand[j]]);
          if (combo && canBeat(lastCombo, combo)) results.push([hand[i], hand[j]]);
        }
      }
    }
  } else if (lastCombo.type === "triple") {
    for (let i = 0; i < hand.length - 2; i++) {
      if (hand[i].label === hand[i + 1]?.label && hand[i].label === hand[i + 2]?.label) {
        const combo = classifyCombo([hand[i], hand[i + 1], hand[i + 2]]);
        if (combo && canBeat(lastCombo, combo)) results.push([hand[i], hand[i + 1], hand[i + 2]]);
      }
    }
  } else if (lastCombo.type === "quad") {
    for (let i = 0; i < hand.length - 3; i++) {
      if (hand[i].label === hand[i + 1]?.label && hand[i].label === hand[i + 2]?.label && hand[i].label === hand[i + 3]?.label) {
        const cards = [hand[i], hand[i + 1], hand[i + 2], hand[i + 3]];
        const combo = classifyCombo(cards);
        if (combo && canBeat(lastCombo, combo)) results.push(cards);
      }
    }
  }

  if (lastCombo.type === "single" && lastCombo.highCard.label === "2") {
    for (let i = 0; i < hand.length - 3; i++) {
      if (hand[i].label === hand[i + 1]?.label && hand[i].label === hand[i + 2]?.label && hand[i].label === hand[i + 3]?.label) {
        results.push([hand[i], hand[i + 1], hand[i + 2], hand[i + 3]]);
      }
    }
  }

  return results;
}

function getBotDelay() {
  return 1000 + Math.floor(Math.random() * 2000);
}

module.exports = { getRandomBotIdentity, getUnoBotAction, getTienLenBotAction, getBotDelay };
