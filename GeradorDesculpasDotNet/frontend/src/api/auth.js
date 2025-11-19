const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8088';

async function parseError(response) {
  let msg = 'Erro ao comunicar com o servidor.';
  try {
    const data = await response.json();
    if (data && (data.message || data.error)) {
      msg = data.message || data.error;
    }
  } catch {
    try {
      const text = await response.text();
      if (text) msg = text;
    } catch {
    }
  }
  return msg;
}

export async function register({ username, email, password }) {
  const res = await fetch(`${API_BASE_URL}/api/auth/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      name: username,
      email,
      password,
    }),
  });

  if (!res.ok) {
    throw new Error(await parseError(res));
  }

  return await res.text();
}

export async function login({ username, password }) {
  const res = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      email: username, 
      password,
    }),
  });

  if (!res.ok) {
    throw new Error(await parseError(res));
  }

  const data = await res.json();

  const token =
    data.token ||
    data.accessToken ||
    data.jwt ||
    data.jwtToken;

  if (!token) {
    throw new Error('Token JWT não encontrado na resposta da API.');
  }

  return { token, raw: data };
}