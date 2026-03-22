# Necrozone — Unity Project Context

> Diseño completo del juego: ver **[GAME_DESIGN.md](./GAME_DESIGN.md)**

## Resumen del proyecto

- **Nombre:** Necrozone
- **Género:** Top-down twin-stick zombie survival · Roguelite de oleadas
- **Engine:** Unity (C#)
- **Plataforma primaria:** Mobile (iOS + Android) — F2P con ads + IAP
- **Plataforma secundaria:** PC (misma build, controles alternativos)
- **Arte:** 3D Low-Poly · Cámara ortográfica top-down
- **Orientación:** Landscape · Canvas resolución de referencia **1920 × 1080**
- **Versión:** 0.1.0
- **Estado:** Fundación completa — iniciando Fase 0 (prototype)

## Concepto central

```
Un mapa. Una casa con HP que defender. Oleadas infinitas.
Entre oleadas: elegir 1 de 3 cartas de mejora (roguelite).
Objetivo: sobrevivir el mayor tiempo posible.
```

---

## Principios de arquitectura

### Mobile-first, PC-ready

- Input siempre a través de la capa `Scripts/Input/` — nunca hardcodear `Input.*` en gameplay.
- Mobile: dual joystick virtual + auto-disparo. PC: WASD + mouse aim.
- UI con Canvas Scaler `Scale With Screen Size`, 1920×1080, Match 0.5.
- Lógica de plataforma solo en handlers de input (`#if UNITY_ANDROID` / `#if UNITY_IOS`).
- Proyecto usa **nuevo Input System** — usar `UnityEngine.InputSystem`, nunca `UnityEngine.Input`.
- Botones touch mínimo 80×80 px en pantalla para cumplir guías de Apple/Google.

### Modular y escalable

- Una responsabilidad por clase. Scripts pequeños y enfocados.
- **ScriptableObjects para datos** — nunca magic numbers en código.
- **Events/Delegates** para comunicación entre sistemas.
- No Singletons en gameplay — solo en Managers que persisten.

### 3D Low-Poly — notas técnicas

- Cámara: `Camera.orthographic = true`, ángulo ~55–65° en X, sin rotación Y/Z.
- Jugador y zombies: NavMesh en plano XZ — nunca mover con `transform.position` directamente en zombies.
- Proyectiles: prefab con `Rigidbody` kinematic + `OnTriggerEnter` para hits, O raycast desde firePoint.
- Object pooling obligatorio para proyectiles y zombies — nunca `Instantiate/Destroy` en gameplay loop.
- Sombras: usar `Shadow Distance` bajo en móvil (~15 unidades), `Hard Shadows Only`.

### NavMesh — setup confirmado

- Usar **AI Navigation package** (no el legacy "Navigation Obsolete").
- Agregar componente **`NavMeshSurface`** al GameObject del suelo → clic **Bake**.
- El suelo debe tener un Collider para que NavMeshSurface lo detecte.
- Re-hornear cada vez que se modifique la geometría de la escena.
- `ZombieAI` ya tiene guard `if (!_agent.isOnNavMesh) return;` — sin NavMesh los zombies no se mueven pero no crashean.

### Notas críticas de implementación Unity

- `??=` NO funciona con objetos Unity — siempre usar `if (x == null)` para componentes.
- `AddComponent<T>()` dispara `Awake()` inmediatamente — no asignar campos después.
- Managers que dependen de `SaveManager` deben leer en `Awake()`, no `Start()`, porque `SaveManager` usa `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`.
- `LocalizationData` asset debe estar en `_Project/Resources/` con nombre exacto `LocalizationData`.
- `GameManager.Awake()` fuerza `Time.timeScale = 1f`.

---

## Pipeline de escenas

```
Boot → MainMenu → GameScene
                     ↕
                  PauseMenu / GameOver (overlays en GameScene)
```

| Escena      | Carpeta        | Notas                                                    |
| ----------- | -------------- | -------------------------------------------------------- |
| Boot        | Scenes/Core/   | Logos splash + inicialización de managers                |
| MainMenu    | Scenes/Menu/   | Play · Personajes · Settings · Leaderboard               |
| GameScene   | Scenes/Game/   | Mapa único · gameplay completo · HUD · oleadas           |

No hay LevelSelect en v1. Todo el gameplay ocurre en GameScene.

---

## Scripts existentes (fundación)

### Core

| Script                | Ruta          | Función                                                    |
| --------------------- | ------------- | ---------------------------------------------------------- |
| `SceneNames`        | Scripts/Core/ | Constantes de nombres de escena                             |
| `BootLoader`        | Scripts/Core/ | Secuencia splash con Animator o fallback por tiempo         |
| `LoadingController` | Scripts/Core/ | Carga async con mínimo 3s                                   |

### Managers (DontDestroyOnLoad — auto-crean con `RuntimeInitializeOnLoadMethod`)

| Script                  | Ruta              | Función                                                  |
| ----------------------- | ----------------- | --------------------------------------------------------- |
| `SceneLoader`         | Scripts/Managers/ | Carga de escenas sync/async                               |
| `SaveManager`         | Scripts/Managers/ | JSON en `persistentDataPath/save.json`                  |
| `AudioManager`        | Scripts/Managers/ | Playlist aleatoria (música) + SFX                        |
| `LocalizationManager` | Scripts/Managers/ | Idioma EN/ES, carga `LocalizationData` desde Resources  |
| `GameManager`         | Scripts/Managers/ | Estado del juego (Playing/Paused/GameOver)                |

### Input

| Script             | Ruta           | Función                                                                  |
| ------------------ | -------------- | ------------------------------------------------------------------------ |
| `IInputHandler`  | Scripts/Input/ | Interfaz base de input                                                    |
| `InputHandler`   | Scripts/Input/ | Touch (mobile) + Mouse (PC)                                              |

### Menu

| Script                 | Ruta          | Función                                            |
| ---------------------- | ------------- | -------------------------------------------------- |
| `MainMenuController` | Scripts/Menu/ | Play · Settings · Quit                             |
| `SettingsController` | Scripts/Menu/ | Sliders volumen + toggle idioma                    |

### Gameplay (existente — a adaptar)

| Script                      | Ruta              | Función                                              |
| --------------------------- | ----------------- | ---------------------------------------------------- |
| `PauseController`         | Scripts/Gameplay/ | Pausa con GameManager + Escape / botón mobile        |
| `GameOverController`      | Scripts/Gameplay/ | Panel GameOver — resultados + botones ad/retry       |

### UI

| Script            | Ruta        | Función                                                        |
| ----------------- | ----------- | -------------------------------------------------------------- |
| `LocalizedText` | Scripts/UI/ | TMP que se actualiza al cambiar idioma                          |

### Data (ScriptableObjects existentes)

| Script               | Ruta          | Función                                         |
| -------------------- | ------------- | ----------------------------------------------- |
| `SaveData`         | Scripts/Data/ | Modelo JSON · ampliar con datos de juego         |
| `SoundData`        | Scripts/Data/ | AudioClip + volume + pitch + loop                |
| `LocalizationData` | Scripts/Data/ | LocalizationEntry (key / english / spanish)      |

---

## Scripts a crear — Fase 0 (prototype)

| Script                | Ruta               | Función                                                     |
| --------------------- | ------------------ | ----------------------------------------------------------- |
| `PlayerController`  | Scripts/Player/    | Movimiento top-down · rotación hacia aim                    |
| `PlayerShooter`     | Scripts/Player/    | Disparo raycast o proyectil · WeaponData SO                 |
| `PlayerHealth`      | Scripts/Player/    | HP jugador · daño · muerte → GameManager.GameOver()         |
| `PlayerInputPC`     | Scripts/Input/     | WASD + mouse aim con InputSystem                            |
| `PlayerInputMobile` | Scripts/Input/     | Dual joystick virtual (OnScreenStick de InputSystem)        |
| `HouseHealth`       | Scripts/Gameplay/  | HP de la casa · daño al contacto con zombie · Game Over     |
| `ZombieAI`          | Scripts/Gameplay/  | NavMesh Agent · target = jugador o casa (el más cercano)    |
| `ZombieHealth`      | Scripts/Gameplay/  | HP · recibir daño · muerte + drop                           |
| `WaveManager`       | Scripts/Gameplay/  | Scheduler de oleadas · spawn config por oleada              |
| `ZombieSpawner`     | Scripts/Gameplay/  | Instancia zombies en bordes del mapa (usar Object Pool)     |
| `ObjectPool`        | Scripts/Utils/     | Pool genérico para proyectiles y zombies                    |
| `FollowCamera`      | Scripts/Core/      | Cámara ortográfica que sigue al jugador con lag suave       |

### ScriptableObjects a crear

| Script         | Ruta          | Función                                                       |
| -------------- | ------------- | ------------------------------------------------------------- |
| `WeaponData` | Scripts/Data/ | Daño · cadencia · munición · alcance · prefab proyectil       |
| `ZombieData` | Scripts/Data/ | HP · velocidad · daño · tipo · drop weights                   |
| `WaveData`   | Scripts/Data/ | Config de oleada: qué zombies · cuántos · patrón de spawn     |

---

## Scripts a crear — Fases posteriores

### Fase 1 (core loop completo)

| Script                 | Ruta               | Función                                                   |
| ---------------------- | ------------------ | ---------------------------------------------------------- |
| `LootDrop`           | Scripts/Gameplay/  | Drop de ammo/botiquín al morir zombie                     |
| `LootPickup`         | Scripts/Gameplay/  | Auto-pickup por radio al pasar encima                      |
| `CardSystem`         | Scripts/Gameplay/  | Selección de 3 cartas aleatorias entre oleadas            |
| `CardData`           | Scripts/Data/      | SO: nombre · descripción · efecto · rareza                 |
| `HUDController`      | Scripts/UI/        | Actualiza barras HP, oleada, munición en tiempo real       |

### Fase 2 (contenido)

| Script                 | Ruta               | Función                                                   |
| ---------------------- | ------------------ | ---------------------------------------------------------- |
| `ScreenShake`        | Scripts/Utils/     | Shake de cámara paramétrico (trauma system)               |
| `DamagePopup`        | Scripts/UI/        | Número de daño flotante sobre el zombie                   |
| `BarricadeObject`    | Scripts/Gameplay/  | Barricada con HP propio, bloquea paso a zombies           |
| `TurretBehavior`     | Scripts/Gameplay/  | Torreta auto: rota y dispara al zombie más cercano        |

### Fase 3 (meta-progresión)

| Script                 | Ruta               | Función                                                   |
| ---------------------- | ------------------ | ---------------------------------------------------------- |
| `CharacterData`      | Scripts/Data/      | SO: nombre · costo · pasiva · modelo prefab               |
| `CharacterSelector`  | Scripts/Menu/      | UI de selección y desbloqueo de personajes                |
| `DailyReward`        | Scripts/Managers/  | Lógica de recompensa por login diario                     |

### Fase 4 (monetización)

| Script                 | Ruta               | Función                                                   |
| ---------------------- | ------------------ | ---------------------------------------------------------- |
| `AdsManager`         | Scripts/Managers/  | Wrapper de Unity Ads / AdMob · rewarded + interstitial    |
| `IAPManager`         | Scripts/Managers/  | Unity IAP · sin anuncios · packs de monedas               |

---

## Patrones de uso

### ZombieAI (target dinámico)

```csharp
// El zombie va hacia lo que esté más cerca: jugador o casa
private Transform GetTarget()
{
    float distPlayer = Vector3.Distance(transform.position, _player.position);
    float distHouse  = Vector3.Distance(transform.position, _house.position);
    return distPlayer < distHouse ? _player : _house;
}

private void Update()
{
    _agent.SetDestination(GetTarget().position);
}
```

### WaveManager — flujo básico

```csharp
// WaveManager dispara eventos que otros sistemas escuchan
public static event Action<int> OnWaveStarted;   // int = número de oleada
public static event Action     OnWaveClearedForCards; // entre oleadas
public static event Action<int> OnWaveCompleted; // para HUD y stats
```

### Object Pool básico

```csharp
// Nunca Instantiate/Destroy en gameplay
ObjectPool.Instance.Get("Zombie_Walker");  // activa desde pool
ObjectPool.Instance.Return("Zombie_Walker", gameObject); // devuelve al pool
```

### WeaponData SO

```csharp
[CreateAssetMenu(fileName = "WeaponData", menuName = "Necrozone/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float fireRate;        // disparos por segundo
    public int magazineSize;
    public float reloadTime;
    public GameObject projectilePrefab; // null = usar raycast
    public SoundData sfxShoot;
    public SoundData sfxReload;
}
```

### SaveData — campos del juego (a agregar)

```csharp
// Añadir en SaveData.cs
public int bestWave;
public int totalKills;
public int coins;
public int selectedCharacter;
public List<int> unlockedCharacters;
public bool adsRemoved;
public int dailyLoginStreak;
public string lastLoginDate;
```

### Audio

```csharp
AudioManager.Instance.PlayPlaylist(AudioManager.Instance.musicGame);
AudioManager.Instance.PlaySFX(sfxData);
AudioManager.Instance.SetMusicVolume(0.8f);
```

---

## Estructura de carpetas (`Assets/_Project/`)

```
Scripts/
├── Core/        → SceneNames, BootLoader, LoadingController, FollowCamera
├── Player/      → PlayerController, PlayerShooter, PlayerHealth
├── Gameplay/    → ZombieAI, ZombieHealth, WaveManager, ZombieSpawner,
│                  HouseHealth, LootDrop, LootPickup, CardSystem,
│                  BarricadeObject, TurretBehavior
├── Input/       → IInputHandler, InputHandler, PlayerInputPC, PlayerInputMobile
├── Managers/    → SceneLoader, SaveManager, AudioManager, LocalizationManager,
│                  GameManager, AdsManager, IAPManager, DailyReward
├── Menu/        → MainMenuController, SettingsController, CharacterSelector
├── UI/          → LocalizedText, HUDController, DamagePopup
├── Data/        → SaveData, SoundData, LocalizationData,
│                  WeaponData, ZombieData, WaveData, CardData, CharacterData
└── Utils/       → ObjectPool, ScreenShake, Constants

Resources/
└── LocalizationData.asset

Scenes/
├── Core/        → Boot, (Loading si se necesita)
├── Menu/        → MainMenu
└── Game/        → GameScene

ScriptableObjects/
├── Audio/       → SoundData assets
├── Weapons/     → WeaponData assets (Pistola, Escopeta, Rifle, SMG...)
├── Zombies/     → ZombieData assets (Walker, Runner, Bloater, Screamer, Brute)
├── Waves/       → WaveData assets (Wave01, Wave02... Wave20+)
├── Cards/       → CardData assets (todas las cartas de mejora)
├── Characters/  → CharacterData assets (Marcus, Elena, Rex, Yara, Doc, Ghost)
└── Settings/    → LocalizationData
```

---

## Convenciones de código

- **Namespaces:** `Necrozone.Core`, `Necrozone.Gameplay`, `Necrozone.Player`, `Necrozone.UI`, `Necrozone.Managers`, `Necrozone.Data`, `Necrozone.Menu`, `Necrozone.Utils`
- **Naming:** PascalCase clases/métodos, `_camelCase` campos privados, prefijo `I` interfaces
- **ScriptableObjects:** sufijo `Data` o `SO`
- **Events:** prefijo `On` (ej. `OnWaveStarted`, `OnPlayerDied`, `OnCardSelected`)
- **Unity null:** siempre `if (x == null)`, nunca `??=` con UnityEngine.Object
- **NavMesh:** siempre `NavMeshAgent.SetDestination()`, nunca `transform.position` en zombies
- **Pool:** nunca `Instantiate/Destroy` en el loop de gameplay — siempre ObjectPool

---

## Checklist de desarrollo

### Fundación (completado)

- [X] Estructura de carpetas
- [X] Boot → MainMenu → Loading → GameScene pipeline
- [X] SceneLoader, SaveManager, AudioManager, LocalizationManager, GameManager
- [X] BootLoader, LoadingController
- [X] LocalizedText (TMP auto-actualizable)
- [X] PauseController, GameOverController
- [X] InputHandler (Touch + Mouse)
- [X] SettingsController, MainMenuController

### Fase 0 — Prototype (siguiente)

- [ ] **FollowCamera** — cámara ortográfica top-down con lag suave
- [ ] **PlayerController** — movimiento + rotación hacia aim (PC: mouse, Mobile: joystick derecho)
- [ ] **PlayerInputPC** — WASD + mouse aim
- [ ] **PlayerInputMobile** — dual joystick virtual
- [ ] **PlayerShooter** — disparo básico (raycast), WeaponData SO (pistola)
- [ ] **PlayerHealth** — HP, daño, muerte
- [ ] **HouseHealth** — HP de la casa, daño por contacto con zombie, Game Over
- [ ] **ZombieAI** — Walker con NavMesh, target dinámico (jugador o casa)
- [ ] **ZombieHealth** — HP, recibir daño, morir
- [ ] **WaveManager** — oleadas con spawn, escalado básico
- [ ] **ZombieSpawner** — spawn en bordes del mapa
- [ ] **ObjectPool** — pool genérico para zombies y proyectiles
- [ ] GameScene funcional con todo lo anterior

### Fase 1 — Core loop

- [ ] Drops de loot (ammo, botiquín)
- [ ] Auto-pickup por radio
- [ ] CardSystem — 3 cartas entre oleadas
- [ ] CardData SO — 10 cartas básicas
- [ ] HUDController — HP jugador, HP casa, oleada, munición
- [ ] GameOver con stats (oleada, kills, tiempo)

### Fase 2 — Contenido y juiciness

- [ ] Todos los tipos de zombie (Runner, Bloater, Screamer, Brute)
- [ ] Todas las cartas de mejora
- [ ] ScreenShake, DamagePopup, partículas de sangre
- [ ] Sonidos completos (disparos, hits, gruñidos, música)
- [ ] Arte low-poly final (Quaternius assets integrados)

### Fase 3 — Meta-progresión

- [ ] Sistema de monedas
- [ ] CharacterData SO + CharacterSelector UI
- [ ] SaveData extendido (best wave, coins, personajes)
- [ ] DailyReward
- [ ] Achievements

### Fase 4 — Monetización

- [ ] AdsManager (Unity Ads o AdMob)
- [ ] Rewarded ad: revivir, x2 monedas, carta extra
- [ ] IAPManager: sin anuncios, starter pack
- [ ] Leaderboard (Google Play Games + Game Center)

### Fase 5 — Launch

- [ ] UI/UX mobile polish
- [ ] Testing en devices reales
- [ ] Optimización (batching, shadow distance, pool tuning)
- [ ] App Store + Google Play listing
