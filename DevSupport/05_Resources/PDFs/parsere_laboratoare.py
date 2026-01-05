"""
Parsere Modulare pentru Buletine de Analize Medicale
=====================================================
Fiecare laborator are parserul său specific.
Utilizatorul selectează laboratorul din dropdown.

Laboratoare suportate:
1. Regina Maria
2. ProMed  
3. MedLife
4. Synevo
5. Bioclinica
6. Clinica Sante
7. SmartLabs
8. Elite Medical / Poliana
"""

import re
import json
from abc import ABC, abstractmethod
from dataclasses import dataclass, asdict, field
from typing import Optional, List, Dict, Tuple
from pathlib import Path

try:
    import fitz  # PyMuPDF
    HAS_FITZ = True
except ImportError:
    HAS_FITZ = False
    print("PyMuPDF nu este instalat. Instalează cu: pip install pymupdf")


# =============================================================================
# DATA MODELS
# =============================================================================

@dataclass
class AnalizaResult:
    """Model unificat pentru o analiză"""
    categorie: str = ""
    nume_analiza: str = ""
    cod_analiza: Optional[str] = None
    rezultat: str = ""
    rezultat_numeric: Optional[float] = None
    unitate_masura: str = ""
    interval_min: Optional[float] = None
    interval_max: Optional[float] = None
    interval_text: str = ""
    este_anormal: bool = False
    directie_anormal: Optional[str] = None  # 'HIGH', 'LOW'


@dataclass
class BuletinResult:
    """Rezultatul parsării unui buletin"""
    laborator: str = ""
    numar_buletin: str = ""
    data_recoltare: str = ""
    pacient_nume: str = ""
    pacient_cnp: str = ""
    analize: List[AnalizaResult] = field(default_factory=list)
    warnings: List[str] = field(default_factory=list)


# =============================================================================
# PARSER ABSTRACT
# =============================================================================

