"""
Parser Universal pentru Buletine de Analize Medicale (România)
==============================================================
Suportă multiple formate de laboratoare:
- Clinica Sante (Buzău)
- Synevo
- MedLife
- Regina Maria
- Bioclinica

Structura output: JSON pentru import în ValyanClinic
"""

import fitz  # PyMuPDF
import re
import json
from pathlib import Path
from dataclasses import dataclass, asdict, field
from typing import Optional, List, Dict
from datetime import datetime
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


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


@dataclass
class BuletinInfo:
    """Informații despre buletin"""
    numar_buletin: str = ""
    data_raport: Optional[str] = None
    data_recoltare: Optional[str] = None
    laborator: str = ""
    medic_trimitator: Optional[str] = None
    loc_recoltare: Optional[str] = None


@dataclass
class AnalizaResult:
    """Model pentru o analiză medicală parsată"""
    categorie: str                          # HEMATOLOGIE, BIOCHIMIE, etc.
    nume_analiza: str                       # Hemoglobina (HGB)
    cod_analiza: Optional[str] = None       # Cod intern laborator
    rezultat: str = ""                      # 10.6
    rezultat_numeric: Optional[float] = None
    unitate_masura: str = ""                # g/dL
    interval_referinta_text: str = ""       # [11.5 - 16]
    interval_min: Optional[float] = None    # 11.5
    interval_max: Optional[float] = None    # 16
    este_anormal: bool = False              # True dacă în afara limitelor
    directie_anormal: Optional[str] = None  # 'HIGH', 'LOW', None
    nota: Optional[str] = None              # Note/comentarii
    metoda: Optional[str] = None            # Metodă de analiză


@dataclass
class BuletinAnalize:
    """Structura completă a unui buletin de analize"""
    pacient: PacientInfo = field(default_factory=PacientInfo)
    buletin: BuletinInfo = field(default_factory=BuletinInfo)
    analize: List[AnalizaResult] = field(default_factory=list)
    raw_text: str = ""
    laborator_detectat: str = ""
    parse_errors: List[str] = field(default_factory=list)


class LaboratorPattern:
    """Pattern-uri pentru un laborator specific"""
    def __init__(self, name: str):
        self.name = name
        self.header_patterns = []
        self.analiza_patterns = []
        self.categorie_patterns = []
        

class ClinicaSantePattern(LaboratorPattern):
    """Pattern-uri pentru Clinica Sante"""
    def __init__(self):
        super().__init__("Clinica Sante")
        
        # Pattern-uri pentru detectarea laboratorului
        self.detection_patterns = [
            r"Clinica\s+Sante",
            r"clinica-sante\.ro",
            r"analizeonline\.ro"
        ]
        
        # Categorii cunoscute
        self.categorii = [
            "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "SEROLOGIE",
            "COAGULARE", "HORMONI", "MARKERI TUMORALI", "URINA",
            "SUMAR URINA", "EXAMEN URINĂ", "EXAMEN URINA"
        ]


class SynevoPattern(LaboratorPattern):
    """Pattern-uri pentru Synevo"""
    def __init__(self):
        super().__init__("Synevo")
        self.detection_patterns = [
            r"SYNEVO",
            r"synevo\.ro"
        ]
        self.categorii = [
            "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "SEROLOGIE",
            "COAGULARE", "ENDOCRINOLOGIE", "MARKERI TUMORALI"
        ]


class MedLifePattern(LaboratorPattern):
    """Pattern-uri pentru MedLife"""
    def __init__(self):
        super().__init__("MedLife")
        self.detection_patterns = [
            r"MedLife",
            r"medlife\.ro"
        ]
        self.categorii = [
            "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "SEROLOGIE"
        ]


class SmartLabsPattern(LaboratorPattern):
    """Pattern-uri pentru SmartLabs (erpos)"""
    def __init__(self):
        super().__init__("SmartLabs")
        self.detection_patterns = [
            r"SmartLabs",
            r"erpos",
            r"Sysmex\s+XT",
            r"Konelab"
        ]
        self.categorii = [
            "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "ANALIZE DE URINA",
            "COAGULARE", "SEROLOGIE"
        ]


