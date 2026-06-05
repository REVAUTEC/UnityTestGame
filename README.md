# Autobazar Tycoon 3D

Jednoduchá, zábavná 3D hra z prostředí autobazaru. Hraješ za prodavače: chodíš po
areálu, prohlížíš auta, obsluhuješ zákazníky, připravuješ vozy v servisu, vyřizuješ
papíry a prodáváš auta. Tohle je **MVP prototyp** stavěný po fázích.

> **Stav: ✅ Kompletní hra** — herní jádro (Fáze 1–5): svět, prodej, servis, kancelář,
> testovací jízda, denní cyklus. **3D modely** aut, postav, stromů i budov z `Assets/Resources/Kits/`.
> **Navíc:** úvodní/pauzové menu s ovládáním (Enter/P), filmový nádech (viněta + teplý tón),
> ambientní hudba na pozadí (generovaná v kódu), barevné hlavičky dialogů.

---

## 🔌 Jak to celé do sebe zapadá (Claude ↔ GitHub ↔ Unity)

Máš dvě možnosti, jak propojit Claude s Unity. **Teď používáme tu první** (přes GitHub),
protože běžím v cloudu a nevidím na tvůj počítač.

### A) Přes GitHub (to, co používáme teď) — funguje hned
```
Claude Code (web/cloud)  ──push──►  GitHub (revautec/unitytestgame)  ──pull/clone──►  Unity na tvém PC
```
Já píšu kód a nahraju ho na GitHub, ty si ho stáhneš do Unity. Žádné MCP není potřeba.
Tahle cesta je spolehlivá a stačí na rozjezd hry.

### B) Přes Unity MCP (volitelný upgrade) — Claude přímo ovládá editor
Pokud chceš, aby Claude **sám klikal v Unity** (vytvářel objekty, spouštěl scénu, četl
chyby z konzole), je potřeba **Unity MCP**, který běží **lokálně na tvém PC** vedle Unity:
```
Claude Code (CLI/desktop na TVÉM PC)  ◄──MCP bridge──►  Unity Editor na témže PC
```
Důležité: Unity MCP **nejde použít z téhle webové session** — musí běžet na stejném
počítači jako Unity. Až budeš chtít, postupuj podle ověřeného projektu
**Unity MCP** (CoplayDev): <https://github.com/CoplayDev/unity-mcp>. Zhruba to obnáší:
1. Nainstaluj **Claude Code** (nebo Claude Desktop) lokálně na PC s Unity.
2. V Unity přidej Unity MCP balíček (Package Manager → Add from git URL podle jejich README).
3. Spusť MCP server (mají instalátor / `uv`) a zaregistruj ho do Clauda
   (`claude mcp add ...` podle jejich návodu).
4. Otevři projekt v Unity a Claude s ním bude umět mluvit.

👉 **Doporučení:** Teď zůstaň u varianty A (GitHub). Až budeš mít hru rozjetou a budeš
chtít rychlejší iterace přímo v editoru, přidej variantu B.

---

## ▶️ Jak rozjet hru (Fáze 1)

### Krok 1 — Získej projekt do Unity
Nejjednodušší je naklonovat tenhle repozitář a otevřít ho v Unity Hubu:

```bash
git clone <URL tohoto repa>
# pak v Unity Hub: Add → vyber složku UnityTestGame
```

- Unity nejspíš nabídne **upgrade verze** (repo je značené jako Unity 6 `6000.0.0f1`).
  Klidně potvrď a otevři ve své verzi (6.4) — projekt se sám upgraduje.
- Chybějící systémové soubory (ProjectSettings, Library…) si Unity při prvním otevření
  **vygeneruje samo**. To je v pořádku.

> Alternativa: pokud už máš lokálně rozdělaný prázdný 3D projekt, stačí do něj zkopírovat
> složku `Assets/Scripts/`.

### Krok 2 — Postav scénu
Máš dvě cesty (vyber jednu):

**Cesta 1 (doporučená): přes menu**
1. Nahoře v Unity klikni na **`Autobazar → Build Phase 1 Scene`**.
   → Postaví se celý autobazar (zem, budovy, 5 aut, hráč, kamera, manažeři).
2. Ulož scénu: **Ctrl+S** (např. jako `Assets/Scenes/Autobazar.unity`).
3. Stiskni **Play**.

