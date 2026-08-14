using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Astronaut : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private string[] _messages;
    [SerializeField] private AudioSource _audioSource;
    private int _currentMessageIndex = 0;

    private void Awake()
    {
        ARTrackedImage trackedImage = FindAnyObjectByType<ARTrackedImage>();
        trackedImage.enabled = false;
        transform.parent = null;
        Next();
    }

    public void PlayAudio()
    {
        if(_audioSource != null)
        {
            _audioSource.Play();
        }
    }

    public void Next()
    {
        if(_currentMessageIndex < _messages.Length)
        {
            _text.text = _messages[_currentMessageIndex];
            _currentMessageIndex++;
        }
    }
}
