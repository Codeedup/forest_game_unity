# Project instructions for Codex

## Goal and working style

Build and refine the Unity project in this repository while preserving the course template and its existing work. Read this file, the repository README, any relevant nested `AGENTS.md` files, and the Unity project settings before editing. Treat the checked-out project and the current user request as the source of truth; do not assume the template matches an earlier description.

The project is a whimsical fairy-tale woodland viewed at different character scales. Preserve a warm, inviting, cute visual identity. The mood can suggest the cosy countryside of *The Hobbit*, the magical history of *Skyrim*, and the wonder of the early *Harry Potter* films. Treat these as atmosphere references; create original assets and details.

## Save tokens and keep work focused

- Start by inspecting only the files relevant to the current task. Use `rg --files`, targeted search, and short file excerpts instead of printing the entire repository or long files.
- Do not repeatedly reread files already inspected. Keep a brief note of the relevant paths, Unity version, current scene, and decisions.
- Before making a change, state the intended result in one short sentence and identify the smallest useful set of files to change.
- Reuse the template's existing input, rendering, and coding patterns. Do not add packages, asset packs, systems, or abstractions unless the task needs them.
- Keep explanations, plans, tool output, and progress updates concise. Summarise long logs and show only the relevant errors or results.
- Do not generate large reports, duplicate documentation, speculative variants, or broad refactors. Do not spend time on optional polish until the requested playable behaviour works.
- Run focused checks that address a real risk. Avoid repeating a successful Unity import, compile, or Play Mode walkthrough without a reason. If a check fails, inspect the relevant output and fix the cause before retrying.
- Never claim the project was opened, compiled, or visually inspected unless that check actually ran. Clearly note any check that the current environment cannot perform.
- Exclude generated Unity folders such as Library, Temp, and obj from broad searches. Use targeted searches and show only relevant log excerpts

## Implementation rules

- Preserve the Unity version, render pipeline, input system, folder structure, and useful course-template examples. Avoid upgrades or unrelated cleanup.
- Protect the working tree: check git status --short and the current branch before editing. Don’t reset, switch branches, or overwrite existing changes.
- Make targeted, reviewable edits. Do not overwrite unrelated user work, delete assets, or change project-wide settings without a clear need.
- Prefer simple procedural geometry and original materials when assets are not supplied. Make models from coherent shapes, colours, and silhouettes instead of leaving unrelated default primitives as the finished art.
- Keep the forest readable at both human and miniature scale. Keep key landmarks present while revealing detail; make scale changes feel continuous and grounded.
- Use real Unity physics when the experience calls for physics. Do not present scripted movement as a physics simulation.
- Keep runtime spawning bounded and repeatable. Avoid duplicates when a player repeatedly crosses a size threshold; avoid unbounded object creation.
- Preserve safe movement, collisions, camera framing, and interface usability at each intended scale.
- Keep optional audio or visual polish secondary to requested functionality and assessment requirements.

## Sub-agent policy

Use **exactly one sub-agent** for the final visual-style review when the project can be run or otherwise inspected. Do not use multiple sub-agents, and do not delegate implementation to the reviewer. The main agent owns code, assets, and corrections. Give the reviewer the prompt below and the smallest set of evidence needed to inspect the finished result.

### Visual-style reviewer prompt

> Review the completed Unity scene and playable result for visual coherence with the project's intended style: cute, whimsical, warm, and fairy-tale-like, with a cosy woodland and a gentle sense of magic. Use the agreed atmosphere references only as broad mood guidance. Check the hooded human wanderer, gnome, roots, leaf, mushrooms, miniature details, seed trail, and warmly lit home. Inspect the scene at normal and miniature scale, including the route into the root home.
>
> Your task is review only. Do not edit files or assets. Report concise, actionable findings with severity (`must fix`, `should fix`, or `polish`), the specific object or view affected, and what visual change would help. Check for mismatched art styles, default-looking or incoherent shapes, clutter, poor readability, harsh or bleak mood, details that disappear at close range, unclear wayfinding, or a home that does not feel inviting. Also name the strongest visual elements worth preserving. If you cannot run the project, say exactly what evidence you reviewed and what remains unverified. Do not claim a runtime visual review from source code alone.

The main agent must consider the review and make any necessary corrections, then perform its own focused verification. Do not let the reviewer and implementer edit the same files concurrently.

## Completion report

When the requested task is complete, report briefly:

1. What changed and where.
2. Which meaningful checks actually passed.
3. Any remaining limitation or required user action.
