using GameBoard;
using ObjectManagers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _sfxAudioSource;
        [SerializeField] private AudioConfig _config;

        private void Awake()
        {
            GameBoardManager.EvtBoardShuffled += PlayClearClip;
            DraggableObject.EvtOnAnyDragStarted += PlaySelectClip;
            InteractableObject.EvtAnySwapSuccess += PlaySwapClip;
        }
        private void OnDestroy()
        {
            GameBoardManager.EvtBoardShuffled -= PlayClearClip;
            DraggableObject.EvtOnAnyDragStarted -= PlaySelectClip;
            InteractableObject.EvtAnySwapSuccess -= PlaySwapClip;
        }
        
        private void PlayClearClip()
        {
            PlayClip(_config.Clear);
        }

        private void PlaySelectClip()
        {
            PlayClip(_config.Select);
        }

        private void PlaySwapClip()
        {
            PlayClip(_config.Swap);
        }

        private void PlayClip(AudioClip target)
        {
            _sfxAudioSource.clip = target;
            _sfxAudioSource.Play();
        }
    }
}