class LaboratorParser(ABC):
    """Clasă abstractă pentru parsere de laborator"""
    
    NAME: str = "Abstract"
    DESCRIPTION: str = ""
    
    # Unități de măsură comune
    UNITATI = [
        'g/dL', 'g/dl', 'g/L', 'mg/dL', 'mg/dl', 'mg/L',
        'µg/dL', 'ng/mL', 'pg/mL', 'pg/ml', 'ng/ml',
        'mmol/L', 'mmol/l', 'µmol/L', 'nmol/L', 'pmol/L',
        'mU/L', 'U/L', 'U/l', 'IU/L', 'mIU/mL', 'µIU/mL',
        'mil./µL', 'mii/µL', 'x10^6/µl', 'x10^3/µl',
        '*10^6/µl', '*10^6/µL', 'x10^9/L', 'x10^12/L',
        '/mm³', '/mm3', 'mm/h', 'sec', 's',
        '%', 'fl', 'fL', 'pg', 'µm³', 'µm^3',
        'mEq/L', 'mg%', 'UI/mL'
    ]
    
    CATEGORII = [
        'HEMATOLOGIE', 'BIOCHIMIE', 'IMUNOLOGIE', 'SEROLOGIE',
        'COAGULARE', 'HORMONI', 'ENDOCRINOLOGIE', 'MARKERI TUMORALI',
        'ANALIZE DE URINA', 'SUMAR URINA', 'VSH'
    ]
    
    def parse_text(self, text: str) -> BuletinResult:
        """Parsează textul extras din PDF"""
        result = BuletinResult(laborator=self.NAME)
        result = self._extract_header_info(text, result)
        result.analize = self._parse_analize(text)
        return result
    
    def parse_pdf(self, pdf_path: str) -> BuletinResult:
        """Parsează un fișier PDF"""
        if not HAS_FITZ:
            result = BuletinResult(laborator=self.NAME)
            result.warnings.append("PyMuPDF nu este instalat")
            return result
        
        doc = fitz.open(pdf_path)
        text = ""
        for page in doc:
            text += page.get_text() + "\n"
        doc.close()
        
        return self.parse_text(text)
    
    @abstractmethod
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        """Parsează analizele - implementat de fiecare subclasă"""
        pass
    
    def _extract_header_info(self, text: str, result: BuletinResult) -> BuletinResult:
        """Extrage informații header comune"""
        # Număr buletin
        for pattern in [r'nr[.\s:]+(\d{5,12})', r'Nr\.\s*:?\s*(\d+)']:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                result.numar_buletin = match.group(1)
                break
        
        # Data recoltare
        match = re.search(r'[Rr]ecolt[aă](?:re|t)[:\s]+(\d{2}[./]\d{2}[./]\d{4})', text)
        if match:
            result.data_recoltare = match.group(1)
        
        # CNP
        match = re.search(r'\b(\d{13})\b', text)
        if match:
            result.pacient_cnp = match.group(1)
        
        # Nume pacient
        match = re.search(r'Nume[:\s]+([A-ZĂÂÎȘȚ][A-ZĂÂÎȘȚa-zăâîșț\s\-]+)', text)
        if match:
            result.pacient_nume = match.group(1).strip()
        
        return result
    
    def _parse_numeric(self, text: str) -> Tuple[str, Optional[float]]:
        """Parsează o valoare numerică"""
        if not text:
            return "", None
        text = text.strip().replace(',', '.')
        text_clean = text.replace('<', '').replace('>', '').replace('=', '').strip()
        try:
            return text, float(text_clean)
        except ValueError:
            return text, None
    
    def _parse_interval(self, text: str) -> Tuple[Optional[float], Optional[float]]:
        """Parsează interval [min - max] sau (min - max)"""
        if not text:
            return None, None
        
        # [min - max] sau (min - max)
        match = re.search(r'[\[(]([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)[\])]', text)
        if match:
            try:
                min_v = float(match.group(1).replace(',', '.').replace('<', '').replace('>', ''))
                max_v = float(match.group(2).replace(',', '.').replace('<', '').replace('>', ''))
                return min_v, max_v
            except ValueError:
                pass
        
        # min - max simplu
        match = re.search(r'^(\d+[.,]?\d*)\s*[-–]\s*(\d+[.,]?\d*)$', text.strip())
        if match:
            try:
                return float(match.group(1).replace(',', '.')), float(match.group(2).replace(',', '.'))
            except ValueError:
                pass
        
        # < valoare
        match = re.search(r'^[<]\s*(\d+[.,]?\d*)$', text.strip())
        if match:
            try:
                return None, float(match.group(1).replace(',', '.'))
            except ValueError:
                pass
        
        return None, None
    
    def _check_anormal(self, val: Optional[float], min_v: Optional[float], max_v: Optional[float]) -> Tuple[bool, Optional[str]]:
        """Verifică dacă valoarea e în afara limitelor"""
        if val is None:
            return False, None
        if min_v is not None and val < min_v:
            return True, 'LOW'
        if max_v is not None and val > max_v:
            return True, 'HIGH'
        return False, None
    
    def _find_unitate(self, text: str) -> str:
        """Găsește unitatea de măsură"""
        for um in self.UNITATI:
            if um in text:
                return um
        return ""
    
    def _detect_categorie(self, line: str, current: str) -> str:
        """Detectează categoria din linie"""
        line_upper = line.upper().strip()
        for cat in self.CATEGORII:
            if cat in line_upper and len(line) < 60:
                return cat
        return current


# =============================================================================
# PARSER: REGINA MARIA
# =============================================================================

