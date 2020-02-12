using System;
using System.Collections;

using UnityEngine;


public class Character : MonoBehaviour
{
    
    //    private const int AnimatorFieldSpeed = Animator.StringToHash("speed");
    public enum State
    {
        Idle,
        RunningToEnemy,
        RunningFromEnemy,
        BeginAttack, // близкая атака
        Attack,
        BeginShot, // дист аттака
        Shot,
        Dead
    }
  

    public enum CharacterType
    {
        None,
        PoliceMan,
        Hooligan,
        Zombie
    }

    private const string AnimatorFieldSpeed = "speed";
    private const string AnimatorAttack = "attack";
    private const string AnimatorAttackArm = "attack arm";
    private const string AnimatorShot = "shot";
    private const string AnimatorDead = "dead";
    
    private const string TagWeaponHand = "Hand";
    private State state;


    [SerializeField] private Transform target;
    [SerializeField] private Weapons.WeaponsType weaponsType = Weapons.WeaponsType.None;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Animator animator;
    private Character targetCharacter;
    private WeaponsController weaponsController;
    private GameObject weaponHand;
    private HealthBar healthBar;
    
    private int weapontDamage = 1;


    public CharacterType type;
    public float runSpeed = 0.05f;
    public float distanceFromEnemy = 1.2f;
    public int health = 4;
    public int maxHealth = 4;

    public Transform Target
    {
        get => target;
        set => SetTarget(value);
    }

    public Weapons.WeaponsType WeaponsType
    {
        
        get => weaponsType;
        set => SetWeapon(value);
    }


    private void Awake()
    {
        InitWeaponCharacter();
        
        healthBar = GetComponentInChildren<HealthBar>();
        healthBar.SetMaxHealth(maxHealth);
    }


    // Start is called before the first frame update
    void Start()
    {
        
        state = State.Idle;
        startPosition = transform.position;
        startRotation = transform.rotation;
        animator = GetComponentInChildren<Animator>();
        SetHealth(health);
        if (target == null) AutoSelectTarget();
        SetTarget(target);
        SetWeapon(weaponsType);
    }

    private bool AutoSelectTarget()
    {
        Character temp;
        switch (type)
        {
            case CharacterType.PoliceMan:
                temp = SearchTarget(CharacterType.Hooligan);
                if (temp == null) temp = SearchTarget(CharacterType.Zombie);
                if (temp == null) temp = SearchTarget(CharacterType.PoliceMan);
                return SetTarget(temp);
            case CharacterType.Hooligan:
                temp = SearchTarget(CharacterType.PoliceMan);
                if (temp == null) temp = SearchTarget(CharacterType.Zombie);
                if (temp == null) temp = SearchTarget(CharacterType.Hooligan);
                return SetTarget(temp);
            case CharacterType.Zombie:
                temp = SearchTarget(CharacterType.PoliceMan);
                if (temp == null) temp = SearchTarget(CharacterType.Hooligan);
                if (temp == null) temp = SearchTarget(CharacterType.Zombie);
                return SetTarget(temp);
        }

        return false;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        switch (state)
        {
            case State.Idle:
                transform.rotation = startRotation;
                break;

            case State.RunningToEnemy:
                if (RunToTowards(target.position, distanceFromEnemy))
                {
                    SetState(State.Idle);
                    SetState(State.BeginAttack);
                }

                break;

            case State.RunningFromEnemy:
                if (RunToTowards(startPosition, 0.0f))
                    SetState(State.Idle);
                break;
            case State.Attack:
                if (targetCharacter != null)
                {
                    if (targetCharacter.SetDamage(weapontDamage)) AutoSelectTarget();
                    SetState(State.RunningFromEnemy);
                }

                break;
            case State.Shot:
                if (targetCharacter != null)
                {
                    if (targetCharacter.SetDamage(weapontDamage)) AutoSelectTarget();
                    RotateToTarget(startRotation.eulerAngles);
                }

                SetState(State.Idle);
                break;
        }
    }

    private void SetWeapon(Weapons.WeaponsType value)
    {
        weaponsType = value;
        if (weaponHand.transform.childCount > 0)
        {
            Destroy(weaponHand.transform.GetChild(0).gameObject);
        }

        GameObject mesh = weaponsController.GetMeshWeapont(weaponsType);
        if (mesh != null)
        {
            GameObject obj = Instantiate(mesh, weaponHand.transform);
            //obj.transform.parent = weaponHand.transform;
        }

        weapontDamage = weaponsController.GetDamage(weaponsType);
    }

    public bool SetDamage(int damage)
    {
        if (state == State.Dead) return true;
        SetHealth(health-damage);
        print(name + " get damage - " + damage);
        if (health <= 0)
        {
            SetHealth(0);
            SetState(State.Idle);
            SetState(State.Dead);
            print(name + " is dead !");
            return true;
        }

        return false;
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
        healthBar.SetHealth(health);
    }


    public bool SetTarget(Character newTargetCharacter)
    {
        if (newTargetCharacter == null)
            return SetTarget((Transform) null);
        else return SetTarget(newTargetCharacter.transform);
    }

    public bool SetTarget(Transform newTargetTransform)
    {
        if (newTargetTransform == null)
        {
            print($"{name} reported - target not found! ");
            return false;
        }

        //if (state != State.Idle) return false;
        target = newTargetTransform;
        targetCharacter = target.gameObject.GetComponent<Character>();
        print($"{name} set as target - {target.name}");
        return true;
    }

