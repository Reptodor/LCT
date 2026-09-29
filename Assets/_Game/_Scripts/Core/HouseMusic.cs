using UnityEngine;

public class HouseMusic : MonoBehaviour
{
    public const string ResourceFolder = "Music";

    static HouseMusic _instance;
    static int _external;

    AudioSource _source;
    AudioClip[] _clips = new AudioClip[0];
    int _index;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (FindFirstObjectByType<HouseMusic>() != null)
        {
            return;
        }

        var go = new GameObject("HouseMusic");
        DontDestroyOnLoad(go);
        go.AddComponent<HouseMusic>();
    }

    public static void PushExternal()
    {
        _external++;
        if (_instance != null)
        {
            _instance.Apply();
        }
    }

    public static void PopExternal()
    {
        _external = Mathf.Max(0, _external - 1);
        if (_instance != null)
        {
            _instance.Apply();
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.volume = 0.4f;
        _clips = Resources.LoadAll<AudioClip>(ResourceFolder) ?? new AudioClip[0];
        GameAudio.Changed += Apply;
        GameAudio.Reload();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            GameAudio.Changed -= Apply;
        }
    }

    void Update()
    {
        if (_clips.Length <= 1 || _source == null || _external > 0 || !GameAudio.MusicOn)
        {
            return;
        }

        if (_source.isPlaying)
        {
            return;
        }

        _index = (_index + 1) % _clips.Length;
        PlayCurrent();
    }

    void Apply()
    {
        if (_source == null)
        {
            return;
        }

        bool play = GameAudio.MusicOn && _external == 0 && _clips.Length > 0;
        _source.mute = !GameAudio.MusicOn;
        if (!play)
        {
            if (_source.isPlaying)
            {
                _source.Pause();
            }

            return;
        }

        if (_source.clip == null)
        {
            PlayCurrent();
            return;
        }

        if (!_source.isPlaying)
        {
            _source.UnPause();
            if (!_source.isPlaying)
            {
                PlayCurrent();
            }
        }
    }

    void PlayCurrent()
    {
        if (_source == null || _clips.Length == 0)
        {
            return;
        }

        if (_index < 0 || _index >= _clips.Length)
        {
            _index = 0;
        }

        _source.clip = _clips[_index];
        _source.loop = _clips.Length == 1;
        _source.mute = !GameAudio.MusicOn;
        _source.Play();
    }
}
