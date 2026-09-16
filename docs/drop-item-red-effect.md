# Red item-drop effect preview

The two animations are registered separately in `RoRebuildServer/GameConfig/ServerData/Db/Effects.csv`:

| ID | Name | Animation |
| --- | --- | --- |
| 147 | DropItemRedTop | dropitem_red |
| 148 | DropItemRedBottom | dropitem_red_bottom |

Regenerate the client effect list from the repository root:

```text
dotnet run --project RoRebuildServer/DataToClientUtility -- --effects-only
```

Then in Unity, choose **Ragnarok > Load Effects**. The standard importer reads
the generated `effects.json`, creates both prefabs, and registers them with
Addressables. The source files come from the configured Ragnarok data directory:

```text
texture/effect/new_dropitem/dropitem_red/dropitem_red/dropitem_red.str
texture/effect/new_dropitem/dropitem_red/dropitem_red_bottom/dropitem_red_bottom.str
```

The top entry assigns `SoundFile=drop_red`, matching the executable's
`wav/effect/drop_red.wav`. Copy that file from the configured data directory to
`Assets/Sounds/effect/drop_red.wav` before importing the effect (the normal data
copy workflow also copies it). The bottom entry has no sound, so the combined
preview plays the clip once at startup. The sound does not repeat with the STR loop.

Enter Play mode, log in, and type:

```text
/effect dropitem_red
```

`/effect DropItemRed` also works. Both parts start together, loop for ten seconds,
and disappear together. The preview stays where your character was when you
issued the command. Move away and rotate/tilt the camera to inspect it. Repeating
the command creates another independent preview. It works in a built client too,
provided the effect was imported before building.

The individual prefabs are generated at `Assets/Effects/Prefabs/DropItemRedTop.prefab`
and `Assets/Effects/Prefabs/DropItemRedBottom.prefab`. The combined preview resolves
both entries through the client effect registry and cache, loads their Addressables,
and waits for both before creating them under a shared camera-facing parent.
There is no combined Resources prefab or separate import menu.

Every ground item also receives this combined effect. It is attached to the item
root, so it stays at the drop location while the item sprite bounces, inherits
the item's billboard rotation, and disappears when the item is picked up,
removed, or cleared on a map change. Item beams have no ten-second timeout.
Fresh drops play the sound once; existing items revealed on entering a map are
silent. The effect waits for terrain placement and both assets before spawning,
and a removed item cannot acquire a beam after an asynchronous load completes.

The standard importer sets these two animations to the native origin `(320, 240)`
and disables randomized loop starts. Other effects retain the existing origin
`(320, 320)` and loop settings. The ten-second lifetime applies only to the slash-command preview.

`/effect DropItemRedTop` and `/effect DropItemRedBottom` also work through the
normal effect lookup (or use IDs 147 and 148). These individual looping effects
do not have the combined preview's ten-second cleanup; restart Play mode to clear them.

The STR files control the animation. No replacement particles or textures are
generated. The imported assets remain in the existing ignored effects folder;
commit the importer and client code, not the generated assets or compiled DLLs.
The standard importer skips existing prefabs. To reimport changed source STR files
or import settings, delete the two generated prefabs in Unity and run **Load Effects** again.

Verification in game:

- The base glow and light/star animation appear together at your feet.
- Both remain camera-facing as you move the camera, with no floating 1.6-unit
  origin offset.
- Both loop without randomly starting at different times.
- The drop sound plays once when the combined effect starts.
- The entire preview disappears after ten seconds.
- A newly dropped item has a beam and one sound; its beam remains after ten seconds.
- Picking up/removing the item or changing maps removes its beam.
- Entering a map with existing drops shows their beams without replaying drop sounds.
