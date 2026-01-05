"""
Parser Universal v2 pentru Buletine de Analize Medicale (România)
=================================================================
Suportă laboratoarele:
- Regina Maria
- ProMed
- MedLife
- Synevo
- Bioclinica
- Clinica Sante
- SmartLabs
- Elite Medical / Poliana

Pattern-uri identificate din analiza vizuală a 6 buletine reale.
"""

import fitz  # PyMuPDF
import re
import json
from pathlib import Path
from dataclasses import dataclass, asdict, field
from typing import Optional, List, Dict, Tuple
from datetime import datetime
import logging

logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
logger = logging.getLogger(__name__)


# =============================================================================
# DATA MODELS
# =============================================================================

@dataclass
class PacientInfo:
    """Informații pacient extrase din buletin"""
    nume_prenume: str = ""
    cnp: str = ""
    data_nastere: Optional[str] = None
    varsta: Optional[str] = None
    sex: Optional[str] = None
    telefon: Optional[str] = None
    adresa: Optional[str] = None
    cod_pacient: Optional[str] = None


@dataclass
class BuletinInfo:
    """Informații despre buletin"""
    numar_buletin: str = ""
    data_recoltare: Optional[str] = None
    data_eliberare: Optional[str] = None
    data_validare: Optional[str] = None
    laborator: str = ""
    punct_recoltare: Optional[str] = None
    medic_trimitator: Optional[str] = None
    contract: Optional[str] = None


@dataclass
class AnalizaResult:
    """Model pentru o analiză medicală parsată"""
    categorie: str                          # HEMATOLOGIE, BIOCHIMIE, etc.
    nume_analiza: str                       # Hemoglobina (HGB)
    cod_analiza: Optional[str] = None       # HGB, RBC, ALT
    rezultat: str = ""                      # 14.3
    rezultat_numeric: Optional[float] = None
    unitate_masura: str = ""                # g/dL
    interval_referinta_text: str = ""       # [11.9 - 14.6]
    interval_min: Optional[float] = None    # 11.9
    interval_max: Optional[float] = None    # 14.6
    este_anormal: bool = False              # True dacă în afara limitelor
    directie_anormal: Optional[str] = None  # 'HIGH', 'LOW', None
    metoda: Optional[str] = None            # Metodă de analiză
    observatii: Optional[str] = None


@dataclass
class BuletinAnalize:
    """Structura completă a unui buletin de analize"""
    pacient: PacientInfo = field(default_factory=PacientInfo)
    buletin: BuletinInfo = field(default_factory=BuletinInfo)
    analize: List[AnalizaResult] = field(default_factory=list)
    raw_text: str = ""
    laborator_detectat: str = ""
    parse_warnings: List[str] = field(default_factory=list)


# =============================================================================
# REGEX PATTERNS
# =============================================================================

