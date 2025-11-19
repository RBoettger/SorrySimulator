import React from 'react';
import { Routes, Route, Navigate, Link } from 'react-router-dom';
import { AuthProvider, useAuth } from './auth/AuthContext.jsx';

import LoginPage from './pages/LoginPage.jsx';
import RegisterPage from './pages/RegisterPage.jsx';
import DashboardPage from './pages/DashboardPage.jsx';

import NewExcusePage from './pages/NewExcusePage.jsx';
import ExcuseHistoryPage from './pages/ExcuseHistoryPage.jsx';
import SettingsPage from './pages/SettingsPage.jsx';

function PrivateRoute({ children }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" replace />;
}

function Layout({ children }) {
  const { isAuthenticated, logout } = useAuth();

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>SorrySimulator</h1>

        <nav>
          {isAuthenticated ? (
            <>
              <Link to="/">Dashboard</Link>
              <Link to="/excuse/new">Criar desculpa</Link>
              <Link to="/excuse/history">Histórico</Link>
              <Link to="/settings">Configurações</Link>

              <button onClick={logout} className="link-button">
                Sair
              </button>
            </>
          ) : (
            <>
              <Link to="/login">Login</Link>
              <Link to="/register">Cadastro</Link>
            </>
          )}
        </nav>
      </header>

      <main className="app-main">{children}</main>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <Layout>
        <Routes>
          <Route
            path="/"
            element={
              <PrivateRoute>
                <DashboardPage />
              </PrivateRoute>
            }
          />

          {/* novas páginas */}
          <Route
            path="/excuse/new"
            element={
              <PrivateRoute>
                <NewExcusePage />
              </PrivateRoute>
            }
          />

          <Route
            path="/excuse/history"
            element={
              <PrivateRoute>
                <ExcuseHistoryPage />
              </PrivateRoute>
            }
          />

          <Route
            path="/settings"
            element={
              <PrivateRoute>
                <SettingsPage />
              </PrivateRoute>
            }
          />

          {/* login / cadastro */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Layout>
    </AuthProvider>
  );
}
