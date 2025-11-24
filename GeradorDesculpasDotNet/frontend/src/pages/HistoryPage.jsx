import React, { useEffect, useState } from "react";
import "../styles/history.css";
import { useAuth } from "../auth/AuthContext";
import { getUserIdFromToken } from "../auth/getUserId";

export default function HistoryPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const { token } = useAuth();

  const API_BASE_EMAIL =
    import.meta.env.VITE_API_BASE_EMAIL || "http://localhost:8088";

  useEffect(() => {
    const loadHistory = async () => {
      setLoading(true);

      const headers = { "Content-Type": "application/json" };

      const userId = getUserIdFromToken(token);

      if (userId) {
        headers["X-User-Id"] = userId;
      } else {
        console.warn("⚠️ Nenhum UserId encontrado no token");
      }

      try {
        const res = await fetch(`${API_BASE_EMAIL}/api/email/history`, {
          method: "GET",
          headers,
        });

        if (!res.ok) throw new Error("Erro ao carregar histórico");

        const data = await res.json();
        setItems(data);
      } catch (err) {
        console.error("Erro ao buscar histórico:", err);
      } finally {
        setLoading(false);
      }
    };

    loadHistory();
  }, [token]);

  return (
    <div className="history-container">
      <h2>Histórico de mensagens enviadas</h2>

      {loading && <p className="loading">Carregando...</p>}

      {!loading && items.length === 0 && (
        <p className="empty">Nenhuma desculpa enviada ainda.</p>
      )}

      <div className="history-list">
        {items.map(item => (
          <div key={item.historyId} className="history-card">
            <div className="history-header">
              <strong>Para:</strong> {item.toEmail}
            </div>

            <div className="history-body">
              {item.excuseText}
            </div>

            <div className="history-footer">
              <span>{new Date(item.sentAt).toLocaleString("pt-BR")}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}