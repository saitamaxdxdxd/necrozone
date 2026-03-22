# Necrozone — Documento de Diseño del Juego (GDD)

> Versión 0.2 · 2026-03-21
> Scope: Solo developer · Mobile-first · 3D Low-Poly · Monetización ads + IAP

---

## Visión

Necrozone es un **shooter de supervivencia zombie top-down** para mobile. El jugador defiende su casa del avance de hordas de zombies mientras recoge recursos del mapa. Entre oleadas elige mejoras de una carta roguelite. El objetivo es sobrevivir el mayor tiempo posible.

**Feel:** Caótico, rápido, satisfactorio. "Una partida más."
**Referentes:** Dead Ops Arcade · Vampire Survivors · 20 Minutes Till Dawn · Minigore
**Arte:** 3D Low-Poly con cámara ortográfica top-down
**Plataforma:** Mobile (iOS + Android) — primaria
**Monetización:** Free-to-play · Rewarded ads + IAP

---

## Arte y estética

### Por qué 3D Low-Poly

- La rotación de personajes y zombies es automática — sin spritesheets de 8 direcciones
- Iluminación y sombras dinámicas de Unity dan atmósfera gratis
- NavMesh para IA funciona nativo en 3D
- Assets gratuitos disponibles (Quaternius, Unity Asset Store)
- Performance excelente en mobile con meshes low-poly (~500 triángulos por personaje)

### Paleta de colores

| Elemento       | Color                         |
| -------------- | ----------------------------- |
| Suelo          | Verde oscuro / tierra marrón  |
| Casa / base    | Gris azulado, madera          |
| Zombies        | Verde putrefacto, piel grisácea|
| Jugador        | Colores según personaje       |
| UI / HUD       | Negro con acento rojo neón    |
| Sangre / FX    | Rojo saturado, partículas     |

### Cámara

- **Ortográfica** top-down, ángulo ligeramente inclinado (3/4 estilo Dead Ops Arcade)
- Sigue al jugador con lag suave (`Vector3.Lerp` en `LateUpdate`)
- Sin zoom — campo de visión fijo
- Ligero screen shake al recibir daño o al explotar

### Assets a usar (gratis)

| Recurso          | Fuente                      | Uso                                 |
| ---------------- | --------------------------- | ----------------------------------- |
| Personajes/zombies| Quaternius (quaternius.com) | Pack "Animated Characters" gratis  |
| Entorno (casa, árboles, props) | Quaternius "Nature Pack" / "City Pack" | Mapa y base |
| Efectos VFX      | Unity Particle Pack (Asset Store, gratis) | Disparos, explosiones |
| Sonidos          | freesound.org / Sonniss GDC Bundle | SFX y música                  |
| Fuentes          | Google Fonts (Oswald, Bebas Neue) | UI                             |

---

## Diseño del juego

### Concepto central

```
Un mapa. Una casa. Oleadas infinitas de zombies.
Sobrevive el mayor tiempo posible.
```

La casa está en el centro del mapa. Tiene **HP propio**. Si la casa llega a 0 o el jugador muere → **Game Over**.

Los zombies siempre van hacia la casa o hacia el jugador (el que esté más cerca). Esto crea tensión: si el jugador se aleja demasiado a recoger loot, la casa queda expuesta.

### El mapa

- Área cuadrada de ~40×40 unidades
- **Centro:** la casa (con HP visible sobre ella)
- **Alrededor:** zona abierta con obstáculos (árboles, autos, contenedores) que sirven de cover
- **Bordes del mapa:** donde spawnean los zombies
- El mapa es siempre el mismo — la dificultad escala por tipo y cantidad de zombies

```
[ Z ][ Z ][ Z ][ Z ][ Z ]   ← spawn edge
[   ][árb][   ][auto][   ]
[   ][   ][ CASA ][   ][   ]
[   ][cnt][   ][árb][   ]
[ Z ][ Z ][ Z ][ Z ][ Z ]   ← spawn edge
```

### Recursos en el mapa

- **Munición** → dropea de zombies muertos (90% de drops)
- **Botiquín** → dropea de zombies (10%) o aparece en puntos fijos del mapa
- **Madera / Tablones** → crates fijos en el mapa, respawnean entre oleadas
- Los recursos se recogen al pasar por encima (auto-pickup con radio)

---

## Core Loop

