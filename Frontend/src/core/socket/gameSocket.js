import { io } from "socket.io-client";
import { appConfig } from "../config/env";

export const gameSocket = io(appConfig.serverUrl, {
  autoConnect: true,
});