class ReginaMariaParser(LaboratorParser):
    """
    Parser pentru Regina Maria
    Format: Denumire (COD) = Valoare UM [min - max]
    """
    NAME = "Regina Maria"
    DESCRIPTION = "Format: Denumire (COD) = Valoare UM [min - max]"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        
        # Pattern: Nume (COD) = valoare UM [interval]
        pattern = re.compile(
            r'^\s*(.+?)\s*'                    # Nume
            r'=\s*([<>]?\d+[.,]?\d*)\s*'       # = Valoare
            r'([a-zA-Z/%µ\.\^0-9]+)?\s*'       # UM
            r'(\[[^\]]+\])?',                  # [interval]
            re.MULTILINE
        )
        
        for line in text.split('\n'):
            current_cat = self._detect_categorie(line, current_cat)
            
            if '=' not in line:
                continue
            
            match = pattern.match(line.strip())
            if match:
                nume = match.group(1).strip()
                val_text, val_num = self._parse_numeric(match.group(2))
                um = (match.group(3) or "").strip()
                interval = match.group(4) or ""
                
                min_v, max_v = self._parse_interval(interval)
                anormal, directie = self._check_anormal(val_num, min_v, max_v)
                
                # Extrage cod din nume
                cod = None
                cod_match = re.search(r'\(([A-Z]{2,10})\)', nume)
                if cod_match:
                    cod = cod_match.group(1)
                
                analize.append(AnalizaResult(
                    categorie=current_cat,
                    nume_analiza=nume,
                    cod_analiza=cod,
                    rezultat=val_text,
                    rezultat_numeric=val_num,
                    unitate_masura=um,
                    interval_min=min_v,
                    interval_max=max_v,
                    interval_text=interval,
                    este_anormal=anormal,
                    directie_anormal=directie
                ))
        
        return analize


# =============================================================================
# PARSER: PROMED
# =============================================================================

class ProMedParser(LaboratorParser):
    """
    Parser pentru ProMed
    Format tabel: Nr | Denumire test | Rezultat | U.M. | Interval
    """
    NAME = "ProMed"
    DESCRIPTION = "Format tabel: Nr. | Denumire | Rezultat | U.M. | Interval"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        
        # Pattern pentru linie cu număr la început
        pattern = re.compile(
            r'^\s*(\d+)\s+'                    # Nr.
            r'(.+?)\s+'                        # Denumire
            r'(\d+[.,]?\d*)\s+'                # Rezultat
            r'([a-zA-Z/%µ]+)\s+'               # U.M.
            r'(\d+[.,]?\d*\s*[-–]\s*\d+[.,]?\d*)',  # Interval
            re.MULTILINE
        )
        
        for line in text.split('\n'):
            current_cat = self._detect_categorie(line, current_cat)
            
            match = pattern.match(line.strip())
            if match:
                nume = match.group(2).strip()
                val_text, val_num = self._parse_numeric(match.group(3))
                um = match.group(4).strip()
                interval = match.group(5)
                
                min_v, max_v = self._parse_interval(interval)
                anormal, directie = self._check_anormal(val_num, min_v, max_v)
                
                analize.append(AnalizaResult(
                    categorie=current_cat,
                    nume_analiza=nume,
                    rezultat=val_text,
                    rezultat_numeric=val_num,
                    unitate_masura=um,
                    interval_min=min_v,
                    interval_max=max_v,
                    interval_text=interval,
                    este_anormal=anormal,
                    directie_anormal=directie
                ))
        
        return analize


# =============================================================================
# PARSER: MEDLIFE
# =============================================================================

class MedLifeParser(LaboratorParser):
    """
    Parser pentru MedLife
    Format: Test | Rezultat | UM | min - max UM
    """
    NAME = "MedLife"
    DESCRIPTION = "Format: Test | Rezultat | UM | Interval"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        lines = text.split('\n')
        
        for i, line in enumerate(lines):
            current_cat = self._detect_categorie(line, current_cat)
            line = line.strip()
            
            # Căutăm pattern: Nume analiza urmată de valoare numerică
            # Exemplu: "Nr. eritrocite    4.38    *10^6/µl    3.8 - 5.3 *10^6/µl"
            
            # Găsim toate numerele din linie
            numbers = re.findall(r'(\d+[.,]?\d*)', line)
            if len(numbers) >= 1:
                # Verificăm dacă linia pare să fie o analiză
                um = self._find_unitate(line)
                if um:
                    # Extragem numele (tot ce e înainte de primul număr)
                    first_num_pos = line.find(numbers[0])
                    if first_num_pos > 5:
                        nume = line[:first_num_pos].strip()
                        val_text, val_num = self._parse_numeric(numbers[0])
                        
                        # Căutăm interval
                        interval_match = re.search(r'(\d+[.,]?\d*)\s*[-–]\s*(\d+[.,]?\d*)', line)
                        min_v, max_v = None, None
                        interval_text = ""
                        if interval_match:
                            min_v, max_v = self._parse_interval(interval_match.group(0))
                            interval_text = interval_match.group(0)
                        
                        anormal, directie = self._check_anormal(val_num, min_v, max_v)
                        
                        # Validare: numele trebuie să arate a analiză
                        if len(nume) > 3 and not nume[0].isdigit():
                            analize.append(AnalizaResult(
                                categorie=current_cat,
                                nume_analiza=nume,
                                rezultat=val_text,
                                rezultat_numeric=val_num,
                                unitate_masura=um,
                                interval_min=min_v,
                                interval_max=max_v,
                                interval_text=interval_text,
                                este_anormal=anormal,
                                directie_anormal=directie
                            ))
        
        return analize


