using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    private Character character;

    private void Start()
    {
        character = GetComponentInParent<Character>();
    }

    private void AttackEndEvent()
    {
        character.SetState(Character.State.Attack);
    }

    private void ShotEndEvent()
    {
        character.SetState(Character.State.Shot);
    }

    private void AttackArmEndEvent()
    {
        character.SetState(Character.State.Attack);
    }

    private void SetDamageEvent()
    {
        character.SetDamageEvent();
    }
}