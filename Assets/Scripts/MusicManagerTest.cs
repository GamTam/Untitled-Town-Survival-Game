using UnityEngine;

public class MusicManagerTest : MonoBehaviour
{
    [SerializeField] private string _song;
    [SerializeField] private string _song2;

    [SerializeField] private bool _fade;

    private int _songVersion = 0;

    private void Start()
    {
        Globals.MusicManager.Play(_song);
    }

    private void Update()
    {
        if (!_fade) return;
        
        _fade = false;

        if (_songVersion == 0)
        {
            _songVersion = 1;
            Globals.MusicManager.FadeVariation(_song2);
        }
        else
        {
            _songVersion = 0;
            Globals.MusicManager.FadeVariation(_song);
        }
    }
}