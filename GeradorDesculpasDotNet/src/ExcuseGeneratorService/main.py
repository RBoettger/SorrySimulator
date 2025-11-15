from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import datetime
from typing import Optional
import os

from google import genai  

app = FastAPI(title="Excuse Generator Service (Gemini)")

client = genai.Client() 

class ExcuseRequest(BaseModel):
    nome: Optional[str] = "Usuário"
    motivo: Optional[str] = None
    tom: Optional[str] = "Profissional"  

class ExcuseResponse(BaseModel):
    desculpa: str
    dataGeracao: str
    fonte: str = "gemini-2.5-flash"

@app.post("/gerar", response_model=ExcuseResponse)
async def gerar(request: ExcuseRequest):
    """
    Gera uma desculpa com base no motivo informado, usando Gemini.
    """
    motivo = request.motivo or "imprevistos pessoais"
    nome = request.nome or "Usuário"
    tom = (request.tom or "profissional").lower()

    prompt = (
        f"Escreva uma desculpa curta e natural em português sobre '{motivo}', "
        f"no tom {tom}. Assine como {nome}. "
        f"A desculpa deve ter no máximo 4 frases, ser convincente e educada."
    )

    try:
        response = await client.aio.models.generate_content(
            model="gemini-2.5-flash",  
            contents=prompt
        )

        excuse_text = (response.text or "").strip()
        if not excuse_text:
            if response.candidates:
                parts = response.candidates[0].content.parts
                excuse_text = " ".join(
                    (p.text or "") for p in parts if hasattr(p, "text")
                ).strip()

        if not excuse_text:
            raise HTTPException(
                status_code=502,
                detail="Gemini retornou resposta vazia."
            )

    except Exception as ex:
        raise HTTPException(
            status_code=502,
            detail=f"Erro ao gerar desculpa com Gemini: {ex}"
        )

    return ExcuseResponse(
        desculpa=excuse_text,
        dataGeracao=datetime.utcnow().isoformat() + "Z",
        fonte="gemini-2.5-flash"
    )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8083, reload=True)
