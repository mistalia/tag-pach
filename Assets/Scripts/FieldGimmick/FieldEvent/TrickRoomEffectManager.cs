using UnityEngine;

public class TrickRoomEffectManager : MonoBehaviour
{
    public static TrickRoomEffectManager Instance { get; private set; }

    [SerializeField] ParticleSystem trickRoomObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayEffect()
    {
        if (trickRoomObject != null)
        {
            trickRoomObject.Play();
        }
    }

    public void StopEffect()
    {
        if (trickRoomObject != null)
        {
            trickRoomObject.Stop();
        }
    }

    public void SetLifeTime(int lifeTime)
    {
        if (trickRoomObject != null)
        {
            var main = trickRoomObject.main;
            main.duration = lifeTime;
        }
    }


}
