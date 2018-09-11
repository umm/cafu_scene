# CAFU Scene

## What

* Scene loader for CAFU

## Requirement

* CAFU v3 (installed by umm)
* Zenject (installed by umm)

## Install

```shell
yarn add "umm/cafu_scene#^1.0.0"
```

## Usage

### Basic

* To load/unload scene simply

#### Setup

Create MonoInstaller to install bindings uses Zenject.

Call `CAFU.Scene.Application.Installer.SceneInstaller.Install()` in created installer.

```csharp
using CAFU.Scene.Application.Installer;
using Zenject;

public class SystemInstaller : MonoInstaller<SystemInstaller>
{
    public override void InstallBindings()
    {
        // SceneInstaller uses SimpleLoaderUseCase by default
        SceneInstaller.Install(Container);
    }
}
```

#### Load scene

Call `ILoadRequestEntity.RequestLoad()` with scene name in argument.

The calling class must be Bind by Zenject Installer

```csharp
public class Sample : MonoBehaviour
{
    [Inject] private ILoadRequestEntity LoadRequestEntity { get; }

    private void Start()
    {
        LoadRequestEntity.RequestLoad("Foo");
    }

    public void Unload()
    {
        LoadRequestEntity.RequestUnload("Bar");
    }
}

public class SampleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<Sample>().FromComponentInHierarchy().AsCached();
    }
}
```

You can use `enum` instead of string argument.

If you pass `enum` into argument of `RequestLoad()/RequestUnload()` method, invoke `.ToString()` when load/unload scene.

### Advanced

#### Strategic scene loading/unloading

You can setup strategic setting such as below things.

* Specify scenes pre-load scene.
* Specify scenes post-unload scene.
* Scene load multiple.
* Scene load as single.
  * Like as [`UnityEngine.SceneManagement.LoadSceneMode.Single`](https://docs.unity3d.com/ScriptReference/SceneManagement.LoadSceneMode.html).

##### 1. Create script for ScriptableObject

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CAFU.Core;
using CAFU.Scene.Domain.Structure;
using UnityEngine;

public interface ISceneStrategyStructureList : IStructure
{
    IReadOnlyDictionary<string, ISceneStrategyStructure> AsDictionary();
}

[CreateAssetMenu(fileName = "SceneStrategyStructureList", menuName = "Structures/Scene Strategy Structure List")]
public class SceneStrategyStructureList : ScriptableObject, ISceneStrategyStructureList
{
    [SerializeField] private List<SceneStrategyStructure> list;

    private IEnumerable<ISceneStrategyStructure> List => list;

    public IReadOnlyDictionary<string, ISceneStrategyStructure> AsDictionary()
    {
        return List.ToDictionary(x => x.SceneName, x => (ISceneStrategyStructure)x);
    }
}
```

##### 2. Create ScriptableObject from menu

<img src="https://user-images.githubusercontent.com/838945/45344601-15084c80-b5df-11e8-8dbb-8beaca87b7b2.png" width="697" height="161" />

`Create` &gt; `Structures` &gt; `Scene Strategy Structure List`

##### 3. Setup strategy

Open generated ScriptableObject in Inspector.

Setup all scenes strategy.

<img src="https://user-images.githubusercontent.com/838945/45345048-2b62d800-b5e0-11e8-98cf-a0116e187a81.png" width="362.5" height="960" />

##### 4. Bind strategies in Installer

```csharp
public class SystemInstaller : MonoInstaller<SystemInstaller>
{
    [SerializeField] private SceneStrategyStructureList sceneStrategyStructureList;

    private ISceneStrategyStructureList SceneStrategyStructureList => sceneStrategyStructureList;

    public override void InstallBindings()
    {
        // Use StrategicLoaderUseCase instead of SimpleLoaderUseCase
        SceneInstaller<StrategicLoaderUseCase>.Install(Container);
        // Bind initial loaded scenes
        Container
            .Bind<IEnumerable<string>>()
            .WithId(CAFU.Scene.Application.Constant.InjectId.InitialSceneNameList)
            .FromInstance(
                Enumerable
                    .Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .Select(x => x.name)
            );
        // Bind strategies
        Container
            .Bind<IReadOnlyDictionary<string, ISceneStrategyStructure>>()
            .WithId(CAFU.Scene.Application.Constant.InjectId.UseCase.SceneStrategyMap)
            .FromInstance(SceneStrategyStructureList.AsDictionary());
    }
}
```

#### Customize scene name

For excample, if your scene name has `Foo_` prefix then bind completer in Installer.

This completer will invoke before call `UnityEngine.SceneManagement.SceneManager.LoadScene()/UnloadScene()`.

```csharp
Container
    .Bind<Func<string, string>>()
    .WithId(CAFU.Scene.Application.Constant.InjectId.SceneNameCompleter)
    .FromInstance(sceneName => $"Foo_{sceneName}");
}
```

Of cource, you can bind more complicative callback.

## License

Copyright (c) 2018 Tetsuya Mori

Released under the MIT license, see [LICENSE.txt](LICENSE.txt)
