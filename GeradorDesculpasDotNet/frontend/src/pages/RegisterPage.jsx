import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { register as registerApi } from '../api/auth.js';

export default function RegisterPage() {
  const [username, setUsername] = useState('');
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm]   = useState('');
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState('');
  const [success, setSuccess]   = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (password !== confirm) {
      setError('As senhas não conferem.');
      return;
    }

    setLoading(true);
    try {
      await registerApi({ username, email, password });
      setSuccess('Cadastro realizado com sucesso! Você já pode fazer login.');
      setTimeout(() => navigate('/login'), 1200);
    } catch (err) {
      setError(err.message || 'Erro ao cadastrar.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-card">
      <h2>Cadastro</h2>
      <form onSubmit={handleSubmit} className="auth-form">
        <label>
          Usuário
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            autoComplete="off"
          />
        </label>

        <label>
          E-mail
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
          />
        </label>

        <label>
          Senha
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="new-password"
          />
        </label>

        <label>
          Confirmar senha
          <input
            type="password"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            required
          />
        </label>

        {error && <div className="auth-error">{error}</div>}
        {success && <div className="auth-success">{success}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Cadastrando...' : 'Cadastrar'}
        </button>
      </form>

      <p className="auth-switch">
        Já tem conta? <Link to="/login">Faça login</Link>
      </p>
    </div>
  );
}