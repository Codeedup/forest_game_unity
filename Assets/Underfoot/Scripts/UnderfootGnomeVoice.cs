using System.Collections.Generic;
using UnityEngine;

namespace Underfoot
{
    // Pip's "voice": a short, original babble synthesised from each caption, so the
    // dialogue has a sound without recorded speech. Captions always carry the words.
    // Deterministic: the same line always produces the same melody.
    public static class UnderfootGnomeVoice
    {
        static readonly float[] Scale = { 1f, 9f / 8f, 5f / 4f, 3f / 2f, 5f / 3f, 2f };
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

        public static AudioClip For(string caption, float basePitch = 620f, float maxSeconds = 1.8f)
        {
            if (string.IsNullOrEmpty(caption)) return null;
            if (cache.TryGetValue(caption, out var cached) && cached) return cached;

            int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            string line = caption.StartsWith("Pip:") ? caption.Substring(4) : caption;
            var rng = new System.Random(StableHash(line));
            var samples = new List<float>(rate * 2);
            float phase = 0, limit = maxSeconds * rate;
            int letters = 0;

            for (int i = 0; i < line.Length && samples.Count < limit; i++)
            {
                char c = line[i];
                if (char.IsLetter(c))
                {
                    // Roughly one syllable for every three letters.
                    if (++letters % 3 != 0) continue;
                    bool question = NextIs(line, i, '?'), exclaim = NextIs(line, i, '!');
                    float pitch = basePitch * Scale[rng.Next(Scale.Length)] * (exclaim ? 1.12f : 1f);
                    float glide = question ? 1.28f : .93f + (float)rng.NextDouble() * .04f;
                    Syllable(samples, rate, pitch, glide, .055f + (float)rng.NextDouble() * .03f, ref phase);
                    Silence(samples, rate, .018f + (float)rng.NextDouble() * .016f);
                }
                else if (c == '.' || c == '?' || c == '!' || c == '\n') Silence(samples, rate, .11f);
                else if (c == ',' || c == ';') Silence(samples, rate, .06f);
            }
            if (samples.Count == 0) return null;
            if (samples.Count > limit) samples.RemoveRange((int)limit, samples.Count - (int)limit);
            FadeTail(samples, rate);

            var clip = AudioClip.Create("Pip babble", samples.Count, 1, rate, false);
            clip.SetData(samples.ToArray(), 0);
            cache[caption] = clip;
            return clip;
        }

        // A soft, rounded "bip": a few harmonics with a gentle vibrato and a falling (or, for questions, rising) glide.
        static void Syllable(List<float> o, int rate, float pitch, float glide, float seconds, ref float phase)
        {
            int n = (int)(seconds * rate);
            for (int s = 0; s < n; s++)
            {
                float t = s / (float)n;
                float freq = pitch * Mathf.Lerp(1f, glide, t) * (1f + .012f * Mathf.Sin(2 * Mathf.PI * 7f * s / rate));
                phase += 2 * Mathf.PI * freq / rate;
                if (phase > 2 * Mathf.PI) phase -= 2 * Mathf.PI;
                float env = Mathf.Min(1f, s / (.008f * rate)) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(.5f + .5f * t));
                float tone = Mathf.Sin(phase) + .32f * Mathf.Sin(2 * phase) + .1f * Mathf.Sin(3 * phase);
                o.Add(.36f * env * tone);
            }
        }

        static void Silence(List<float> o, int rate, float seconds)
        {
            for (int s = (int)(seconds * rate); s > 0; s--) o.Add(0);
        }

        static void FadeTail(List<float> o, int rate)
        {
            int n = Mathf.Min(o.Count, rate / 50);
            for (int s = 0; s < n; s++) o[o.Count - 1 - s] *= s / (float)n;
        }

        static bool NextIs(string line, int from, char mark)
        {
            // True when the rest of the current word ends in the given punctuation mark.
            int i = from + 1;
            while (i < line.Length && char.IsLetter(line[i])) i++;
            return i < line.Length && line[i] == mark;
        }

        static int StableHash(string text)
        {
            unchecked
            {
                int h = (int)2166136261;
                foreach (char c in text) h = (h ^ c) * 16777619;
                return h;
            }
        }
    }
}
