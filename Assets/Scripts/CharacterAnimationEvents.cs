using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    private Character character;

    private void Start()
    {
        character = GetComponentInParent<Character>();
    }

    private void AttackEnd()
    {
        character.SetState(Character.State.Attack);
    }
    private void ShotEnd()
    {
        character.SetState(Character.State.Shot);
    }
    
    private void AttackArmEnd()
    {
        character.SetState(Character.State.Attack);
    }

}