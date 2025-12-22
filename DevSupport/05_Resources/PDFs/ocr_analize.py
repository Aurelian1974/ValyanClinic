"""
OCR Script pentru extragerea analizelor medicale din PDF
Extrage: ANALIZE, REZULTATE, UM, INTERVAL BIOLOGIC DE REFERINTA
"""

import fitz  # PyMuPDF
import re
import json
import sys
from pathlib import Path

def extract_text_from_pdf(pdf_path: str) -> str:
    """Extrage textul din PDF folosind PyMuPDF (nu necesită Tesseract pentru PDF-uri text-based)"""
    doc = fitz.open(pdf_path)
    full_text = ""
    
    for page_num, page in enumerate(doc):
        text = page.get_text()
        full_text += f"\n--- PAGE {page_num + 1} ---\n"
        full_text += text
        
    doc.close()
    return full_text

def extract_text_with_ocr(pdf_path: str) -> str:
    """Extrage textul folosind OCR (pentru PDF-uri scanate)"""
    import pytesseract
    from PIL import Image
    import io
    
    doc = fitz.open(pdf_path)
    full_text = ""
    
    for page_num, page in enumerate(doc):
        # Render page to image
        pix = page.get_pixmap(dpi=300)
        img_data = pix.tobytes("png")
        img = Image.open(io.BytesIO(img_data))
        
        # OCR
        text = pytesseract.image_to_string(img, lang='ron')  # Romanian language
        full_text += f"\n--- PAGE {page_num + 1} ---\n"
        full_text += text
        
    doc.close()
    return full_text

def parse_analize(text: str) -> list:
    """Parsează textul pentru a extrage analizele în format structurat"""
    analize = []
    
    # Pattern pentru a găsi linii cu analize
    # Format tipic: NUME_ANALIZA   VALOARE   UM   INTERVAL_REFERINTA
    lines = text.split('\n')
    
    for line in lines:
        # Skip empty lines or headers
        if not line.strip() or len(line.strip()) < 5:
            continue
            
        # Încercăm să parsăm linia
        # Pattern: text numeric unit range
        # Exemplu: "Hemoglobina   14.5   g/dl   12.0 - 16.0"
        
        parts = line.split()
        if len(parts) >= 2:
            analize.append({
                'raw_line': line.strip(),
                'parts': parts
            })
    
    return analize

def main():
    pdf_path = r"D:\Lucru\CMS\DevSupport\05_Resources\PDFs\42381_Disp.Prim. nr.54 din 14.02.2024 -stabilire stimulent educatonal.pdf"
    
    if not Path(pdf_path).exists():
        print(f"ERROR: File not found: {pdf_path}")
        sys.exit(1)
    
    print(f"Processing: {pdf_path}")
    print("=" * 80)
    
    # First try text extraction (faster, works for text-based PDFs)
    print("\n1. Attempting direct text extraction...")
    text = extract_text_from_pdf(pdf_path)
    
    if len(text.strip()) < 100:
        print("   Text extraction returned little content. PDF might be scanned.")
        print("   Attempting OCR extraction...")
        try:
            text = extract_text_with_ocr(pdf_path)
        except Exception as e:
            print(f"   OCR failed: {e}")
            print("   You may need to install Tesseract OCR: https://github.com/tesseract-ocr/tesseract")
    
    print("\n2. Extracted text preview (first 3000 chars):")
    print("-" * 80)
    print(text[:3000] if len(text) > 3000 else text)
    print("-" * 80)
    
    print(f"\n3. Total characters extracted: {len(text)}")
    
    # Save full text to file for analysis
    output_path = Path(pdf_path).parent / "extracted_text.txt"
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(text)
    print(f"\n4. Full text saved to: {output_path}")
    
    # Parse for analize patterns
    print("\n5. Looking for analysis patterns...")
    analize = parse_analize(text)
    print(f"   Found {len(analize)} potential analysis lines")

if __name__ == "__main__":
    main()
