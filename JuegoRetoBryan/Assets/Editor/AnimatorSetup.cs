using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Script editor que configura automáticamente el Animator Controller para Etapa 2.
/// Uso: Click derecho en el Player → Setup Animator for Stage 2
/// </summary>
public class AnimatorSetup
{
    [MenuItem("GameObject/Setup Animator for Stage 2")]
    public static void SetupAnimator()
    {
        GameObject player = Selection.activeGameObject;
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Selecciona el Player en la Hierarchy", "OK");
            return;
        }

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Error", "El Player no tiene componente Animator. Agrega uno primero.", "OK");
            return;
        }

        string animationPath = "Assets/Animation";
        if (!AssetDatabase.IsValidFolder(animationPath))
            AssetDatabase.CreateFolder("Assets", "Animation");

        string controllerPath = animationPath + "/PlayerAnimator.controller";

        AnimatorController controller;
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
        {
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        }
        else
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        animator.runtimeAnimatorController = controller;

        // Agregar parámetros
        AddParameter(controller, "Speed", AnimatorControllerParameterType.Float);
        AddParameter(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
        AddParameter(controller, "Jump", AnimatorControllerParameterType.Trigger);

        // Obtener root state machine
        var rootStateMachine = controller.layers[0].stateMachine;

        // Limpiar estados previos
        var states = rootStateMachine.states;
        foreach (var state in states)
        {
            if (state.state.name != "Entry")
                rootStateMachine.RemoveState(state.state);
        }

        // Crear estado Locomotion con Blend Tree
        var locomotionState = rootStateMachine.AddState("Locomotion");

        BlendTree blendTree = new BlendTree();
        blendTree.name = "Locomotion BlendTree";
        blendTree.blendType = BlendTreeType.Simple1D;
        blendTree.blendParameter = "Speed";

        // Agregar movimientos vacíos (el usuario asignará animaciones después)
        blendTree.AddChild(null, 0f); // Idle
        blendTree.AddChild(null, 1f); // Run

        AssetDatabase.AddObjectToAsset(blendTree, controller);
        locomotionState.motion = blendTree;

        // Conectar Entry → Locomotion
        rootStateMachine.AddEntryTransition(locomotionState);

        // Crear transición Locomotion → Exit (para Jump)
        var exitTransition = locomotionState.AddExitTransition();
        exitTransition.AddCondition(AnimatorConditionMode.IfNot, 1f, "IsGrounded");

        // Crear transición Exit → Locomotion (volver del salto)
        var landTransition = rootStateMachine.AddAnyStateTransition(locomotionState);
        landTransition.AddCondition(AnimatorConditionMode.If, 1f, "IsGrounded");

        // Jump trigger en Any State
        var jumpTransition = rootStateMachine.AddAnyStateTransition(locomotionState);
        jumpTransition.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Éxito", "Animator Controller configurado correctamente para Etapa 2.\n\nYa puedes presionar Play.", "OK");
    }

    private static void AddParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == paramName)
                return;
        }

        controller.AddParameter(paramName, type);
    }

    [MenuItem("GameObject/Setup Animator for Stage 2", validate = true)]
    private static bool ValidateSetupAnimator()
    {
        return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<CharacterController>() != null;
    }
}