class Patterns:
    """Toate pattern-urile regex centralizate"""
    
    # Detectare laborator
    LABORATOR = {
        'Regina Maria': [r'REGINA\s+MARIA', r'regina\s*maria', r'reteaua\s+privata'],
        'ProMed': [r'PROMED\s+SRL', r'ProMed', r'policlinicapromed'],
        'MedLife': [r'MedLife', r'MEDLIFE', r'medlife\.ro'],
        'Synevo': [r'synevo', r'SYNEVO', r'Synevo\s+Romania'],
        'Bioclinica': [r'BIOCLINICA', r'Bioclinica'],
        'Clinica Sante': [r'Clinica\s+Sante', r'clinica-sante'],
        'SmartLabs': [r'SmartLabs', r'erpos'],
        'Elite Medical': [r'Elite\s+Medical', r'poliana\.ro'],
    }
    
    # Categorii analize
    CATEGORII = [
        'HEMATOLOGIE', 'BIOCHIMIE', 'IMUNOLOGIE', 'SEROLOGIE',
        'COAGULARE', 'HORMONI', 'ENDOCRINOLOGIE', 'MARKERI TUMORALI',
        'ANALIZE DE URINA', 'SUMAR URINA', 'EXAMEN URINA',
        'Hematologie', 'Biochimie', 'Imunologie', 'Coagulare',
        'Hemoleucograma', 'Formula leucocitara'
    ]
    
    # Intervale de referință
    INTERVAL_BRACKET = re.compile(r'\[([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)\]')
    INTERVAL_PAREN = re.compile(r'\(([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)\)')
    INTERVAL_SIMPLE = re.compile(r'^([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)$')
    INTERVAL_LESS = re.compile(r'^[<]\s*(\d+[.,]?\d*)$')
    INTERVAL_GREATER = re.compile(r'^[>]\s*(\d+[.,]?\d*)$')
    
    # Valoare cu = în față (Regina Maria)
    VALOARE_EGAL = re.compile(r'=\s*([<>]?\d+[.,]?\d*)')
    
    # Valoare numerică simplă
    VALOARE_NUMERIC = re.compile(r'^[<>]?\d+[.,]?\d*$')
    
    # Cod analiză în paranteză
    COD_ANALIZA = re.compile(r'\(([A-Z]{2,10}[%]?)\)')
    
    # Unități de măsură
    UNITATI = [
        'g/dL', 'g/dl', 'g/L', 'mg/dL', 'mg/dl', 'mg/L', 
        'µg/dL', 'ng/mL', 'pg/mL', 'pg/ml',
        'mmol/L', 'mmol/l', 'µmol/L', 'nmol/L', 'pmol/L',
        'mU/L', 'U/L', 'U/l', 'IU/L', 'mIU/mL', 'µIU/mL',
        'mil./µL', 'mii/µL', 'x10^6/µl', 'x10^3/µl', 
        '*10^6/µl', '*10^6/µL', 'x10^9/L', 'x10^12/L',
        '/mm³', '/mm3', 'mm/h', 's', 'sec', 
        '%', 'fl', 'fL', 'pg', 'µm³', 'µm^3',
        'mEq/L', 'mg%', 'UI/mL'
    ]
    
    # Date
    DATA_FORMAT = re.compile(r'(\d{2}[./]\d{2}[./]\d{4})')
    
    # CNP
    CNP = re.compile(r'\b(\d{13})\b')


# =============================================================================
# PARSER PRINCIPAL
# =============================================================================

