from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import datetime
from urllib.parse import quote
import requests

app = FastAPI(title="Excuse Generator Service (Pollinations.AI)")

class ExcuseRequest(BaseModel):
    nome: str | None = "Usuário"
    motivo: str | None = None
    tom: str | None = "Profissional"  # ou "Informal"

class ExcuseResponse(BaseModel):
    desculpa: str
    dataGeracao: str
    fonte: str = "pollinations.ai"

@app.post("/gerar", response_model=ExcuseResponse)
async def gerar(request: ExcuseRequest):
    """
    Gera uma desculpa com base no motivo informado, usando Pollinations.AI
    """
    motivo = request.motivo or "imprevistos pessoais"
    nome = request.nome or "Usuário"
    tom = (request.tom or "profissional").lower()

    prompt = (
        f"Escreva uma desculpa curta e natural em português sobre '{motivo}', "
        f"no tom {tom}. Assine como {nome}. "
        f"A desculpa deve ter no máximo 4 frases, ser convincente e educada."
    )

    url = f"https://text.pollinations.ai/prompt/{quote(prompt)}"

    try:
        response = requests.get(url, timeout=30)
        response.raise_for_status()
        excuse_text = response.text
    except Exception as ex:
        raise HTTPException(status_code=502, detail=f"Erro ao gerar desculpa: {ex}")

    return ExcuseResponse(
        desculpa=excuse_text,
        dataGeracao=datetime.utcnow().isoformat() + "Z"
    )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8083)