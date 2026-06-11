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

## 📚 Learn more

- **Setup guide** → [`Documentation~/README.md`](Documentation~/README.md)
- **Full architecture** → [`Docs/README.md`](../../Docs/README.md)
- **Import into a game** → [`IMPORT_GUIDE.md`](../../IMPORT_GUIDE.md)

---

<p align="center"><sub>Built with ❤️ on Unity 6 LTS · Mudit Template</sub></p>
