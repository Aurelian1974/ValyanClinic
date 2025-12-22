"""
Parser avansat pentru buletine de analize medicale
Extrage: ANALIZE, REZULTATE, UM, INTERVAL BIOLOGIC DE REFERINTA
Output: JSON structurat pentru import în baza de date
"""

import fitz  # PyMuPDF
import re
import json
from pathlib import Path
from dataclasses import dataclass, asdict
from typing import Optional, List

@dataclass
class AnalyzaResult:
    """Model pentru o analiză medicală"""
    categorie: str  # Ex: HEMATOLOGIE, BIOCHIMIE
    nume_analiza: str  # Ex: Hemoglobina (HGB)
    rezultat: str  # Ex: 10.6
    unitate_masura: str  # Ex: g/dL
    interval_referinta_min: Optional[float]  # Ex: 11.5
    interval_referinta_max: Optional[float]  # Ex: 16
    interval_referinta_text: str  # Ex: [11.5 - 16]
    in_afara_limitelor: bool  # True dacă rezultatul e în afara limitelor

def extract_text_from_pdf(pdf_path: str) -> str:
    """Extrage textul din PDF"""
    doc = fitz.open(pdf_path)
    full_text = ""
    for page in doc:
        full_text += page.get_text() + "\n"
    doc.close()
    return full_text

def parse_interval(interval_text: str) -> tuple:
    """Parsează intervalul de referință [min - max]"""
    match = re.search(r'\[?\s*([<>]?\d+\.?\d*)\s*[-–]\s*([<>]?\d+\.?\d*)\s*\]?', interval_text)
    if match:
        try:
            min_val = float(match.group(1).replace('<', '').replace('>', ''))
            max_val = float(match.group(2).replace('<', '').replace('>', ''))
            return min_val, max_val
        except:
            pass
    return None, None

def parse_analize_from_text(text: str) -> List[AnalyzaResult]:
    """Parsează textul pentru a extrage analizele structurate"""
    analize = []
    current_categorie = "NECUNOSCUT"
    
    # Categorii cunoscute
    categorii = [
        "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "SEROLOGIE", 
        "COAGULARE", "HORMONI", "MARKERI TUMORALI", "URINA",
        "GLICEMIE", "LIPIDE", "HEPATIC", "RENAL", "TIROIDIAN"
    ]
    
    lines = text.split('\n')
    i = 0
    
    while i < len(lines):
        line = lines[i].strip()
        
        # Detectăm categoria
        for cat in categorii:
            if cat in line.upper():
                current_categorie = cat
                break
        
        # Pattern pentru analiză: NUME   VALOARE   UM   [INTERVAL]
        # Sau uneori valorile sunt pe linii separate
        
        # Pattern complex pentru linie completă
        # Ex: "Hemoglobina (HGB)   10.6   g/dL   [11.5 - 16]"
        pattern = r'^(.+?)\s+(\d+\.?\d*)\s+([a-zA-Z%/^µ\d\s]+)\s+\[(.+?)\]'
        match = re.match(pattern, line)
        
        if match:
            nume = match.group(1).strip()
            rezultat = match.group(2)
            um = match.group(3).strip()
            interval_text = f"[{match.group(4)}]"
            min_val, max_val = parse_interval(interval_text)
            
            # Verificăm dacă rezultatul e în afara limitelor
            try:
                rez_val = float(rezultat)
                in_afara = (min_val and rez_val < min_val) or (max_val and rez_val > max_val)
            except:
                in_afara = False
            
            analize.append(AnalyzaResult(
                categorie=current_categorie,
                nume_analiza=nume,
                rezultat=rezultat,
                unitate_masura=um,
                interval_referinta_min=min_val,
                interval_referinta_max=max_val,
                interval_referinta_text=interval_text,
                in_afara_limitelor=in_afara
            ))
        else:
            # Încercăm să găsim pattern pentru linie cu nume + interval pe aceeași linie
            # și valoare pe linia următoare sau anterioară
            
            # Pattern: Nume analiza   [interval]
            pattern2 = r'^(.+?)\s+\[([^\]]+)\]$'
            match2 = re.match(pattern2, line)
            
            if match2:
                nume = match2.group(1).strip()
                interval_text = f"[{match2.group(2)}]"
                
                # Căutăm valoarea și UM în liniile apropiate
                for j in range(max(0, i-3), min(len(lines), i+3)):
                    if j != i:
                        val_match = re.match(r'^\s*(\d+\.?\d*)\s*$', lines[j].strip())
                        if val_match:
                            rezultat = val_match.group(1)
                            min_val, max_val = parse_interval(interval_text)
                            
                            try:
                                rez_val = float(rezultat)
                                in_afara = (min_val and rez_val < min_val) or (max_val and rez_val > max_val)
                            except:
                                in_afara = False
                            
                            # Găsim UM în alte linii
                            um = ""
                            for k in range(max(0, i-3), min(len(lines), i+3)):
                                um_match = re.match(r'^([a-zA-Z%/^µ\d\s]+)$', lines[k].strip())
                                if um_match and len(lines[k].strip()) < 20:
                                    um = lines[k].strip()
                                    break
                            
                            analize.append(AnalyzaResult(
                                categorie=current_categorie,
                                nume_analiza=nume,
                                rezultat=rezultat,
                                unitate_masura=um,
                                interval_referinta_min=min_val,
                                interval_referinta_max=max_val,
                                interval_referinta_text=interval_text,
                                in_afara_limitelor=in_afara
                            ))
                            break
        
        i += 1
    
    return analize

