using UnityEngine;

namespace Features.Gameplay.Environment
{
    [DisallowMultipleComponent]
    public sealed class InfiniteParallaxLayer2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpriteRenderer[] segments;

        [Header("Parallax")]
        [Tooltip("0 = locked to camera, 1 = normal world movement, >1 = faster foreground movement.")]
        [SerializeField, Min(0f)] private float horizontalScrollFactor = 0.3f;

        [SerializeField, Min(0f)] private float verticalScrollFactor = 0f;
        [SerializeField] private bool useVerticalParallax = false;

        [Header("Wrapping")]
        [SerializeField, Min(0f)] private float wrapPadding = 1f;

        private Vector3 _lastCameraPosition;
        private float _segmentWidth;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (segments == null || segments.Length == 0)
                segments = GetComponentsInChildren<SpriteRenderer>();

            if (targetCamera == null || segments == null || segments.Length == 0)
            {
                enabled = false;
                return;
            }

            _segmentWidth = segments[0].bounds.size.x;
            _lastCameraPosition = targetCamera.transform.position;

            if (_segmentWidth <= 0.01f)
                enabled = false;
        }

        private void LateUpdate()
        {
            Vector3 cameraPosition = targetCamera.transform.position;
            Vector3 cameraDelta = cameraPosition - _lastCameraPosition;

            transform.position += new Vector3(
                cameraDelta.x * (1f - horizontalScrollFactor),
                useVerticalParallax ? cameraDelta.y * (1f - verticalScrollFactor) : 0f,
                0f
            );

            _lastCameraPosition = cameraPosition;

            WrapSegments();
        }

        private void WrapSegments()
        {
            float cameraHalfWidth = GetCameraHalfWidth();
            float cameraLeft = targetCamera.transform.position.x - cameraHalfWidth;
            float cameraRight = targetCamera.transform.position.x + cameraHalfWidth;

            float loopWidth = _segmentWidth * segments.Length;

            foreach (SpriteRenderer segment in segments)
            {
                if (segment == null)
                    continue;

                Transform segmentTransform = segment.transform;
                Vector3 position = segmentTransform.position;

                while (position.x + _segmentWidth * 0.5f < cameraLeft - wrapPadding)
                    position.x += loopWidth;

                while (position.x - _segmentWidth * 0.5f > cameraRight + wrapPadding)
                    position.x -= loopWidth;

                segmentTransform.position = position;
            }
        }

        private float GetCameraHalfWidth()
        {
            if (targetCamera.orthographic)
                return targetCamera.orthographicSize * targetCamera.aspect;

            return 10f;
        }
    }
}