### Durante la oleada

1. Zombies spawnean desde los bordes del mapa
2. Van hacia la casa o hacia el jugador
3. El jugador dispara, mata zombies, recoge drops
4. Si el jugador se aleja de la casa → la casa queda expuesta
5. Tensión constante entre "recojo loot" vs "protejo la casa"

### Entre oleadas (15 segundos)

1. **Pausa** — los zombies restantes desaparecen (o se elimina el conteo si son pocos)
2. Aparecen **3 cartas de mejora** → el jugador elige 1
3. Aparece opción de **reparar la casa** (si tiene madera)
4. Countdown → empieza la siguiente oleada (más zombies, tipos más duros)

### Game Over

1. Pantalla de resultados: oleadas sobrevividas, kills, tiempo
2. Botón **"Revivir — Ver anuncio"** (1 vez por partida) → rewarded ad
3. Si no revive o ya usó el revive → puntuación final + recompensa de monedas
4. Opción: "x2 monedas — Ver anuncio"
5. Botón volver al menú

---

## Sistema de oleadas

### Escalado de dificultad

| Oleada | Zombies | Tipos disponibles       | Notas                        |
| ------ | ------- | ----------------------- | ---------------------------- |
| 1–3    | 8–15    | Walker                  | Tutorial implícito           |
| 4–6    | 15–25   | Walker, Runner          | Empieza la presión           |
| 7–10   | 25–40   | Walker, Runner, Bloater | Necesitas upgrades           |
| 11–15  | 40–60   | + Screamer              | Screamer atrae más zombies   |
| 16–20  | 60–80   | + Brute                 | Brute ataca la casa directo  |
| 21+    | 80+     | Todos + eventos         | Modo caos                    |

### Spawn pattern

- Zombies spawnean en grupos desde puntos aleatorios de los 4 bordes
- Cada 3 oleadas se añade una "mini-horda" sorpresa a mitad de oleada
- Oleadas pares: spawn repartido en los 4 bordes
- Oleadas impares: spawn concentrado en 1-2 bordes (presión direccional)

---

## Tipos de zombie

| Tipo          | HP   | Vel. | Daño | Descripción                                                |
| ------------- | ---- | ---- | ---- | ---------------------------------------------------------- |
| **Walker**    | 50   | 1.5  | 10/s | Base. Lento, en grupos grandes es peligroso                |
| **Runner**    | 30   | 3.5  | 15/s | Rápido y frágil. Sprint en línea recta hacia el objetivo   |
| **Bloater**   | 120  | 1.0  | 5/s  | Al morir explota (área verde venenosa 2 seg)               |
| **Screamer**  | 40   | 1.8  | 5/s  | Al recibirle daño, grita → spawn inmediato de 5 Walkers    |
| **Brute**     | 400  | 1.2  | 30/hit| Va directo a la casa. Ignora al jugador salvo que se acerque |

---

## Sistema de mejoras (cartas roguelite)

Entre cada oleada el jugador elige 1 de 3 cartas aleatorias. Las cartas se dividen en categorías:

### Armas / Disparo

| Carta                   | Efecto                                              |
| ----------------------- | --------------------------------------------------- |
| Bala Perforante         | Las balas atraviesan hasta 3 zombies               |
| Disparo Doble           | Dispara 2 balas en paralelo                        |
| Cadencia +30%           | Aumenta la velocidad de disparo                    |
| Daño +25%               | Aumenta daño base de todas las armas               |
| Escopeta                | Cambia arma actual a escopeta (5 perdigones)       |
| Rifle de Precisión      | Cambia arma a rifle (daño alto, lento, penetra)    |
| Granada cada 8s         | Lanza automáticamente una granada periódicamente   |
| Bala explosiva (10%)    | 1 de cada 10 balas explota en área                 |

### Defensa / Casa

| Carta                   | Efecto                                              |
| ----------------------- | --------------------------------------------------- |
| Barricada Norte         | Coloca barricada en el lado norte de la casa (+HP) |
| Barricada Sur           | Coloca barricada en el lado sur de la casa         |
| Barricada Este/Oeste    | Lo mismo para los flancos                          |
| Trampa Eléctrica        | Daño a zombies que entran al radio de la casa      |
| Torretas x2             | 2 mini-torretas automáticas básicas en la casa     |
| Reparación Emergencia   | Restaura 30 HP de la casa                          |

