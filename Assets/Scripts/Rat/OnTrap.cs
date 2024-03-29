using UnityEngine;


public class OnTrap : IRatState
{
    public Vector2 Direction { get; private set; }

    private string _anim;
    
    public OnTrap(bool cheeseTrap)
    {
        _anim = cheeseTrap ? "Trap" : "Shock";
    }
    
    public void Enter(RatController controller, IRatState previousState = null)
    {
        Direction = previousState != null ? previousState.Direction : new(-1, -1);
        controller.PlayOneTimeAnimationXY(_anim, Direction);
        controller.StartCoroutine(controller.Die());
    }

    public void Update()
    {
        
    }

    public void Exit(IRatState nextState = null)
    {
        
    }
}
