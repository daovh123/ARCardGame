function createGetRoomStateUseCase({ roomRepository, roomStateSerializer }) {
  return ({ roomCode, viewerId }) => {
    const room = roomRepository.findByCode(roomCode);

    if (!room) {
      return null;
    }

    return roomStateSerializer.serialize(room, viewerId);
  };
}

module.exports = { createGetRoomStateUseCase };
