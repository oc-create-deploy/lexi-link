using UnityEngine;
using TMPro;

public sealed class LexiOnboarding : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install() { new GameObject("Language guidance").AddComponent<LexiOnboarding>(); }
    private float next;
    private void Update()
    {
        if(Time.unscaledTime<next) return;next=Time.unscaledTime+1;
        foreach(TMP_Text t in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if(!t.gameObject.scene.IsValid()) continue;
            string value=t.text.Replace("\n"," ").ToUpperInvariant();
            if(value.Contains("СОБИРАЙТЕ") && value.Contains("СЛОВА"))
            { t.text="Connect letters to form Russian words.\nFind every word to complete the level.";t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=42; }
            else if(value.Contains("НАЖМИТЕ") && value.Contains("ПРОДОЛЖИТЬ"))
            { t.text="TAP TO CONTINUE";t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=42; }
            else if(value.Contains("НАЖМИТЕ") && value.Contains("НАЧАТЬ"))
            { t.text="TAP TO START\nRussian word puzzles";t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=48; }
        }
    }
}
