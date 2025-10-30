# Gerador de Desculpas de Trabalho — .NET 8 (Etapa 1)

**Conforme o enunciado:**
- Serviços inicializados usando .NET.
- `auth-service` e `gateway` configurados roles necessárias: `ADMIN` e `USER`.
- Estrutura de pastas inspirada no **modelo hexagonal** (pastas Domain/Application/Infrastructure/Api dentro de cada serviço — pronto para extrair para projetos separados futuramente).

## Serviços e portas
- **Gateway (Ocelot):** `http://localhost:8088` (rotas `/auth/*`, `/excuses/*`, `/email/*`).
- **AuthService:** `http://localhost:8080/swagger`
- **ExcusesService:** `http://localhost:8081/swagger`
- **EmailService:** `http://localhost:8082/swagger`

## Executar com Docker
```bash
docker compose up --build
```
- O SQL Server sobe na porta `14333`. O `AuthService` aplica migrations automaticamente e cria o usuário admin:
  - email: `admin@excuses.local`
  - senha: `Admin#12345`
  - role: `ADMIN`

## Teste rápido
```bash
# 1) Registro
curl -X POST http://localhost:8088/auth/register -H "Content-Type: application/json" -d "{"username":"gabriel","email":"gabriel@example.com","password":"P@ssw0rd!"}"

# 2) Login
curl -X POST http://localhost:8088/auth/login -H "Content-Type: application/json" -d "{"username":"gabriel","password":"P@ssw0rd!"}"

# 3) Gerar desculpa
curl "http://localhost:8088/excuses/generate?motivo=atraso&tom=formal"
```

## Observações de migração do repositório Java
- Substituímos o Nginx por **Ocelot** (gateway nativo .NET).
- Apenas **roles necessárias**: `ADMIN` e `USER`.
- Estrutura de **camadas** já preparada para evoluir conforme o padrão do `auth-service`.
