# Fishblitz Development Guide

## Overview
Fishblitz is a top-down pixel art RPG built with Unity (2022.3+). It features fishing, birding, crafting, and survival mechanics.

## Design Philosophy

A local community drama about learning to live within limits of nature — a cycle of adaptation and repair. IDEAS.md is the source of detail; these principles guide decisions.

1. People never hurt the player. Conflict comes from the environment and the land, strained by years of expansion and extraction — not instant or cartoonish evil.

2. Progress comes from choices, tradeoffs, and cooperation. Keep environmentalism in the background.

3. The Presences: real enough to matter, never so direct they become the solution. Ancient, limited, observant. They operate as omens, whispers, memory, and pressure from the land. Never confirm them in a way that removes doubt; NPCs interpret the same signs differently.

4. Storytelling must never interrupt the primary act of playing. Keep direct dialogue very brief. Longer content is delivered as audio overlay you can listen to while playing. Environment should reinforce the story.

5. Story is filtered through bias. NPCs withhold, misremember, misinterpret, rationalize.

6. Costly resources shape behavior. Scarcity is a design lever — arrow cost encourages stealth, careful aim, and evasion. Every tool carries a cost or downside; tradeoffs over raw stats.

7. Community arrives slowly. People return based on time, events, or player milestones, each filling a role.

8. Seasons structure the drama. Main seasons carry chronic problems (hot summers, cold winters); shoulder seasons bring catastrophic, somewhat-random events with subtle foreshadowing. The player should realize damage will happen but couldn't prepare enough.

## Communication Style

When talking to the user:

- Be extremely concise. Sacrifice grammar for the sake of concision.
- Keep a light voice, no ornament. A little personality is welcome; no aphorisms or rhetorical flourishes.
- Be casual and direct.
- Be a teammate, support me, but also push-back when I might be wrong.
- If my approach breaks standard conventions, call me out.

## Code Organization
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

### Patterns Used

#### Reactive Properties (ReactiveUnity)
```csharp
public Reactive<CompassDirection> FacingDirection = new Reactive<CompassDirection>(CompassDirection.South);
public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(PlayerStates.Idle);
// Access value with .Value property
```

### General Guidelines
- Keep classes focused and single-responsibility
- Comment complex logic, but avoid obvious comments
- Use meaningful variable names - avoid single letters except for common patterns (x, y for coordinates)

### Common Patterns
- Static instance: `private static PlayerMovementController _instance;`
- Hard dependencies (same GameObject): declare `[RequireComponent(typeof(Rigidbody2D))]`, then cache with `GetComponent` in Awake — guaranteed non-null, no assert needed
- Optional dependencies: use `TryGetComponent` and null-guard
- Assert non-null (`Assert.IsNotNull(...)`) only when `RequireComponent` can't guarantee a dependency — inspector-assigned `[SerializeField]` refs, cross-object `GetComponent`, `GetComponentInParent`/`GetComponentInChildren`, interface lookups