### Jugador

| Carta                   | Efecto                                              |
| ----------------------- | --------------------------------------------------- |
| Vida +25                | Aumenta HP máximo del jugador                       |
| Curación                | Restaura 30 HP del jugador                         |
| Velocidad +20%          | El jugador se mueve más rápido                     |
| Imán de loot            | Radio de auto-pickup x2                            |
| Dash                    | Desbloquea habilidad de dash (doble tap/botón)     |
| Escudo temporal 10s     | Invulnerabilidad breve al inicio de cada oleada    |

### Rareza de cartas

- **Común** (blanco): efectos simples, aparecen siempre
- **Raro** (azul): efectos más fuertes, 30% de aparecer
- **Épico** (morado): cambia el arma o da habilidad especial, 10% de aparecer

---

## Personajes (meta-progresión)

El jugador desbloquea personajes con monedas ganadas en partidas. Cada uno tiene una pasiva diferente que cambia el estilo de juego.

| Personaje     | Costo     | Pasiva                                              |
| ------------- | --------- | --------------------------------------------------- |
| **Marcus**    | Gratis    | Sin pasiva — personaje base                        |
| **Elena**     | 500 monedas| Empieza con rifle · +15% velocidad de recarga     |
| **Rex**       | 800 monedas| +50 HP · las barricadas tienen el doble de dureza |
| **Yara**      | 800 monedas| Velocidad +25% · dash desbloqueado desde el inicio |
| **Doc**       | 1200 monedas| Curación lenta pasiva · botiquines curan +50%    |
| **Ghost**     | 1500 monedas| Zombie te detectan 30% menos · silenciador (sin atraer hordas por ruido) |

---

## Jugador — Control

### PC (secundario)

| Acción       | Control                              |
| ------------ | ------------------------------------ |
| Mover        | WASD                                 |
| Apuntar      | Ratón (el personaje rota hacia él)  |
| Disparar     | Clic izquierdo / mantener            |
| Recargar     | R                                    |
| Dash         | Espacio (si desbloqueado)            |
| Pausa        | Esc                                  |

### Mobile (primario)

| Acción       | Control                                          |
| ------------ | ------------------------------------------------ |
| Mover        | Joystick virtual izquierdo (fijo en pantalla)    |
| Apuntar      | Joystick virtual derecho                         |
| Disparar     | Auto-disparo al apuntar con joystick derecho     |
| Recargar     | Auto-recarga al vaciar el cargador               |
| Dash         | Botón en pantalla (si desbloqueado)              |
| Pausa        | Botón pausa en esquina superior derecha          |

---

## HUD en partida

```
┌──────────────────────────────────────────────────┐
│ [❤️ CASA: ████████░░] OLEADA 7        [⏸ PAUSA] │
│                                                  │
│                   gameplay                       │
│                                                  │
│ [❤️ ██████░░]   [🔫 Pistola  12/45]             │
│ jugador HP      arma + munición                  │
└──────────────────────────────────────────────────┘
```

- HP de la casa: barra en la parte superior (roja cuando baja)
- HP del jugador: barra inferior izquierda
- Arma + munición: inferior derecha
- Número de oleada: centro superior
- Entre oleadas: overlay con las 3 cartas centrado en pantalla

---

## Monetización

### Ads (rewarded)

| Momento                  | Oferta                              | Frecuencia     |
| ------------------------ | ----------------------------------- | -------------- |
| Game Over                | "Ver anuncio → Revivir"             | 1 vez / partida|
| Game Over                | "Ver anuncio → x2 monedas"          | Siempre        |
| Entre oleadas (opcional) | "Ver anuncio → Carta extra"         | 1 vez / partida|
| Menú principal           | Interstitial                        | Cada 2-3 partidas|

### IAP (compras)

| Pack                     | Precio   | Contenido                                    |
| ------------------------ | -------- | -------------------------------------------- |
| Sin Anuncios             | $2.99    | Elimina interstitials (rewarded siguen disponibles) |
| Starter Pack             | $0.99    | 500 monedas + personaje Elena                |
| Pack Marcus Especial     | $1.99    | Skin dorado de Marcus + 300 monedas          |

### Retención (para maximizar ad revenue)