    public void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        //animator setting    
        switch (state)
        {
            case State.Idle:
                animator.SetFloat(AnimatorFieldSpeed, 0.0f);
                break;
            case State.RunningToEnemy:
                startPosition = transform.position;
                startRotation = transform.rotation;
                animator.SetFloat(AnimatorFieldSpeed, runSpeed);
                break;
            case State.RunningFromEnemy:
                animator.SetFloat(AnimatorFieldSpeed, runSpeed);
                break;
            case State.BeginAttack:
                if (targetCharacter.state == State.Dead)
                    if (!AutoSelectTarget())
                    {
                        SetState(State.Idle);
                        break;
                    }

                if (weaponsType == Weapons.WeaponsType.Bat)
                    animator.SetTrigger(AnimatorAttack);
                if (weaponsType == Weapons.WeaponsType.None)
                    animator.SetTrigger(AnimatorAttackArm);
                break;
            case State.BeginShot:
                if (targetCharacter.state == State.Dead)
                    if (!AutoSelectTarget())
                    {
                        SetState(State.Idle);
                        break;
                    }

                startRotation = transform.rotation;
                RotateToTarget(target.position);
                animator.SetTrigger(AnimatorShot);
                break;

            case State.Dead:
                animator.SetTrigger(AnimatorDead);
                
                break;
        }
    }


    public void RotateToTarget(Vector3 targetPosition)
    {
        transform.LookAt(targetPosition);
    }


    public bool RunToTowards(Vector3 targetPosition, float distanceFromTarget)
    {
        // print($"{distanceFromTarget}");
        Vector3 selfPosition = transform.position;
        // считаем  вектор до цели
        Vector3 distance = targetPosition - selfPosition;
        Vector3 direction = distance.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        //     transform.LookAt(targetPosition);

        // цель с учётом отступа от неё
        targetPosition -= direction * distanceFromTarget;

        // вектор перемещения
        Vector3 vector = direction * runSpeed;
        // скоректированный вектор до цели
        distance = (targetPosition - selfPosition);

        // проверяем через квадрат дистанции - дошли или нет 
        if (vector.sqrMagnitude < distance.sqrMagnitude)
        {
            transform.position += vector;
            return false;
        }

        transform.position = targetPosition;
        return true;
    }

    private Character SearchTarget(CharacterType type)
    {
        Character[] chars = GameObject.FindObjectsOfType<Character>();
        foreach (var character in chars)
        {
            // конкретный тип
            if (character.type == type && character.state != State.Dead && character != this)
            {
                return character;
            }
            else
                // любой живой
            if (type == CharacterType.None && character.state != State.Dead && character != this)
                return character;
        }

        return null;
    }


    private void InitWeaponCharacter()
    {
        weaponsController = FindObjectOfType<WeaponsController>();
        if (weaponsController == null)
            throw new ApplicationException("Не найден нужный класс WeaponsController");

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.CompareTag(TagWeaponHand))
            {
                weaponHand = child.gameObject;
                break;
            }
        }

        if (weaponHand == null)
            throw new ApplicationException("Не найдена рука для оружия");
    }


    [ContextMenu("Attack")]
    public void Attack()
    {
        if (state != State.Idle) return;

        if (target == null)
        {
            print($"{name} не задана цель!");
            return;
        }

        switch (weaponsType)
        {
            case Weapons.WeaponsType.Bat:
            case Weapons.WeaponsType.None:
                SetState(State.RunningToEnemy);
                break;
            case Weapons.WeaponsType.Pistol:
                SetState(State.BeginShot);
                break;
        }
    }

    [ContextMenu("Target/Zombie")]
    public bool SetZombieTarget()
    {
        return SetTarget(SearchTarget(CharacterType.Zombie));
    }

    [ContextMenu("Target/PoliceMan")]
    public bool SetPoliceManTarget()
    {
        return SetTarget(SearchTarget(CharacterType.PoliceMan));
    }

    [ContextMenu("Target/Hooligan")]
    public bool SetHooliganTarget()
    {
        return SetTarget(SearchTarget(CharacterType.Hooligan));
    }

    [ContextMenu("Target/Find Any Target")]
    public bool SetAnyTarget()
    {
        return SetTarget(SearchTarget(CharacterType.None));
    }

    [ContextMenu("Weapon/Pistol")]
    public void SetPistolWeapon()
    {
        SetWeapon(Weapons.WeaponsType.Pistol);
    }

    [ContextMenu("Weapon/Bat")]
    public void SetBatWeapon()
    {
        SetWeapon(Weapons.WeaponsType.Bat);
    }

    [ContextMenu("Weapon/None")]
    public void SetNoWeapon()
    {
        SetWeapon(Weapons.WeaponsType.None);
    }

    [ContextMenu("Бойня !")]
    public void BeginWar()
    {
        StartCoroutine(War());
    }

    public IEnumerator War()
    {
        Character[] listChar = GameObject.FindObjectsOfType<Character>();
        var countLife = listChar.Length;

        while (countLife > 1)
        {
            countLife = 0;
            foreach (Character character in listChar)
            {
                if (character.state == State.Idle)
                {
                    character.Attack();
                    yield return new WaitForFixedUpdate();
                }
                yield return new WaitForFixedUpdate();
                if (character.state != State.Dead) countLife++;
                
            }
        }

        yield break;
    }
}