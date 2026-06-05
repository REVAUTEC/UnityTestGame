# Checkpoint Rush

Malá, ale do detailu vyladěná **závodní hra na čas**. Sedneš do auta, projedeš všechny
brány ve správném pořadí co nejrychleji a snažíš se překonat svůj **nejlepší čas**.

> Postaveno na assetech z Kenney kitů (auta, příroda) a na osvědčeném arcade řízení.
> Předchozí verze (Autobazar Tycoon) zůstává v historii gitu.

---

## ▶️ Jak spustit
1. `git pull`
2. V Unity dej **`File → New Scene`** (prázdná) — ať nemáš ve scéně staré objekty.
3. Nahoře klikni **`Checkpoint Rush → Build Scene`** → **Ctrl+S** (ulož) → **Play**.
4. V menu stiskni **Enter**. Po odpočtu jeď! 🏁

> (Pokud máš ve scéně zbytky staré hry, použij **`Checkpoint Rush → Rebuild Scene`** —
> staré objekty se uklidí.)

## 🎮 Ovládání
| Akce | Klávesa |
|------|---------|
| Plyn | **W** |
| Brzda / couvání | **S** |
| Zatáčení | **A / D** |
| Boost | **Shift** |
| Restart kola | **R** |
| Menu / pauza | **Esc** |
| Potvrdit (start/menu) | **Enter** |

## 🏁 Cíl hry
- Projeď **brány 1 → 2 → … → CÍL** ve správném pořadí (vždy svítí ta další, zeleně; cíl zlatě).
- Měří se **čas**. Po dojetí se uloží tvůj **rekord** (zůstává i po restartu hry).
- **R** = jet znovu a zkusit lepší čas.

## 🧱 Struktura kódu
```
Assets/Scripts/
├── Core/
│   ├── RaceWorldBuilder.cs  ← postaví scénu (zem, obloha, auto, kamera, kulisy, manažery)
│   ├── ModelLibrary.cs      ← načítání 3D modelů z Kits (+ auto-fit, textura/colormap)
│   ├── MaterialFactory.cs   ← materiály (Built-in i URP), emise, sklo, textury
│   ├── TextFactory.cs, Billboard.cs, GameState.cs, GameBootstrap.cs
├── Racing/
│   ├── RaceManager.cs       ← stavový automat: menu → odpočet → závod → cíl, brány, rekord
│   └── RaceUI.cs            ← HUD (čas, rychlost, brána), odpočet, menu, výsledek
├── Vehicles/CarController.cs ← arcade řízení (W/S/A/D, Shift boost)
├── Player/CameraController.cs ← kamera za autem (chase cam)
├── Managers/MusicManager.cs   ← ambientní hudba generovaná v kódu
├── UI/CinematicOverlay.cs     ← viněta + teplý nádech (filmový vzhled)
└── Editor/Phase1SceneBuilder.cs ← menu „Checkpoint Rush" (Build/Rebuild/Clear)
```

## 🎨 Modely
Auta a kulisy (stromy, kameny, kužely) se načítají z `Assets/Resources/Kits/`. Když je tam
`colormap.png`, použijí se pravé barvy; jinak náhradní obarvení. Když model chybí, naskočí
jednoduchá záloha (kostka/válec).

- Auto závoďáku: `Cars/sedan-sports`. Kdyby koukalo obráceně, přehoď `CarYaw` v `ModelLibrary.cs`.

## 🚀 Co dál (nápady na vypilování)
- Víc tratí / výběr auta
- Zvuky (motor, průjezd bránou, rekord)
- Boost pady na trati, mantinely / reset při vyjetí
- Tabulka časů (víc rekordů)
