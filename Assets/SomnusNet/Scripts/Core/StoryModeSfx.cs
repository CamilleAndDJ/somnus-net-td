using UnityEngine;

namespace SomnusNet.Core
{
    public static class StoryModeSfx
    {
        static AudioClip _crackClip;

        public static void PlayCrack(Transform host)
        {
            if (host == null)
                return;

            EnsureAudioListener();

            var source = host.GetComponent<AudioSource>();
            if (source == null)
                source = host.gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.9f;
            source.ignoreListenerPause = true;
            source.clip = GetCrackClip();
            source.Play();
        }

        static void EnsureAudioListener()
        {
            if (Object.FindAnyObjectByType<AudioListener>() != null)
                return;

            var cam = Camera.main;
            if (cam != null)
            {
                if (cam.GetComponent<AudioListener>() == null)
                    cam.gameObject.AddComponent<AudioListener>();
                return;
            }

            var listenerGo = new GameObject("AudioListener");
            listenerGo.AddComponent<AudioListener>();
        }

        static AudioClip GetCrackClip()
        {
            if (_crackClip != null)
                return _crackClip;

            const int sampleRate = 44100;
            const float duration = 0.38f;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[sampleCount];
            var rng = new System.Random(913);

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleCount;
                var envelope = (1f - t) * (1f - t);
                var noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                var snap = Mathf.Sin(t * 920f) * 0.35f;
                data[i] = (noise * 0.75f + snap) * envelope * 0.55f;
            }

            _crackClip = AudioClip.Create("StoryModeCrack", sampleCount, 1, sampleRate, false);
            _crackClip.SetData(data, 0);
            return _crackClip;
        }
    }
}
