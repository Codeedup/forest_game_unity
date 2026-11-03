"""Original Underfoot score and stylised woodland ambience. Requires NumPy only.

Run from the repository root: python Tools/generate_underfoot_audio.py
No samples, recordings, melodies or soundfonts from third parties are used.
"""
from pathlib import Path
import wave
import numpy as np

RATE = 44100
RNG = np.random.default_rng(431)
OUT = Path(__file__).resolve().parents[1] / "Assets/Underfoot/Audio"


def hz(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def place(buffer, mono, start, gain=1, pan=0):
    # Circular accumulation includes reverb and note tails across the loop boundary.
    index = (np.arange(len(mono)) + int(start * RATE)) % len(buffer)
    angle = (pan + 1) * np.pi / 4
    buffer[index, 0] += mono * gain * np.cos(angle)
    buffer[index, 1] += mono * gain * np.sin(angle)


def note(midi, duration, voice):
    release = .65 if voice == "flute" else 1.7
    t = np.arange(int((duration + release) * RATE)) / RATE
    frequency = hz(midi)
    if voice == "flute":
        vibrato = .0021 * np.sin(2 * np.pi * 4.8 * t) * np.minimum(t / .6, 1)
        phase = 2 * np.pi * frequency * np.cumsum(1 + vibrato) / RATE
        tone = np.sin(phase) + .18 * np.sin(2 * phase + .2) + .045 * np.sin(3 * phase)
        breath = RNG.normal(0, .016, len(t))
        attack = np.minimum(t / .055, 1) ** 1.3
        envelope = attack * np.exp(-np.maximum(t - duration, 0) / .14)
        envelope *= .92 + .08 * np.sin(2 * np.pi * 1.3 * t + .4)
        return (tone + breath) * envelope
    tone = np.zeros(len(t))
    # Decaying harmonics, slight string stiffness, soft pick and resonant body.
    for harmonic in range(1, 9):
        decay = (.85 if voice == "bass" else .52) / (1 + harmonic * .29)
        tone += np.sin(2 * np.pi * frequency * harmonic * (1 + .000035 * harmonic ** 2) * t) \
            * np.exp(-t / decay) / harmonic ** 1.55
    tone *= np.minimum(t / .008, 1)
    tone *= np.exp(-np.maximum(t - duration, 0) / .18)
    return tone


def room(buffer):
    dry = buffer.copy()
    for delay, level in ((.079, .09), (.131, .07), (.211, .05), (.337, .035), (.503, .02)):
        buffer += np.roll(dry[:, ::-1], int(delay * RATE), axis=0) * level
    return buffer


def save(name, buffer, peak):
    buffer -= np.mean(buffer, axis=0)
    buffer *= peak / np.max(np.abs(buffer))
    pcm = (np.clip(buffer, -1, 1) * 32767).astype("<i2")
    OUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUT / name), "wb") as file:
        file.setnchannels(2)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(pcm.tobytes())
    rms = float(np.sqrt(np.mean(buffer ** 2)))
    seam = float(np.max(np.abs(buffer[0] - buffer[-1])))
    print(f"{name}: {len(buffer)/RATE:.2f}s stereo, peak {peak:.2f}, RMS {rms:.4f}, loop seam {seam:.5f}")


