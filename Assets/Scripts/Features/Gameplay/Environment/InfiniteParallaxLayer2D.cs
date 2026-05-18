using System;
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

        [Header("Setup")]
        [SerializeField] private bool autoArrangeSegments = true;

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

            SortSegments();

            _segmentWidth = GetSegmentWidth(segments[0]);
            _lastCameraPosition = targetCamera.transform.position;

            if (_segmentWidth <= 0.01f)
            {
                enabled = false;
                return;
            }

            if (autoArrangeSegments)
                ArrangeSegments();

            ValidateSegmentCount();
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

        private void ArrangeSegments()
        {
            SortSegments();

            float startX = segments[0].transform.position.x;

            for (int i = 0; i < segments.Length; i++)
            {
                Transform segmentTransform = segments[i].transform;
                Vector3 position = segmentTransform.position;

                position.x = startX + _segmentWidth * i;

                segmentTransform.position = position;
            }
        }

        private void WrapSegments()
        {
            float cameraHalfWidth = GetCameraHalfWidth();
            float cameraLeft = targetCamera.transform.position.x - cameraHalfWidth;
            float cameraRight = targetCamera.transform.position.x + cameraHalfWidth;

            int safetyCounter = 0;
            const int maxIterations = 20;

            while (safetyCounter < maxIterations)
            {
                safetyCounter++;

                SortSegments();

                SpriteRenderer leftSegment = segments[0];
                SpriteRenderer rightSegment = segments[segments.Length - 1];

                float leftEdge = leftSegment.transform.position.x + _segmentWidth * 0.5f;
                float rightEdge = rightSegment.transform.position.x - _segmentWidth * 0.5f;

                if (leftEdge < cameraLeft - wrapPadding)
                {
                    Vector3 position = leftSegment.transform.position;
                    position.x = rightSegment.transform.position.x + _segmentWidth;
                    leftSegment.transform.position = position;
                    continue;
                }

                if (rightEdge > cameraRight + wrapPadding)
                {
                    Vector3 position = rightSegment.transform.position;
                    position.x = leftSegment.transform.position.x - _segmentWidth;
                    rightSegment.transform.position = position;
                    continue;
                }

                break;
            }
        }

        private void SortSegments()
        {
            Array.Sort(segments, (a, b) =>
            {
                if (a == null || b == null)
                    return 0;

                return a.transform.position.x.CompareTo(b.transform.position.x);
            });
        }

        private float GetSegmentWidth(SpriteRenderer segment)
        {
            if (segment == null)
                return 0f;

            return segment.bounds.size.x;
        }

        private float GetCameraHalfWidth()
        {
            if (targetCamera.orthographic)
                return targetCamera.orthographicSize * targetCamera.aspect;

            return 10f;
        }

        private void ValidateSegmentCount()
        {
            float cameraWidth = GetCameraHalfWidth() * 2f;
            int requiredSegments = Mathf.CeilToInt((cameraWidth + wrapPadding * 2f) / _segmentWidth) + 2;

            if (segments.Length < requiredSegments)
            {
                Debug.LogWarning(
                    $"{nameof(InfiniteParallaxLayer2D)} on {name}: " +
                    $"You probably need at least {requiredSegments} segments. Current count: {segments.Length}. " +
                    $"Camera width: {cameraWidth}, segment width: {_segmentWidth}."
                );
            }
        }
    }
}