class AnalizeMedicaleParserV2:
    """Parser universal pentru buletine de analize medicale - Versiunea 2"""
    
    def __init__(self):
        self.patterns = Patterns()
    
    # -------------------------------------------------------------------------
    # DETECTARE LABORATOR
    # -------------------------------------------------------------------------
    
    def detect_laborator(self, text: str) -> str:
        """Detectează laboratorul din text"""
        for lab_name, patterns in Patterns.LABORATOR.items():
            for pattern in patterns:
                if re.search(pattern, text, re.IGNORECASE):
                    return lab_name
        return "Necunoscut"
    
    # -------------------------------------------------------------------------
    # EXTRAGERE INFO PACIENT
    # -------------------------------------------------------------------------
    
    def extract_pacient(self, text: str) -> PacientInfo:
        """Extrage informațiile pacientului"""
        pacient = PacientInfo()
        
        # Nume - multiple formate
        patterns_nume = [
            r'Nume\s*(?:pacient)?[:\s]+([A-ZĂÂÎȘȚ][A-ZĂÂÎȘȚa-zăâîșț\s\-]+)',
            r'Nume[:\s]+([A-ZĂÂÎȘȚ][A-ZĂÂÎȘȚa-zăâîșț\s\-]+?)(?:\s+CNP|\s+Prenume|\n)',
        ]
        for pattern in patterns_nume:
            match = re.search(pattern, text)
            if match:
                pacient.nume_prenume = match.group(1).strip()
                break
        
        # Prenume separat
        match = re.search(r'Prenume[:\s]+([A-ZĂÂÎȘȚ][A-ZĂÂÎȘȚa-zăâîșț\s\-]+)', text)
        if match and pacient.nume_prenume:
            pacient.nume_prenume = f"{pacient.nume_prenume} {match.group(1).strip()}"
        
        # CNP
        match = Patterns.CNP.search(text)
        if match:
            pacient.cnp = match.group(1)
            # Extragem sexul din CNP
            if pacient.cnp[0] in ['1', '3', '5', '7']:
                pacient.sex = 'M'
            elif pacient.cnp[0] in ['2', '4', '6', '8']:
                pacient.sex = 'F'
        
        # Vârstă
        match = re.search(r'V[aâ]rst[aă][:\s]+(\d+\s*ani[,\s]*\d*\s*luni?)', text, re.IGNORECASE)
        if match:
            pacient.varsta = match.group(1)
        
        # Sex (dacă nu l-am dedus din CNP)
        if not pacient.sex:
            match = re.search(r'Sex[:\s]+([MF])', text)
            if match:
                pacient.sex = match.group(1)
        
        # Cod pacient
        match = re.search(r'Cod\s*pacient[:\s]+(\d+)', text)
        if match:
            pacient.cod_pacient = match.group(1)
        
        # Adresa
        match = re.search(r'Adresa[:\s]+(.+?)(?=\n|CNP|Tel|Varsta)', text)
        if match:
            pacient.adresa = match.group(1).strip()
        
        return pacient
    
    # -------------------------------------------------------------------------
    # EXTRAGERE INFO BULETIN
    # -------------------------------------------------------------------------
    
    def extract_buletin(self, text: str, laborator: str) -> BuletinInfo:
        """Extrage informațiile buletinului"""
        buletin = BuletinInfo()
        buletin.laborator = laborator
        
        # Număr buletin - multiple formate
        patterns_nr = [
            r'nr[.\s]*:?\s*(\d{6,12})',
            r'Nr\.\s*:?\s*(\d+)',
            r'Buletin\s+(?:de\s+)?analize?\s+(?:medicale\s+)?nr[.\s]*(\d+)',
            r'Cod\s+proba[:\s]+(\d+)',
        ]
        for pattern in patterns_nr:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                buletin.numar_buletin = match.group(1)
                break
        
        # Data recoltare
        match = re.search(r'(?:Data\s+)?[Rr]ecolt[aă](?:re|t)[:\s]+(\d{2}[./]\d{2}[./]\d{4})', text)
        if match:
            buletin.data_recoltare = match.group(1)
        
        # Data eliberare/rezultat
        match = re.search(r'(?:Data\s+)?(?:[Ee]liber|[Rr]ezultat)[:\s]+(\d{2}[./]\d{2}[./]\d{4})', text)
        if match:
            buletin.data_eliberare = match.group(1)
        
        # Data validare
        match = re.search(r'(?:Data\s+)?[Vv]alidar[ei][:\s]+(\d{2}[./]\d{2}[./]\d{4})', text)
        if match:
            buletin.data_validare = match.group(1)
        
        # Medic trimitător
        match = re.search(r'(?:Medic\s+)?[Tt]rimi[tț][aă]tor[:\s]+(?:Dr\.?\s*)?([A-ZĂÂÎȘȚ][A-ZĂÂÎȘȚa-zăâîșț\s\-]+)', text)
        if match:
            buletin.medic_trimitator = match.group(1).strip()
        
        # Punct recoltare
        match = re.search(r'(?:Punct|Unitate)\s+recolt(?:are)?[:\s]+(.+?)(?=\n|Tel|Adresa)', text)
        if match:
            buletin.punct_recoltare = match.group(1).strip()
        
        # Contract
        match = re.search(r'Contract[:\s]+(.+?)(?=\n)', text)
        if match:
            buletin.contract = match.group(1).strip()
        
        return buletin
    
    # -------------------------------------------------------------------------
    # PARSARE INTERVALE
    # -------------------------------------------------------------------------
    
    def parse_interval(self, text: str) -> Tuple[Optional[float], Optional[float], str]:
        """Parsează intervalul de referință și returnează (min, max, text_original)"""
        if not text:
            return None, None, ""
        
        text = text.strip()
        
        # Format [min - max]
        match = Patterns.INTERVAL_BRACKET.search(text)
        if match:
            try:
                min_val = float(match.group(1).replace(',', '.').replace('<', '').replace('>', ''))
                max_val = float(match.group(2).replace(',', '.').replace('<', '').replace('>', ''))
                return min_val, max_val, text
            except ValueError:
                pass
        
        # Format (min - max)
        match = Patterns.INTERVAL_PAREN.search(text)
        if match:
            try:
                # Handle format like (4.300.000 - 5.750.000)
                min_str = match.group(1).replace('.', '').replace(',', '.')
                max_str = match.group(2).replace('.', '').replace(',', '.')
                min_val = float(min_str)
                max_val = float(max_str)
                return min_val, max_val, text
            except ValueError:
                pass
        
        # Format simplu min - max
        match = Patterns.INTERVAL_SIMPLE.match(text)
        if match:
            try:
                min_val = float(match.group(1).replace(',', '.'))
                max_val = float(match.group(2).replace(',', '.'))
                return min_val, max_val, text
            except ValueError:
                pass
        
        # Format < valoare
        match = Patterns.INTERVAL_LESS.match(text)
        if match:
            try:
                max_val = float(match.group(1).replace(',', '.'))
                return None, max_val, text
            except ValueError:
                pass
        
        # Format > valoare
        match = Patterns.INTERVAL_GREATER.match(text)
        if match:
            try:
                min_val = float(match.group(1).replace(',', '.'))
                return min_val, None, text
            except ValueError:
                pass
        
        return None, None, text
    
    # -------------------------------------------------------------------------
    # PARSARE VALOARE
    # -------------------------------------------------------------------------
    
    def parse_valoare(self, text: str) -> Tuple[str, Optional[float]]:
        """Parsează valoarea și returnează (text, numeric)"""
        if not text:
            return "", None
        
        text = text.strip()
        
        # Remove = dacă există
        if text.startswith('='):
            text = text[1:].strip()
        
        # Încearcă parsare numerică
        try:
            # Handle format like 5.490.000 (thousand separators)
            clean = text.replace(' ', '')
            if clean.count('.') > 1:
                # Multiple dots = thousand separator
                clean = clean.replace('.', '')
            else:
                clean = clean.replace(',', '.')
            clean = clean.replace('<', '').replace('>', '')
            numeric = float(clean)
            return text, numeric
        except ValueError:
            return text, None
    
    # -------------------------------------------------------------------------
    # VERIFICARE ANORMAL
    # -------------------------------------------------------------------------
    
    def check_anormal(self, val: Optional[float], min_val: Optional[float], 
                      max_val: Optional[float]) -> Tuple[bool, Optional[str]]:
        """Verifică dacă valoarea e în afara limitelor"""
        if val is None:
            return False, None
        
        if min_val is not None and val < min_val:
            return True, 'LOW'
        if max_val is not None and val > max_val:
            return True, 'HIGH'
        
        return False, None
    
    # -------------------------------------------------------------------------
    # PARSARE LINIE ANALIZĂ
    # -------------------------------------------------------------------------
    
    def extract_cod_from_nume(self, nume: str) -> Tuple[str, Optional[str]]:
        """Extrage codul din numele analizei"""
        match = Patterns.COD_ANALIZA.search(nume)
        if match:
            return nume, match.group(1)
        return nume, None
    
    def find_unitate(self, text: str) -> str:
        """Găsește unitatea de măsură în text"""
        for um in Patterns.UNITATI:
            if um in text:
                return um
        return ""
    
    # -------------------------------------------------------------------------
    # PARSER UNIVERSAL (bazat pe tabel 4 coloane)
    # -------------------------------------------------------------------------
    
    def parse_analize_tabel(self, text: str, laborator: str) -> List[AnalizaResult]:
        """
        Parser universal pentru format tabel cu 4 coloane:
        Denumire | Rezultat | UM | Interval
        """
        analize = []
        current_categorie = "GENERAL"
        
        lines = text.split('\n')
        
        # Regex pentru linie de analiză tipică
        # Formatul: Nume (COD) ... valoare ... UM ... interval
        
        for i, line in enumerate(lines):
            line = line.strip()
            if len(line) < 3:
                continue
            
            # Detectare categorie
            for cat in Patterns.CATEGORII:
                if cat.upper() in line.upper() and len(line) < 50:
                    current_categorie = cat.upper()
                    break
            
            # Skip linii de header sau footer
            if any(skip in line.lower() for skip in ['denumire', 'rezultat', 'interval', 'pagina', 'disclaimer']):
                continue
            
            # Încercăm să găsim o analiză
            analiza = self._try_parse_line(line, lines, i, current_categorie, laborator)
            if analiza:
                analize.append(analiza)
        
        return analize
    
    def _try_parse_line(self, line: str, all_lines: List[str], idx: int, 
                        categorie: str, laborator: str) -> Optional[AnalizaResult]:
        """Încearcă să parseze o linie ca analiză"""
        
        # Pattern 1: Linie completă cu toate elementele
        # Exemplu: "Hemoglobina (HGB) = 14.3 g/dL [11.9 - 14.6]"
        
        # Găsim codul analizei
        cod_match = Patterns.COD_ANALIZA.search(line)
        cod = cod_match.group(1) if cod_match else None
        
        # Găsim valoarea (cu sau fără =)
        valoare_text = ""
        valoare_num = None
        
        # Căutăm = valoare
        egal_match = Patterns.VALOARE_EGAL.search(line)
        if egal_match:
            valoare_text, valoare_num = self.parse_valoare(egal_match.group(1))
        else:
            # Căutăm numere în linie
            numbers = re.findall(r'(?<![A-Za-z])(\d+[.,]?\d*)(?![A-Za-z\d])', line)
            if numbers:
                valoare_text, valoare_num = self.parse_valoare(numbers[0])
        
        # Găsim unitatea de măsură
        um = self.find_unitate(line)
        
        # Găsim intervalul
        interval_text = ""
        interval_min, interval_max = None, None
        
        for pattern in [Patterns.INTERVAL_BRACKET, Patterns.INTERVAL_PAREN]:
            match = pattern.search(line)
            if match:
                interval_min, interval_max, interval_text = self.parse_interval(match.group(0))
                break
        
        # Dacă nu am găsit interval în linie, căutăm în liniile apropiate
        if not interval_text:
            for offset in range(1, 4):
                if idx + offset < len(all_lines):
                    next_line = all_lines[idx + offset].strip()
                    for pattern in [Patterns.INTERVAL_BRACKET, Patterns.INTERVAL_PAREN, Patterns.INTERVAL_SIMPLE]:
                        match = pattern.search(next_line)
                        if match:
                            interval_min, interval_max, interval_text = self.parse_interval(match.group(0))
                            break
                    if interval_text:
                        break
        
        # Extragem numele analizei (tot ce e înainte de valoare/cod)
        nume = line
        if cod_match:
            # Păstrăm tot până la sfârșitul parantezei cu cod
            end_pos = cod_match.end()
            nume = line[:end_pos].strip()
        elif egal_match:
            nume = line[:egal_match.start()].strip()
        elif valoare_text and valoare_text in line:
            pos = line.find(valoare_text)
            if pos > 5:  # Trebuie să fie ceva înainte
                nume = line[:pos].strip()
        
        # Validare: trebuie să avem cel puțin nume și valoare
        if len(nume) > 3 and valoare_text:
            este_anormal, directie = self.check_anormal(valoare_num, interval_min, interval_max)
            
            return AnalizaResult(
                categorie=categorie,
                nume_analiza=nume,
                cod_analiza=cod,
                rezultat=valoare_text,
                rezultat_numeric=valoare_num,
                unitate_masura=um,
                interval_referinta_text=interval_text,
                interval_min=interval_min,
                interval_max=interval_max,
                este_anormal=este_anormal,
                directie_anormal=directie
            )
        
        return None
    
    # -------------------------------------------------------------------------
    # PARSER SPECIFIC - REGINA MARIA
    # -------------------------------------------------------------------------
    
    def parse_regina_maria(self, text: str) -> List[AnalizaResult]:
        """
        Parser pentru Regina Maria
        Format: Denumire | = Valoare | UM | [min - max]
        """
        analize = []
        current_categorie = "GENERAL"
        
        lines = text.split('\n')
        
        # Pattern pentru linie Regina Maria
        # Exemplu: "Numar de eritrocite (RBC) = 4.22 mil./µL [3.92 - 5.08]"
        pattern = re.compile(
            r'^(.+?)\s*'                           # Nume analiză
            r'=\s*([<>]?\d+[.,]?\d*)\s*'           # = Valoare
            r'([a-zA-Z/%µ\.\^0-9]+)?\s*'           # UM (opțional)
            r'(\[[^\]]+\])?'                       # [interval] (opțional)
        )
        
        for line in lines:
            line = line.strip()
            
            # Detectare categorie
            for cat in Patterns.CATEGORII:
                if cat.upper() in line.upper() and len(line) < 50:
                    current_categorie = cat.upper()
                    break
            
            # Căutăm = în linie
            if '=' not in line:
                continue
            
            match = pattern.match(line)
            if match:
                nume = match.group(1).strip()
                valoare_text, valoare_num = self.parse_valoare(match.group(2))
                um = match.group(3) or ""
                interval = match.group(4) or ""
                
                interval_min, interval_max, interval_text = self.parse_interval(interval)
                nume, cod = self.extract_cod_from_nume(nume)
                
                este_anormal, directie = self.check_anormal(valoare_num, interval_min, interval_max)
                
                analize.append(AnalizaResult(
                    categorie=current_categorie,
                    nume_analiza=nume,
                    cod_analiza=cod,
                    rezultat=valoare_text,
                    rezultat_numeric=valoare_num,
                    unitate_masura=um.strip(),
                    interval_referinta_text=interval_text,
                    interval_min=interval_min,
                    interval_max=interval_max,
                    este_anormal=este_anormal,
                    directie_anormal=directie
                ))
        
        return analize
    
    # -------------------------------------------------------------------------
    # PARSER SPECIFIC - BIOCLINICA
    # -------------------------------------------------------------------------
    
    def parse_bioclinica(self, text: str) -> List[AnalizaResult]:
        """
        Parser pentru Bioclinica
        Format: Denumire | Valoare /UM | (min - max)
        """
        analize = []
        current_categorie = "GENERAL"
        
        lines = text.split('\n')
        
        # Pattern pentru linie Bioclinica
        # Exemplu: "Hematii 5.490.000 /mm³ (4.300.000 - 5.750.000)"
        pattern = re.compile(
            r'^(.+?)\s+'                           # Nume
            r'(\d[\d.,]*)\s*'                      # Valoare
            r'/?([a-zA-Z/%µ\.\^³0-9]+)?\s*'        # UM
            r'\(([^)]+)\)?'                        # (interval)
        )
        
        for line in lines:
            line = line.strip()
            
            # Detectare categorie
            for cat in Patterns.CATEGORII:
                if cat.upper() in line.upper() and len(line) < 50:
                    current_categorie = cat.upper()
                    break
            
            match = pattern.match(line)
            if match:
                nume = match.group(1).strip()
                valoare_text, valoare_num = self.parse_valoare(match.group(2))
                um = match.group(3) or ""
                interval = f"({match.group(4)})" if match.group(4) else ""
                
                interval_min, interval_max, interval_text = self.parse_interval(interval)
                
                este_anormal, directie = self.check_anormal(valoare_num, interval_min, interval_max)
                
                analize.append(AnalizaResult(
                    categorie=current_categorie,
                    nume_analiza=nume,
                    rezultat=valoare_text,
                    rezultat_numeric=valoare_num,
                    unitate_masura=um.strip(),
                    interval_referinta_text=interval_text,
                    interval_min=interval_min,
                    interval_max=interval_max,
                    este_anormal=este_anormal,
                    directie_anormal=directie
                ))
        
        return analize
    
    # -------------------------------------------------------------------------
    # PARSER PRINCIPAL
    # -------------------------------------------------------------------------
    
    def parse_pdf(self, pdf_path: str) -> BuletinAnalize:
        """Parsează un PDF complet"""
        buletin = BuletinAnalize()
        
        try:
            # Extragem textul
            doc = fitz.open(pdf_path)
            full_text = ""
            for page in doc:
                full_text += page.get_text() + "\n"
            doc.close()
            
            buletin.raw_text = full_text
            
            # Detectăm laboratorul
            buletin.laborator_detectat = self.detect_laborator(full_text)
            
            # Extragem info pacient și buletin
            buletin.pacient = self.extract_pacient(full_text)
            buletin.buletin = self.extract_buletin(full_text, buletin.laborator_detectat)
            
            # Parsăm analizele
            if buletin.laborator_detectat == "Regina Maria":
                buletin.analize = self.parse_regina_maria(full_text)
            elif buletin.laborator_detectat == "Bioclinica":
                buletin.analize = self.parse_bioclinica(full_text)
            else:
                # Parser universal
                buletin.analize = self.parse_analize_tabel(full_text, buletin.laborator_detectat)
            
            logger.info(f"Parsate {len(buletin.analize)} analize din {Path(pdf_path).name}")
            
        except Exception as e:
            buletin.parse_warnings.append(f"Eroare: {str(e)}")
            logger.error(f"Eroare la parsare: {e}")
        
        return buletin
    
    # -------------------------------------------------------------------------
    # EXPORT
    # -------------------------------------------------------------------------
    
    def to_json(self, buletin: BuletinAnalize) -> str:
        """Export JSON complet"""
        data = {
            'pacient': asdict(buletin.pacient),
            'buletin': asdict(buletin.buletin),
            'analize': [asdict(a) for a in buletin.analize],
            'laborator_detectat': buletin.laborator_detectat,
            'numar_analize': len(buletin.analize),
            'parse_warnings': buletin.parse_warnings
        }
        return json.dumps(data, indent=2, ensure_ascii=False)
    
    def to_valyan_import(self, buletin: BuletinAnalize) -> List[Dict]:
        """Export în format ValyanClinic pentru import"""
        return [{
            'NumeAnaliza': a.nume_analiza,
            'CodAnaliza': a.cod_analiza,
            'TipAnaliza': a.categorie,
            'Valoare': a.rezultat,
            'ValoareNumerica': a.rezultat_numeric,
            'UnitatiMasura': a.unitate_masura,
            'ValoareNormalaMin': a.interval_min,
            'ValoareNormalaMax': a.interval_max,
            'ValoareNormalaText': a.interval_referinta_text,
            'EsteInAfaraLimitelor': a.este_anormal,
            'DirectieAnormal': a.directie_anormal,
            'Metoda': a.metoda,
            'DataRecoltare': buletin.buletin.data_recoltare,
            'Laborator': buletin.laborator_detectat,
            'NumarBuletin': buletin.buletin.numar_buletin
        } for a in buletin.analize]


