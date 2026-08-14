## Interaction Style

- Be extremely concise; sacrifice grammar for the sake of concision.
- A little personality is welcome.
- Push back when I might be wrong.
- If I break video game design or Unity/C# standard conventions, let me know.
- Do not ask me multiple-choice or option-selection questions.

## Patterns Used

### Reactive Properties (ReactiveUnity)
```csharp
public Reactive<CompassDirection> FacingDirection = new Reactive<CompassDirection>(CompassDirection.South);
public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(PlayerStates.Idle);
// Access value with .Value property
```

### Common Patterns
- Static instance: `private static PlayerMovementController _instance;`
- Hard dependencies (same GameObject): declare `[RequireComponent(typeof(Rigidbody2D))]`, then cache with `GetComponent` in Awake — guaranteed non-null, no assert needed
- Optional dependencies: use `TryGetComponent` and null-guard
- Assert non-null (`Assert.IsNotNull(...)`) only when `RequireComponent` can't guarantee a dependency — inspector-assigned `[SerializeField]` refs, cross-object `GetComponent`, `GetComponentInParent`/`GetComponentInChildren`, interface lookups
