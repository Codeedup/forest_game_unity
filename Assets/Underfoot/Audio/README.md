# Original Underfoot audio

- `Underfoot_Meadow_Waltz.wav`: 22.86-second lilting 6/8 tune, with a breathy flute, soft plucked strings and a restrained bass accompaniment.
- `Underfoot_Forest_Air.wav`: 48-second stereo woodland bed of filtered wind and leaf rustle, sparse original bird calls and faint insects.

Both are original procedural compositions / sound designs made for this project, not field recordings or samples from an existing soundtrack. There are no third-party audio attribution requirements. The broad mood is a gentle, cosy countryside; no melody from the atmosphere references is used.

The deterministic source is `Tools/generate_underfoot_audio.py` at the repository root. Run it with Python and NumPy to regenerate the supplied 44.1 kHz, 16-bit stereo WAV files. Notes, tails and ambience are arranged across a circular buffer for continuous loops. Unity imports the clips as Vorbis and the soundscape component handles comfortable playback levels and fades.

## Blocked spring feedback

- `Underfoot_Shrink.wav`: 0.60-second soft descending glass/wood tone with filtered leaf rustle.
- `Underfoot_Grow.wav`: 0.68-second warmer ascending pluck and gentle upper note.
- `Underfoot_Spring.wav`: 6-second quiet stereo stream with synthesized droplets.

These are original procedural designs, with no imported samples or voice recordings. Run `python Tools/generate_underfoot_feedback.py` (NumPy) to regenerate only these three clips, leaving the original music and forest ambience untouched. Playback level, mute and reset are handled by `UnderfootSoundscape.cs`. No speech output or microphone recognition is included; Pip always uses captions and visible choices.
