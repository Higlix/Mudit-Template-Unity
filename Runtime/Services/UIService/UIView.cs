using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Mudit.Core.Services.UIService
{
	public enum TransitionType
	{
		None,
		Fade,
		SlideFromLeft,
		SlideFromRight,
		SlideFromTop,
		SlideFromBottom,
		Scale,
		FadeAndScale,
		SlideAndFadeLeft,
		SlideAndFadeRight,
		SlideAndFadeTop,
		SlideAndFadeBottom,
	}

	[Serializable]
	public class TransitionConfig
	{
		[Tooltip("Visual style of the transition.")]
		public TransitionType type = TransitionType.Fade;

		[Min(0f)]
		[Tooltip("Length of the transition in seconds. 0 = instant.")]
		public float duration = 0.2f;

		[Tooltip("Easing curve. X = time [0..1], Y = progress [0..1].")]
		public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[Tooltip("Pixel offset used by Slide* transitions.")]
		public float slideDistance = 500f;

		[Min(0f)]
		[Tooltip("Starting scale for Scale / FadeAndScale transitions.")]
		public float fromScale = 0.8f;

		[Tooltip("Tick with unscaled time so UI animates while the game is paused.")]
		public bool useUnscaledTime = true;
	}

	public abstract class UIView : MonoBehaviour
	{
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private bool isOverlay = false;

		[Header("Transitions")]
		[SerializeField] private TransitionConfig showTransition = new TransitionConfig();
		[SerializeField] private TransitionConfig hideTransition = new TransitionConfig();

		public bool IsOverlay => isOverlay;

		// Resting values captured before the first transition so we can restore exactly.
		private Vector2 restAnchoredPos;
		private Vector3 restScale = Vector3.one;
		private bool restCached;

		public virtual async UniTask Initialize()
		{
			if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
			if (rectTransform == null) rectTransform = transform as RectTransform;

			CacheRestState();
			await UniTask.CompletedTask;
		}

		public virtual async UniTask OnShow(object args)
		{
			gameObject.SetActive(true);

			if (canvasGroup != null)
			{
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}

			await PlayTransition(showTransition, isShowing: true);

			if (canvasGroup != null)
			{
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
		}

		public virtual async UniTask OnHide()
		{
			if (canvasGroup != null)
			{
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}

			await PlayTransition(hideTransition, isShowing: false);

			gameObject.SetActive(false);
		}

		// --- Transition core ----------------------------------------------------

		private void CacheRestState()
		{
			if (restCached || rectTransform == null) return;
			restAnchoredPos = rectTransform.anchoredPosition;
			restScale = rectTransform.localScale;
			restCached = true;
		}

		private async UniTask PlayTransition(TransitionConfig cfg, bool isShowing)
		{
			CacheRestState();

			bool fade  = UsesFade(cfg.type)  && canvasGroup   != null;
			bool slide = UsesSlide(cfg.type) && rectTransform != null;
			bool scale = UsesScale(cfg.type) && rectTransform != null;

			Vector2 slideOffset = slide ? GetSlideOffset(cfg) : Vector2.zero;

			// Start state
			if (fade)  canvasGroup.alpha           = isShowing ? 0f : 1f;
			if (slide) rectTransform.anchoredPosition = isShowing ? restAnchoredPos + slideOffset : restAnchoredPos;
			if (scale) rectTransform.localScale    = isShowing ? restScale * cfg.fromScale : restScale;

			// Instant path
			if (cfg.type == TransitionType.None || cfg.duration <= 0f)
			{
				ApplyEndState(cfg, isShowing, fade, slide, scale, slideOffset);
				return;
			}

			// Animated path
			float elapsed = 0f;
			while (elapsed < cfg.duration)
			{
				float dt = cfg.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
				elapsed += dt;

				float linear = Mathf.Clamp01(elapsed / cfg.duration);
				float t = cfg.curve.Evaluate(linear);
				// When hiding, invert so "0 → 1" curve still reads as "fully shown → hidden"
				float p = isShowing ? t : 1f - t;

				if (fade)  canvasGroup.alpha = p;
				if (slide) rectTransform.anchoredPosition = Vector2.LerpUnclamped(restAnchoredPos + slideOffset, restAnchoredPos, p);
				if (scale) rectTransform.localScale = Vector3.LerpUnclamped(restScale * cfg.fromScale, restScale, p);

				await UniTask.Yield();
			}

			ApplyEndState(cfg, isShowing, fade, slide, scale, slideOffset);
		}

		private void ApplyEndState(TransitionConfig cfg, bool isShowing, bool fade, bool slide, bool scale, Vector2 slideOffset)
		{
			if (canvasGroup == null || rectTransform == null) return;
			if (fade)  canvasGroup.alpha           = isShowing ? 1f : 0f;
			if (slide) rectTransform.anchoredPosition = isShowing ? restAnchoredPos : restAnchoredPos + slideOffset;
			if (scale) rectTransform.localScale    = isShowing ? restScale : restScale * cfg.fromScale;
		}

		// --- Classification helpers --------------------------------------------

		private static bool UsesFade(TransitionType t) =>
			t == TransitionType.Fade ||
			t == TransitionType.FadeAndScale ||
			t == TransitionType.SlideAndFadeLeft ||
			t == TransitionType.SlideAndFadeRight ||
			t == TransitionType.SlideAndFadeTop ||
			t == TransitionType.SlideAndFadeBottom;

		private static bool UsesSlide(TransitionType t) =>
			t == TransitionType.SlideFromLeft ||
			t == TransitionType.SlideFromRight ||
			t == TransitionType.SlideFromTop ||
			t == TransitionType.SlideFromBottom ||
			t == TransitionType.SlideAndFadeLeft ||
			t == TransitionType.SlideAndFadeRight ||
			t == TransitionType.SlideAndFadeTop ||
			t == TransitionType.SlideAndFadeBottom;

		private static bool UsesScale(TransitionType t) =>
			t == TransitionType.Scale ||
			t == TransitionType.FadeAndScale;

		private static Vector2 GetSlideOffset(TransitionConfig cfg)
		{
			switch (cfg.type)
			{
				case TransitionType.SlideFromLeft:
				case TransitionType.SlideAndFadeLeft:
					return new Vector2(-cfg.slideDistance, 0f);
				case TransitionType.SlideFromRight:
				case TransitionType.SlideAndFadeRight:
					return new Vector2(cfg.slideDistance, 0f);
				case TransitionType.SlideFromTop:
				case TransitionType.SlideAndFadeTop:
					return new Vector2(0f, cfg.slideDistance);
				case TransitionType.SlideFromBottom:
				case TransitionType.SlideAndFadeBottom:
					return new Vector2(0f, -cfg.slideDistance);
				default:
					return Vector2.zero;
			}
		}
	}
}
