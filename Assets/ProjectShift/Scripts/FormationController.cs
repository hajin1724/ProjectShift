using UnityEngine;

namespace ProjectShift
{
    public enum FormationKind { Compact, Line, Wedge }

    public static class FormationController
    {
        // (앞뒤, 좌우). 첫 9명은 두 날개, 다음 두 묶음은 삼각형의 내부를 채운다.
        private static readonly Vector2[] WedgeSlots =
        {
            new(2, 0), new(1, -0.5f), new(1, 0.5f),
            new(0, -1), new(0, 1), new(-1, -1.5f), new(-1, 1.5f),
            new(-2, -2), new(-2, 2),
            new(0, 0), new(-1, -0.5f), new(-1, 0.5f),
            new(-2, 0), new(-2, -1), new(-2, 1)
        };

        public static Vector3 GetSlot(FormationKind kind, int index, int count, int columns)
        {
            const float spacing = 1.1f;
            if (kind == FormationKind.Line)
                return Vector3.right * ((index - (count - 1) * 0.5f) * spacing);

            if (kind == FormationKind.Wedge)
            {
                // BattleTest 카메라의 화면 오른쪽은 월드 +X, -Z 방향이다.
                Vector2 slot = WedgeSlots[index];
                return new Vector3(slot.x + slot.y, 0f, slot.y - slot.x)
                    * (0.70710678f * spacing);
            }

            int rows = Mathf.CeilToInt((float)count / columns);
            int rank = index / columns;
            int rowSize = Mathf.Min(columns, count - rank * columns);
            return new Vector3(index % columns - (rowSize - 1) * 0.5f, 0f,
                (rows - 1) * 0.5f - rank) * spacing;
        }
    }
}