# =============================================================================
# PARSER: SYNEVO
# =============================================================================

class SynevoParser(LaboratorParser):
    """
    Parser pentru Synevo
    Format: Denumire | Rezultat | UM | Interval (cu indicator 23 pentru anormal)
    """
    NAME = "Synevo"
    DESCRIPTION = "Format: Denumire | Rezultat | UM | Interval"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        lines = text.split('\n')
        
        for i, line in enumerate(lines):
            current_cat = self._detect_categorie(line, current_cat)
            line = line.strip()
            
            # Skip linii scurte sau de header
            if len(line) < 10:
                continue
            if any(skip in line.lower() for skip in ['denumire', 'rezultat', 'interval', 'pagina']):
                continue
            
            # Pattern Synevo: poate avea "23" la început pentru anormal
            is_anormal_marker = line.startswith('23')
            if is_anormal_marker:
                line = line[2:].strip()
            
            # Căutăm valori numerice
            numbers = re.findall(r'(\d+[.,]?\d*)', line)
            um = self._find_unitate(line)
            
            if numbers and um:
                first_num_pos = line.find(numbers[0])
                if first_num_pos > 3:
                    nume = line[:first_num_pos].strip()
                    val_text, val_num = self._parse_numeric(numbers[0])
                    
                    # Interval
                    min_v, max_v = None, None
                    interval_text = ""
                    
                    # Format < valoare
                    less_match = re.search(r'<\s*(\d+[.,]?\d*)', line)
                    if less_match:
                        max_v = float(less_match.group(1).replace(',', '.'))
                        interval_text = f"< {less_match.group(1)}"
                    else:
                        interval_match = re.search(r'(\d+[.,]?\d*)\s*[-–]\s*(\d+[.,]?\d*)', line)
                        if interval_match:
                            min_v, max_v = self._parse_interval(interval_match.group(0))
                            interval_text = interval_match.group(0)
                    
                    anormal, directie = self._check_anormal(val_num, min_v, max_v)
                    if is_anormal_marker:
                        anormal = True
                    
                    if len(nume) > 2 and not nume[0].isdigit():
                        analize.append(AnalizaResult(
                            categorie=current_cat,
                            nume_analiza=nume,
                            rezultat=val_text,
                            rezultat_numeric=val_num,
                            unitate_masura=um,
                            interval_min=min_v,
                            interval_max=max_v,
                            interval_text=interval_text,
                            este_anormal=anormal,
                            directie_anormal=directie
                        ))
        
        return analize


# =============================================================================
# PARSER: BIOCLINICA
# =============================================================================

class BioclinicaParser(LaboratorParser):
    """
    Parser pentru Bioclinica
    Format: Denumire | Valoare /UM | (min - max)
    """
    NAME = "Bioclinica"
    DESCRIPTION = "Format: Denumire | Valoare /UM | (min - max)"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        
        for line in text.split('\n'):
            current_cat = self._detect_categorie(line, current_cat)
            line = line.strip()
            
            # Pattern Bioclinica: Nume valoare /um (interval)
            # Exemplu: "Hematii 5.490.000 /mm³ (4.300.000 - 5.750.000)"
            
            match = re.match(
                r'^(.+?)\s+'                      # Nume
                r'([\d.,]+)\s*'                   # Valoare (poate avea . ca separator mii)
                r'/?([a-zA-Z/%µ³0-9]+)?\s*'       # UM
                r'\(([^)]+)\)?',                  # (interval)
                line
            )
            
            if match:
                nume = match.group(1).strip()
                
                # Parsează valoarea (poate fi 5.490.000)
                val_str = match.group(2).replace('.', '').replace(',', '.')
                val_text = match.group(2)
                try:
                    val_num = float(val_str)
                except ValueError:
                    val_num = None
                
                um = (match.group(3) or "").strip()
                interval = f"({match.group(4)})" if match.group(4) else ""
                
                min_v, max_v = self._parse_interval(interval)
                anormal, directie = self._check_anormal(val_num, min_v, max_v)
                
                if len(nume) > 2:
                    analize.append(AnalizaResult(
                        categorie=current_cat,
                        nume_analiza=nume,
                        rezultat=val_text,
                        rezultat_numeric=val_num,
                        unitate_masura=um,
                        interval_min=min_v,
                        interval_max=max_v,
                        interval_text=interval,
                        este_anormal=anormal,
                        directie_anormal=directie
                    ))
        
        return analize


