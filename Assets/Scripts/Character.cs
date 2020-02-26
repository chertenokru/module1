using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;


public class Character : MonoBehaviour, ISelectable
{
    public enum State
    {
        Idle,
        BeginAttack, // близкая атака
        Attack,
        BeginShot, // дист аттака
        Shot,
        Dead,
        Run
    }


    public enum CharacterType
    {
        [Description("Нет")]
        None,
        [Description("Полицейский")]
        PoliceMan,
        [Description("Хулиганьё")]
        Hooligan,
        [Description("Зомби")]
        Zombie,
        [Description("Женщина")]
        Woman,
        [Description("Мужик")]
        Man
    }

    private const string ANIMATOR_FIELD_SPEED = "speed";
    private const string ANIMATOR_ATTACK = "attack";
    private const string ANIMATOR_ATTACK_ARM = "attack arm";
    private const string ANIMATOR_SHOT = "shot";
    private const string ANIMATOR_DEAD = "dead";
    private const string TAG_WEAPON_HAND = "Hand";
    private bool isWarMode = false;
    private Outline outline;
    public NavMeshAgent navMeshAgent;
    public const string TAG_CHARACTER = "Character";


    private State state;
    //todo переделать на id - придумать как хранить
    // private const int AnimatorFieldSpeed = Animator.StringToHash("speed");

    [SerializeField] private Transform target;
    [SerializeField] private Weapons.WeaponsType weaponsType = Weapons.WeaponsType.None;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Animator animator;
    private Character targetCharacter;
    private WeaponsController weaponsController;
    private GameObject weaponHand;

    private HealthBar healthBar;

    public CharacterType type = CharacterType.None;
    public float runSpeed = 0.05f;
    public float distanceFromEnemy = 1.2f;
    public int health = 4;
    public int maxHealth = 4;
    public float distanceCurrentMove = 3.0f;
    public float distanceMaxMove = 3.0f;

    private NavMeshPath path;