class EliteMedicalPattern(LaboratorPattern):
    """Pattern-uri pentru Elite Medical / Poliana (Brașov)"""
    def __init__(self):
        super().__init__("Elite Medical")
        self.detection_patterns = [
            r"Elite\s+Medical",
            r"poliana\.ro",
            r"Poliana",
            r"SR\s+EN\s+ISO\s+15189"
        ]
        self.categorii = [
            "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "COAGULARE",
            "HORMONI", "MARKERI TUMORALI"
        ]


class AnalizeMedicaleParser:
    """Parser universal pentru buletine de analize medicale"""
    
    def __init__(self):
        self.patterns = [
            ClinicaSantePattern(),
            SynevoPattern(),
            MedLifePattern(),
            SmartLabsPattern(),
            EliteMedicalPattern()
        ]
        
        # Regex patterns comune
        self.interval_pattern = re.compile(
            r'\[?\s*([<>]?\d+[.,]?\d*)\s*[-–—]\s*([<>]?\d+[.,]?\d*)\s*\]?'
        )
        
        self.numeric_pattern = re.compile(r'^[<>]?\d+[.,]?\d*$')
        
        # Unități de măsură cunoscute
        self.unitati_masura = [
            'g/dL', 'g/L', 'mg/dL', 'mg/L', 'µg/dL', 'ng/mL', 'pg/mL',
            'mmol/L', 'µmol/L', 'nmol/L', 'pmol/L',
            'mU/L', 'U/L', 'IU/L', 'mIU/mL', 'µIU/mL',
            'x10^6/mm3', 'x10^3/mm3', 'x10^9/L', 'x10^12/L',
            'mm/h', 's', 'sec', '%', 'fl', 'fL', 'pg', 'µm^3',
            'mEq/L', 'mg%', 'UI/mL'
        ]
        
        # Categorii standard
        self.categorii_standard = [
            "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "SEROLOGIE",
            "COAGULARE", "HORMONI", "ENDOCRINOLOGIE", "MARKERI TUMORALI",
            "URINA", "SUMAR URINA", "EXAMEN URINĂ"
        ]

    def detect_laborator(self, text: str) -> str:
        """Detectează laboratorul din text"""
        for pattern in self.patterns:
            for regex in pattern.detection_patterns:
                if re.search(regex, text, re.IGNORECASE):
                    logger.info(f"Laborator detectat: {pattern.name}")
                    return pattern.name
        return "Necunoscut"

    def extract_pacient_info(self, text: str) -> PacientInfo:
        """Extrage informațiile pacientului"""
        pacient = PacientInfo()
        
        # Nume/Prenume
        match = re.search(r'Nume[/\s]?Prenume[:\s]+([A-ZĂÂÎȘȚa-zăâîșț\s-]+)', text)
        if match:
            pacient.nume_prenume = match.group(1).strip()
        
        # CNP
        match = re.search(r'CNP[:\s]+(\d{13})', text)
        if match:
            pacient.cnp = match.group(1)
            # Extragem sexul din CNP
            if pacient.cnp[0] in ['1', '3', '5', '7']:
                pacient.sex = 'M'
            elif pacient.cnp[0] in ['2', '4', '6', '8']:
                pacient.sex = 'F'
        
        # Vârsta
        match = re.search(r'Varsta[:\s]+(\d+\s*ani[,\s]*\d*\s*luni?)', text, re.IGNORECASE)
        if match:
            pacient.varsta = match.group(1)
        
        # Telefon
        match = re.search(r'Tel[:\s]+(\d{10,12})', text)
        if match:
            pacient.telefon = match.group(1)
        
        # Adresa
        match = re.search(r'Adresa[:\s]+(.+?)(?=\n|CNP|Tel|Data)', text)
        if match:
            pacient.adresa = match.group(1).strip()
        
        return pacient

    def extract_buletin_info(self, text: str) -> BuletinInfo:
        """Extrage informațiile buletinului"""
        buletin = BuletinInfo()
        buletin.laborator = self.detect_laborator(text)
        
        # Număr buletin
        match = re.search(r'(?:Buletin|nr\.?)[:\s]*(\d{5,10})', text, re.IGNORECASE)
        if match:
            buletin.numar_buletin = match.group(1)
        
        # Data raport
        match = re.search(r'Data\s+raportului[:\s]+(\d{2}[./]\d{2}[./]\d{4})', text)
        if match:
            buletin.data_raport = match.group(1)
        
        # Data recoltare
        match = re.search(r'Data\s+(?:si\s+ora\s+)?recoltare[:\s]+(\d{2}[./]\d{2}[./]\d{4})', text)
        if match:
            buletin.data_recoltare = match.group(1)
        
        # Medic trimitător
        match = re.search(r'Medic\s+trimitator[:\s]+([A-Za-zĂÂÎȘȚăâîșț\s]+)', text)
        if match:
            buletin.medic_trimitator = match.group(1).strip()
        
        # Loc recoltare
        match = re.search(r'Recoltat\s+la[:\s]+([A-Za-zĂÂÎȘȚăâîșț\s]+)', text)
        if match:
            buletin.loc_recoltare = match.group(1).strip()
        
        return buletin

    def parse_interval(self, text: str) -> tuple:
        """Parsează intervalul de referință"""
        match = self.interval_pattern.search(text)
        if match:
            try:
                min_val = float(match.group(1).replace(',', '.').replace('<', '').replace('>', ''))
                max_val = float(match.group(2).replace(',', '.').replace('<', '').replace('>', ''))
                return min_val, max_val
            except ValueError:
                pass
        return None, None

    def is_numeric(self, text: str) -> bool:
        """Verifică dacă textul e numeric"""
        return bool(self.numeric_pattern.match(text.strip()))

    def parse_numeric(self, text: str) -> Optional[float]:
        """Parsează o valoare numerică"""
        try:
            clean = text.strip().replace(',', '.').replace('<', '').replace('>', '')
            return float(clean)
        except ValueError:
            return None

    def check_abnormal(self, rezultat: float, min_val: Optional[float], max_val: Optional[float]) -> tuple:
        """Verifică dacă rezultatul e în afara limitelor"""
        if rezultat is None:
            return False, None
        
        if min_val is not None and rezultat < min_val:
            return True, 'LOW'
        if max_val is not None and rezultat > max_val:
            return True, 'HIGH'
        
        return False, None

    def parse_clinica_sante(self, text: str) -> List[AnalizaResult]:
        """
        Parser specific pentru Clinica Sante
        Format (pe linii separate):
        UM (g/dL)
        Valoare (10.6)
        Nume analiză (Hemoglobina (HGB))
        [min - max] (interval)
        """
        analize = []
        current_categorie = "NECUNOSCUT"
        
        lines = text.split('\n')
        i = 0
        
        # Pattern pentru interval
        interval_regex = re.compile(r'^\[([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)\]$')
        
        # Pattern pentru detectarea numelui analizei (conține paranteze cu cod sau text lung)
        nume_analiza_pattern = re.compile(r'^([A-Za-zĂÂÎȘȚăâîșț\s.\-]+)\s*\(([A-Z0-9%\-]+)\)\s*$')
        
        while i < len(lines):
            line = lines[i].strip()
            
            if len(line) < 2:
                i += 1
                continue
            
            # Detectăm categoria
            for cat in self.categorii_standard:
                if cat.upper() in line.upper():
                    current_categorie = cat
                    break
            
            # Căutăm interval de referință care marchează sfârșitul unei analize
            int_match = interval_regex.match(line)
            if int_match:
                try:
                    interval_min = float(int_match.group(1).replace(',', '.'))
                    interval_max = float(int_match.group(2).replace(',', '.'))
                    interval_text = line
                    
                    # Căutăm înapoi pentru a găsi: UM, Valoare, Nume
                    # Structura tipică (de jos în sus):
                    # [interval] <- suntem aici
                    # empty line
                    # UM
                    # Valoare
                    # Nume analiza
                    
                    um = ""
                    rezultat = None
                    nume = ""
                    
                    # Scanăm liniile anterioare
                    search_idx = i - 1
                    found_parts = []
                    
                    while search_idx >= max(0, i - 8) and len(found_parts) < 4:
                        prev_line = lines[search_idx].strip()
                        search_idx -= 1
                        
                        if not prev_line:
                            continue
                        
                        # Skip dacă e alt interval sau categorie
                        if interval_regex.match(prev_line):
                            break
                        if any(cat.upper() in prev_line.upper() for cat in self.categorii_standard):
                            break
                        
                        found_parts.insert(0, prev_line)
                    
                    # Procesăm părțile găsite
                    # Tipic: [UM, Valoare, Nume] sau [Valoare, Nume]
                    for j, part in enumerate(found_parts):
                        # Verificăm dacă e valoare numerică
                        if self.is_numeric(part) and rezultat is None:
                            rezultat = part.replace(',', '.')
                        # Verificăm dacă e unitate de măsură cunoscută
                        elif any(unit in part for unit in self.unitati_masura):
                            um = part
                        # Verificăm dacă e nume analiză (conține paranteze sau text lung)
                        elif '(' in part and ')' in part:
                            nume = part
                        elif len(part) > 10 and not self.is_numeric(part):
                            nume = part
                    
                    if nume and rezultat:
                        rez_numeric = self.parse_numeric(rezultat)
                        este_anormal, directie = self.check_abnormal(rez_numeric, interval_min, interval_max)
                        
                        analize.append(AnalizaResult(
                            categorie=current_categorie,
                            nume_analiza=nume,
                            rezultat=rezultat,
                            rezultat_numeric=rez_numeric,
                            unitate_masura=um,
                            interval_referinta_text=interval_text,
                            interval_min=interval_min,
                            interval_max=interval_max,
                            este_anormal=este_anormal,
                            directie_anormal=directie
                        ))
                except ValueError:
                    pass
            
            i += 1
        
        return analize

    def parse_generic(self, text: str) -> List[AnalizaResult]:
        """Parser generic pentru format necunoscut"""
        # Folosim parsing-ul pentru Clinica Sante ca bază
        return self.parse_clinica_sante(text)

    def parse_smartlabs(self, text: str) -> List[AnalizaResult]:
        """
        Parser pentru SmartLabs (erpos)
        Format: Nr. Denumire analiza | Rezultat / U.M. | Valori biologice de referinta / U.M.
        Exemplu:
        1. Hemoleucograma completa
        (RBC) Hematii
        4,76  x10^6/µl
        4,2-5,5x10^6/µl
        """
        analize = []
        current_categorie = "NECUNOSCUT"
        
        lines = text.split('\n')
        i = 0
        
        # Pattern pentru categorii
        categorie_pattern = re.compile(r'^(HEMATOLOGIE|BIOCHIMIE|IMUNOLOGIE|ANALIZE DE URINA|COAGULARE|SEROLOGIE)\s*$', re.IGNORECASE)
        
        # Pattern pentru linie cu denumire analiză (cod) Nume
        analiza_cod_pattern = re.compile(r'^\(([A-Z]+)\)\s+(.+)$')
        
        # Pattern pentru valoare și UM
        valoare_um_pattern = re.compile(r'^([<>]?\d+[.,]?\d*)\s+(.+)$')
        
        # Pattern pentru interval de referință
        interval_pattern = re.compile(r'^([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)(.*)$')
        
        while i < len(lines):
            line = lines[i].strip()
            
            if len(line) < 2:
                i += 1
                continue
            
            # Detectăm categoria
            cat_match = categorie_pattern.match(line)
            if cat_match:
                current_categorie = cat_match.group(1).upper()
                i += 1
                continue
            
            # Detectăm analiză cu cod în paranteză
            cod_match = analiza_cod_pattern.match(line)
            if cod_match:
                cod = cod_match.group(1)
                nume = cod_match.group(2).strip()
                
                # Căutăm valoarea în linia următoare
                rezultat = None
                um = ""
                interval_min = None
                interval_max = None
                interval_text = ""
                
                if i + 1 < len(lines):
                    val_line = lines[i + 1].strip()
                    val_match = valoare_um_pattern.match(val_line)
                    if val_match:
                        rezultat = val_match.group(1).replace(',', '.')
                        um = val_match.group(2).strip()
                
                # Căutăm intervalul în linia următoare
                if i + 2 < len(lines):
                    int_line = lines[i + 2].strip()
                    int_match = interval_pattern.match(int_line)
                    if int_match:
                        try:
                            interval_min = float(int_match.group(1).replace(',', '.').replace('<', '').replace('>', ''))
                            interval_max = float(int_match.group(2).replace(',', '.').replace('<', '').replace('>', ''))
                            interval_text = f"[{int_match.group(1)} - {int_match.group(2)}]"
                        except ValueError:
                            pass
                
                if rezultat:
                    rez_numeric = self.parse_numeric(rezultat)
                    este_anormal, directie = self.check_abnormal(rez_numeric, interval_min, interval_max)
                    
                    analize.append(AnalizaResult(
                        categorie=current_categorie,
                        nume_analiza=f"{nume} ({cod})",
                        cod_analiza=cod,
                        rezultat=rezultat,
                        rezultat_numeric=rez_numeric,
                        unitate_masura=um,
                        interval_referinta_text=interval_text,
                        interval_min=interval_min,
                        interval_max=interval_max,
                        este_anormal=este_anormal,
                        directie_anormal=directie
                    ))
                    i += 3  # Skip liniile procesate
                    continue
            
            i += 1
        
        return analize

    def parse_elite_medical(self, text: str) -> List[AnalizaResult]:
        """
        Parser pentru Elite Medical / Poliana
        Format: Analize | Rezultate | Interval biologic de referinta | UM
        Exemplu:
           Numar de eritrocite (RBC)
        =   4.53 mil./µL
                                  [4.44 - 5.61] / mil./µL
        """
        analize = []
        current_categorie = "NECUNOSCUT"
        
        lines = text.split('\n')
        i = 0
        
        # Pattern pentru categorii
        categorie_pattern = re.compile(r'^(Hematologie|Biochimie|Imunologie|Coagulare|Hormoni|Markeri\s+tumorali)\s*$', re.IGNORECASE)
        
        # Pattern pentru denumire analiză cu cod în paranteză
        analiza_pattern = re.compile(r'^\s*(.+?)\s*\(([A-Z0-9%-]+)\)\s*$')
        
        # Pattern pentru rezultat cu = în față
        rezultat_pattern = re.compile(r'^=\s*([<>]?\d+[.,]?\d*)\s+(.+)$')
        
        # Pattern pentru interval [min - max] / UM
        interval_um_pattern = re.compile(r'\[([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)\]\s*/\s*(.+)$')
        
        while i < len(lines):
            line = lines[i].strip()
            
            if len(line) < 2:
                i += 1
                continue
            
            # Detectăm categoria
            cat_match = categorie_pattern.match(line)
            if cat_match:
                current_categorie = cat_match.group(1).upper()
                i += 1
                continue
            
            # Detectăm analiză
            ana_match = analiza_pattern.match(line)
            if ana_match:
                nume = ana_match.group(1).strip()
                cod = ana_match.group(2)
                
                # Căutăm rezultatul în liniile următoare
                rezultat = None
                um = ""
                interval_min = None
                interval_max = None
                interval_text = ""
                
                for offset in range(1, 4):
                    if i + offset >= len(lines):
                        break
                    
                    check_line = lines[i + offset].strip()
                    
                    # Căutăm rezultat
                    if rezultat is None:
                        rez_match = rezultat_pattern.match(check_line)
                        if rez_match:
                            rezultat = rez_match.group(1).replace(',', '.')
                            um = rez_match.group(2).strip()
                    
                    # Căutăm interval
                    int_match = interval_um_pattern.search(check_line)
                    if int_match:
                        try:
                            interval_min = float(int_match.group(1).replace(',', '.'))
                            interval_max = float(int_match.group(2).replace(',', '.'))
                            interval_text = f"[{int_match.group(1)} - {int_match.group(2)}]"
                            # UM poate fi mai precis din interval
                            if int_match.group(3):
                                um = int_match.group(3).strip()
                        except ValueError:
                            pass
                
                if rezultat:
                    rez_numeric = self.parse_numeric(rezultat)
                    este_anormal, directie = self.check_abnormal(rez_numeric, interval_min, interval_max)
                    
                    analize.append(AnalizaResult(
                        categorie=current_categorie,
                        nume_analiza=f"{nume} ({cod})",
                        cod_analiza=cod,
                        rezultat=rezultat,
                        rezultat_numeric=rez_numeric,
                        unitate_masura=um,
                        interval_referinta_text=interval_text,
                        interval_min=interval_min,
                        interval_max=interval_max,
                        este_anormal=este_anormal,
                        directie_anormal=directie
                    ))
            
            i += 1
        
        return analize

    def parse_pdf(self, pdf_path: str) -> BuletinAnalize:
        """Parsează un PDF și returnează structura completă"""
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
            buletin.pacient = self.extract_pacient_info(full_text)
            buletin.buletin = self.extract_buletin_info(full_text)
            
            # Parsăm analizele în funcție de laborator
            if buletin.laborator_detectat == "Clinica Sante":
                buletin.analize = self.parse_clinica_sante(full_text)
            elif buletin.laborator_detectat == "SmartLabs":
                buletin.analize = self.parse_smartlabs(full_text)
            elif buletin.laborator_detectat == "Elite Medical":
                buletin.analize = self.parse_elite_medical(full_text)
            else:
                buletin.analize = self.parse_generic(full_text)
            
            logger.info(f"Parsate {len(buletin.analize)} analize din {pdf_path}")
            
        except Exception as e:
            buletin.parse_errors.append(str(e))
            logger.error(f"Eroare la parsare: {e}")
        
        return buletin

    def to_json(self, buletin: BuletinAnalize) -> str:
        """Convertește buletinul în JSON"""
        data = {
            'pacient': asdict(buletin.pacient),
            'buletin': asdict(buletin.buletin),
            'analize': [asdict(a) for a in buletin.analize],
            'laborator_detectat': buletin.laborator_detectat,
            'numar_analize': len(buletin.analize),
            'parse_errors': buletin.parse_errors
        }
        return json.dumps(data, indent=2, ensure_ascii=False)

    def to_import_format(self, buletin: BuletinAnalize) -> List[Dict]:
        """Convertește în format pentru import în ValyanClinic"""
        import_data = []
        
        for analiza in buletin.analize:
            import_data.append({
                'NumeAnaliza': analiza.nume_analiza,
                'CodAnaliza': analiza.cod_analiza,
                'TipAnaliza': analiza.categorie,
                'Valoare': analiza.rezultat,
                'ValoareNumerica': analiza.rezultat_numeric,
                'UnitatiMasura': analiza.unitate_masura,
                'ValoareNormalaMin': analiza.interval_min,
                'ValoareNormalaMax': analiza.interval_max,
                'ValoareNormalaText': analiza.interval_referinta_text,
                'EsteInAfaraLimitelor': analiza.este_anormal,
                'DirectieAnormal': analiza.directie_anormal,
                'Metoda': analiza.metoda,
                'DataRecoltare': buletin.buletin.data_recoltare,
                'Laborator': buletin.laborator_detectat,
                'NumarBuletin': buletin.buletin.numar_buletin
            })
        
        return import_data


