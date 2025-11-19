import React, { useState } from "react";
import "../styles/newExcuse.css";

export default function NewExcusePage() {
  const [nome, setNome] = useState("");
  const [motivo, setMotivo] = useState("");
  const [tom, setTom] = useState("Profissional");

  const [loading, setLoading] = useState(false);
  const [resultado, setResultado] = useState("");
  const [erro, setErro] = useState("");

  // usa env, mas default é gateway local
  const API_BASE_URL =
    import.meta.env.VITE_API_BASE_EXCUSE || "http://localhost:8088";

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
      // resposta vem do Python: { desculpa, dataGeracao, fonte }
      setResultado(data.desculpa || "Desculpa gerada, mas resposta inesperada.");
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
            placeholder="Ex: Gabriel"
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

          <button className="copiar-btn" onClick={copiarTexto}>
            Copiar texto
          </button>
        </div>
      )}
    </div>
  );
}