# Fishblitz Development Guide

## Overview
Fishblitz is a top-down pixel art RPG built with Unity (2022.3+). It features fishing, birding, crafting, and survival mechanics.

## Project Structure
```
Assets/
├── Scripts/
│   ├── Base Systems/     # Core systems (GameClock, Singleton, Logger, Persistence)
│   ├── Player/          # Player-related scripts (movement, inventory, UI)
│   ├── Characters/      # NPC characters and dialogue
│   ├── WorldObjects/    # Trees, stumps, campfires, etc.
│   ├── Items/           # Item definitions (tools, fish, mushrooms)
│   ├── Birding/         # Birding mini-game
│   ├── Combat/          # Combat systems (projectiles, steering, health, knockback)
│   ├── Fishing/         # Fishing mini-game
│   ├── Weather/         # Weather systems
│   ├── UI/              # UI components
│   ├── Scene/           # Scene management
│   └── Utilities/       # Logger, Singleton, etc.
├── Graphics/            # Sprite assets organized by category
├── Prefabs/             # Reusable game objects
├── Scenes/              # Unity scenes
├── Resources/           # Runtime-loaded assets
├── Settings/           # Project settings
└── Shaders/             # Custom shaders
```

## Build & Development Commands

### Unity Editor
- Open project in Unity Hub (version 2022.3 or later recommended)
- Use Unity Editor to build and run: `File > Build Settings`
- Tests can be run via `Window > General > Test Runner`

### Command Line Build (macOS)
```bash
# Open Unity and build (requires Unity Pro)
"/Applications/Unity/Unity.app/Contents/MacOS/Unity" -quit -batchmode -projectPath /path/to/fishblitz -buildTarget MacStandalone -buildPath ./Build
```

### Running a Single Test
1. Open Unity Editor
2. Go to `Window > General > Test Runner`
3. Right-click a test and select "Run Selected" or use keyboard shortcut

Note: This project currently has no formal unit tests in the NUnit framework.

## Code Style Guidelines

### Naming Conventions
- **Classes/Structs**: `PascalCase` (e.g., `PlayerMovementController`, `GameClock`)
- **Interfaces**: `I` prefix (e.g., `IGiftable`, `ITickable`)
- **Enums**: `PascalCase` with values also in `PascalCase`
- **Public Methods/Properties**: `PascalCase`
- **Private Fields**: `_camelCase` with underscore prefix (e.g., `_playerData`, `_rb`)
- **Protected Fields**: Same as private (`_camelCase`)
- **Constants**: `PascalCase` (e.g., `DEFAULT_MOVE_SPEED`)
- **Unity Events**: `PascalCase` (e.g., `OnEnable`, `Start`)

### Code Organization
```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;
using NUnit.Framework;

public class MyClass : MonoBehaviour
{
    // Nested types (enums, structs, classes)
    public enum MyEnum { Value1, Value2 }
    
    // Public serialized fields
    [SerializeField] private PlayerData _playerData;
    
    // Private fields
    private Rigidbody2D _rb;
    private const float DEFAULT_SPEED = 3.5f;
    
    // Reactive properties (from ReactiveUnity)
    public Reactive<bool> IsActive = new Reactive<bool>(false);
    
    // Events
    public event Action OnSomethingHappened;
    
    // Unity lifecycle methods first
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    
    // Public methods
    public void DoSomething() { }
    
    // Private methods
    private void HandleInput() { }
}
```

### Imports
Always order imports alphabetically:
1. System namespaces (`System`, `System.Collections`, `System.IO`)
2. Unity namespaces (`UnityEngine`, `UnityEngine.InputSystem`)
3. Third-party (`ReactiveUnity`, `NUnit.Framework`)
4. Project-specific (none)

### Attributes
- Use `[SerializeField]` for private fields exposed in Inspector
- Use `[Header("Section Title")]` to organize Inspector fields
- Use `[NonSerialized]` for public fields not in Inspector
- Use `[CreateAssetMenu(fileName = "...", menuName = "...")]` for ScriptableObjects
- Use `[Serializable]` for non-MonoBehaviour classes serialized by Unity

