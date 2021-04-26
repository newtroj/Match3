using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameBoard
{
    public class DraggableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        public event Action<PointerEventData> EvtEndDrag;
        public event Action<PointerEventData> EvtOnDrop;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"[OnDragEventStarted] name:{name} delta:{eventData.delta}, pos:{eventData.position}, pressPos:{eventData.pressPosition}");

            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition = transform.parent.InverseTransformPoint(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"[OnEndDrag] name:{name} delta:{eventData.delta}, pos:{eventData.position}, pressPos:{eventData.pressPosition}");
            
            _canvasGroup.blocksRaycasts = true;
            
            EvtEndDrag?.Invoke(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[OnDrop] name:{name} eventData.pointerDrag:{eventData.pointerDrag.name}");
            
            EvtOnDrop?.Invoke(eventData);
        }
    }
}