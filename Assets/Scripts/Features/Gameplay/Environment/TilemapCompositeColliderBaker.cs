using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Features.Gameplay.Environment
{
    [ExecuteAlways]
    [RequireComponent(typeof(Tilemap))]
    public sealed class TilemapCompositeColliderBaker : MonoBehaviour
    {
        [Header("Source")] [SerializeField] private Tilemap tilemap;
        [SerializeField] private CompositeCollider2D sourceComposite;

        [Header("Generated Collider")] [SerializeField]
        private string bakedObjectName = "Baked Clean Ground Collider";

        [SerializeField] private bool disableOriginalCollidersAfterBake = true;

        [Header("Top Surface Cleaning")]
        [Tooltip("How much lower than the tile top the final collider surface should be.")]
        [SerializeField]
        private float topInset = 0.18f;

        [Tooltip("How far below the tile top a point can be and still count as a top-surface point.")] [SerializeField]
        private float topDetectionBand = 0.45f;

        [Tooltip("Small offset used when checking which tile is below a collider point.")] [SerializeField]
        private float probeOffset = 0.02f;

        [Header("Simplification")] [SerializeField]
        private bool simplify = true;

        [SerializeField] private float minVertexDistance = 0.01f;
        [SerializeField] private float collinearTolerance = 0.0001f;

        private void Reset()
        {
            tilemap = GetComponent<Tilemap>();
            sourceComposite = GetComponent<CompositeCollider2D>();
        }

        [ContextMenu("Bake Clean Collider From Composite")]
        public void Bake()
        {
            if (tilemap == null)
                tilemap = GetComponent<Tilemap>();

            if (sourceComposite == null)
                sourceComposite = GetComponent<CompositeCollider2D>();

            if (tilemap == null || sourceComposite == null)
            {
                Debug.LogError("Missing Tilemap or CompositeCollider2D.", this);
                return;
            }

            sourceComposite.GenerateGeometry();

            PolygonCollider2D targetCollider = GetOrCreateBakedCollider();
            targetCollider.enabled = false;

            List<Vector2[]> cleanedPaths = new();

            for (int pathIndex = 0; pathIndex < sourceComposite.pathCount; pathIndex++)
            {
                int pointCount = sourceComposite.GetPathPointCount(pathIndex);
                Vector2[] sourcePoints = new Vector2[pointCount];

                sourceComposite.GetPath(pathIndex, sourcePoints);

                List<Vector2> cleanedPoints = new(pointCount);

                foreach (Vector2 point in sourcePoints)
                {
                    Vector2 cleanedPoint = CleanPoint(point);
                    cleanedPoints.Add(cleanedPoint);
                }

                if (simplify)
                    cleanedPoints = SimplifyPath(cleanedPoints);

                if (cleanedPoints.Count >= 3)
                    cleanedPaths.Add(cleanedPoints.ToArray());
            }

            targetCollider.pathCount = cleanedPaths.Count;

            for (int i = 0; i < cleanedPaths.Count; i++)
                targetCollider.SetPath(i, cleanedPaths[i]);

            targetCollider.enabled = true;

            if (disableOriginalCollidersAfterBake)
            {
                TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();

                if (tilemapCollider != null)
                    tilemapCollider.enabled = false;

                sourceComposite.enabled = false;
            }

            Debug.Log($"Baked {cleanedPaths.Count} cleaned collider paths.", this);
        }

        private Vector2 CleanPoint(Vector2 point)
        {
            if (TryGetCleanTopY(point, out float cleanTopY))
                point.y = cleanTopY;

            return point;
        }

        private bool TryGetCleanTopY(Vector2 point, out float cleanTopY)
        {
            cleanTopY = point.y;

            Vector3 probePosition = new(point.x, point.y - probeOffset, 0f);
            Vector3Int cell = tilemap.LocalToCell(probePosition);

            if (!tilemap.HasTile(cell))
            {
                float fallbackProbeDistance = Mathf.Max(topDetectionBand, probeOffset);
                Vector3 fallbackProbePosition = new(point.x, point.y - fallbackProbeDistance, 0f);

                cell = tilemap.LocalToCell(fallbackProbePosition);

                if (!tilemap.HasTile(cell))
                    return false;
            }

            Vector3Int aboveCell = new(cell.x, cell.y + 1, cell.z);

            if (tilemap.HasTile(aboveCell))
                return false;

            Vector3 cellOrigin = tilemap.CellToLocal(cell);
            float cellTopY = cellOrigin.y + tilemap.layoutGrid.cellSize.y;

            bool pointIsNearTopSurface = point.y >= cellTopY - topDetectionBand;

            if (!pointIsNearTopSurface)
                return false;

            cleanTopY = cellTopY - topInset;
            return true;
        }

        private PolygonCollider2D GetOrCreateBakedCollider()
        {
            Transform bakedTransform = transform.Find(bakedObjectName);

            if (bakedTransform == null)
            {
                GameObject bakedObject = new(bakedObjectName);
                bakedObject.transform.SetParent(transform, false);

                bakedTransform = bakedObject.transform;
            }

            bakedTransform.localPosition = Vector3.zero;
            bakedTransform.localRotation = Quaternion.identity;
            bakedTransform.localScale = Vector3.one;

            Rigidbody2D rigidbody = bakedTransform.GetComponent<Rigidbody2D>();

            if (rigidbody == null)
                rigidbody = bakedTransform.gameObject.AddComponent<Rigidbody2D>();

            rigidbody.bodyType = RigidbodyType2D.Static;

            PolygonCollider2D polygonCollider = bakedTransform.GetComponent<PolygonCollider2D>();

            if (polygonCollider == null)
                polygonCollider = bakedTransform.gameObject.AddComponent<PolygonCollider2D>();

            polygonCollider.usedByComposite = false;

            return polygonCollider;
        }

        private List<Vector2> SimplifyPath(List<Vector2> points)
        {
            if (points.Count < 3)
                return points;

            RemoveDuplicateLastPoint(points);
            points = RemoveClosePoints(points);
            points = RemoveCollinearPoints(points);

            return points;
        }

        private void RemoveDuplicateLastPoint(List<Vector2> points)
        {
            if (points.Count < 2)
                return;

            Vector2 first = points[0];
            Vector2 last = points[^1];

            if (Vector2.Distance(first, last) <= minVertexDistance)
                points.RemoveAt(points.Count - 1);
        }

        private List<Vector2> RemoveClosePoints(List<Vector2> points)
        {
            List<Vector2> result = new(points.Count);

            foreach (Vector2 point in points)
            {
                if (result.Count == 0)
                {
                    result.Add(point);
                    continue;
                }

                Vector2 previous = result[^1];

                if (Vector2.Distance(previous, point) > minVertexDistance)
                    result.Add(point);
            }

            return result;
        }

        private List<Vector2> RemoveCollinearPoints(List<Vector2> points)
        {
            if (points.Count < 3)
                return points;

            List<Vector2> result = new(points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 previous = points[(i - 1 + points.Count) % points.Count];
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % points.Count];

                Vector2 a = current - previous;
                Vector2 b = next - current;

                float cross = Mathf.Abs(a.x * b.y - a.y * b.x);
                float dot = Vector2.Dot(a.normalized, b.normalized);

                bool isCollinear = cross <= collinearTolerance && dot > 0f;

                if (!isCollinear)
                    result.Add(current);
            }

            return result;
        }
    }
}