### Patterns Used

#### Singleton Pattern
```csharp
public class GameClock : Singleton<GameClock>
{
    protected override void Awake()
    {
        base.Awake();
        // initialization
    }
}
```

#### Reactive Properties (ReactiveUnity)
```csharp
public Reactive<CompassDirection> FacingDirection = new Reactive<CompassDirection>(CompassDirection.South);
public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(PlayerStates.Idle);
// Access value with .Value property
```

#### Logger Usage
```csharp
[SerializeField] private Logger _logger = new();

_logger.Info("Player moved");
_logger.Warning("Low health");
_logger.Verbose("Detailed debug info");
```

#### Interface Pattern
```csharp
public interface ITickable
{
    void OnGameMinuteTick();
}
```

### Error Handling
- Use `Debug.LogError()` for critical errors that should halt execution
- Use `Debug.LogWarning()` for non-critical issues
- Use `Debug.Assert()` or `Assert.IsNotNull()` from NUnit for development checks
- Use try-catch for file I/O operations

### General Guidelines
- Keep classes focused and single-responsibility
- Use `ScriptableObject` for data containers (items, inventory)
- Use `MonoBehaviour` for components attached to GameObjects
- Comment complex logic, but avoid obvious comments
- Use meaningful variable names - avoid single letters except for common patterns (x, y for coordinates)
- Follow 100-character line limit when practical
- Use braces even for single-line statements

### Common Patterns
- Static instance: `private static PlayerMovementController _instance;`
- Component caching: `_rb = GetComponent<Rigidbody2D>();` in Awake
- Assert non-null: `Assert.IsNotNull(_rb);` after GetComponent
- Coroutines for timed operations: `StartCoroutine(MyCoroutine())`
- Input System: Use `UnityEngine.InputSystem` with `OnMove(InputValue value)`

### Testing
- NUnit is available (used in PlayerMovementController imports)
- Add tests to appropriate test assemblies
- Use `[Test]` and `[UnityTest]` attributes
- Use `Assert.IsNotNull()`, `Assert.AreEqual()`, etc.

## Additional Notes
- Uses Universal Render Pipeline (URP) for 2D graphics
- Uses Cinemachine for camera control
- Uses Unity Input System (new) not legacy Input Manager
- Custom shaders in Assets/Shaders/
- Audio in Assets/Sound/
- Save system uses JSON persistence via BlueOyster package

## Prefab Setup: FlyingChaser

Create the prefab at `Assets/Prefabs/Combat/FlyingChaser.prefab` with the following structure:

### Root GameObject ("FlyingChaser")

| Component | Settings |
|---|---|
| `FlyingChaser` | Configure wander speeds, chase speed (5), view radius (8), contact damage (1), self-knockback force (15) |
| `Rigidbody2D` | Body Type: Dynamic, Linear Drag: 2, Gravity Scale: 0, Sleeping Mode: Never Sleep |
| `CircleCollider2D` (body) | Is Trigger: false, used for world collision |
| `CircleCollider2D` (hitbox) | Is Trigger: true, slightly smaller than body, for contact damage detection |
| `SpriteRenderer` | Assign your flying enemy sprite |
| `EnemyHealth` | Assign `_maxHealth` in Inspector |

### Child "Hurtbox"

| Component | Settings |
|---|---|
| `Collider2D` | Is Trigger: true, layer: `EnemyHurtbox` |
| `EnemyHurtbox` | — |

### Child "ViewRange" (optional, for gizmo visualization)

| Component | Settings |
|---|---|
| `CircleCollider2D` | Is Trigger: true, Radius: matching `_viewRadius` setting |

### Layer setup
- Root GameObject layer: add a new layer like "FlyingChaser" or use existing non-player layer (must not collide with FriendlyHurtbox via collision matrix so that the hitbox trigger handles contact instead)
- Ensure `FlyingChaser._obstacleLayers` in the Inspector includes the layers the chaser should avoid (e.g., Default, Ground, Cover)

### Player setup
- `PlayerHealth` component must be added to the Player root GameObject (alongside PlayerMovementController)
