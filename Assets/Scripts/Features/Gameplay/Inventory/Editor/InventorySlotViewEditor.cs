#if UNITY_EDITOR
using Features.Gameplay.Inventory.UI;
using UnityEditor;
using UnityEditor.UI;

namespace Features.Gameplay.Inventory.Editor
{
    [CustomEditor(typeof(InventorySlotView))]
    [CanEditMultipleObjects]
    public sealed class InventorySlotViewEditor : ButtonEditor
    {
        private SerializedProperty _icon;
        private SerializedProperty _amountText;
        private SerializedProperty _backgroundImage;
        private SerializedProperty _selectedSprite;
        private SerializedProperty _deselectedSprite;

        protected override void OnEnable()
        {
            base.OnEnable();

            _icon = serializedObject.FindProperty("icon");
            _amountText = serializedObject.FindProperty("amountText");
            _backgroundImage = serializedObject.FindProperty("backgroundImage");
            _selectedSprite = serializedObject.FindProperty("selectedSprite");
            _deselectedSprite = serializedObject.FindProperty("deselectedSprite");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Button Settings", EditorStyles.boldLabel);
            base.OnInspectorGUI();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Inventory Slot References", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_icon);
            EditorGUILayout.PropertyField(_amountText);
            EditorGUILayout.PropertyField(_backgroundImage);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Inventory Slot Sprites", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_selectedSprite);
            EditorGUILayout.PropertyField(_deselectedSprite);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif