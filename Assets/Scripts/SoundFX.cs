using System.Collections.Generic;
using UnityEngine;

// Plays short sound effects by name: SoundFX.Play("impact").
// Clips are loaded from Assets/Resources/Sounds/<name> (wav/ogg/mp3); a name
// with no file is simply silent, so sounds can be added or swapped by just
// dropping a file with that name into the folder.
public class SoundFX : MonoBehaviour
{
    const int Voices = 10;            // how many sounds can overlap
    const float MasterVolume = 0.8f;

    static SoundFX instance;
    static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    AudioSource[] sources;
    int next;

    public static void Play(string name, float volume = 1f, float pitchVariation = 0.06f)
    {
        AudioClip clip = Load(name);
        if (clip == null) return;
        if (instance == null) Create();

        AudioSource source = instance.sources[instance.next];
        instance.next = (instance.next + 1) % Voices;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation); // a little variety on repeats
        source.volume = Mathf.Clamp01(volume * MasterVolume);
        source.clip = clip;
        source.Play();
    }

    static AudioClip Load(string name)
    {
        AudioClip clip;
        if (!clips.TryGetValue(name, out clip))
        {
            clip = Resources.Load<AudioClip>("Sounds/" + name);
            clips[name] = clip; // remember misses too
        }
        return clip;
    }

    // One player for the whole game, kept across scene loads.
    static void Create()
    {
        GameObject go = new GameObject("SoundFX");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SoundFX>();
        instance.sources = new AudioSource[Voices];
        for (int i = 0; i < Voices; i++)
        {
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f; // 2D: same loudness wherever the camera is
            instance.sources[i] = s;
        }
    }
}
