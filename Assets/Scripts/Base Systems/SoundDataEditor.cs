using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundData))]
public class SoundDataEditor : Editor
{
    private SerializedProperty _clip;
    private SerializedProperty _volume;
    private SerializedProperty _isLooping;
    private SerializedProperty _usePitchVariation;
    private SerializedProperty _pitchVariationAmount;
    private SerializedProperty _useVolumeVariation;
    private SerializedProperty _volumeVariationAmount;
    private SerializedProperty _fadeIn;
    private SerializedProperty _fadeOut;
    private SerializedProperty _fadeDuration;
    private SerializedProperty _loopSpacing;
    private SerializedProperty _loopVariationPct;

    private void OnEnable()
    {
        _clip = serializedObject.FindProperty("Clip");
        _volume = serializedObject.FindProperty("Volume");
        _isLooping = serializedObject.FindProperty("IsLooping");
        _usePitchVariation = serializedObject.FindProperty("UsePitchVariation");
        _pitchVariationAmount = serializedObject.FindProperty("PitchVariationAmount");
        _useVolumeVariation = serializedObject.FindProperty("UseVolumeVariation");
        _volumeVariationAmount = serializedObject.FindProperty("VolumeVariationAmount");
        _fadeIn = serializedObject.FindProperty("FadeIn");
        _fadeOut = serializedObject.FindProperty("FadeOut");
        _fadeDuration = serializedObject.FindProperty("FadeDuration");
        _loopSpacing = serializedObject.FindProperty("LoopSpacing");
        _loopVariationPct = serializedObject.FindProperty("LoopVariationPct");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_clip);
        EditorGUILayout.PropertyField(_volume);
        EditorGUILayout.PropertyField(_usePitchVariation);
        if (_usePitchVariation.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pitchVariationAmount);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_useVolumeVariation);
        if (_useVolumeVariation.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_volumeVariationAmount);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_isLooping);
        if (_isLooping.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_fadeIn);
            EditorGUILayout.PropertyField(_fadeOut);
            EditorGUILayout.PropertyField(_fadeDuration);
            EditorGUILayout.PropertyField(_loopSpacing);
            EditorGUILayout.PropertyField(_loopVariationPct);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}