| Mecánica             | Función                                           |
| -------------------- | ------------------------------------------------- |
| Recompensa diaria    | Log-in diario → monedas crecientes                |
| High score           | Tu mejor oleada guardada → quieres superarlo      |
| Leaderboard          | Top 10 global / amigos via Game Center / Google Play |
| Achievements         | "Sobrevive 20 oleadas" · "Mata 500 zombies" etc.  |
| Partida rápida       | Sin carga, sin menus largos → fácil volver        |

---

## Progresión de SaveData

```csharp
// SaveData — campos del juego
public int bestWave;            // mejor oleada global
public int totalKills;          // kills acumulados
public int totalCoins;          // monedas actuales
public int coinsEarned;         // total histórico (para achievements)
public int selectedCharacter;   // índice del personaje seleccionado
public int[] unlockedCharacters;// qué personajes están desbloqueados
public bool adsRemoved;         // compra "sin anuncios"
public int dailyLoginStreak;    // racha de logins
public string lastLoginDate;    // para calcular recompensa diaria
```

---

## Pipeline de escenas (simplificado)

```
Boot → MainMenu → GameScene
                     ↕
                  PauseMenu
                     ↕
                  GameOver → MainMenu
```

| Escena       | Función                                                     |
| ------------ | ----------------------------------------------------------- |
| Boot         | Splash logo + inicialización de managers                    |
| MainMenu     | Play · Personajes · Settings · Leaderboard                  |
| GameScene    | El juego completo (mapa único, HUD, sistema de oleadas)     |

No hay LevelSelect ni múltiples mapas en v1. Todo ocurre en GameScene.

---

## Roadmap técnico — Fases de desarrollo

### Fase 0 — Prototype (¿funciona?)

- [ ] Cámara ortográfica top-down con seguimiento
- [ ] Jugador: movimiento WASD + rotación hacia mouse
- [ ] Disparo básico: raycast o proyectil simple
- [ ] Zombie Walker: NavMesh hacia jugador/casa
- [ ] Casa con HP que recibe daño al contacto
- [ ] Spawner de oleadas: N zombies desde borde aleatorio
- [ ] Game Over al morir jugador o casa

**Meta:** Que sea jugable y divertido en 30 minutos de sesión de prueba.
**No incluir:** arte bueno, menús, sonido — solo que funcione.

### Fase 1 — Core loop completo

- [ ] Sistema de drops (munición, botiquines)
- [ ] Auto-pickup por radio
- [ ] Sistema de cartas entre oleadas (3 cartas random)
- [ ] Escalado de oleadas (Runners aparecen en oleada 4, Bloaters en 7...)
- [ ] HUD: HP jugador, HP casa, oleada, munición
- [ ] Pantalla Game Over con kills y oleada

### Fase 2 — Contenido y juiciness

- [ ] Todos los tipos de zombie (Bloater, Screamer, Brute)
- [ ] Todas las cartas de mejora
- [ ] Screen shake, partículas de sangre, flash de daño
- [ ] Sonidos: disparos, hits, gruñidos, música tensa
- [ ] Arte low-poly final (reemplazar placeholder)

### Fase 3 — Meta-progresión

- [ ] Sistema de monedas
- [ ] Pantalla de personajes con desbloqueo
- [ ] SaveData completo (best wave, coins, personajes)
- [ ] Recompensa diaria
- [ ] Achievements básicos

### Fase 4 — Monetización

- [ ] Integración AdMob (Unity Ads o AdMob SDK)
- [ ] Rewarded ad: revivir
- [ ] Rewarded ad: x2 monedas
- [ ] IAP: Sin Anuncios, Starter Pack
- [ ] Leaderboard (Google Play Games / Game Center)

### Fase 5 — Polish y lanzamiento

- [ ] UI/UX final mobile (tamaño de botones, feedback táctil)
- [ ] Testing en devices reales (iOS + Android)
- [ ] Optimización (batching, nivel de detalle)
- [ ] App Store / Google Play listing
- [ ] Build pipeline automatizado

---

## Lo que NO hay en v1 (para futuras versiones si funciona)

- ❌ Múltiples mapas / zonas
- ❌ Base building compleja
- ❌ Sistema de recursos complejos
- ❌ Supervivientes
- ❌ Ciclo día/noche largo
- ❌ Historia / narrativa

Si el juego tiene tracción → v1.1 puede añadir un segundo mapa o modo especial. Primero hay que lanzar.
