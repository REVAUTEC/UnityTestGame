# Cars – modely aut (UŽ NAPOJENO)

Sem nahraj modely aut (`.fbx`, `.obj`, `.gltf`/`.glb`) i s jejich texturami/materiály.

## Co se stane po nahrání
- Modely se **automaticky rozdělí** mezi 5 aut v bazaru (v pořadí podle názvu souboru).
- Každý se **sám zvětší/zmenší** na délku ~4,3 m a posadí na zem (auto-fit).
- Auto dostane správný „obal" (collider), data o ceně/stavu i plovoucí ceduli.
- Když je tu víc aut než 5, použijí se dokola; když míň, opakují se.

## Doporučený balíček (zdarma, bez účtu)
- **Kenney – Car Kit**: https://kenney.nl/assets/car-kit
  (obsahuje `sedan`, `hatchback`, `suv`, `van`, `truck`, `police`, `ambulance`, `race`…)

Stačí přetáhnout třeba `sedan.obj`, `suv.obj`, `van.obj`, `hatchback.obj`, `truck.obj`.

## Kdyby něco nesedělo
- **Auta koukají dozadu** → napiš mi, přehodím `CarModelYaw` na 180.
- **Moc velká/malá** → uprav `CarTargetLength` v `Assets/Scripts/Core/ModelLibrary.cs`
  (nebo napiš a udělám to).
