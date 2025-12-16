# 🌐 ICD-10 Translation Tool

## Descriere

Tool pentru traducerea descrierilor codurilor ICD-10 din engleză în română folosind servicii de traducere automată.

## Caracteristici

- ✅ Suport pentru multiple API-uri de traducere (Azure, DeepL, LibreTranslate)
- ✅ Traducere în batch pentru eficiență
- ✅ Prioritizare coduri comune (IsCommon)
- ✅ Traducere per categorie medicală
- ✅ Statistici detaliate despre progresul traducerii
- ✅ Salvare automată în baza de date
- ✅ Marcare coduri traduse (IsTranslated, TranslatedAt, TranslatedBy)

## Utilizare

### Opțiunea 1: Din meniul principal DevSupport

```powershell
cd D:\Lucru\CMS\DevSupport
dotnet run
# Selectează opțiunea 2 (ICD-10 Traducere)
```

### Opțiunea 2: Direct cu comandă

```powershell
dotnet run translate
```

## Configurare API

### Azure Translator (Recomandat)

1. Creează un serviciu Azure Translator în [Azure Portal](https://portal.azure.com)
2. Obține API Key și Region
3. Configurează în tool (opțiunea 5)

```json
{
  "Translation": {
    "Provider": "Azure",
    "ApiKey": "your-api-key",
    "Region": "westeurope",
    "BatchSize": 50
  }
}
```

### DeepL

1. Creează cont pe [DeepL API](https://www.deepl.com/pro-api)
2. Obține API Key
3. Configurează în tool

### LibreTranslate (Gratuit)

LibreTranslate este open-source și poate fi self-hosted. Calitatea traducerilor poate fi mai slabă pentru termeni medicali.

## Meniu Principal

```
📋 MENIU TRADUCERE ICD-10
════════════════════════════════════════
  1. Afișează statistici traduceri
  2. Traduce toate codurile (automat)
  3. Traduce doar codurile comune
  4. Traduce o categorie specifică
  5. Configurare API traducere
  6. Test traducere (un singur cod)
  0. Ieșire
════════════════════════════════════════
```

## Statistici Exemplu

```
📊 STATISTICI TRADUCERE ICD-10
========================================
  Total coduri:      46,881
  Traduse:           5,000 (10.7%)
  Netraduse:         41,881
  Coduri comune:     200
  Comune traduse:    200
========================================

📊 Per categorie:
----------------------------------------
Categorie            Total    Traduse  Procent
----------------------------------------
Traumatisme         13,333      1,500   11.3%
Musculo-scheletic    7,100        800   11.3%
Cardiovascular       1,798        300   16.7%
...
----------------------------------------
```

## Categorii Medicale

| Categorie | Coduri ICD-10 |
|-----------|---------------|
| Infectioase | A00-B99 |
| Neoplasme | C00-D49 |
| Sange | D50-D89 |
| Endocrin | E00-E89 |
| Mental | F01-F99 |
| Nervos | G00-G99 |
| Ochi | H00-H59 |
| Ureche | H60-H95 |
| Cardiovascular | I00-I99 |
| Respirator | J00-J99 |
| Digestiv | K00-K95 |
| Piele | L00-L99 |
| Musculo-scheletic | M00-M99 |
| Genito-urinar | N00-N99 |
| Obstetric | O00-O9A |
| Perinatal | P00-P96 |
| Malformatii | Q00-Q99 |
| Simptome | R00-R99 |
| Traumatisme | S00-T88 |
| Cauze externe | V00-Y99 |
| Factori sanatate | Z00-Z99 |

## Structura Fișierelor

```
DevSupport\
├── Program.cs                    # Entry point principal (meniu)
├── Tools\
│   ├── ICD10Import\
│   │   ├── Program.cs            # Entry point import
│   │   ├── ICD10XmlImporter.cs   # Logica de import
│   │   └── README.md
│   └── ICD10Translate\
│       ├── TranslateProgram.cs   # Entry point traducere
│       ├── ICD10Translator.cs    # Logica de traducere batch
│       ├── ICD10TranslationService.cs  # Serviciu API traducere
│       └── README.md             # Acest fișier
└── DevSupport.csproj
```

## Costuri Estimate

### Azure Translator
- Free tier: 2 milioane caractere/lună
- Pay-as-you-go: $10 per 1M caractere
- Pentru ~47,000 coduri (~5M caractere): ~$50

### DeepL
- Free tier: 500,000 caractere/lună
- Pro: €5.49 per 1M caractere

### LibreTranslate
- Self-hosted: Gratuit
- Public API: Rate limited

## Sfaturi pentru Calitate

1. **Revizuire manuală** - Traducerile automate pot avea erori pentru termeni medicali specifici
2. **Traducere în etape** - Traduceți mai întâi codurile comune, apoi pe categorii
3. **Backup** - Faceți backup la baza de date înainte de traducere masivă
4. **Verificare** - Folosiți opțiunea de test pentru a verifica calitatea înainte de batch

## Pașii Următori

1. **Marcare coduri comune** - Identificați cele 200-500 coduri frecvent utilizate:
   ```sql
   UPDATE ICD10_Codes SET IsCommon = 1 WHERE Code IN ('I10', 'E11.9', 'J06.9', ...)
   ```

2. **Traducere coduri comune** - Folosiți opțiunea 3 din meniu

3. **Traducere pe categorii** - Prioritizați categoriile relevante pentru clinică

4. **Revizuire manuală** - Verificați traducerile pentru termeni medicali critici

---

**Versiune:** 1.0  
**Data:** 2025-01-15  
**Autor:** ValyanClinic DevTeam
