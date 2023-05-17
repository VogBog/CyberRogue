using UnityEngine;

public class BeforeBossTrigger : MonoBehaviour
{
    protected BasePlayer player;
    private GameManager _game;
    protected GameManager game
    {
        get
        {
            if(_game == null)
                _game = FindObjectOfType<GameManager>();
            return _game;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player"))
        {
            player = other.gameObject.GetComponent<BasePlayer>();
            player.SetNotice("Совет: сохранитесь перед финальной битвой.");
            if (game.IsAutoSafe)
                game.SaveData();
            PlayerIsCollided();
        }
    }

    protected virtual void PlayerIsCollided() { }
}
