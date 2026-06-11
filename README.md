# 🧩 Mudit Core

> A production-ready **mobile game framework** for Unity 6 LTS — boot, services, UI, and saves wired up so you can start building the *game*, not the plumbing.

<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-6000.0%20LTS-000?logo=unity">
  <img alt="URP" src="https://img.shields.io/badge/Render-URP%2017-blue">
  <img alt="DI" src="https://img.shields.io/badge/DI-VContainer-7952B3">
  <img alt="Async" src="https://img.shields.io/badge/Async-UniTask-2C8EBB">
  <img alt="Platforms" src="https://img.shields.io/badge/Platforms-Android%20%7C%20iOS-success">
</p>

---

## ✨ What you get

| | Service | What it does |
|---|---|---|
| 📊 | **Analytics** | Firebase event tracking |
| 📺 | **Ads** | IronSource LevelPlay (banner, interstitial, rewarded) |
| 💳 | **Payments** | Platform IAP (Android / iOS / Mock) |
| 🔊 | **Audio** | Crossfade music + pooled SFX |
| 🖼️ | **UI** | Stack-based views with fade & overlay support |
| 🎬 | **Scenes** | Async scene loading with loading screen |
| 💾 | **Save** | Encrypted · compressed · plain modes |

All powered by **VContainer** (DI), **MessagePipe** (events), **UniTask** (async), and **UniRx** (reactive state).

---

## 🚀 Boot Flow

```
Boot scene  ─►  RootLifetimeScope        register every service
            ─►  AppBootstrapper          InitializeAsync() each service, in order
            ─►  MainMenu / GamePlay       your game starts here
```

> One entry point. Every service lives for the whole app lifetime — no singletons, everything injected.

---

## 🔍 Under the Hood

### Runtime boot sequence

```mermaid
sequenceDiagram
    autonumber
    participant Ed as EditorBootStrapper
    participant Root as RootLifetimeScope
    participant Entry as SingleEntryPoint
    participant Boot as BootLifetimeScope (child)
    participant App as AppBootstrapper
    participant Svc as IRootService list
    participant Scene as SceneLoaderService

    Ed->>Root: force-load "Boot" scene
    Root->>Root: Configure() registers MessagePipe + services
    Root->>Entry: RegisterEntryPoint(IStartable)
    Note over Entry: Start() fires after container build
    Entry->>Boot: CreateChildFromPrefab(BootLifetimeScope)
    Boot->>App: Resolve AppBootstrapper
    loop each IRootService, in order
        App->>Svc: await InitializeAsync(serviceData)
    end
    App-->>Entry: boot complete
    Entry->>Boot: Dispose() child scope
    Entry->>Scene: LoadSceneAsync(MainMenu / GamePlay)
```

### How the pieces connect

```mermaid
flowchart TB
    Config["ScriptableObjects<br/>ServiceData · AppData · AudioData<br/>ScopeRegistryData · SceneUIDatabase"]

    subgraph Container["VContainer — Root DI Container · app lifetime"]
        direction TB
        MP(["MessagePipe<br/>pub / sub events"])
        subgraph Services["Registered as IRootService · Singleton"]
            direction LR
            A["📊 Analytics"]
            B["📺 Ads"]
            P["💳 Payments*"]
            AU["🔊 Audio"]
            UI["🖼️ UI"]
            SC["🎬 SceneLoader"]
            LO["⏳ Loading"]
            SV["💾 Save"]
        end
    end

    Config -->|injected| Services
    Services -->|InitializeAsync| App["AppBootstrapper"]
    UI -->|Show&lt;T&gt; / Back stack| Views["UIView screens"]
    SC -->|LoadSceneAsync| Scenes["MainMenu · GamePlay"]
    SC -.->|SetUIViewPrefabsAsync| UI
    MP -.->|decoupled events| Services

    %% * Payments resolves per platform: Android / iOS / Mock
```

> **\* Payments** swaps implementation at compile time — `AndroidPaymentService`, `IOSPaymentService`, or `MockPaymentService` (editor) — all behind the same `IPaymentService`.
> Each service can be toggled off via a checkbox on `RootLifetimeScope` (`isAdsEnabled`, `isAudioEnabled`, …).

---

## 🧱 Architecture at a glance

```
Assets/Core/
├── Runtime/
│   ├── Boot/             ← bootstrapper + entry point
│   ├── LifetimeScopes/   ← DI registration (Root / Boot)
│   ├── Interfaces/       ← IRootService contracts
│   ├── Services/         ← Analytics, Ads, Audio, UI, Save, …
│   ├── ScriptableObjects/← config data (ServiceData, AppData, …)
│   └── UI/               ← UIView base + helpers
└── Samples~/            ← starter scenes, prefabs & demo views
```

---

## 🔌 Add your own service in 3 steps

```csharp
// 1. Contract
public interface IMyService : IRootService { }

// 2. Implementation
public class MyService : IMyService
{
    public UniTask InitializeAsync(ServiceData settings) => UniTask.CompletedTask;
}

// 3. Register in RootLifetimeScope.Configure()
builder.Register<IMyService, MyService>(Lifetime.Singleton);
```

---

## ⚙️ Configure everything from `Create ▸ Mudit ▸ …`

`ServiceData` · `AppData` · `AudioData` · `ScopeRegistryData` · `SceneUIDatabase` — drop in your keys, clips, scenes, and UI prefabs. No code required.

---