def music():
    # An original unhurried 6/8 tune in G, 16 bars, with a responding second phrase.
    eighth = 60 / 84 / 3
    bar = eighth * 6
    buffer = np.zeros((round(bar * 16 * RATE), 2))
    chords = [(43, 55, 59, 62), (48, 55, 60, 64), (40, 55, 59, 64), (38, 54, 57, 62),
              (43, 55, 59, 62), (45, 57, 60, 64), (38, 54, 57, 62), (43, 55, 59, 62)] * 2
    melody = [
        [(0, 67, 2), (2, 71, 1), (3, 74, 2)], [(0, 76, 2), (3, 72, 2)],
        [(0, 71, 3), (4, 67, 1)], [(0, 69, 2), (2, 66, 1), (3, 62, 2)],
        [(1, 67, 1), (2, 69, 1), (3, 71, 2)], [(0, 72, 2), (3, 69, 2)],
        [(0, 66, 2), (2, 69, 1), (3, 74, 2)], [(0, 71, 2), (2, 69, 1), (3, 67, 2)],
        [(0, 74, 2), (2, 71, 1), (3, 79, 2)], [(0, 76, 3), (4, 74, 1)],
        [(0, 71, 2), (2, 74, 1), (3, 76, 2)], [(0, 74, 2), (3, 69, 2)],
        [(0, 71, 2), (3, 67, 2)], [(0, 69, 2), (2, 72, 1), (3, 76, 2)],
        [(0, 74, 2), (2, 69, 1), (3, 66, 2)], [(0, 67, 4)]
    ]
    for index, chord in enumerate(chords):
        start = index * bar
        place(buffer, note(chord[0], bar * .7, "bass"), start, .19, -.08)
        for step, degree in enumerate((1, 2, 3, 1, 3, 2)):
            timing = start + step * eighth + (0 if step == 0 else RNG.uniform(-.01, .01))
            place(buffer, note(chord[degree], eighth * 1.6, "string"), timing,
                  .095 * (1.08 if step in (0, 3) else .85), -.30 if step % 2 == 0 else .24)
        for step, pitch, length in melody[index]:
            place(buffer, note(pitch, length * eighth * .85, "flute"), start + step * eighth + .035, .125, .10)
    save("Underfoot_Meadow_Waltz.wav", room(buffer), .62)


def filtered_noise(count, low, high, power):
    frequencies = np.fft.rfftfreq(count, 1 / RATE)
    spectrum = RNG.normal(size=len(frequencies)) + 1j * RNG.normal(size=len(frequencies))
    spectrum *= np.where((frequencies > low) & (frequencies < high),
                         1 / np.maximum(frequencies, low) ** power, 0)
    result = np.fft.irfft(spectrum, n=count)
    return result / np.std(result)


def forest():
    seconds = 48
    count = RATE * seconds
    t = np.arange(count) / RATE
    buffer = np.zeros((count, 2))
    for channel in range(2):
        breeze = filtered_noise(count, 130, 1900, .7)
        leaves = filtered_noise(count, 1200, 6500, .3)
        envelope = .65 + .18 * np.sin(2 * np.pi * 3 * t / seconds + channel) \
                   + .12 * np.sin(2 * np.pi * 7 * t / seconds + .4)
        buffer[:, channel] = breeze * envelope * .032 + leaves * envelope ** 3 * .008
    # Sparse, varied woodland bird phrases, with pauses and stereo separation.
    for when, pan, pitch in ((2.2, -.65, 2550), (7.8, .55, 3100), (14.1, -.25, 2250),
                             (20.6, .7, 2800), (29.2, -.6, 2450), (36.1, .4, 3200), (43.8, -.3, 2650)):
        for chirp in range(int(RNG.integers(3, 6))):
            duration = RNG.uniform(.09, .19)
            local = np.arange(int(duration * RATE)) / RATE
            contour = pitch + 600 * np.sin(np.pi * local / duration) + 160 * np.sin(2 * np.pi * 32 * local)
            phase = 2 * np.pi * np.cumsum(contour) / RATE
            call = (np.sin(phase) + .13 * np.sin(2 * phase)) * np.sin(np.pi * local / duration) ** 1.8
            place(buffer, call, when + chirp * .25, .065 * RNG.uniform(.6, 1), pan)
    # Very quiet, intermittent insect wing buzz, below the birds and leaf bed.
    for when in (11, 25, 39):
        local = np.arange(int(1.2 * RATE)) / RATE
        insect = np.sin(2 * np.pi * 3700 * local) * (.5 + .5 * np.sin(2 * np.pi * 42 * local))
        insect *= np.sin(np.pi * local / 1.2) ** 2
        place(buffer, insect, when, .009, RNG.uniform(-.7, .7))
    save("Underfoot_Forest_Air.wav", buffer, .40)


if __name__ == "__main__":
    music()
    forest()