# =============================================================================
# MAIN - TEST
# =============================================================================

def main():
    """Test parser cu PDF-urile din folder"""
    parser = AnalizeMedicaleParserV2()
    
    pdf_folder = Path(__file__).parent
    pdf_files = list(pdf_folder.glob("*.pdf"))
    
    if not pdf_files:
        print("Nu am găsit PDF-uri în folder.")
        return
    
    print(f"\n{'='*80}")
    print(f"PARSER ANALIZE MEDICALE v2 - Test")
    print(f"{'='*80}")
    print(f"PDF-uri găsite: {len(pdf_files)}")
    
    for pdf_path in pdf_files:
        print(f"\n{'─'*80}")
        print(f"📄 {pdf_path.name}")
        print(f"{'─'*80}")
        
        buletin = parser.parse_pdf(str(pdf_path))
        
        print(f"🏥 Laborator: {buletin.laborator_detectat}")
        print(f"👤 Pacient: {buletin.pacient.nume_prenume or 'N/A'}")
        print(f"📋 Nr. buletin: {buletin.buletin.numar_buletin or 'N/A'}")
        print(f"📅 Data recoltare: {buletin.buletin.data_recoltare or 'N/A'}")
        print(f"✅ Analize găsite: {len(buletin.analize)}")
        
        if buletin.analize:
            print(f"\n  {'Nr':<3} {'Categorie':<12} {'Analiză':<35} {'Rezultat':<10} {'UM':<12} {'Interval':<15} {'Status'}")
            print(f"  {'─'*3} {'─'*12} {'─'*35} {'─'*10} {'─'*12} {'─'*15} {'─'*8}")
            
            for i, a in enumerate(buletin.analize[:20], 1):
                status = "⚠️" if a.este_anormal else "✓"
                dir_str = f"({a.directie_anormal})" if a.directie_anormal else ""
                print(f"  {i:<3} {a.categorie[:12]:<12} {a.nume_analiza[:35]:<35} {a.rezultat[:10]:<10} {a.unitate_masura[:12]:<12} {a.interval_referinta_text[:15]:<15} {status} {dir_str}")
            
            if len(buletin.analize) > 20:
                print(f"  ... și încă {len(buletin.analize) - 20} analize")
        
        # Salvăm JSON
        output_path = pdf_path.with_suffix('.v2.json')
        import_data = parser.to_valyan_import(buletin)
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(import_data, f, indent=2, ensure_ascii=False)
        print(f"\n💾 JSON salvat: {output_path.name}")


if __name__ == "__main__":
    main()