**Cesta 2: přes bootstrap (bez menu)**
1. Vytvoř prázdnou scénu (`File → New Scene → Empty`).
2. `GameObject → Create Empty`, a přidej mu komponentu **`GameBootstrap`**
   (Add Component → napiš „GameBootstrap").
3. Stiskni **Play** — svět se postaví automaticky.

### Krok 3 — Hraj
| Akce | Ovládání |
|------|----------|
| Pohyb | **W A S D** |
| Rozhlížení | **myš** |
| Sprint | **Shift** |
| Interakce (prohlédnout auto) | **E** (když jsi blízko auta) |
| Uvolnit myš / zamknout zpět | **Esc** / **levé tlačítko** |

---

## ✅ Co otestovat v Unity (Fáze 1)

- [ ] Po `Build Phase 1 Scene` (nebo Play s GameBootstrap) se objeví areál autobazaru.
- [ ] Vlevo nahoře vidíš **Peníze (10 000 Kč)**, **Reputaci (50/100)**, **Prodáno aut (0)**.
- [ ] Nahoře uprostřed je **ÚKOL**.
- [ ] Chodíš pomocí WASD, myší se rozhlížíš, kamera tě sleduje zezadu (3. osoba).
- [ ] Nad každým z 5 aut je **plovoucí cedule** (název, cena, status) otočená k tobě.
- [ ] Když přijdeš k autu, dole se objeví **`[E] Prohlédnout: …`**.
- [ ] Po stisku **E** vyskočí uprostřed **detail auta** (typ, cena, stav, atraktivita, status).
- [ ] Auta „Škoda Felicia" a „BMW E46" mají status **Potřebuje servis** (oranžová).
- [ ] Vidíš budovy **KANCELÁŘ** a **SERVIS** a zóny **VSTUP** / **PŘEDÁNÍ**.

---

## 🧱 Struktura projektu

```
Assets/Scripts/
├── Core/            stavba světa a pomocníci
│   ├── WorldBuilder.cs      ← postaví celou scénu z kódu
│   ├── GameBootstrap.cs     ← postaví svět při Play
│   ├── MaterialFactory.cs   ← materiály pro Built-in i URP (žádné růžové objekty)
│   ├── TextFactory.cs       ← 3D popisky/cedule
│   └── Billboard.cs         ← otáčí text za kamerou
├── Player/
│   ├── PlayerController.cs   ← WASD, sprint, gravitace, otáčení
│   └── CameraController.cs   ← kamera třetí osoby
├── Interaction/
│   ├── IInteractable.cs      ← interface pro vše ovladatelné přes E
│   └── InteractionSystem.cs  ← hledá nejbližší objekt + spouští interakci
├── Vehicles/
│   ├── CarType.cs            ← enum typů aut
│   ├── CarData.cs            ← data auta (cena, stav, status…)
│   └── CarInteractable.cs    ← auto ve světě + cedule + prohlídka
├── Managers/
│   ├── EconomyManager.cs     ← peníze, prodaná auta (singleton)
│   ├── ReputationManager.cs  ← reputace 0–100 (singleton)
│   ├── TaskManager.cs        ← aktuální úkol (singleton)
│   └── UIManager.cs          ← postaví a aktualizuje HUD (singleton)
└── Editor/
    └── Phase1SceneBuilder.cs ← menu „Autobazar → Build/Clear"
```

**Proč se scéna staví z kódu?** Aby se daly soubory bezpečně posílat přes GitHub bez
ručního klikání a bez křehkých `.unity`/`.prefab` souborů. Stačí jeden klik v menu.

---

## 🛠️ Když něco nefunguje

- **Objekty jsou růžové (missing shader)** — to řeší `MaterialFactory` automaticky pro
  Built-in i URP. Pokud používáš HDRP, přepni projekt na **Built-in** nebo **URP**.
- **Chyba u WASD / „InvalidOperationException: … Input System"** — jdi do
  `Edit → Project Settings → Player → Active Input Handling` a nastav **Both**
  (nebo „Input Manager (Old)"). Hra používá staré, jednoduché Input API.
- **Není vidět text cedulí** — to je ošéfované přiřazením vestavěného fontu; pokud přesto
  nic, zkontroluj, že kamera vidí objekty (text je 3D, ne UI).
- **Hráč propadává zemí** — zem musí být `Ground` (Plane s colliderem), což `WorldBuilder`
  vytváří. Při ruční úpravě nechej collider zapnutý.

---

## 🚀 Co bude dál (další fáze)

- **Fáze 2:** zákazníci (preference auta, trpělivost), dialog, výběr auta, šance na prodej.
- **Fáze 3:** servis (umýt/opravit) + kancelář (papíry) + dokončení prodeje a výplata.
- **Fáze 4:** testovací jízda s arcade řízením a checkpointy.
- **Fáze 5:** denní cyklus, statistika na konci dne, čištění UI a ladění.

Až bude Fáze 1 odzkoušená, napiš „pokračuj Fází 2" a navážu.
