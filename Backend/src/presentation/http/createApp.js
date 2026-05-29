const express = require("express");
const cors = require("cors");

function createHttpApp() {
  const app = express();
  app.use(cors());
  app.use(express.json());

  app.get("/health", (_request, response) => {
    response.json({ ok: true, service: "ar-table-simulator-backend" });
  });

  return app;
}

module.exports = { createHttpApp };
