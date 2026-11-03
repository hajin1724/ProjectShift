using UnityEngine;

namespace ProjectShift
{
    public sealed class Squad : MonoBehaviour
    {
        [SerializeField, Range(1, 15)] private int soldierCount = 9;
        [SerializeField, Range(1, 15)] private int columns = 3;
        [SerializeField] private Material uniform;
        [SerializeField] private BoxCollider battlefield;
        [SerializeField, Min(0.1f)] private float moveSpeed = 4f;

        private FormationKind formation;
        private SoldierMovement[] soldiers;
        private Vector3 center;
        private Quaternion facing = Quaternion.identity;
        private const float Radius = 0.35f;
        private Renderer[] bodies;
        private Vector3 destination;

        private void Awake()
        {
            center = destination = transform.position;
            soldiers = new SoldierMovement[soldierCount];
            bodies = new Renderer[soldierCount];
            for (int i = 0; i < soldierCount; i++)
            {
                Vector3 slot = Slot(i, formation);
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = $"Soldier {i + 1:00}";
                body.transform.SetParent(transform, false);
                body.transform.localPosition = slot + Vector3.up * 0.7f;
                body.transform.localScale = Vector3.one * (Radius * 2f);
                bodies[i] = body.GetComponent<Renderer>();
                bodies[i].sharedMaterial = uniform;

                // 캡슐의 앞쪽에 짧은 챙을 달아 바라보는 방향을 보여 준다.
                GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visor.name = "Visor";
                visor.transform.SetParent(body.transform, false);
                visor.transform.localPosition = new Vector3(0f, 0.52f, 0.37f);
                visor.transform.localScale = new Vector3(0.62f, 0.13f, 0.48f);
                visor.GetComponent<Renderer>().sharedMaterial = uniform;
                Destroy(visor.GetComponent<BoxCollider>());

                soldiers[i] = body.AddComponent<SoldierMovement>();
            }
        }

        private Vector3 Slot(int index, FormationKind kind)
        {
            return FormationController.GetSlot(kind, index, soldiers.Length, columns);
        }

        // 전장 경계 안의 두 지점을 잇는 직선도 경계 안에 있다.
        // 현재 중심과 목적지에서 모두 펼칠 수 있어야 이동 중 전환을 허용한다.
        private bool Fits(Vector3 anchor, FormationKind kind)
        {
            Bounds bounds = battlefield.bounds;
            const float margin = 0.49995f;
            for (int i = 0; i < soldiers.Length; i++)
            {
                Vector3 p = anchor + Slot(i, kind);
                if (p.x < bounds.min.x + margin || p.x > bounds.max.x - margin ||
                    p.z < bounds.min.z + margin || p.z > bounds.max.z - margin)
                    return false;
            }
            return true;
        }

        public bool TryChangeFormation(FormationKind kind)
        {
            if (!Fits(center, kind) || !Fits(destination, kind)) return false;
            formation = kind;
            return true;
        }

        public void SetSelected(bool selected)
        {
            var highlight = new MaterialPropertyBlock();
            highlight.SetColor("_BaseColor", Color.cyan);
            foreach (Renderer body in bodies)
                body.SetPropertyBlock(selected ? highlight : null);
        }

        public bool TryMoveTo(Vector3 point)
        {
            if (!Fits(point, formation)) return false;
            destination = new Vector3(point.x, center.y, point.z);
            Vector3 direction = destination - center;
            if (direction.sqrMagnitude > 0.000001f)
                facing = Quaternion.LookRotation(direction);
            return true;
        }

        private void Update()
        {
            float step = moveSpeed * Time.deltaTime;
            center = Vector3.MoveTowards(center, destination, step);
            for (int i = 0; i < soldiers.Length; i++)
            {
                Vector3 target = center + Slot(i, formation) + Vector3.up * 0.7f;
                soldiers[i].Follow(target, facing, step * 1.6f);
            }
        }
    }
}
