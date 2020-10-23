using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace R
{

    public struct PlayingAudio
    {
        public Audio_WAV audio_file;
        public uint played_samples;
    }

    public unsafe static class Audio
    {
        static SDL_AudioCallback callback = AudioCallback;

        public static SDL_AudioSpec spec;

        public static void Init()
        {

            SDL_AudioSpec spec_want = new SDL_AudioSpec();
            spec_want.channels = 2;
            spec_want.freq = 48000;
            spec_want.samples = 4096;
            spec_want.format = SDL_Audio_Format.AUDIO_U16;
            spec_want.silence = 0;
            spec_want.callback = Marshal.GetFunctionPointerForDelegate(callback);

            SDL_AudioSpec spec_have;
            var dev = SDL.OpenAudioDevice(null, 0, &spec_want, &spec_have, 0);
            SDL.PauseAudioDevice(dev, 0);

            Console.WriteLine($"Opened audio device: {dev}");
            Console.WriteLine(spec_have.Stringify());

            spec = spec_have;

        }

        static List<PlayingAudio> playing_audios = new List<PlayingAudio>();

        static void AudioCallback(void* userdata, byte* stream, int len)
        {
            byte* ptr = stream;

            for (int i = 0; i < len; i++)
            {
                *ptr = 0;
                ptr++;
            }

            for (int i = 0; i < playing_audios.Count; i++)
            {
                var pa = playing_audios[i];

                uint number_remaining_samples = pa.played_samples - pa.audio_file.length;
                int samples_to_copy = Math.Max((int)number_remaining_samples, len);

                Buffer.MemoryCopy(pa.audio_file.buffer + pa.played_samples, stream, samples_to_copy, samples_to_copy);

                pa.played_samples += (uint)samples_to_copy;

                if (pa.played_samples == pa.audio_file.length)
                {
                    playing_audios.RemoveAt(i);
                    i--;
                }
                else
                {
                    playing_audios[i] = pa;
                }
            }
        }

        public static void PlayMusic(Audio_WAV audio)
        {
            if (audio.spec.freq == spec.freq && audio.spec.format == spec.format)
            {
                playing_audios.Add(new PlayingAudio()
                {
                    audio_file = audio,
                    played_samples = 0
                });
            }
        }

    }
}
