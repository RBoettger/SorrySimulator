import React, { useState } from "react";
import "../styles/newExcuse.css";
import { useAuth } from "../auth/AuthContext";
import { getUserIdFromToken } from "../auth/getUserId";

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function NewExcusePage() {
  const [nome, setNome] = useState("");
  const [motivo, setMotivo] = useState("");
  const [tom, setTom] = useState("Profissional");

  const [loading, setLoading] = useState(false);
  const [resultado, setResultado] = useState("");
  const [erro, setErro] = useState("");

  const [showEmailModal, setShowEmailModal] = useState(false);
  const [emailTo, setEmailTo] = useState("");
  const [emailError, setEmailError] = useState("");
  const [emailSuccess, setEmailSuccess] = useState("");
  const [emailSending, setEmailSending] = useState(false);
  const { token } = useAuth();

  const API_BASE_URL =
    import.meta.env.VITE_API_BASE_EXCUSE || "http://localhost:8088";
    
const API_BASE_EMAIL =
    import.meta.env.VITE_API_BASE_EMAIL || "http://localhost:8088";

  const gerar = async () => {
    setErro("");
    setResultado("");

    if (!nome.trim()) {
      setErro("Informe seu nome.");
      return;
    }

    setLoading(true);

    try {
      const res = await fetch(`${API_BASE_URL}/excuses/generate`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          nome,
          motivo: motivo.trim() === "" ? null : motivo,
          tom,
        }),
      });

      if (!res.ok) throw new Error("Erro ao gerar desculpa");

      const data = await res.json();
      setResultado(
        data.desculpa || "Desculpa gerada, mas resposta inesperada."
      );
    } catch (error) {
      console.error(error);
      setErro("Falha ao gerar a desculpa.");
    } finally {
      setLoading(false);
    }
  };

  const copiarTexto = () => {
    if (!resultado) return;
    navigator.clipboard.writeText(resultado);
  };

  const abrirModalEmail = () => {
    if (!resultado) {
      setErro("Gere uma desculpa antes de enviar por e-mail.");
      return;
    }
    setEmailTo("");
    setEmailError("");
    setEmailSuccess("");
    setShowEmailModal(true);
  };

  const fecharModalEmail = () => {
    if (emailSending) return;
    setShowEmailModal(false);
  };

  const handleEnviarEmail = async (e) => {
    e.preventDefault();
    setEmailError("");
    setEmailSuccess("");

    const headers = { "Content-Type": "application/json" };

    const userId = getUserIdFromToken(token);
    if (userId) {
      headers["X-User-Id"] = userId;
    }
    
    const trimmed = emailTo.trim();

    if (!trimmed) {
      setEmailError("Informe o e-mail do destinatário.");
      return;
    }

    if (!emailRegex.test(trimmed)) {
      setEmailError("Digite um e-mail válido.");
      return;
    }

    setEmailSending(true);

    try {
      const res = await fetch(`${API_BASE_EMAIL}/api/email/send`, {
        method: "POST",
        headers,
        body: JSON.stringify({
          to: trimmed,
          subject: "Motivo ausência",
          body: resultado,
        }),
      });

      if (!res.ok) {
        throw new Error("Erro ao enviar e-mail");
      }

      setEmailSuccess("E-mail enviado com sucesso! 🎉");
    } catch (err) {
      console.error(err);
      setEmailError("Falha ao enviar o e-mail.");
    } finally {
      setEmailSending(false);
    }
  };

  return (
    <div className="excuse-container">
      <h2>Gerar nova desculpa</h2>

      <div className="excuse-form">
        <label>
          Seu nome
          <input
            type="text"
            value={nome}
            onChange={(e) => setNome(e.target.value)}
            placeholder="Ex: João"
          />
        </label>

        <label>
          Motivo (opcional)
          <input
            type="text"
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            placeholder="Ex: Atraso, Falta, Reunião..."
          />
        </label>

        <label>
          Tom da mensagem
          <select value={tom} onChange={(e) => setTom(e.target.value)}>
            <option value="Profissional">Profissional</option>
            <option value="Informal">Informal</option>
            <option value="Educado">Educado</option>
            <option value="Sério">Sério</option>
            <option value="Divertido">Divertido</option>
          </select>
        </label>

        {erro && <div className="excuse-error">{erro}</div>}

        <button onClick={gerar} disabled={loading}>
          {loading ? "Gerando..." : "Gerar Desculpa"}
        </button>
      </div>

      {resultado && (
        <div className="resultado-box">
          <h3>Desculpa gerada:</h3>
          <p>{resultado}</p>

          <div className="result-actions">
            <button className="copiar-btn" onClick={copiarTexto}>
              Copiar texto
            </button>
            <button className="email-btn" onClick={abrirModalEmail}>
              Enviar e-mail
            </button>
          </div>
        </div>
      )}

      {showEmailModal && (
        <div className="email-modal-backdrop">
          <div className="email-modal">
            <header className="email-modal-header">
              <h3>Enviar por e-mail</h3>
              <button
                type="button"
                className="email-modal-close"
                onClick={fecharModalEmail}
              >
                ×
              </button>
            </header>

            <form onSubmit={handleEnviarEmail} className="email-modal-form">
              <label>
                E-mail do destinatário
                <input
                  type="email"
                  value={emailTo}
                  onChange={(e) => setEmailTo(e.target.value)}
                  placeholder="exemplo@dominio.com"
                />
              </label>

              {emailError && <div className="email-error">{emailError}</div>}
              {emailSuccess && (
                <div className="email-success">{emailSuccess}</div>
              )}

              <button type="submit" disabled={emailSending}>
                {emailSending ? "Enviando..." : "Enviar"}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
