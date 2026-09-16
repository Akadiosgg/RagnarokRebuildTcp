# Shared Guard effect

`GuardEffect.Create(target)` plays the blocked-hit shield and its sound once.
The existing `EntityMessageType.Guard` event calls it, so attacks and skill hits
reported as `AttackResult.Block` share the same presentation. Kyrie Eleison's
casting effect remains separate. This does not implement Auto Guard's server
skill or its block-chance rules.

Preview in game with `/effect Guard` or `/effect 149`. The CSV entry selects a
C# effect, like Grand Cross; it does not import a STR or instantiate a prefab.
The client effects JSON must be regenerated with DataToClientUtility after
changing the CSV.

The original `texture/effect/guardk.tga` and `guardk2.tga` are stored under
`Assets/Textures/Resources`, alongside the other C# effect textures. No Guard
texture import step is required, following the Grand Cross approach. Their
materials are managed by `EffectSharedMaterialManager`. Sound uses the existing
`kyrie_guard.ogg` address (the imported source is `wav/effect/kyrie_guard.wav`).
No additional sound should be played by the skill or blocked-hit handler.

## Reference and animation

Reference: EddaTales.exe, SHA-256
`94e5a5d71aeec55672912a4fe08cae23f864ff3f9e0937f4339365a79802081a`.

Native effect 336 dispatches at `0x6DF101` to `0x710F90`, loading both guard
textures and the Kyrie guard sound. Primitive 61 updates at `0x681952` and
renders at `0x6AAE00`. It builds three layers of 15 textured quads around the
character's facing direction. The default variant uses green tints
(205,255,205), (155,255,155), and (225,255,225).

Alpha rises by 20 to 100, holds through frame 26, then loses 6 per frame. The
outer layer expands during that fade. The implementation ends at frame 43
when alpha reaches zero, using the client's 60-frame effect convention and
elapsed time to avoid stretching the animation at low frame rates. Native
coordinates are scaled by 1/5 with the vertical axis inverted, following the
client's C# effect convention.

In-game comparison is still required for apparent size, layering against
sprites, and final visual fidelity to the original executable.