# =============================================================================
# PARSER: CLINICA SANTE
# =============================================================================

class ClinicaSanteParser(LaboratorParser):
    """
    Parser pentru Clinica Sante
    Format vertical (ordinea liniilor):
    1. Nume analiza (COD)
    2. [interval min - max]
    3. (linie goală)
    4. UM
    5. valoare
    """
    NAME = "Clinica Sante"
    DESCRIPTION = "Format vertical: Nume → [Interval] → UM → Valoare"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        lines = text.split('\n')
        
        # Pattern pentru interval
        interval_pattern = re.compile(r'^\[([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)\]$')
        # Pattern pentru nume analiză (conține paranteze cu cod)
        nume_pattern = re.compile(r'^([A-Za-zĂÂÎȘȚăâîșț\s.\-]+)\s*\(([A-Z0-9\-%]+)\)\s*$')
        # Pattern pentru valoare (număr cu spații)
        valoare_pattern = re.compile(r'^\s*(\d+[.,]?\d*)\s*$')
        
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            
            # Detectare categorie
            current_cat = self._detect_categorie(line, current_cat)
            
            # Căutăm nume analiză cu cod în paranteză
            nume_match = nume_pattern.match(line)
            if nume_match:
                nume = line
                cod = nume_match.group(2)
                
                # Căutăm intervalul în linia următoare
                interval_min, interval_max = None, None
                interval_text = ""
                um = ""
                valoare_text = ""
                valoare_num = None
                
                # Scanăm următoarele 5 linii pentru interval, UM și valoare
                for offset in range(1, 6):
                    if i + offset >= len(lines):
                        break
                    
                    next_line = lines[i + offset].strip()
                    
                    # Interval?
                    int_match = interval_pattern.match(next_line)
                    if int_match and not interval_text:
                        interval_min = float(int_match.group(1).replace(',', '.'))
                        interval_max = float(int_match.group(2).replace(',', '.'))
                        interval_text = next_line
                        continue
                    
                    # UM?
                    if not um and self._find_unitate(next_line):
                        um = next_line.strip()
                        continue
                    
                    # Valoare numerică?
                    val_match = valoare_pattern.match(lines[i + offset])  # Cu spații originale
                    if val_match and not valoare_text:
                        valoare_text, valoare_num = self._parse_numeric(val_match.group(1))
                        break  # Am găsit valoarea, oprim căutarea
                
                # Validare și adăugare
                if valoare_text and interval_text:
                    anormal, directie = self._check_anormal(valoare_num, interval_min, interval_max)
                    
                    analize.append(AnalizaResult(
                        categorie=current_cat,
                        nume_analiza=nume,
                        cod_analiza=cod,
                        rezultat=valoare_text,
                        rezultat_numeric=valoare_num,
                        unitate_masura=um,
                        interval_min=interval_min,
                        interval_max=interval_max,
                        interval_text=interval_text,
                        este_anormal=anormal,
                        directie_anormal=directie
                    ))
            
            i += 1
        
        return analize


# =============================================================================
# PARSER: SMARTLABS
# =============================================================================

