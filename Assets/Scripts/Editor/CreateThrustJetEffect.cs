using UnityEditor;
using UnityEngine;

public static class CreateThrustJetEffect
{
    [MenuItem("Tools/Create FX_ThrustJet Prefab")]
    public static void Create()
    {
        var root = new GameObject("FX_ThrustJet");

        var ps = root.AddComponent<ParticleSystem>();
        var renderer = root.GetComponent<ParticleSystemRenderer>();

        // ── Main ──────────────────────────────────────────
        var main = ps.main;
        main.duration        = 0.2f;
        main.loop            = false;
        main.playOnAwake     = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(7f, 11f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.gravityModifier = 0f;
        main.maxParticles    = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // 주황 → 노랑 그라디언트
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.4f, 0f), 0f),
                    new GradientColorKey(new Color(1f, 0.9f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(gradient);

        // ── Emission ──────────────────────────────────────
        var emission = ps.emission;
        emission.enabled     = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

        // ── Shape: 좁은 Cone ─────────────────────────────
        var shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Cone;
        shape.angle        = 6f;
        shape.radius       = 0.02f;

        // ── Size over Lifetime: 끝으로 갈수록 작아짐 ──────
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        );
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── Color over Lifetime: 투명해짐 ─────────────────
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var fadeGrad = new Gradient();
        fadeGrad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(fadeGrad);

        // ── Renderer: Stretched Billboard (길게 늘어남) ───
        renderer.renderMode        = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale     = 0f;
        renderer.lengthScale       = 4f;   // 길이 배율
        renderer.material          = GetDefaultParticleMaterial();

        // ── 프리팹 저장 ────────────────────────────────────
        const string savePath = "Assets/Prefabs/Particles/FX_ThrustJet.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[CreateThrustJetEffect] 프리팹 생성 완료: {savePath}");
    }

    private static Material GetDefaultParticleMaterial()
    {
        // URP Default Particle (Additive) 재질 — 없으면 기본 재질 반환
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/FX_ThrustJet.mat");
        if (mat != null) return mat;

        // URP Particles/Additive 셰이더 시도
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Additive");

        if (shader != null)
        {
            mat = new Material(shader);
            mat.SetFloat("_Surface", 1);          // Transparent
            mat.SetFloat("_BlendMode", 3);        // Additive
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, "Assets/Materials/FX_ThrustJet.mat");
        }
        else
        {
            mat = new Material(Shader.Find("Standard"));
        }

        return mat;
    }
}
