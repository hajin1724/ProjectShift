using UnityEngine;

namespace ProjectShift
{
    public sealed class BattleTile
    {
        public Vector2Int Coordinates { get; }
        public Vector3 Center { get; }
        public bool Walkable { get; set; } = true;

        public BattleTile(Vector2Int coordinates, Vector3 center)
        {
            Coordinates = coordinates;
            Center = center;
        }
    }

    [RequireComponent(typeof(BoxCollider), typeof(MeshRenderer))]
    public sealed class BattlefieldGrid : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float tileSize = 3.2f;
        [SerializeField] private Vector2Int[] blockedTiles = new Vector2Int[0];

        private BattleTile[,] tiles;
        private Vector3 origin;
        private LineRenderer destinationOutline;

        private void Awake()
        {
            Bounds bounds = GetComponent<BoxCollider>().bounds;
            int width = Mathf.FloorToInt(bounds.size.x / tileSize);
            int depth = Mathf.FloorToInt(bounds.size.z / tileSize);
            tiles = new BattleTile[width, depth];

            // 완전한 정사각형만 중앙에 배치하고 남는 가장자리는 이동 대상에서 제외한다.
            origin = new Vector3(bounds.center.x - width * tileSize / 2f, bounds.max.y,
                bounds.center.z - depth * tileSize / 2f);
            for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
            {
                Vector3 center = origin + new Vector3(x + 0.5f, 0f, z + 0.5f) * tileSize;
                tiles[x, z] = new BattleTile(new Vector2Int(x, z), center);
            }

            foreach (Vector2Int cell in blockedTiles)
                if (Contains(cell.x, cell.y)) tiles[cell.x, cell.y].Walkable = false;

            if (width == 0 || depth == 0) return;
            for (int x = 0; x <= width; x++)
            {
                Vector3 start = origin + Vector3.right * (x * tileSize);
                DrawBoundary(start, start + Vector3.forward * (depth * tileSize));
            }
            for (int z = 0; z <= depth; z++)
            {
                Vector3 start = origin + Vector3.forward * (z * tileSize);
                DrawBoundary(start, start + Vector3.right * (width * tileSize));
            }

            destinationOutline = CreateLine("Destination", Color.yellow, 0.10f, 4);
            destinationOutline.loop = true;
            destinationOutline.enabled = false;
        }

        private bool Contains(int x, int z)
        {
            return x >= 0 && x < tiles.GetLength(0) && z >= 0 && z < tiles.GetLength(1);
        }

        public bool TryGetWalkableTile(Vector3 point, out BattleTile tile)
        {
            int x = Mathf.FloorToInt((point.x - origin.x) / tileSize);
            int z = Mathf.FloorToInt((point.z - origin.z) / tileSize);
            tile = Contains(x, z) ? tiles[x, z] : null;
            return tile != null && tile.Walkable;
        }

        private LineRenderer CreateLine(string label, Color color, float thickness, int count)
        {
            var line = new GameObject(label).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.useWorldSpace = true;
            line.generateLightingData = true;
            line.widthMultiplier = thickness;
            line.positionCount = count;
            line.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", color);
            line.SetPropertyBlock(properties);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return line;
        }

        private void DrawBoundary(Vector3 start, Vector3 end)
        {
            LineRenderer line = CreateLine("Tile boundary", new Color(0.14f, 0.23f, 0.18f), 0.04f, 2);
            line.SetPosition(0, start + Vector3.up * 0.04f);
            line.SetPosition(1, end + Vector3.up * 0.04f);
        }

        public void ShowDestination(BattleTile tile)
        {
            float half = tileSize * 0.47f;
            Vector3 center = tile.Center + Vector3.up * 0.08f;
            destinationOutline.SetPosition(0, center + new Vector3(-half, 0, -half));
            destinationOutline.SetPosition(1, center + new Vector3(-half, 0, half));
            destinationOutline.SetPosition(2, center + new Vector3(half, 0, half));
            destinationOutline.SetPosition(3, center + new Vector3(half, 0, -half));
            destinationOutline.enabled = true;
        }
    }
}
