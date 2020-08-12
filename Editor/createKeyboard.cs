using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.IO;
public class TestAnim : EditorWindow
{
    public AnimatorController controller = null;
    public Transform keyboard = null;
    [UnityEditor.MenuItem("TentenVRCKeyboard/CreateKeyboard")]

    public static void ShowWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(TestAnim));
        window.maxSize = new Vector2(400, 200);
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("TentenVRCkeyboard", EditorStyles.boldLabel);


        keyboard = (Transform) EditorGUILayout.ObjectField("keyboard", keyboard, typeof(Transform), true);
        if (GUILayout.Button("Generate Animation"))
        {
            CreateAnim();
        }
        controller = (AnimatorController)EditorGUILayout.ObjectField("controller", controller, typeof(AnimatorController), true);
        if (GUILayout.Button("Generate Controller") && controller)
        {
            CreateController();
        }

    }
    private static string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        string[] splitString = path.Split('/');

        ArrayUtility.RemoveAt(ref splitString, 0);
        path = string.Join("/", splitString);
        return path;
    }
    public void CreateAnim()
    {
        if (!Directory.Exists("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!Directory.Exists("Assets/Resources/tentenVRCkeyboard"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "tentenVRCkeyboard");
        }
        for (int j = 1; j < 13; j++)
        {
            if (!Directory.Exists("Assets/Resources/tentenVRCkeyboard/" + j))
            {
                AssetDatabase.CreateFolder("Assets/Resources/tentenVRCkeyboard", j.ToString());
            } 
            for (int i = 1; i < 28; i++)
            {
                AnimationClip clip = new AnimationClip();
                Keyframe[] keys;
                keys = new Keyframe[2];
                keys[0] = new Keyframe(0.0f, i);
                keys[1] = new Keyframe(0.016f, i);
                AnimationCurve curve = new AnimationCurve(keys);
                clip.SetCurve(GetGameObjectPath(keyboard), typeof(MeshRenderer), "material._Text" + j, curve);

                AssetDatabase.CreateAsset(clip, "Assets/Resources/tentenVRCkeyboard/" + j + "/" + j + " " + i + ".anim");
            }
            
        }

        AnimationClip clipFinal = new AnimationClip();
        Keyframe[] keysFinal;
        keysFinal = new Keyframe[2];
        keysFinal[0] = new Keyframe(0.0f, 27f);
        keysFinal[1] = new Keyframe(0.016f, 27f);
        AnimationCurve curvefinal = new AnimationCurve(keysFinal);
        for (int i = 0; i < 13; i++)
        {
            clipFinal.SetCurve(GetGameObjectPath(keyboard), typeof(MeshRenderer), "material._Text" + i, curvefinal);
        }

        AssetDatabase.CreateAsset(clipFinal, "Assets/Resources/tentenVRCkeyboard/reset.anim");

        AnimationClip enable = new AnimationClip();
        Keyframe[] keysEnable;
        keysEnable = new Keyframe[2];
        keysEnable[0] = new Keyframe(0.0f, 1);
        keysEnable[1] = new Keyframe(0.016f, 1);
        AnimationCurve curveEnable = new AnimationCurve(keysEnable);
        enable.SetCurve(GetGameObjectPath(keyboard), typeof(GameObject), "m_IsActive", curveEnable);
        AssetDatabase.CreateAsset(enable, "Assets/Resources/tentenVRCkeyboard/enableKeyboard.anim");

        AnimationClip disable = new AnimationClip();
        Keyframe[] keysDisable;
        keysDisable = new Keyframe[2];
        keysDisable[0] = new Keyframe(0.0f, 0);
        keysDisable[1] = new Keyframe(0.016f, 0);
        AnimationCurve curveDisable = new AnimationCurve(keysDisable);
        disable.SetCurve(GetGameObjectPath(keyboard), typeof(GameObject), "m_IsActive", curveDisable);
        AssetDatabase.CreateAsset(disable, "Assets/Resources/tentenVRCkeyboard/disableKeyboard.anim");

    }
    void CreateController()
    {
        // Creates the controller


        // Add parameters
        controller.AddParameter("Letter", AnimatorControllerParameterType.Int);
        //add first layer
        int layer_start = -1;
        foreach (var x in controller.layers)
        {
            layer_start += 1;
        }
        for (int j = 1; j < 13; j++)
        {
            var layer = new UnityEditor.Animations.AnimatorControllerLayer
            {
                name = "Letter" + j,
                defaultWeight = 1f,
                stateMachine = new UnityEditor.Animations.AnimatorStateMachine()
            };
            var working_layer = j + layer_start;
            controller.AddLayer(layer);
            // Add StateMachines
            var rootStateMachine = controller.layers[working_layer].stateMachine;
            Motion reset = Resources.Load<Motion>("tentenVRCkeyboard/reset");

            var entry = controller.AddMotion(reset, working_layer);
            AnimatorState idle = entry;
            AnimatorState input = entry;
            for (int k = 1; k < j; k++)
            {
                var old_idle = idle;
                input = rootStateMachine.AddState(k.ToString() + "input");
                var trans = idle.AddTransition(input);
                trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0, "Letter");
                trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.NotEqual, 255, "Letter");
                idle = rootStateMachine.AddState(k.ToString() + "idle");
                var trans2 = input.AddTransition(idle);
                trans2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0, "Letter");
                var reset_transition = idle.AddTransition(entry);
                reset_transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 255, "Letter");


                var back_space = rootStateMachine.AddState(k.ToString() + "back space buffer");

                var trans3 = idle.AddTransition(back_space);
                trans3.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 254, "Letter");

                var trans4 = back_space.AddTransition(old_idle);
                trans4.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0, "Letter");
            }

            for (int i = 1; i < 28; i++)
            {

                var file = "tentenVRCkeyboard/" + j + "/" + j + " " + i;
                Motion motion = Resources.Load<Motion>(file);

                var state = controller.AddMotion(motion, working_layer);
                var reset_transition = state.AddTransition(entry);
                reset_transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 255, "Letter");
                var transition = idle.AddTransition(state);
                transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, i, "Letter");
                /*
                for (int x = 1; x < 13; x++)    
                {
                    var newstate = controller.AddMotion(motion, working_layer);
                    var newstateAdvance = rootStateMachine.AddState(i + x + "back space Advance");
                    var backspaceBuffer = rootStateMachine.AddState(i + x + "back space Backward");

                    var newtransition = state.AddTransition(newstateAdvance);
                    newtransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0, "Letter");
                    newtransition = newstateAdvance.AddTransition(newstate);
                    newtransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0, "Letter");
                    var newtransition2 = newstate.AddTransition(backspaceBuffer);

                    newtransition2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0, "Letter");
                    newtransition2 = backspaceBuffer.AddTransition(state);
                    newtransition2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 254, "Letter");
                }
                */
            }

        }
        var resetLayer = new UnityEditor.Animations.AnimatorControllerLayer
        {
            name = "Toggle",
            defaultWeight = 1f,
            stateMachine = new UnityEditor.Animations.AnimatorStateMachine()
        };
        controller.AddLayer(resetLayer);
        controller.AddParameter("ToggleKeyboard", AnimatorControllerParameterType.Int);
        Motion enable = Resources.Load<Motion>("tentenVRCkeyboard/enableKeyboard");
        Motion disable = Resources.Load<Motion>("tentenVRCkeyboard/disableKeyboard");
        var disableState = controller.AddMotion(disable, 13 + layer_start);
        var enableState = controller.AddMotion(enable, 13 + layer_start);
        var toEnable = disableState.AddTransition(enableState);
        var toDisable = enableState.AddTransition(disableState);
        toEnable.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1, "ToggleKeyboard");
        toDisable.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0, "ToggleKeyboard");
    }
}