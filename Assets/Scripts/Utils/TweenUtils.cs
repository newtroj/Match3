using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class TweenUtils
    {
        public static Tweener DoCanvasGroupColor(CanvasGroup target, float endValue, float duration, bool snapping = false)
        {
            return DOTween.To(() => target.alpha, color => target.alpha = color, endValue, duration).SetOptions(snapping).SetTarget(target).SetUpdate(true);
        }
        
        public static Tweener DoImageColor(Image target, Color endValue, float duration, bool snapping = false)
        {
            return DOTween.To(() => target.color, color => target.color = color, endValue, duration).SetOptions(snapping).SetTarget(target).SetUpdate(true);
        }
    }
}