def main():
    """Test parser cu toate fișierele PDF din folder"""
    parser = AnalizeMedicaleParser()
    
    pdf_folder = Path(__file__).parent
    pdf_files = [
        "AnalizeMedicale.pdf",
        "1111200901011bolnavul.pdf",  # SmartLabs
        "analize-b-51-ro.pdf"          # Elite Medical / Poliana
    ]
    
    for pdf_name in pdf_files:
        pdf_path = pdf_folder / pdf_name
        
        if not pdf_path.exists():
            print(f"⚠️ Nu există: {pdf_name}")
            continue
        
        print(f"\n{'='*80}")
        print(f"📄 Procesare: {pdf_name}")
        print("=" * 80)
        
        buletin = parser.parse_pdf(str(pdf_path))
        
        print(f"\n🏥 Laborator detectat: {buletin.laborator_detectat}")
        print(f"👤 Pacient: {buletin.pacient.nume_prenume}")
        print(f"📋 CNP: {buletin.pacient.cnp}")
        print(f"📊 Număr buletin: {buletin.buletin.numar_buletin}")
        print(f"📅 Data raport: {buletin.buletin.data_raport}")
        print(f"\n✅ Analize găsite: {len(buletin.analize)}")
        
        if buletin.analize:
            print("\n" + "-" * 60)
            print("ANALIZE PARSATE:")
            print("-" * 60)
            
            for i, a in enumerate(buletin.analize[:15], 1):  # Limităm la primele 15
                status = "⚠️" if a.este_anormal else "✓"
                dir_str = f" ({a.directie_anormal})" if a.directie_anormal else ""
                print(f"{i:2}. {status} [{a.categorie[:10]:10}] {a.nume_analiza[:30]:30} = {a.rezultat:8} {a.unitate_masura:10} {a.interval_referinta_text}{dir_str}")
            
            if len(buletin.analize) > 15:
                print(f"    ... și încă {len(buletin.analize) - 15} analize")
        
        # Salvăm JSON pentru import
        output_path = pdf_path.with_suffix('.import.json')
        import_data = parser.to_import_format(buletin)
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(import_data, f, indent=2, ensure_ascii=False)
        print(f"\n💾 JSON salvat: {output_path.name}")


if __name__ == "__main__":
    main()
