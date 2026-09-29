using UnityEngine;


namespace LCT.MiniGames.Shop
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Озвучка уровней")]
        public AudioClip[] levelVoiceClips;

        [Header("Звуки")]
        public AudioClip coinDropSound;
        public AudioClip levelCompleteSound;
        public AudioClip gameOverSound;
        public AudioClip wrongSound;
        public AudioClip banknoteUseSound;

        [Header("Музыка")]
        public AudioClip backgroundMusic;
        public float musicVolume = 0.5f;

        private AudioSource audioSource;
        private AudioSource musicSource;
        bool _pushedExternalMusic;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;
        }

        void OnEnable()
        {
            GameAudio.Changed += ApplyMix;
            if (Instance == this && backgroundMusic != null && !_pushedExternalMusic)
            {
                _pushedExternalMusic = true;
                HouseMusic.PushExternal();
            }

            ApplyMix();
        }

        void OnDisable()
        {
            GameAudio.Changed -= ApplyMix;
        }

        void Start()
        {
            if (Instance != this)
            {
                return;
            }

            PlayBackgroundMusic();
        }

        void OnDestroy()
        {
            if (_pushedExternalMusic)
            {
                HouseMusic.PopExternal();
            }
        }

        public void PlayBackgroundMusic()
        {
            if (!GameAudio.MusicOn || backgroundMusic == null || musicSource == null)
            {
                return;
            }

            musicSource.mute = false;
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        void ApplyMix()
        {
            if (audioSource != null)
            {
                audioSource.mute = !GameAudio.SoundOn;
            }

            if (musicSource == null)
            {
                return;
            }

            musicSource.mute = !GameAudio.MusicOn;
            if (!GameAudio.MusicOn)
            {
                musicSource.Pause();
                return;
            }

            if (backgroundMusic == null)
            {
                return;
            }

            if (musicSource.clip != backgroundMusic)
            {
                PlayBackgroundMusic();
            }
            else if (!musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }

        public void StopBackgroundMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        public void ToggleMusic()
        {
            if (musicSource == null) return;

            if (musicSource.isPlaying)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }

        public void PlayCoinDropSound()
        {
            if (coinDropSound != null && audioSource != null)
                audioSource.PlayOneShot(coinDropSound);
        }

        public void PlayLevelCompleteSound()
        {
            if (levelCompleteSound != null && audioSource != null)
                audioSource.PlayOneShot(levelCompleteSound);
        }

        public void PlayGameOverSound()
        {
            if (gameOverSound != null && audioSource != null)
                audioSource.PlayOneShot(gameOverSound);
        }

        public void PlayWrongSound()
        {
            if (wrongSound != null && audioSource != null)
                audioSource.PlayOneShot(wrongSound);
        }

        public void PlayBanknoteUseSound()
        {
            if (banknoteUseSound != null && audioSource != null)
                audioSource.PlayOneShot(banknoteUseSound);
        }

        // ===== ОЗВУЧКА УРОВНЕЙ =====

        public void PlayLevelVoice(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
                Debug.Log($"Озвучка: {clip.name}");
            }
        }

        public void PlayLevelVoiceByIndex(int levelIndex)
        {
            if (levelVoiceClips == null || levelVoiceClips.Length == 0)
            {
                Debug.LogWarning("levelVoiceClips не назначен!");
                return;
            }

            if (levelIndex >= 0 && levelIndex < levelVoiceClips.Length)
            {
                AudioClip clip = levelVoiceClips[levelIndex];
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                    Debug.Log($"Озвучка уровня {levelIndex + 1}: {clip.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Индекс {levelIndex} вне массива (0-{levelVoiceClips.Length - 1})!");
            }
        }
    }
}
