"""Synthesizes the game's sound effects as 16-bit mono WAV files.

Every sound is built from scratch (pitch sweeps, filtered noise, bell tones),
so there are no licensing concerns. The game plays them by file name through
SoundFX.Play("<name>"), e.g. "impact" -> Assets/Resources/Sounds/impact.wav.

Usage (from the project folder):
    python Tools/make_sounds.py              # writes all sounds
    python Tools/make_sounds.py impact heal  # writes only these
    python Tools/make_sounds.py --out <folder> [names...]

To retune a sound, edit its function below (pitch, length, brightness) and
run the script again; Unity picks up the changed file automatically.
Requires numpy.
"""
import os
import sys
import wave

import numpy as np

SR = 44100
rng = np.random.default_rng(7)


def t_axis(seconds):
    return np.arange(int(SR * seconds)) / SR


def env(n, attack=0.005, release=0.05, curve=3.0):
    """Fast attack, exponential-ish decay, short fade at both ends (no clicks)."""
    x = np.linspace(0, 1, n)
    e = (1 - x) ** curve
    a = max(1, int(SR * attack))
    e[:a] *= np.linspace(0, 1, a)
    r = max(1, int(SR * release))
    e[-r:] *= np.linspace(1, 0, r)
    return e


def sweep(f0, f1, seconds, shape=np.sin):
    """Sine (or other wave) whose pitch glides from f0 to f1."""
    t = t_axis(seconds)
    f = np.geomspace(f0, f1, len(t))
    phase = 2 * np.pi * np.cumsum(f) / SR
    return shape(phase)


def lowpass(x, window):
    window = max(1, int(window))
    k = np.ones(window) / window
    return np.convolve(x, k, mode="same")


def noise(seconds):
    return rng.uniform(-1, 1, int(SR * seconds))


def bell(freq, seconds, partials=((1, 1.0), (2.0, 0.5), (3.01, 0.25), (4.2, 0.12))):
    t = t_axis(seconds)
    out = np.zeros_like(t)
    for mult, amp in partials:
        out += amp * np.sin(2 * np.pi * freq * mult * t) * np.exp(-t * (3 + mult * 2))
    return out


def place(dst, src, start_sec):
    i = int(start_sec * SR)
    end = min(len(dst), i + len(src))
    dst[i:end] += src[: end - i]
    return dst


