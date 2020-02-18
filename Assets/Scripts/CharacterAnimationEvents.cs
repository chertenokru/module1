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
        // завершить атаку
        character.SetState(Character.State.Attack);
    }

    private void ShotEndEvent()
    {
        // завершить атаку
        character.SetState(Character.State.Shot);
    }

    private void AttackArmEndEvent()
    {
        // завершить атаку
        character.SetState(Character.State.Attack);
    }

    private void SetDamageEvent()
    {
        // пора нанести урон посреди атаки
        character.SetDamageEvent();
    }
}