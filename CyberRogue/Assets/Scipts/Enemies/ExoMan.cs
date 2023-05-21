using UnityEngine;

public class ExoMan : BaseAnimatedEnemy
{
    public float CloneCooldown; //Не бейте за публичные поля, пожалуйста
    public BaseAnimatedEnemy Clone;

    private float cloneTime = 20;
    private AIManager _ai;
    private AIManager AI
    {
        get
        {
            if(_ai == null)
                _ai = FindObjectOfType<AIManager>();
            return _ai;
        }
    }

    protected override void Update()
    {
        if(isFighting)
        {
            base.Update();
            if (cloneTime > 0)
                cloneTime -= Time.deltaTime;
            else
            {
                cloneTime = CloneCooldown;
                AI.AddNewEnemy(Clone, transform.position);
            }
        }
    }
}
