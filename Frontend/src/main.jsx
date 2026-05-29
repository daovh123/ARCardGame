import React from "react";
import ReactDOM from "react-dom/client";
import { Toaster } from "react-hot-toast";
import App from "./App.jsx";
import "./shared/styles/global.css";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <Toaster
      position="top-center"
      toastOptions={{
        duration: 3000,
        style: {
          background: "rgba(15, 22, 38, 0.95)",
          color: "#f4f7fb",
          border: "1px solid rgba(255, 255, 255, 0.12)",
          backdropFilter: "blur(12px)",
          borderRadius: "14px",
          fontSize: "0.9rem",
        },
        success: {
          iconTheme: { primary: "#22c55e", secondary: "#f4f7fb" },
        },
        error: {
          iconTheme: { primary: "#ef4444", secondary: "#f4f7fb" },
        },
      }}
    />
    <App />
  </React.StrictMode>,
);
