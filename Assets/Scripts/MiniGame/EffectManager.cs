using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EffectManager : MonoBehaviour
{
    [Serializable]
    private class EffectEntry
    {
        public string id;
        public GameObject particlePrefab;
    }

    [SerializeField] private EffectEntry[] effects;

    public GameObject Spawn(string id, Vector3 position)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].id != id)
                continue;

            var effect = Instantiate(effects[i].particlePrefab, position, effects[i].particlePrefab.transform.rotation);
            SceneManager.MoveGameObjectToScene(effect, gameObject.scene);
            return effect;
        }

        return null;
    }
}
