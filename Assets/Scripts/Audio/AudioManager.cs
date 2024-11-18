using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SuperPupSystems.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public AudioMixerGroup musicMixer, sfxMixer;
        public AudioSet currentAudioSet;
        private AudioInfo currentMusicPlaying;

        private List<AudioInfo> currentSoundList = new List<AudioInfo>();

        private List<AudioInfo> currentMusicList = new List<AudioInfo>();
        void Start()
        {
            CreateAudioSources();
            LoadAudioSet(currentAudioSet, true);
        }

        public void Play(string _name)
        {
            foreach (AudioInfo sound in currentSoundList)
            {
                if (sound.name == _name)
                {
                    sound.source.Play();
                    return;
                }
            }

            Debug.LogWarning("Sound: " + _name + " not found!");
        }

        public void TransitionSound(AudioInfo _current, AudioInfo _next)
        {
            if (_current == null || _next == null)
            {
                return;
            }

            StartCoroutine(FadeOut(_current));
            StartCoroutine(FadeIn(_next));
        }

        public void PlayMusic()
        {

            if (!(currentMusicList.Count > 0))
            {
                return;
            }

            Play(currentMusicList[0].name);
        }

        public void LoadAudioSet(AudioSet _audioSet)
        {
            LoadAudioSet(_audioSet, false);
        }
        public void LoadAudioSet(AudioSet _audioSet, bool fadeMusic)
        {
            bool isFirstSet = (_audioSet == currentAudioSet);
            Debug.Log("Is first set: " + isFirstSet);
            AudioInfo previousMusic = currentMusicPlaying;

            if (isFirstSet == false)
            {
                if (_audioSet == null)
                {
                    Debug.LogError("No Audio Set Loaded!");
                    return;
                }

                currentAudioSet = _audioSet;

                ClearMusicList();

                //add exception for old music
                ClearAudioSources(previousMusic);
            }

            if (_audioSet.music.Count > 0)
            {
                foreach (AudioInfo sound in _audioSet.music)
                {
                    currentMusicList.Add(sound);
                }

                Shuffle(currentMusicList);
            }

            PopulateSoundList();
            CreateAudioSources();

            if (fadeMusic)
            {
                if (isFirstSet)
                {
                    Debug.Log("Fade in first music");
                    FadeIn(currentMusicList[0]);
                }
                else
                {
                    TransitionSound(previousMusic, currentMusicList[0]);
                }
            }
            else
            {
                PlayMusic();
            }
        }

        private void ClearAudioSources(AudioInfo _exception)
        {
            AudioSource[] sources = GetComponents<AudioSource>();

            foreach (AudioSource source in sources)
            {
                if (_exception != null)
                {
                    if (_exception.source != source)
                    {
                        Destroy(source);
                    }
                    else
                    {
                        Destroy(_exception.source, 10f);
                        //time destroy source
                    }
                }
                else
                {
                    Destroy(source);
                }

            }
        }

        private void PopulateSoundList()
        {
            if (currentAudioSet != null)
            {
                currentSoundList.Clear();

                foreach (AudioInfo sfx in currentAudioSet.sFX)
                {
                    currentSoundList.Add(sfx);
                }
                foreach (AudioInfo music in currentAudioSet.music)
                {
                    currentSoundList.Add(music);
                }
            }
            else
            {
                Debug.LogError("No AudioSet Loaded!");
                return;
            }
        }

        private void CreateAudioSources()
        {
            //replace with object pooler

            foreach (AudioInfo info in currentSoundList)
            {
                info.source = gameObject.AddComponent<AudioSource>();
                info.source.clip = info.clip;
                info.source.volume = info.volume;
                info.source.pitch = info.pitch;
                info.source.loop = info.loop;
                info.source.playOnAwake = false;


                if (musicMixer && sfxMixer != null)
                {
                    if (info.channel == AudioInfo.AudioChannel.Music)
                    {
                        info.source.outputAudioMixerGroup = musicMixer;
                    }
                    else
                    {
                        info.source.outputAudioMixerGroup = sfxMixer;
                    }
                }
            }
        }

        IEnumerator FadeIn(AudioInfo _audioInfo)
        {
            if (_audioInfo == null && _audioInfo.source == null)
            {
                yield return null;
            }

            float speed = .01f;
            _audioInfo.source.volume = 0.0f;
            _audioInfo.source.Play();

            for (float f = 0.0f; f < _audioInfo.volume; f += speed)
            {
                _audioInfo.source.volume = f;
                yield return null;
            }

            _audioInfo.source.volume = _audioInfo.volume;
        }

        IEnumerator FadeOut(AudioInfo _audioInfo)
        {
            if (_audioInfo == null && _audioInfo.source == null)
            {
                yield return null;
            }

            float speed = .01f;

            for (float f = _audioInfo.source.volume; f > 0.0f; f -= speed)
            {
                _audioInfo.source.volume = f;
                yield return null;
            }

            _audioInfo.source.volume = 0.0f;
        }

        private void ClearMusicList()
        {
            if (currentMusicList != null)
            {
                currentMusicList.Clear();
            }
        }

        private void Shuffle(List<AudioInfo> _audioList)
        {
            System.Random rng = new System.Random();
            int n = _audioList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                AudioInfo value = _audioList[k];
                _audioList[k] = _audioList[n];
                _audioList[n] = value;
            }
        }
    }
}