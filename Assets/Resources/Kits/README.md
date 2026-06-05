# Kits – sem nahraj hotové 3D modely

Tady jsou složky na hotové modely (např. zdarma z **kenney.nl**). Kód si je odsud
**najde sám** – proto je to ve speciální složce `Resources`. Nemusíš nic ručně klikat
ani propojovat.

## Kam co nahrát
```
Assets/Resources/Kits/
├── Cars/        ← modely aut        (UŽ NAPOJENO – po nahrání se rovnou použijí)
├── Buildings/   ← budovy (kancelář, garáž…)   (napojím později)
├── Props/       ← doplňky (lampy, stromy, ploty, kužely…)  (napojím později)
└── People/      ← postavy (prodavač, zákazníci)  (napojím později)
```

## Jak na to
1. Stáhni balíček modelů (doporučuju **Kenney – Car Kit**, je zdarma a bez účtu).
2. Rozbal ho a **přetáhni soubory modelů** (`.fbx`, `.obj` nebo `.gltf`/`.glb`
   včetně jejich textur/materiálů) do správné podsložky výše.
3. V Unity dej **`Autobazar → Rebuild Scene (Clear + Build)`** a **Play**.

## Důležité
- **Auta jsou už napojená.** Jakýkoli počet modelů aut ve složce `Cars/` se automaticky
  rozdělí mezi auta v bazaru, **sám je zvětším/zmenším** na správnou velikost a posadím na zem.
- Když ve složce nic není, hra běží dál na jednoduchých kostkách (nic se nerozbije).
- Kdyby auta „koukala" obráceně nebo měla divnou velikost, napiš mi – doladím to
  (stačí jedno číslo v `Assets/Scripts/Core/ModelLibrary.cs`).

Až modely nahraješ, klidně mi napiš **„mám modely"** a kdyžtak pošli názvy souborů –
doladím rozmístění a napojím i budovy/postavy/doplňky.
