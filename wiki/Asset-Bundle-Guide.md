# Asset Bundle Guide

AssetBundles are Unity's format for packaging 3D models, textures, prefabs, and other assets.

---

## Creating a Bundle

### In Unity

1. Create a new Unity project
2. Import your models/textures
3. Create prefabs from your models
4. Mark your assets as `AssetBundle` in the inspector (bottom of the asset import window)
5. Build the bundles via a script or the `AssetBundle Browser` window

### Build Script

Create an `Editor/` folder and add this script:

```csharp
using UnityEditor;

public class BuildBundles
{
    [MenuItem("Tools/Build AssetBundles")]
    static void Build()
    {
        BuildPipeline.BuildAssetBundles(
            "Assets/AssetBundles",
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );
    }
}
```

The output `.bundle` files go into your mod's `assets/bundles/` folder.

---

## Loading Bundles in Your Mod

Bundles are NOT loaded automatically. You must explicitly load them in `OnLoad()`:

```csharp
public void OnLoad(IModContext api)
{
    api = api;
    api.LoadBundles();   // loads all *.bundle from assets/bundles/
}
```

After loading, access assets by name:

```csharp
// Get a prefab to spawn later
var prefab = api.LoadAsset<GameObject>("MyPrefab");

// Or spawn it directly
api.SpawnAsset("MyPrefab", new Vector3(0, 1, 0));
```

---

## Folder Layout

```
YourMod/
└── assets/
    └── bundles/
        ├── characters.bundle
        └── props.bundle
```

Each `.bundle` file gets loaded and all its assets become available by name (without extension).

---

## Best Practices

- **Name assets clearly** — you reference them by name in code
- **Keep bundles focused** — one bundle per logical group (characters, props, UI)
- **Don't forget `LoadBundles()`** — no error is thrown if you skip it, but `LoadAsset` and `SpawnAsset` will return `null`