class SmartLabsParser(LaboratorParser):
    """
    Parser pentru SmartLabs
    Format: (COD) Nume / Valoare UM / Interval UM
    """
    NAME = "SmartLabs"
    DESCRIPTION = "Format: (COD) Nume | Valoare UM | Interval UM"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        lines = text.split('\n')
        
        # Pattern pentru linie cu cod
        cod_pattern = re.compile(r'^\(([A-Z]+)\)\s+(.+)$')
        
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            current_cat = self._detect_categorie(line, current_cat)
            
            cod_match = cod_pattern.match(line)
            if cod_match:
                cod = cod_match.group(1)
                nume = cod_match.group(2).strip()
                
                # Următoarea linie: valoare UM
                if i + 1 < len(lines):
                    val_line = lines[i + 1].strip()
                    val_match = re.match(r'^([\d.,]+)\s+(.+)$', val_line)
                    if val_match:
                        val_text, val_num = self._parse_numeric(val_match.group(1))
                        um = val_match.group(2).strip()
                        
                        # Următoarea: interval
                        min_v, max_v = None, None
                        interval_text = ""
                        if i + 2 < len(lines):
                            int_line = lines[i + 2].strip()
                            int_match = re.match(r'^([\d.,]+)\s*[-–]\s*([\d.,]+)', int_line)
                            if int_match:
                                min_v = float(int_match.group(1).replace(',', '.'))
                                max_v = float(int_match.group(2).replace(',', '.'))
                                interval_text = int_match.group(0)
                        
                        anormal, directie = self._check_anormal(val_num, min_v, max_v)
                        
                        analize.append(AnalizaResult(
                            categorie=current_cat,
                            nume_analiza=f"{nume} ({cod})",
                            cod_analiza=cod,
                            rezultat=val_text,
                            rezultat_numeric=val_num,
                            unitate_masura=um,
                            interval_min=min_v,
                            interval_max=max_v,
                            interval_text=interval_text,
                            este_anormal=anormal,
                            directie_anormal=directie
                        ))
                        i += 2
            i += 1
        
        return analize


# =============================================================================
# PARSER: ELITE MEDICAL / POLIANA
# =============================================================================

class EliteMedicalParser(LaboratorParser):
    """
    Parser pentru Elite Medical / Poliana
    Format: Nume (COD) / = Valoare UM / [min - max] / UM
    """
    NAME = "Elite Medical"
    DESCRIPTION = "Format: Nume (COD) | = Valoare UM | [min - max] / UM"
    
    def _parse_analize(self, text: str) -> List[AnalizaResult]:
        analize = []
        current_cat = "GENERAL"
        lines = text.split('\n')
        
        # Pattern pentru linie cu = valoare
        val_pattern = re.compile(r'^=\s*([\d.,]+)\s+(.+)$')
        # Pattern pentru interval
        int_pattern = re.compile(r'\[([\d.,]+)\s*[-–]\s*([\d.,]+)\]')
        
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            current_cat = self._detect_categorie(line, current_cat)
            
            # Căutăm linie cu denumire care conține (COD)
            if '(' in line and ')' in line and '=' not in line and '[' not in line:
                nume = line.strip()
                cod_match = re.search(r'\(([A-Z%0-9-]+)\)', nume)
                cod = cod_match.group(1) if cod_match else None
                
                # Căutăm = valoare în liniile următoare
                for offset in range(1, 4):
                    if i + offset >= len(lines):
                        break
                    next_line = lines[i + offset].strip()
                    
                    val_match = val_pattern.match(next_line)
                    if val_match:
                        val_text, val_num = self._parse_numeric(val_match.group(1))
                        um = val_match.group(2).strip()
                        
                        # Căutăm interval
                        min_v, max_v = None, None
                        interval_text = ""
                        for offset2 in range(offset + 1, offset + 4):
                            if i + offset2 >= len(lines):
                                break
                            int_line = lines[i + offset2].strip()
                            int_match = int_pattern.search(int_line)
                            if int_match:
                                min_v = float(int_match.group(1).replace(',', '.'))
                                max_v = float(int_match.group(2).replace(',', '.'))
                                interval_text = f"[{int_match.group(1)} - {int_match.group(2)}]"
                                break
                        
                        anormal, directie = self._check_anormal(val_num, min_v, max_v)
                        
                        analize.append(AnalizaResult(
                            categorie=current_cat,
                            nume_analiza=nume,
                            cod_analiza=cod,
                            rezultat=val_text,
                            rezultat_numeric=val_num,
                            unitate_masura=um,
                            interval_min=min_v,
                            interval_max=max_v,
                            interval_text=interval_text,
                            este_anormal=anormal,
                            directie_anormal=directie
                        ))
                        break
            
            i += 1
        
        return analize


