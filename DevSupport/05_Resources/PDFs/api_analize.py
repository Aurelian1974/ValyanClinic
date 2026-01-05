"""
API pentru parsare buletine analize medicale
============================================
Endpoint-uri:
- GET /laboratoare - Lista laboratoarelor disponibile
- POST /parse - Parsează un PDF

Rulare: uvicorn api_analize:app --host 0.0.0.0 --port 5050
"""

from fastapi import FastAPI, File, UploadFile, HTTPException, Form
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import tempfile
import os

# Import parserele
from parsere_laboratoare import (
    PARSERS, list_parsers, get_parser, to_valyan_format,
    BuletinResult, AnalizaResult
)

app = FastAPI(
    title="Analize Medicale Parser API",
    description="API pentru parsarea buletinelor de analize medicale din PDF",
    version="1.0.0"
)

# CORS pentru Blazor
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# =============================================================================
# MODELE RESPONSE
# =============================================================================

class LaboratorInfo(BaseModel):
    key: str
    name: str
    description: str


class AnalizaParsata(BaseModel):
    categorie: str
    nume_analiza: str
    cod_analiza: Optional[str] = None
    rezultat: str
    rezultat_numeric: Optional[float] = None
    unitate_masura: str
    interval_min: Optional[float] = None
    interval_max: Optional[float] = None
    interval_text: str
    este_anormal: bool
    directie_anormal: Optional[str] = None


class ParseResult(BaseModel):
    success: bool
    laborator: str
    numar_buletin: str
    data_recoltare: str
    pacient_nume: str
    pacient_cnp: str
    analize: List[AnalizaParsata]
    warnings: List[str]
    total_analize: int
    analize_anormale: int


class ImportFormat(BaseModel):
    NumeAnaliza: str
    CodAnaliza: Optional[str] = None
    TipAnaliza: str
    Valoare: str
    ValoareNumerica: Optional[float] = None
    UnitatiMasura: str
    ValoareNormalaMin: Optional[float] = None
    ValoareNormalaMax: Optional[float] = None
    ValoareNormalaText: str
    EsteInAfaraLimitelor: bool
    DirectieAnormal: Optional[str] = None
    DataRecoltare: str
    Laborator: str
    NumarBuletin: str


# =============================================================================
# ENDPOINTS
# =============================================================================

@app.get("/")
async def root():
    """Health check"""
    return {"status": "ok", "message": "Analize Medicale Parser API"}


@app.get("/laboratoare", response_model=List[LaboratorInfo])
async def get_laboratoare():
    """Returnează lista laboratoarelor disponibile"""
    return list_parsers()


@app.post("/parse", response_model=ParseResult)
async def parse_pdf(
    file: UploadFile = File(...),
    laborator: str = Form(...)
):
    """
    Parsează un PDF de analize medicale.
    
    - **file**: Fișierul PDF
    - **laborator**: Cheia laboratorului (ex: regina_maria, synevo, etc.)
    """
    # Validare laborator
    parser = get_parser(laborator)
    if not parser:
        raise HTTPException(
            status_code=400, 
            detail=f"Laborator necunoscut: {laborator}. Folosește /laboratoare pentru lista."
        )
    
    # Verificare tip fișier
    if not file.filename.lower().endswith('.pdf'):
        raise HTTPException(
            status_code=400,
            detail="Fișierul trebuie să fie PDF"
        )
    
    # Salvare temporară și parsare
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix='.pdf') as tmp:
            content = await file.read()
            tmp.write(content)
            tmp_path = tmp.name
        
        # Parsare
        result = parser.parse_pdf(tmp_path)
        
        # Cleanup
        os.unlink(tmp_path)
        
        # Convertire în response
        analize_parsate = [
            AnalizaParsata(
                categorie=a.categorie,
                nume_analiza=a.nume_analiza,
                cod_analiza=a.cod_analiza,
                rezultat=a.rezultat,
                rezultat_numeric=a.rezultat_numeric,
                unitate_masura=a.unitate_masura,
                interval_min=a.interval_min,
                interval_max=a.interval_max,
                interval_text=a.interval_text,
                este_anormal=a.este_anormal,
                directie_anormal=a.directie_anormal
            )
            for a in result.analize
        ]
        
        return ParseResult(
            success=True,
            laborator=result.laborator,
            numar_buletin=result.numar_buletin,
            data_recoltare=result.data_recoltare,
            pacient_nume=result.pacient_nume,
            pacient_cnp=result.pacient_cnp,
            analize=analize_parsate,
            warnings=result.warnings,
            total_analize=len(result.analize),
            analize_anormale=sum(1 for a in result.analize if a.este_anormal)
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/parse/import-format", response_model=List[ImportFormat])
async def parse_pdf_import(
    file: UploadFile = File(...),
    laborator: str = Form(...)
):
    """
    Parsează PDF și returnează în format ValyanClinic pentru import direct.
    """
    parser = get_parser(laborator)
    if not parser:
        raise HTTPException(status_code=400, detail=f"Laborator necunoscut: {laborator}")
    
    if not file.filename.lower().endswith('.pdf'):
        raise HTTPException(status_code=400, detail="Fișierul trebuie să fie PDF")
    
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix='.pdf') as tmp:
            content = await file.read()
            tmp.write(content)
            tmp_path = tmp.name
        
        result = parser.parse_pdf(tmp_path)
        os.unlink(tmp_path)
        
        return to_valyan_format(result)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# =============================================================================
# MAIN
# =============================================================================

if __name__ == "__main__":
    import uvicorn
    print("\n" + "="*60)
    print("ANALIZE MEDICALE PARSER API")
    print("="*60)
    print("Endpoint-uri:")
    print("  GET  /              - Health check")
    print("  GET  /laboratoare   - Lista laboratoarelor")
    print("  POST /parse         - Parsează PDF")
    print("  POST /parse/import-format - PDF → format import")
    print("="*60 + "\n")
    
    uvicorn.run(app, host="127.0.0.1", port=5050)
