
# 📘 SorrySimulator — Gerador de Desculpas com IA + Microsserviços

O **SorrySimulator** é uma aplicação completa baseada em **microsserviços** que permite:

✔️ Gerar desculpas personalizadas usando IA (Gemini)  
✔️ Enviar essas mensagens por e-mail (SendGrid)  
✔️ Salvar e consultar histórico por usuário  
✔️ Gerenciar autenticação com JWT  
✔️ Interface web moderna em React

O projeto foi desenvolvido com foco em arquitetura limpa, escalabilidade e modularidade.

---

## 🏗️ Arquitetura Geral

A aplicação é composta por vários microsserviços independentes:

| Serviço | Porta | Função |
|--------|-------|--------|
| **Gateway** | 8088 | API Gateway que unifica as rotas dos outros serviços |
| **AuthService** | 8080 | Autenticação, cadastro, login e emissão de JWT |
| **ExcuseGeneratorService** | 8083 | Gera mensagens usando a API Gemini |
| **ExcusesService** | 8081 | Orquestra geração de desculpas e integra com o serviço Gemini |
| **EmailService** | 8082 | Envio de e-mails via SendGrid e salvamento de histórico |
| **SQL Server** | 14333 | Banco de dados principal |
| **Frontend (React)** | 5173 | Interface do usuário |

---

## 🗂️ Estrutura do Repositório

```
.
├── docker-compose.yml
├── frontend/              → Aplicação React (Dashboard, Login, Histórico, Criar Desculpa)
└── src/
    ├── AuthService/       → Autenticação + JWT
    ├── EmailService/      → Envio de e-mail + histórico
    ├── ExcuseGeneratorService/ → Integração com Gemini
    ├── ExcusesService/    → API do gerador de desculpas
    └── Gateway/           → API Gateway
```

---

## 🚀 Tecnologias Utilizadas

### Backend
- **.NET 8**
- **Entity Framework Core**
- **SQL Server 2022**
- **JWT Authentication**
- **SendGrid API**
- **Gemini AI API**
- Minimal APIs
- CORS configurado por serviço

### Frontend
- **React 18**
- **React Router**
- **Context API**
- **Fetch API**
- **CSS customizado**

---

## 🧪 Funcionalidades

### 🔐 Autenticação
- Cadastro e login
- Senhas com hash e salt (PBKDF2)
- JWT salvo no `localStorage` como `auth_token`

### 📝 Geração de Desculpas
- Personalização: nome, motivo, tom da mensagem
- Geração por IA usando Gemini

### 📧 Envio por E-mail (Não funcional)
- Configuração de remetente via variáveis de ambiente
- Envio via SendGrid
- Registro no histórico por **UserId extraído do JWT**

### 📜 Histórico por Usuário
- Listagem das mensagens enviadas
- Relacionamento com tabela `gd.Users`
- Horário ajustado para **America/Sao_Paulo**

---

## 🔧 Como Rodar o Projeto

### 1️⃣ Clonar o repositório

```sh
git clone https://github.com/RBoettger/SorrySimulator
cd SorrySimulator
```

### 2️⃣ Configurar variáveis de ambiente

Crie um arquivo `.env` na raiz:

```
GEMINI_API_KEY=...
SENDGRID_API_KEY=...
SMTP_FROM=seuemail@dominio.com
SMTP_FROM_NAME=Gerador de Desculpas
```

### 3️⃣ Subir com Docker

```sh
docker-compose up --build
```

---

## 🌐 Endpoints Principais

### Gateway (`localhost:8088`)

```
POST /auth/login
POST /auth/register
POST /excuses/generate
POST /email/send
GET  /email/history
```

Todos protegidos via encaminhamento para os microsserviços.

---

## 🛢️ Banco de Dados

O SQL Server contém:

### Tabelas principais:

#### `gd.Users`
- UserId  
- Name  
- Email  
- PasswordHash  
- PasswordSalt  
- CreatedAt  

#### `gd.ExcuseHistory`
- HistoryId  
- UserId  
- SenderName  
- ToEmail  
- Subject  
- ExcuseText  
- SentAt  

---

## 🧩 Fluxo Completo de Uso

1. Usuário cadastra ou faz login  
2. Recebe JWT  
3. Acessa página de gerar desculpa  
4. Gera mensagem com IA  
5. Envia por e-mail  
6. Histórico é salvo no banco  
7. Tela de histórico exibe mensagens do usuário autenticado  

---

## 📄 Licença

Projeto de estudo. Livre para uso e modificação.

---

## ⭐ Feedback

Se gostou, deixe uma star ⭐ no repositório!