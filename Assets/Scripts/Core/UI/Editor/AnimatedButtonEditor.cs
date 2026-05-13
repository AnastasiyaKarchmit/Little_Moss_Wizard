using Core.UI.Animations;
using UnityEditor;
using UnityEditor.UI;

namespace Core.UI.Editor
{
    [CustomEditor(typeof(AnimatedButton), true)]
    [CanEditMultipleObjects]
    public sealed class AnimatedButtonEditor : ButtonEditor
    {
        private SerializedProperty _pressedScale;
        private SerializedProperty _selectedScale;
        private SerializedProperty _normalScale;

        private SerializedProperty _pressDuration;
        private SerializedProperty _releaseDuration;

        private SerializedProperty _pressEase;
        private SerializedProperty _releaseEase;
        
        private SerializedProperty _arrows;

        protected override void OnEnable()
        {
            base.OnEnable();

            _pressedScale = serializedObject.FindProperty("pressedScale");
            _selectedScale = serializedObject.FindProperty("selectedScale");
            _normalScale = serializedObject.FindProperty("normalScale");

            _pressDuration = serializedObject.FindProperty("pressDuration");
            _releaseDuration = serializedObject.FindProperty("releaseDuration");

            _pressEase = serializedObject.FindProperty("pressEase");
            _releaseEase = serializedObject.FindProperty("releaseEase");
            
            _arrows = serializedObject.FindProperty("arrows");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Button Animation", EditorStyles.boldLabel);

            serializedObject.Update();

            EditorGUILayout.PropertyField(_pressedScale);
            EditorGUILayout.PropertyField(_selectedScale);
            EditorGUILayout.PropertyField(_normalScale);

            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(_pressDuration);
            EditorGUILayout.PropertyField(_releaseDuration);

            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(_pressEase);
            EditorGUILayout.PropertyField(_releaseEase);

            EditorGUILayout.Space(4);
            
            EditorGUILayout.PropertyField(_arrows);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}