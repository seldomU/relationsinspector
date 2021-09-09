<p align="center">

  <h2 align="center">RelationsInspector</h2>

  <p align="center">
    An editor extension for the Unity game engine.
    <br />
    <a href="https://assetstore.unity.com/packages/tools/utilities/relationsinspector-53147"><strong>Download from the Asset Store</strong></a>
    <br />
    <br />
    <a href="https://github.com/seldomU/RIBackendUtil/wiki/RelationsInspector-Manual">User docs</a>
    ·
    <a href="https://forum.unity.com/threads/relationsinspector-reveal-structures-in-your-project-demo.382792/">Discussion</a>
  </p>
</p>


## About

This is a node based editor and viewer that can be connected to all kinds of data in your Unity project. Its main advantage is probably the automatic layout of nodes, allowing you to quickly grasp how they are connected. Support exists for [asset references](https://seldomu.github.io/riBackends/AssetReferenceBackend/), [asset dependencies](https://seldomu.github.io/riBackends/AssetReferenceBackend/), [UI events](https://seldomu.github.io/riBackends/UGUIeventBackend/), [playmaker state machines](https://seldomu.github.io/riBackends/PlayMakerFsmCommunicationBackend/) and more, but you can also add your own.


## Usage

If you simply want to use RelationsInspector, [install it](https://assetstore.unity.com/packages/tools/utilities/relationsinspector-53147) through the asset store. If you want to add support for your use case, [see the documentation](https://github.com/seldomU/RIBackendUtil/wiki/RelationsInspector-Manual#backend-development) for the [utility classes](https://github.com/seldomU/RIBackendUtil). If you want to change the tool's internal library, this repository is for you.

### Build

Run `build.bat` to generate `RIDLLProject\bin\Release\RelationsInspector.dll`. Copy that file to any Unity project that has RelationsInspector already installed, to the folder `Assets\Plugins\Editor\RelationsInspector`. There are two paths that you have to adjust to your environment:
 * the path to msbuild.exe in `build.bat`
 * the path to your Unity editor in `build.bat` and in `RIDllProject/RelationsInspectorLib.csproj`. Replace the paths containing *Unity 5.5*.

## License

Distributed under the MIT License. See `LICENSE` for more information.