    // залипуха временная
    public RuntimeAnimatorController danceAnimatorController;

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
        path = new NavMeshPath();
        // подставляем нужное оружие в модель
        InitWeaponCharacter();
        healthBar = GetComponentInChildren<HealthBar>();
        animator = GetComponentInChildren<Animator>();
        outline = GetComponentInChildren<Outline>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }


    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent.updateRotation = false;

        healthBar.SetMaxHealth(maxHealth);
        state = State.Idle;

        startPosition = transform.position;
        startRotation = transform.rotation;

        SetHealth(health);
        if (target == null) AutoSelectTarget();
        else SetTarget(target);
        SetWeapon(weaponsType);
    }

    // ищет случайную цель
    private bool AutoSelectTarget()
    {
        Character tempCharacter;
        switch (type)
        {
            case CharacterType.PoliceMan:
                tempCharacter = SearchTarget(CharacterType.Hooligan);
                if (tempCharacter == null) tempCharacter = SearchTarget(CharacterType.Zombie);
                if (tempCharacter == null) tempCharacter = SearchTarget(CharacterType.PoliceMan);
                return SetTarget(tempCharacter);
            case CharacterType.Hooligan:
                tempCharacter = SearchTarget(CharacterType.PoliceMan);
                if (tempCharacter == null) tempCharacter = SearchTarget(CharacterType.Zombie);
                if (tempCharacter == null) tempCharacter = SearchTarget(CharacterType.Hooligan);
                return SetTarget(tempCharacter);
            case CharacterType.Zombie:
                tempCharacter = SearchTarget(CharacterType.PoliceMan);
                if (tempCharacter == null) tempCharacter = SearchTarget(CharacterType.Hooligan);
                if (tempCharacter == null) tempCharacter = SearchTarget(CharacterType.Zombie);
                return SetTarget(tempCharacter);
        }

        return false;
    }


    void FixedUpdate()
    {

        //if (navMeshAgent.velocity != Vector3.zero)
        
        
        // статусы кроме анимации
        switch (state)
        {
            case State.Idle:
             //   transform.rotation = startRotation;
          //   animator.SetFloat(ANIMATOR_FIELD_SPEED, (navMeshAgent.velocity != Vector3.zero) ? 1 : 0);
                break;
            case State.Run:
                animator.SetFloat(ANIMATOR_FIELD_SPEED, (navMeshAgent.velocity != Vector3.zero) ? 1 : 0);
                if ((!navMeshAgent.hasPath) ||(navMeshAgent.hasPath && navMeshAgent.remainingDistance < 0.6f && navMeshAgent.velocity.sqrMagnitude < 0.05f   ))
                {
                    navMeshAgent.ResetPath();
                    SetState(State.Idle);
                } else
                    transform.rotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
                break;

/*            case State.RunningToEnemy:
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
                    SetState(State.RunningFromEnemy);
                }

                break;
 */
            case State.Shot:
            case State.Attack:
                if (targetCharacter != null)
                {
                    transform.rotation = startRotation;
                }

                SetState(State.Idle);
                break;
        }
    }

    /// подменяет mesh оружия и заменяет ущерб  
    private void SetWeapon(Weapons.WeaponsType value)
    {
        
        bool stateOutline = outline.enabled;
        if (outline.enabled) outline.enabled = false;
        weaponsType = value;
        if (weaponHand.transform.childCount > 0)
        {
            //Destroy(weaponHand.transform.GetChild(0).gameObject);
            weaponHand.transform.GetChild(0).gameObject.SetActive(false);
        }

        GameObject mesh = weaponsController.GetMeshWeapont(weaponsType);
        if (mesh != null)
        {
            GameObject obj = Instantiate(mesh, weaponHand.transform);
        }

        // изменились меши - перекэшируем
        outline.UpdateMeshRenders();

        if (outline.enabled != stateOutline) outline.enabled = stateOutline;
    }

    // наночит урон и если труп то возвращает да
    public bool SetDamage(int damage)
    {
        if (state == State.Dead) return true;
        SetHealth(health - damage);
        if (health <= 0)
        {
            SetHealth(0);
            SetState(State.Idle);
            SetState(State.Dead);
            return true;
        }

        return false;
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
        healthBar.SetHealth(health);
    }


    // устанавливает цель, принимает Character 
    public bool SetTarget(Character newTargetCharacter)
    {
        if (newTargetCharacter == null)
            return SetTarget((Transform) null);
        else return SetTarget(newTargetCharacter.transform);
    }

    // устанавливает цель, принимает transform
    public bool SetTarget(Transform newTargetTransform)
    {
        if (newTargetTransform == null)
        {
            return false;
        }

        target = newTargetTransform;
        targetCharacter = target.gameObject.GetComponent<Character>();
        return true;
    }

    // установка статусов
    public void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        //animator setting    
        switch (state)
        {
            //     case State.Run:
               // animator.SetFloat(ANIMATOR_FIELD_SPEED, 1.0f);
              //  break;

            case State.Idle:
                animator.SetFloat(ANIMATOR_FIELD_SPEED, 0.0f);
                break;

            case State.BeginAttack:
                if (targetCharacter.state == State.Dead)
                {
                    SetState(State.Idle);
                    break;
                }

                switch (weaponsType)
                {
                    case Weapons.WeaponsType.Bat:
                        animator.SetTrigger(ANIMATOR_ATTACK);
                        break;
                    case Weapons.WeaponsType.None:
                        animator.SetTrigger(ANIMATOR_ATTACK_ARM);
                        break;
                    default:
                        animator.SetTrigger(ANIMATOR_ATTACK_ARM);
                        break;
                }

                break;
            case State.BeginShot:
                if (targetCharacter.state == State.Dead)

                    startRotation = transform.rotation;
                RotateToTarget(target.position);
                animator.SetTrigger(ANIMATOR_SHOT);
                break;

            case State.Dead:
                healthBar.gameObject.SetActive(false);
                animator.SetTrigger(ANIMATOR_DEAD);
                break;
        }
    }


    public void RotateToTarget(Vector3 targetPosition)
    {
        transform.LookAt(targetPosition);
    }


    public bool RunToTowards(Vector3 targetPosition, float distanceFromTarget)
    {
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

// поиск цели по типу, если находит, то возвращает цель
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

//Начальная настройка оружия 
    private void InitWeaponCharacter()
    {
// контроллер оружия
        weaponsController = FindObjectOfType<WeaponsController>();
        if (weaponsController == null)
            throw new ApplicationException("Не найден нужный класс WeaponsController");
        // ищем руку для оружия, по метке
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.CompareTag(TAG_WEAPON_HAND))
            {
                weaponHand = child.gameObject;
                break;
            }
        }

        if (weaponHand == null)
            throw new ApplicationException(
                $"Не найдена контейнер в руке для оружия, пометьте его тэгом {TAG_WEAPON_HAND}");
    }


    [ContextMenu("Attack")]
    public void Attack()
    {
        if (state != State.Idle) return;
        if (target == null)
        {
            return;
        }

        if (weaponsController.isWeapontDistanceAttack(weaponsType))
        {
            SetState(State.BeginShot);
        }
        else
        {
            SetState(State.BeginAttack);
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

    [ContextMenu("Weapon/Knife")]
    public void SetKnifeWeapon()
    {
        SetWeapon(Weapons.WeaponsType.Knife);
    }

    [ContextMenu("Бойня !")]
    public void BeginWar()
    {
        StartCoroutine(War());
    }

//все против всех
    public IEnumerator War()
    {
        isWarMode = true;
        Character[] listChar = FindObjectsOfType<Character>();

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
    }

// вызывается тригером анимации в момент урона, анимация играется дальше
//  а программа наносит урон
    public void SetDamageEvent()
    {
// наносим удар и если враг мёртв, то ищем следующего
        if (targetCharacter.SetDamage(GetWeaponDamage())) AutoSelectTarget();
        // все здохли! а теперь - танцы !
        if (isWarMode && targetCharacter.state == State.Dead)
        {
            animator.avatar = null;
            animator.runtimeAnimatorController = danceAnimatorController;
        }
    }

    public int GetWeaponDamage()
    {
        return weaponsController.GetDamage(weaponsType);
    }

    public bool IsDead()
    {
        return state == State.Dead;
    }

    public bool IsIdle()
    {
        return state == State.Idle;
    }

    public void Win()
    {
        animator.avatar = null;
        animator.runtimeAnimatorController = danceAnimatorController;
    }

    private void SwitchOutline(bool setOn)
    {
        SwitchOutline(setOn, outline.OutlineColor, outline.OutlineWidth);
    }

    private void SwitchOutline(bool setOn, Color color, float with)
    {
        if (setOn == outline.enabled) return;
        if (setOn)
        {
            outline.OutlineColor = color;
            outline.OutlineWidth = with;
        }

        outline.enabled = setOn;
    }

    public void SwitchSelect(bool setOn)
    {
        SwitchOutline(setOn);
    }

    public void SwitchSelect(bool setOn, Color color, float with)
    {
        SwitchOutline(setOn, color, with);
    }

    public bool GetSelectStatus()
    {
        return outline.enabled;
    }

// вычисляет путь который может пройти игрок до указанной точки 
// возвращает длину этого пути и скорректированный navMeshPath
    public float GetAlowedPath(Vector3 point, NavMeshPath navMeshPath)
    {
// отходился
        if (distanceCurrentMove < float.Epsilon) return 0;
// если есть путь
        if (navMeshAgent.CalculatePath(point, navMeshPath))
        {
            // накапливаем длину исходного пути в квадратах
            float pathLenght = 0;
            Vector3 beginPoint = transform.position;

            foreach (Vector3 corner in navMeshPath.corners)
            {
                float d = Vector3.Distance(beginPoint, corner);
                if ((pathLenght + d) < distanceCurrentMove)
                    // ок, дальше
                {
                    pathLenght += d;
                    beginPoint = corner;
                }
                // многовато будет
                else
                {
                    //попробуем посчитать вектор последнего участка и умножить его на доступный остаток пути
                    Vector3 v = (corner - beginPoint);
                    beginPoint = beginPoint + v.normalized * (distanceCurrentMove - pathLenght);
                    navMeshAgent.CalculatePath(beginPoint, navMeshPath);
                    return distanceCurrentMove;
                }
            }


            /*
                Vector3 beginPoint = transform.position;
                float sqrDistance = distanceCurrentMove * distanceCurrentMove;
        
                foreach (Vector3 corner in navMeshPath.corners)
                {
                    float d = (beginPoint - corner).sqrMagnitude;
                    if ((pathLenght + d) < sqrDistance)
                        // ок, дальше
                    {
                        pathLenght += d;
                        beginPoint = corner;
                    }
                    // многовато будет
                    else
                    {
                        //попробуем посчитать вектор последнего участка и умножить его на доступный остаток пути
                        Vector3 v = (corner - beginPoint);
                        beginPoint = beginPoint + v.normalized * (float) Math.Sqrt(sqrDistance - pathLenght);
                        navMeshAgent.CalculatePath(beginPoint, navMeshPath);
                        return distanceCurrentMove;
                    }
        
                }
        
            */
            return (float) Math.Sqrt(pathLenght);
        }
        else return 0;
    }

// go!
    public void Move(Vector3 point)
    {
        float dist = GetAlowedPath(point, path);
        if (dist != 0.0f)
        {
            navMeshAgent.SetPath(path);
            SetState(State.Run);
            distanceCurrentMove -= dist;
        }
    }

    public float GetDistanceAttack()
    {
        return weaponsController.getWeapontDistanceAttack(weaponsType);
    }

    public void TurnReset()
    {
        distanceCurrentMove = distanceMaxMove;
    }
}