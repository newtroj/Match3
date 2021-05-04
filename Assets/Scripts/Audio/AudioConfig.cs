using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio Config", order = 1)]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioClip _clear;
        [SerializeField] private AudioClip _select;
        [SerializeField] private AudioClip _swap;

        public AudioClip Clear => _clear;
        public AudioClip Select => _select;
        public AudioClip Swap => _swap;
    }
}