# Imported model slots

Drop a model in this folder, named exactly after a slot, then run **Underfoot > Build or rebuild demo scene**.
With a slot empty, the procedural version is built instead, so you can add or remove models one at a time.

| Slot (file name) | Replaces | Fitted to | Optional named pivots the code animates |
|---|---|---|---|
| `Wanderer` | hooded main character | 1 unit tall, then scaled 1.7 m in game | `LeftFoot`, `RightFoot`, `LeftArm`, `RightArm` |
| `Gnome` | Pip the gnome (the wizard) | 1.25 units tall, then scaled small | `RightArm` (the pointing arm) |
| `Squirrel` | Hazel | 0.38 units tall | `Tail` (it sways) |
| `Mushroom` | the 5 mushrooms (shared) | each mushroom's own height | none |
| `Tree` | the 14 ring trees (trunk, canopy, undergrowth) | trunk height + 1.3 | none |
| `Grass` | the 33 fern/grass clusters | 0.3 units tall | none |

The old guardian tree and root home stay procedural: the entrance, roots and colliders are built around them.

## Rules for a model
- File: `.fbx`, `.glb` (if a glTF importer is installed) or a `.prefab`. One file per slot, name matches exactly.
- Feet or base at the origin, facing **+Z**, upright **+Y**, any scale (the builder resizes it to the height above).
- Built-in pipeline: use Standard/Unlit materials. URP/HDRP materials render pink.
- Keep it light: a few thousand triangles, textures 2K or smaller, no unused animation clips.
- Imported colliders are removed; the builder adds its own. Cameras and lights inside a model are not removed, so delete them in the source.
- Missing pivots are fine: the character just stands still instead of swinging its arms and legs. Don't leave an Animator
  on a model that has named pivots, because the walk code and the Animator would fight over the same bones.
- Credit every model's author and licence in the written document.
