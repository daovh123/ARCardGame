class InMemoryRoomRepository {
  constructor() {
    this.rooms = new Map();
  }

  findByCode(code) {
    return this.rooms.get(code) ?? null;
  }

  save(room) {
    this.rooms.set(room.code, room);
    return room;
  }

  remove(code) {
    this.rooms.delete(code);
  }

  all() {
    return Array.from(this.rooms.values());
  }
}

module.exports = { InMemoryRoomRepository };
