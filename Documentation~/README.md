# Mudit Core

Mobile game framework with DI-based boot sequence, service architecture, and UI management.

---

## Step by Step Setup Guide

### Step 1 — Install dependencies in `manifest.json`

Open `Packages/manifest.json` in your project and add these to `"dependencies"`:

```json
{
  "dependencies": {
    "com.cysharp.messagepipe": "https://github.com/Cysharp/MessagePipe.git?path=src/MessagePipe.Unity/Assets/Plugins/MessagePipe",
    "com.cysharp.messagepipe.vcontainer": "https://github.com/Cysharp/MessagePipe.git?path=src/MessagePipe.Unity/Assets/Plugins/MessagePipe.VContainer",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.neuecc.unirx": "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts"
  }
}
```

VContainer must also be installed. Add it via the Package Manager or manifest:

```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.8"
```

### Step 2 — Install Firebase

Download `FirebaseAnalytics.unitypackage` from the [Firebase console](https://firebase.google.com/) and import it by hand via **Assets > Import Package > Custom Package**.

### Step 3 — Install Mudit Core

In the Package Manager, click **+** and choose **Add package from git URL**. Since this is a private repo, use the SSH link:

```
git@github.com:Higlix/Mudit-UnityLTS6-Template.git
```

Or add it directly to `manifest.json`:

```json
"com.mudit.core": "git@github.com:Higlix/Mudit-UnityLTS6-Template.git"
```

### Step 4 — Create the Boot scene

Create a new scene called `Boot` and add it to **File > Build Settings** at index 0. Add any other game scenes after it (e.g. MainMenu at index 1, GamePlay at index 2).

### Step 5 — Create a VContainerSettings asset

In the Project window, right-click and select **Create > VContainer > VContainer Settings**. Place the asset at the top level of your project (e.g. `Assets/VContainerSettings.asset`).

### Step 6 — Create the LifetimeScope GameObjects

In the Boot scene, create two empty GameObjects:

- `RootLifetimeScope` — add the `RootLifetimeScope` component (or your own subclass of it)
- `BootLifetimeScope` — add the `BootLifetimeScope` component

### Step 7 — Create the ScriptableObject assets

Right-click in the Project window and create each of these:

| Menu Path | Asset Name |
|---|---|
| **Create > Mudit > Data > ScopeRegistryData** | `ScopeRegistryData` |
| **Create > Mudit > Data > AppData** | `AppData` |
| **Create > Mudit > Data > ServiceData** | `ServiceData` |
| **Create > Mudit > Data > AudioData** | `AudioData` |
| **Create > Mudit > Database > SceneUIDatabase** | `SceneUIDatabase` |

### Step 8 — Wire up ServiceData

Select your `ServiceData` asset and assign:

- **Audio Data** → your `AudioData` asset
- **Scene UI Database** → your `SceneUIDatabase` asset
- Fill in your game's API keys, ad unit IDs, etc.

### Step 9 — Wire up RootLifetimeScope

Select the `RootLifetimeScope` GameObject and assign:

- **Scope Registry Data** → your `ScopeRegistryData` asset

### Step 10 — Wire up BootLifetimeScope

Select the `BootLifetimeScope` GameObject and assign:

- **Boot Settings** → your `AppData` asset
- **Service Data** → your `ServiceData` asset

### Step 11 — Turn LifetimeScopes into prefabs

Drag both `RootLifetimeScope` and `BootLifetimeScope` from the hierarchy into your Project folder to create prefabs. Then delete them from the scene.

### Step 12 — Register the Boot prefab in ScopeRegistryData

Select your `ScopeRegistryData` asset. Add a new entry to the **Scope Prefabs** list and assign your `BootLifetimeScope` prefab.

### Step 13 — Set up the RootLifetimeScope prefab hierarchy

Open the `RootLifetimeScope` prefab. Create two child GameObjects under it:

```
RootLifetimeScope (prefab root)
├── RootGameObject        (empty GameObject)
└── LoadingCanvas          (add a Canvas component)
    └── Image              (full-screen image as loading background)
```

Then on the `RootLifetimeScope` component, assign:

- **Root Game Object > Root** → the `RootGameObject` child
- **Root Game Object > Transform** → the same `RootGameObject` child's Transform
- **Loading Canvas** → the `LoadingCanvas` child

### Step 14 — Set the VContainerSettings root

Select your `VContainerSettings` asset and assign the **Root Lifetime Scope** field to your `RootLifetimeScope` prefab.

### Step 15 — Press Play

Open the Boot scene and press Play. The boot sequence will run, initialize all services, then load your first game scene.

---

## Architecture

### Boot Flow

```
App Start
  → EditorBootStrapper (editor only: redirects any scene to Boot)
  → Boot scene loads → VContainer instantiates RootLifetimeScope prefab
  → SingleEntryPoint.Start()
  → Creates BootLifetimeScope as child scope (from ScopeRegistryData)
  → AppBootstrapper.BootAsync()
  → Loops through all IRootService implementations → InitializeAsync()
  → Disposes BootLifetimeScope
  → SceneLoaderService.LoadSceneAsync("MainMenu")
```

**Editor shortcut:** If you press Play from any scene other than Boot, `EditorBootStrapper` automatically redirects to Boot, runs the full boot sequence, then loads the scene you were in. You can test from any scene without manually switching to Boot.

### IRootService Pattern

Every service that needs boot-time initialization implements `IRootService`:

```csharp
public interface IRootService
{
    UniTask InitializeAsync(ServiceData settings);
}
```

Specific interfaces extend it with their own methods:

```csharp
public interface IAdsService : IRootService
{
    void ShowInterstitial();
    void ShowBanner();
    void BannerToggleVisibility();
    void LoadRewarded();
    void ShowRewarded(Action onReward);
}
```

Services are registered in `RootLifetimeScope` with both their specific interface and `IRootService`:

```csharp
builder.Register<AdsService>(Lifetime.Singleton)
    .As<IAdsService>()
    .As<IRootService>();
```

`AppBootstrapper` resolves all `IRootService` implementations automatically:

```csharp
public class AppBootstrapper
{
    readonly IReadOnlyList<IRootService> services;
    readonly ServiceData serviceData;

    [Inject]
    public AppBootstrapper(IReadOnlyList<IRootService> services, ServiceData serviceData)
    {
        this.services = services;
        this.serviceData = serviceData;
    }

    public async UniTask BootAsync()
    {
        foreach (var service in services)
        {
            await service.InitializeAsync(serviceData);
        }
    }
}
```

Registration order in `RootLifetimeScope.Configure()` determines initialization order.

---

## Adding a Custom Service

### 1. Define the interface

```csharp
public interface ILeaderboardService : IRootService
{
    UniTask SubmitScore(int score);
    UniTask<List<ScoreEntry>> GetTopScores(int count);
}
```

### 2. Implement it

```csharp
public class LeaderboardService : ILeaderboardService, IDisposable
{
    public async UniTask InitializeAsync(ServiceData settings)
    {
        // Connect to backend, authenticate, etc.
    }

    public async UniTask SubmitScore(int score) { /* ... */ }
    public async UniTask<List<ScoreEntry>> GetTopScores(int count) { /* ... */ }

    public void Dispose() { /* cleanup */ }
}
```

### 3. Register it

Create a subclass of `RootLifetimeScope` and override the virtual hook:

```csharp
public class MyGameRootLifetimeScope : RootLifetimeScope
{
    protected override void ConfigureLocalRootLifetimeScope(IContainerBuilder builder)
    {
        builder.Register<LeaderboardService>(Lifetime.Singleton)
            .As<ILeaderboardService>()
            .As<IRootService>();
    }
}
```

Use this subclass on your RootLifetimeScope prefab instead of the base `RootLifetimeScope`. The new service is automatically initialized during boot and injectable anywhere.

---

## ScriptableObject Reference

| ScriptableObject | Create Menu | What to configure |
|---|---|---|
| **ServiceData** | `Mudit/Data/ServiceData` | Ad unit IDs, API keys, references to AudioData and SceneUIDatabase |
| **AppData** | `Mudit/Data/AppData` | Debug flag, target FPS |
| **AudioData** | `Mudit/Data/AudioData` | Music/SFX/UI clip lists (key + clip + volume), default volumes |
| **ScopeRegistryData** | `Mudit/Data/ScopeRegistryData` | LifetimeScope prefabs instantiated at runtime |
| **SceneUIDatabase** | `Mudit/Database/SceneUIDatabase` | Per-scene UI view prefab lists with Eager/Lazy mode |

### Wiring Diagram

```
Boot Scene
  └── VContainerSettings.asset → RootLifetimeScope prefab

RootLifetimeScope (prefab)
  ├── [ScopeRegistryData]  → holds BootLifetimeScope prefab
  ├── [RootGameObject]     → root + transform (child GameObject)
  └── [LoadingCanvas]      → loading screen (child GameObject)

BootLifetimeScope (prefab, created at runtime as child scope)
  ├── [AppData]            → debug flag, FPS
  └── [ServiceData]        → API keys, ad config, and references to:
        ├── [AudioData]        → clip lists, volumes
        └── [SceneUIDatabase]  → per-scene UI view prefabs
```

---

## UI System

### Creating a View

All screen views inherit from `UIView`:

```csharp
public class SettingsView : UIView
{
    [SerializeField] private Slider volumeSlider;

    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    public override async UniTask Initialize()
    {
        await base.Initialize();
        volumeSlider.onValueChanged.AddListener(v =>
            audioService.SetVolume(AudioChannel.Master, v));
    }
}
```

Create a prefab from this MonoBehaviour and add it to your scene's entry in `SceneUIDatabase`.

### Showing and Navigating Views

```csharp
// Show a view (pushes onto stack, hides previous unless it's an overlay)
await uiService.Show<SettingsView>();

// Go back to previous view
uiService.Back();

// React to view changes
uiService.CurrentView.Subscribe(view => Debug.Log($"Now showing: {view}"));
```

### Scene UI Setup

In your `SceneUIDatabase` asset, add an entry for each scene:

- **Scene Name** — must match the Unity scene name exactly (e.g. `"MainMenu"`)
- **View Registry Data** — ordered list of UIView prefabs. The first is shown on scene load.
- **Instantiation Mode**:
  - **Eager** — all views instantiated at scene load (only first shown)
  - **Lazy** — only first instantiated; others created on first `Show<T>()`

### Switching Scenes

```csharp
// Inject ISceneLoaderService, then:
sceneLoaderService.LoadSceneAsync("GamePlay").Forget();
```

This shows the loading screen, loads the scene, sets up UI from `SceneUIDatabase`, then hides the loading screen.

---

## Built-in Services

| Service | Interface | What it does |
|---|---|---|
| Analytics | `IAnalyticsService` | Firebase Analytics init and event logging |
| Ads | `IAdsService` | LevelPlay banner, interstitial, rewarded ads |
| Payments | `IPaymentService` | Platform-specific IAP (Android/iOS/Mock) |
| Audio | `IAudioService` | Music crossfade, SFX pool, per-channel volume |
| UI | `IUIService` | View stack, scene-based UI, show/hide/back |
| Scene Loader | `ISceneLoaderService` | Scene transitions with loading screen |
| Loading | `ILoadingService` | Loading screen show/hide |
| Save | `ISaveService` | Interface only — implement per project |
