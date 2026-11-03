using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectShift
{
    public sealed class SquadInput : MonoBehaviour
    {
        [SerializeField] private Camera battleCamera;
        [SerializeField] private BoxCollider ground;
        [SerializeField] private BattlefieldGrid grid;
        private Squad selected;

        private bool menuOpen;
        private string message;

        private void OnGUI()
        {
            if (!menuOpen || selected == null) return;
            GUILayout.BeginArea(new Rect(16, 16, 240, 195), GUI.skin.box);
            GUILayout.Label("SHIFT · 진형 선택");
            FormationButton("밀집 진형", FormationKind.Compact);
            FormationButton("일자 진형", FormationKind.Line);
            FormationButton("쐐기 진형", FormationKind.Wedge);
            GUILayout.Label(message);
            GUILayout.EndArea();
        }

        private void FormationButton(string label, FormationKind kind)
        {
            if (!GUILayout.Button(label)) return;
            if (selected.TryChangeFormation(kind)) menuOpen = false;
            else message = "전장 안쪽으로 이동한 뒤 선택하세요.";
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (selected != null && keyboard != null &&
                (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame))
            {
                menuOpen = !menuOpen;
                message = "Shift 키로 닫기";
            }
            if (menuOpen) return;

            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            bool select = mouse.leftButton.wasPressedThisFrame;
            bool move = mouse.rightButton.wasPressedThisFrame;
            if (!select && !move) return;

            Ray ray = battleCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (select)
            {
                if (selected != null) selected.SetSelected(false);
                selected = Physics.Raycast(ray, out RaycastHit hit)
                    ? hit.collider.GetComponentInParent<Squad>() : null;
                if (selected != null) selected.SetSelected(true);
            }

            if (move && selected != null && ground.Raycast(ray, out RaycastHit target, 1000f)
                && target.normal.y > 0.9f && grid.TryGetWalkableTile(target.point, out BattleTile tile)
                && selected.TryMoveTo(tile.Center))
                grid.ShowDestination(tile);
        }
    }
}