def parse_analize_clinica_sante(text: str) -> List[AnalyzaResult]:
    """Parser specific pentru formatul Clinica Sante"""
    analize = []
    current_categorie = "NECUNOSCUT"
    
    lines = text.split('\n')
    
    # Formatul Clinica Sante:
    # ANALIZE -> UM -> REZULTATE -> INTERVAL
    # Dar în text apar intercalat
    
    # Pattern pentru detectare analiză: Nume (Abreviere)
    analiza_pattern = r'^([A-Za-zăâîșțĂÂÎȘȚ\s\.\-]+)\s*\(([A-Z]+(?:-[A-Z]+)?)\)$'
    interval_pattern = r'^\[([^\]]+)\]$'
    value_pattern = r'^\s*(\d+\.?\d*)\s*$'
    um_patterns = [
        r'^(x10\^[36]/\s*mm3)$',
        r'^(g/dL)$',
        r'^(%)$',
        r'^(µm\^3)$',
        r'^(pg)$',
        r'^(mm/h)$',
        r'^(mg/dL)$',
        r'^(U/L)$',
        r'^(mL/min)$',
        r'^([a-zA-Z%/^µ\d\s]{2,15})$'
    ]
    
    # Categorii
    for i, line in enumerate(lines):
        line = line.strip()
        if 'HEMATOLOGIE' in line.upper():
            current_categorie = 'HEMATOLOGIE'
        elif 'BIOCHIMIE' in line.upper():
            current_categorie = 'BIOCHIMIE'
        elif 'IMUNOLOGIE' in line.upper():
            current_categorie = 'IMUNOLOGIE'
    
    # Găsim toate analizele și valorile lor
    for i, line in enumerate(lines):
        line = line.strip()
        
        # Pattern simplu: Nume analiza și în liniile următoare găsim interval, valoare, UM
        # Format observat:
        # Hemoglobina (HGB)
        # [11.5 - 16]
        # 
        # g/dL
        #  10.6
        
        match = re.match(analiza_pattern, line)
        if match:
            nume = f"{match.group(1).strip()} ({match.group(2)})"
            
            # Căutăm în următoarele 10 linii: interval, valoare, UM
            interval_text = ""
            rezultat = ""
            um = ""
            
            for j in range(i+1, min(i+12, len(lines))):
                next_line = lines[j].strip()
                
                # Interval [x - y]
                if re.match(interval_pattern, next_line):
                    interval_text = next_line
                
                # Valoare (număr)
                val_match = re.match(value_pattern, next_line)
                if val_match and not rezultat:
                    rezultat = val_match.group(1)
                
                # UM
                for um_pat in um_patterns[:-1]:  # Excludem ultimul pattern generic
                    if re.match(um_pat, next_line):
                        um = next_line
                        break
                
                # Stop dacă găsim altă analiză
                if re.match(analiza_pattern, next_line) and j > i:
                    break
            
            if rezultat and interval_text:
                min_val, max_val = parse_interval(interval_text)
                
                try:
                    rez_val = float(rezultat)
                    in_afara = (min_val is not None and rez_val < min_val) or (max_val is not None and rez_val > max_val)
                except:
                    in_afara = False
                
                analize.append(AnalyzaResult(
                    categorie=current_categorie,
                    nume_analiza=nume,
                    rezultat=rezultat,
                    unitate_masura=um,
                    interval_referinta_min=min_val,
                    interval_referinta_max=max_val,
                    interval_referinta_text=interval_text,
                    in_afara_limitelor=in_afara
                ))
    
    return analize

def main():
    pdf_path = r"D:\Lucru\CMS\DevSupport\05_Resources\PDFs\42381_Disp.Prim. nr.54 din 14.02.2024 -stabilire stimulent educatonal.pdf"
    
    print("=" * 80)
    print("PARSER ANALIZE MEDICALE - Clinica Sante Format")
    print("=" * 80)
    
    # Extrage text
    text = extract_text_from_pdf(pdf_path)
    print(f"\nText extras: {len(text)} caractere")
    
    # Parsează analize
    analize = parse_analize_clinica_sante(text)
    
    print(f"\nAnalize găsite: {len(analize)}")
    print("\n" + "=" * 80)
    print("REZULTATE STRUCTURATE:")
    print("=" * 80)
    
    # Afișăm rezultatele
    for i, a in enumerate(analize, 1):
        flag = " ⚠️" if a.in_afara_limitelor else ""
        print(f"\n{i}. {a.nume_analiza}{flag}")
        print(f"   Categorie: {a.categorie}")
        print(f"   Rezultat: {a.rezultat} {a.unitate_masura}")
        print(f"   Interval: {a.interval_referinta_text}")
        if a.in_afara_limitelor:
            print(f"   ⚠️ VALOARE ÎN AFARA LIMITELOR!")
    
    # Salvăm ca JSON
    output_json = Path(pdf_path).parent / "analize_parsed.json"
    with open(output_json, 'w', encoding='utf-8') as f:
        json.dump([asdict(a) for a in analize], f, ensure_ascii=False, indent=2)
    
    print(f"\n{'=' * 80}")
    print(f"JSON salvat în: {output_json}")
    
    # Statistici
    print(f"\n{'=' * 80}")
    print("STATISTICI:")
    print(f"  Total analize: {len(analize)}")
    print(f"  În afara limitelor: {sum(1 for a in analize if a.in_afara_limitelor)}")
    
    categorii = {}
    for a in analize:
        categorii[a.categorie] = categorii.get(a.categorie, 0) + 1
    print(f"  Pe categorii: {categorii}")

if __name__ == "__main__":
    main()
