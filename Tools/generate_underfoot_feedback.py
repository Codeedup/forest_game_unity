"""Original, deterministic Underfoot interaction cues. Requires NumPy; no samples.

Run separately from the existing score generator so its two loops stay untouched.
"""
from pathlib import Path
import wave
import numpy as np

RATE = 44100
OUT = Path(__file__).resolve().parents[1] / "Assets/Underfoot/Audio"
RNG = np.random.default_rng(182)


def noise(count, low, high):
    spectrum = np.fft.rfft(RNG.normal(0, 1, count))
    frequency = np.fft.rfftfreq(count, 1 / RATE)
    spectrum *= (frequency > low) * (frequency < high)
    signal = np.fft.irfft(spectrum, n=count)
    return signal / max(np.std(signal), .00001)


def save(name, channels, peak):
    channels = np.asarray(channels)
    channels *= peak / max(np.max(np.abs(channels)), .00001)
    with wave.open(str(OUT / name), "wb") as file:
        file.setnchannels(2)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes((np.clip(channels, -1, 1) * 32767).astype("<i2").tobytes())


def size_cue(growing):
    seconds = .68 if growing else .60
    t = np.arange(int(seconds * RATE)) / RATE
    envelope = np.sin(np.pi * t / seconds) ** 1.5
    frequency = np.linspace(330, 495, len(t)) if growing else np.linspace(660, 330, len(t))
    phase = 2 * np.pi * np.cumsum(frequency) / RATE
    pluck = (np.sin(phase) + .23 * np.sin(2.01 * phase)) * np.exp(-t * 5)
    pluck *= np.minimum(t / .018, 1)
    rustle = noise(len(t), 800, 5500) * envelope * .055
    tone = pluck * .33 + rustle
    if growing:
        tone += np.sin(2 * np.pi * 660 * t) * np.exp(-np.maximum(t - .16, 0) * 9) * (t > .16) * .075 * envelope
    tone *= np.minimum((seconds - t) / .09, 1)
    save("Underfoot_Grow.wav" if growing else "Underfoot_Shrink.wav", np.column_stack([tone, tone]), .55)


def water():
    seconds = 6
    t = np.arange(int(seconds * RATE)) / RATE
    channels = []
    for side in range(2):
        stream = noise(len(t), 350, 5800) * (.75 + .17 * np.sin(2 * np.pi * 5 * t / seconds + side)) * .08
        # Quiet circular droplets over filtered noise; no recordings or external assets.
        for when in (.3, 1.1, 1.8, 2.7, 3.9, 4.5, 5.4):
            local = np.mod(t - when - side * .043, seconds)
            phase = 2 * np.pi * (1150 * local + 190 * (1 - np.exp(-local * 24)))
            stream += np.sin(phase) * np.exp(-local * 45) * .045
        # Crossfade the periodic boundary to avoid clicks when the stream loops.
        length = int(.08 * RATE)
        blend = np.linspace(0, 1, length)
        stream[:length] = stream[-length:] * (1 - blend) + stream[:length] * blend
        channels.append(stream)
    save("Underfoot_Spring.wav", np.column_stack(channels), .48)


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    size_cue(False)
    size_cue(True)
    water()
