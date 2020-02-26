public partial class GameController
{
    public class CharacterInfo
    {
        public enum CharacterState
        {
            CharacterMove,
            CharacterFire,
            CharacterIdle
        }

        public Character character;
        public bool canMove = true;
        public bool canFire = true;
        public bool isEndTurn = false;
        public bool hasTarget = false;
        public CharacterState characterState = CharacterState.CharacterIdle;
        private HitEffectAnimation hitEffectAnimation;

        public CharacterInfo(Character _character)
        {
            character = _character;
        }

        public void TurnReset()
        {
            canFire = true;
            canMove = true;
            isEndTurn = false;
            hasTarget = false;

            character.TurnReset();
            characterState = CharacterState.CharacterIdle;
        }

        public void EndTurn()
        {
            canFire = false;
            canMove = false;
            isEndTurn = true;
        }

        public void Update(bool _canFire)
        {
            canFire = _canFire;
            canMove = (character.distanceCurrentMove > 0.1f);
        }
    }
}