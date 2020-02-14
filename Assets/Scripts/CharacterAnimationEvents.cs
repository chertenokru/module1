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
        print($"{character.name} get event - AttackEndEvent ");
    }

    private void ShotEndEvent()
    {
        character.SetState(Character.State.Shot);
        print($"{character.name} get event - ShotEndEvent ");
    }

    private void AttackArmEndEvent()
    {
        character.SetState(Character.State.Attack);
        print($"{character.name} get event - AttackArmEndEvent ");
    }

    private void SetDamageEvent()
    {
        character.SetDamageEvent();
        print($"{character.name} get event - SetDamageEvent ");
    }
}