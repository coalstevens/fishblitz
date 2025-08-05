using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class BirdOutliner : MonoBehaviour
{
    [SerializeField] private Transform _frameMask;
    [SerializeField] private SpriteRenderer _birdRenderer;
    [SerializeField] private float _removeOutlineAfterTaggedDelaySecs = 3f;
    [SerializeField] private BirdMaterialController _materialController = new();
    private BirdBrain.BirdInstanceData _bird;
    private Action _unsubscribe;

    private void OnEnable()
    {
        _bird = GetComponent<BirdBrain.BirdInstanceData>();

        Assert.IsNotNull(_frameMask, "FrameMask is not assigned in the Inspector.");
        Assert.IsNotNull(_birdRenderer, "Bird SpriteRenderer is not assigned in the Inspector");
        Assert.IsNotNull(_bird, "Bird component is not attached to the GameObject.");

        _materialController.SetRenderer(_birdRenderer);
        _materialController.SetRunner(this);
        _materialController.SetOutlineMask();

        _unsubscribe = _bird.IsTagged.OnChange(curr => OnBirdTagged(curr));
    }

    private void OnDisable()
    {
        _unsubscribe?.Invoke();
        _unsubscribe = null;
    }

    private void OnBirdTagged(bool curr)
    {
        if (curr)
        {
            _materialController.WipeToHighlight();
            Invoke(nameof(_materialController.WipeToMask), _removeOutlineAfterTaggedDelaySecs);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<BirdBrain>(out var bird))
        {
            Vector3 birdLocalPos = _frameMask.InverseTransformPoint(bird.transform.position);
            if (birdLocalPos.x > _frameMask.localPosition.x)
            {
                _materialController.SetOutlineBase();
            }
            else
            {
                _materialController.SetOutlineMask();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<BirdBrain>(out var bird))
        {
            _materialController.SetOutlineMask();
        }
    }

    [Serializable]
    private class BirdMaterialController
    {
        private enum OutlineState { Base, Highlight, Mask }
        [SerializeField] private Shader _outlineShader;
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.black;
        [SerializeField, UnityEngine.Range(0f, 6.2831853f)] private float _startAngle = 0f;
        [SerializeField] private float _wipeDuration = 1f;
        private OutlineState _currentState = OutlineState.Base;
        private Material _mat;
        private SpriteRenderer _renderer;
        private MonoBehaviour _runner;
        private Coroutine _wipeCoroutine;

        public void SetRenderer(SpriteRenderer renderer)
        {
            Assert.IsNotNull(_outlineShader, "Outline shader is not assigned in the Inspector.");
            _renderer = renderer;
            _mat = _renderer.material;

            Assert.IsTrue(_mat.shader == _outlineShader, "Material shader does not match the outline shader.");

            Assert.IsTrue(_mat.HasProperty("_Color1"), "_Color1 property is missing from the material.");
            Assert.IsTrue(_mat.HasProperty("_Color2"), "_Color2 property is missing from the material.");
            Assert.IsTrue(_mat.HasProperty("_Color1IsMask"), "_Color1IsMask property is missing from the material.");
            Assert.IsTrue(_mat.HasProperty("_Color2IsMask"), "_Color2IsMask property is missing from the material.");
            Assert.IsTrue(_mat.HasProperty("_AngleRange"), "_AngleRange property is missing from the material.");
        }

        public void SetRunner(MonoBehaviour mono)
        {
            _runner = mono;
        }

        public void SetOutlineBase()
        {
            SetOutlineState(OutlineState.Base);
        }

        public void SetOutlineHighlight()
        {
            SetOutlineState(OutlineState.Highlight);
        }

        public void SetOutlineMask()
        {
            SetOutlineState(OutlineState.Mask);
        }

        public void WipeToBase()
        {
            WipeToState(OutlineState.Base);
        }

        public void WipeToHighlight()
        {
            WipeToState(OutlineState.Highlight);
        }

        public void WipeToMask()
        {
            WipeToState(OutlineState.Mask);
        }

        private void SetOutlineState(OutlineState state)
        {
            Assert.IsNotNull(_renderer, "Renderer is not set. Please set a SpriteRenderer.");
            _currentState = state;
            SetOutlineToColor1(state);
        }

        private void WipeToState(OutlineState targetState)
        {
            Assert.IsNotNull(_renderer, "Renderer is not set. Please set a SpriteRenderer.");
            Assert.IsNotNull(_runner, "Runner is not set. Please set a MonoBehaviour to run the coroutine.");
            if (_wipeCoroutine != null)
            {
                _runner.StopCoroutine(_wipeCoroutine);
                _wipeCoroutine = null;
            }
            _wipeCoroutine = _runner.StartCoroutine(WipeCoroutine(targetState));
        }

        private IEnumerator WipeCoroutine(OutlineState targetState)
        {
            // Set Wipe Target Color
            switch (targetState)
            {
                case OutlineState.Base:
                    _mat.SetColor("_Color2", _baseColor);
                    _mat.SetInt("_Color2IsMask", 0);
                    break;
                case OutlineState.Highlight:
                    _mat.SetColor("_Color2", _highlightColor);
                    _mat.SetInt("_Color2IsMask", 0);
                    break;
                case OutlineState.Mask:
                    _mat.SetInt("_Color2IsMask", 1);
                    break;
            }

            // Perform Wipe
            float elapsed = 0f;
            float endAngle = _startAngle;

            while (elapsed < _wipeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _wipeDuration);
                endAngle = _startAngle + Mathf.Lerp(0f, Mathf.PI * 2f, t);

                _mat.SetVector("_AngleRange", new Vector4(_startAngle, endAngle, 0f, 0f));

                yield return null;
            }

            _currentState = targetState;
            SetOutlineToColor1(_currentState);
        }

        private void SetOutlineToColor1(OutlineState state)
        {
            if (_wipeCoroutine != null)
            {
                _runner.StopCoroutine(_wipeCoroutine);
                _wipeCoroutine = null;
            }

            switch (state)
            {
                case OutlineState.Base:
                    _mat.SetColor("_Color1", _baseColor);
                    _mat.SetInt("_Color1IsMask", 0);
                    break;
                case OutlineState.Highlight:
                    _mat.SetColor("_Color1", _highlightColor);
                    _mat.SetInt("_Color1IsMask", 0);
                    break;
                case OutlineState.Mask:
                    _mat.SetInt("_Color1IsMask", 1);
                    break;
            }

            _mat.SetVector("_AngleRange", new Vector4(0, Mathf.PI * 2f, 0f, 0f));
        }
    }
}