# =============================================================================
# REGISTRY - Lista tuturor parserelor
# =============================================================================

PARSERS: Dict[str, LaboratorParser] = {
    'regina_maria': ReginaMariaParser(),
    'promed': ProMedParser(),
    'medlife': MedLifeParser(),
    'synevo': SynevoParser(),
    'bioclinica': BioclinicaParser(),
    'clinica_sante': ClinicaSanteParser(),
    'smartlabs': SmartLabsParser(),
    'elite_medical': EliteMedicalParser(),
}

def get_parser(laborator_key: str) -> Optional[LaboratorParser]:
    """Returnează parserul pentru un laborator"""
    return PARSERS.get(laborator_key)

def list_parsers() -> List[Dict[str, str]]:
    """Lista laboratoarelor disponibile pentru dropdown"""
    return [
        {'key': key, 'name': parser.NAME, 'description': parser.DESCRIPTION}
        for key, parser in PARSERS.items()
    ]

def to_valyan_format(result: BuletinResult) -> List[Dict]:
    """Convertește în format ValyanClinic pentru import"""
    return [{
        'NumeAnaliza': a.nume_analiza,
        'CodAnaliza': a.cod_analiza,
        'TipAnaliza': a.categorie,
        'Valoare': a.rezultat,
        'ValoareNumerica': a.rezultat_numeric,
        'UnitatiMasura': a.unitate_masura,
        'ValoareNormalaMin': a.interval_min,
        'ValoareNormalaMax': a.interval_max,
        'ValoareNormalaText': a.interval_text,
        'EsteInAfaraLimitelor': a.este_anormal,
        'DirectieAnormal': a.directie_anormal,
        'DataRecoltare': result.data_recoltare,
        'Laborator': result.laborator,
        'NumarBuletin': result.numar_buletin
    } for a in result.analize]


# =============================================================================
# MAIN - TEST
# =============================================================================

def main():
    print("\n" + "="*70)
    print("PARSERE MODULARE - LABORATOARE DISPONIBILE")
    print("="*70)
    
    for info in list_parsers():
        print(f"\n  📋 {info['key']}")
        print(f"     Nume: {info['name']}")
        print(f"     Format: {info['description']}")
    
    # Test cu PDF-uri existente
    pdf_folder = Path(__file__).parent
    pdf_files = list(pdf_folder.glob("*.pdf"))
    
    if pdf_files:
        print(f"\n{'='*70}")
        print("TEST CU PDF-URI DIN FOLDER")
        print("="*70)
        
        # Mapping manual pentru test
        test_mapping = {
            '1111200901011bolnavul.pdf': 'smartlabs',
            'analize-b-51-ro.pdf': 'elite_medical',
            'AnalizeMedicale.pdf': 'clinica_sante',
        }
        
        for pdf_path in pdf_files:
            parser_key = test_mapping.get(pdf_path.name)
            if not parser_key:
                continue
            
            parser = get_parser(parser_key)
            if not parser:
                continue
            
            print(f"\n{'─'*60}")
            print(f"📄 {pdf_path.name} → Parser: {parser.NAME}")
            print(f"{'─'*60}")
            
            result = parser.parse_pdf(str(pdf_path))
            
            print(f"  Nr. buletin: {result.numar_buletin}")
            print(f"  Data recoltare: {result.data_recoltare}")
            print(f"  Analize găsite: {len(result.analize)}")
            
            if result.analize:
                print(f"\n  {'Analiză':<40} {'Valoare':<10} {'UM':<12} {'Interval':<15} {'St'}")
                for a in result.analize[:10]:
                    status = "⚠️" if a.este_anormal else "✓"
                    print(f"  {a.nume_analiza[:40]:<40} {a.rezultat:<10} {a.unitate_masura:<12} {a.interval_text:<15} {status}")
                
                if len(result.analize) > 10:
                    print(f"  ... și încă {len(result.analize) - 10} analize")


if __name__ == "__main__":
    main()
