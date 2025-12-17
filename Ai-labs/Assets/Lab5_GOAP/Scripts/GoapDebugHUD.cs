using System.Text;
using UnityEngine;

namespace Lab5
{
    public class GoapDebugHUD : MonoBehaviour
    {
        public GoapAgent agent;
        [Header("HUD")]
        public bool show = true;
        public Vector2 screenOffset = new Vector2(10, 10);
        GUIStyle _style;

        void Awake()
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = true
            };
        }
        void OnGUI()
        {
            if (!show || agent == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("<b>GOAP Debug</b>");
            sb.AppendLine(agent.GetDebugString());
            GUI.Label(new Rect(screenOffset.x, screenOffset.y, 600, 800), sb.ToString(), _style);
        }
    }

}