def save(folder, name, x, peak=0.7):
    x = np.asarray(x, dtype=np.float64)
    m = np.max(np.abs(x))
    if m > 0:
        x = x / m * peak
    fade = min(len(x), int(SR * 0.004))
    x[-fade:] *= np.linspace(1, 0, fade)
    data = (x * 32767).astype(np.int16)
    with wave.open(os.path.join(folder, name + ".wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


def whoosh():
    s = 0.32
    n = noise(s)
    t = np.linspace(0, 1, len(n))
    # band-ish: difference of two lowpasses, swept by changing the window
    out = np.zeros_like(n)
    for i, w in enumerate((40, 25, 14, 9)):
        seg = slice(i * len(n) // 4, (i + 1) * len(n) // 4)
        out[seg] = (lowpass(n, w) - lowpass(n, w * 6))[seg]
    shape = np.sin(np.pi * t) ** 1.5
    return out * shape


def impact():
    s = 0.32
    thump = sweep(110, 42, s) * env(int(SR * s), curve=4)
    crack = lowpass(noise(0.06), 3) * env(int(SR * 0.06), curve=2)
    return place(thump, crack * 0.6, 0)


def block():
    s = 0.5
    t = t_axis(s)
    out = sum(a * np.sin(2 * np.pi * f * t) for f, a in ((540, 1), (1290, 0.6), (2130, 0.4), (3310, 0.2)))
    return out * np.exp(-t * 9) * env(len(t), attack=0.002, curve=0.5)


def destroy():
    s = 0.5
    crunch = lowpass(noise(s), 2) * env(int(SR * s), curve=2.5)
    fall = sweep(320, 70, s, shape=lambda p: np.sign(np.sin(p)) * 0.6) * env(int(SR * s), curve=2)
    return crunch * 0.7 + lowpass(fall, 6) * 0.6


def direct_hit():
    s = 0.8
    boom = sweep(90, 28, s) * env(int(SR * s), curve=3)
    rumble = lowpass(noise(s), 30) * env(int(SR * s), curve=2) * 2.5
    crack = lowpass(noise(0.08), 2) * env(int(SR * 0.08), curve=2)
    out = boom + rumble
    out = np.tanh(out * 2.2)  # a bit of grit
    return place(out, crack * 0.8, 0)


def vanish():
    s = 0.65
    t = t_axis(s)
    gliss = sweep(1400, 260, s) * (0.6 + 0.4 * np.sin(2 * np.pi * 18 * t))
    sparkle = lowpass(noise(s), 1) * 0.15 * np.exp(-t * 4)
    return (gliss + sparkle) * env(len(t), attack=0.01, curve=1.6)


def summon():
    s = 0.28
    thump = sweep(150, 60, s) * env(int(SR * s), curve=3.5)
    dust = lowpass(noise(s), 8) * env(int(SR * s), curve=5) * 0.5
    return thump + dust


def hop():
    s = 0.14
    return sweep(320, 640, s) * env(int(SR * s), attack=0.003, curve=1.5)


def damage():
    s = 0.28
    sq = sweep(240, 150, s, shape=lambda p: np.sign(np.sin(p)))
    return lowpass(sq, 8) * env(int(SR * s), curve=2)


def heal():
    out = np.zeros(int(SR * 0.6))
    for i, f in enumerate((523.25, 659.25, 783.99)):  # C5 E5 G5
        place(out, bell(f, 0.4) * 0.8, i * 0.08)
    return out


def turn_yours():
    out = np.zeros(int(SR * 0.75))
    place(out, bell(392.0, 0.6), 0)      # G4
    place(out, bell(523.25, 0.6), 0.14)  # C5
    return out


def turn_theirs():
    out = np.zeros(int(SR * 0.75))
    place(out, bell(329.63, 0.6) * 0.8, 0)    # E4
    place(out, bell(261.63, 0.6) * 0.8, 0.16)  # C4
    return out


def spell():
    s = 0.55
    t = t_axis(s)
    rise = sweep(380, 1500, s) + 0.4 * sweep(760, 3000, s)
    shimmer = 0.6 + 0.4 * np.sin(2 * np.pi * 22 * t)
    return rise * shimmer * env(len(t), attack=0.03, curve=1.2)


def card_set():
    s = 0.12
    return (lowpass(noise(s), 4) - lowpass(noise(s), 30)) * env(int(SR * s), attack=0.002, curve=2.5)


SOUNDS = {
    "whoosh": whoosh, "impact": impact, "block": block, "destroy": destroy,
    "direct_hit": direct_hit, "vanish": vanish, "summon": summon, "hop": hop,
    "damage": damage, "heal": heal, "turn_yours": turn_yours, "turn_theirs": turn_theirs,
    "spell": spell, "card_set": card_set,
}

if __name__ == "__main__":
    args = sys.argv[1:]
    folder = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets", "Resources", "Sounds")
    if len(args) >= 2 and args[0] == "--out":
        folder, args = args[1], args[2:]
    unknown = [a for a in args if a not in SOUNDS]
    if unknown:
        sys.exit("Unknown sound(s): " + ", ".join(unknown) + ". Known: " + ", ".join(SOUNDS))
    os.makedirs(folder, exist_ok=True)
    for name in (args or SOUNDS):
        # each sound gets its own fixed seed, so regenerating one sound on its
        # own gives exactly the same file as regenerating them all
        rng = np.random.default_rng(sum(ord(c) for c in name))
        save(folder, name, SOUNDS[name]())
        print("wrote", name)
