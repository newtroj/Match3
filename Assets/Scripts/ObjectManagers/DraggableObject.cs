using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ObjectManagers
{
    public class DraggableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private RectTransform _targetDraggableRectTransform;
        
        private CanvasGroup _canvasGroup;

        public event Action<PointerEventData> EvtOnInteractableDroppedOnMe; //InteractableObject dropped just on me
        public static event Action<PointerEventData> EvtOnAnyInteractableDropped; //for interactableObjects dropped on ANY interactableObject
        public static event Action EvtOnAnyDragStarted;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"[OnDragEventStarted] name:{transform.parent.name} delta:{eventData.delta}, pos:{eventData.position}, pressPos:{eventData.pressPosition}");

            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _targetDraggableRectTransform.anchoredPosition = transform.parent.InverseTransformPoint(eventData.position);
            EvtOnAnyDragStarted?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"[OnEndDrag] name:{transform.parent.name} delta:{eventData.delta}, pos:{eventData.position}, pressPos:{eventData.pressPosition}");
            
            _targetDraggableRectTransform.anchoredPosition = Vector2.zero;
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[OnDrop] name:{transform.parent.name} eventData.pointerDrag:{eventData.pointerDrag.name}");
            
            EvtOnInteractableDroppedOnMe?.Invoke(eventData);
            EvtOnAnyInteractableDropped?.Invoke(eventData);
        }
    }
}