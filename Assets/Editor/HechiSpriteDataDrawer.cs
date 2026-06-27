using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HechiSpriteData))]
public class HechiSpriteDataDrawer : PropertyDrawer
{
    private const float Spacing = 2f;
    private const float LineH = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 항상: 조건 줄(1) + mainSprite(1) + miniGameSprite(1)
        return (LineH + Spacing) * 3 - Spacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var conditionCntProp = property.FindPropertyRelative("conditionCnt");
        var condition1Prop   = property.FindPropertyRelative("condition1");
        var condition2Prop   = property.FindPropertyRelative("condition2");
        var mainSpriteProp   = property.FindPropertyRelative("mainSprite");
        var miniGameSpriteProp = property.FindPropertyRelative("miniGameSprite");

        int cnt = conditionCntProp.intValue;

        float y = position.y;
        float w = position.width;
        float x = position.x;

        // ── 1행: conditionCnt + condition1 + condition2 (조건 수에 따라 표시) ──
        Rect rowRect = new Rect(x, y, w, LineH);

        // conditionCnt: 항상 첫 번째, 고정 너비
        float cntW = 80f;
        Rect cntRect = new Rect(x, y, cntW, LineH);
        EditorGUI.PropertyField(cntRect, conditionCntProp, GUIContent.none);
        conditionCntProp.intValue = Mathf.Clamp(conditionCntProp.intValue, 0, 2);

        float remaining = w - cntW - Spacing;

        if (cnt == 0)
        {
            // condition 영역을 회색 레이블로 채움
            Rect disabledRect = new Rect(x + cntW + Spacing, y, remaining, LineH);
            EditorGUI.LabelField(disabledRect, "─", EditorStyles.centeredGreyMiniLabel);
        }
        else if (cnt == 1)
        {
            Rect c1Rect = new Rect(x + cntW + Spacing, y, remaining, LineH);
            EditorGUI.PropertyField(c1Rect, condition1Prop, GUIContent.none);
        }
        else // cnt == 2
        {
            float half = (remaining - Spacing) * 0.5f;
            Rect c1Rect = new Rect(x + cntW + Spacing, y, half, LineH);
            Rect c2Rect = new Rect(x + cntW + Spacing + half + Spacing, y, half, LineH);
            EditorGUI.PropertyField(c1Rect, condition1Prop, GUIContent.none);
            EditorGUI.PropertyField(c2Rect, condition2Prop, GUIContent.none);
        }

        // ── 2행: mainSprite ──
        y += LineH + Spacing;
        EditorGUI.PropertyField(new Rect(x, y, w, LineH), mainSpriteProp);

        // ── 3행: miniGameSprite ──
        y += LineH + Spacing;
        EditorGUI.PropertyField(new Rect(x, y, w, LineH), miniGameSpriteProp);

        EditorGUI.EndProperty();
    }
}
