﻿using AbstractBehaviour;
using MovementEffects;
using PlayerBehaviour;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VotanInterfaces;
using VotanLibraries;

namespace EnemyBehaviour
{
    /// <summary>
    /// Движение врага.
    /// По земле.
    /// </summary>
    public class EnemyMove
        : MonoBehaviour, IAIMoving
    {
        #region Переменные
        [SerializeField, Tooltip("Частота обновления позиции врага"),Range(0.1f,3)]
        protected float frequencySearching;
        [SerializeField, Tooltip("Частота интерполяции и отдыха"), Range(0.01f, 3)]
        protected float frequencyResting;
        [SerializeField, Tooltip("Скорость врага"), Range(1, 5)]
        protected float agentSpeed;
        [SerializeField, Tooltip("Коэффициент анимации при движении"), Range(0.5f,5)]
        protected float speedCoefficient;
        [SerializeField, Tooltip("Коэффициент поворота при остановке"), Range(1, 10)]
        protected float angularBooster;
        [SerializeField, Tooltip("Враг никогда не останавливается?")]
        protected bool isAlwaysMoving;
        [SerializeField, Tooltip("Предупредительная дистанция для атаки"), Range(0,100)]
        protected float preDistanceForAttack;
        [SerializeField, Tooltip("Враг")]
        protected AbstractEnemy abstractEnemy;
        [SerializeField, Tooltip("Модель врага")]
        protected Transform modelEnemy;

        protected float radiusForRandomWalk;
        protected float rotationSpeed;
        protected float angularLookSpeed;

        protected bool isStopped;
        protected bool isLookingAtPlayer;

        protected Quaternion lerpRotationQuar;

        protected Vector3 currentVector;
        protected Vector3 tempVector;
        protected Vector3 randomPosition;

        protected EnemyConditions enemyConditions;
        protected EnemyAttack enemyAttack;
        protected EnemyAnimationsController enemyAnimationsController;
        protected NavMeshAgent agent;

        protected Transform playerObjectTransformForFollow;
        protected PlayerConditions playerConditionsComponent;
        protected PlayerCollision playerCollisionComponent;
        private float tempSpeed;
        #endregion

        #region Свойства
        public float RadiusForRandomWalk
        {
            get
            {
                return radiusForRandomWalk;
            }

            set
            {
                radiusForRandomWalk = value;
            }
        }

        public float AgentSpeed
        {
            get
            {
                return agentSpeed;
            }

            set
            {
                agentSpeed = value;
            }
        }

        public float RotationSpeed
        {
            get
            {
                return rotationSpeed;
            }

            set
            {
                rotationSpeed = value;
            }
        }

        public bool IsStopped
        {
            get
            {
                return isStopped;
            }

            set
            {
                isStopped = value;
            }
        }

        public Transform PlayerObjectTransformForFollow
        {
            get
            {
                return playerObjectTransformForFollow;
            }

            set
            {
                playerObjectTransformForFollow = value;
                if (!playerObjectTransformForFollow)
                {
                    isStopped = true;
                    enemyAnimationsController.DisableAllStates();
                }
            }
        }

        public NavMeshAgent Agent
        {
            get
            {
                return agent;
            }

            set
            {
                agent = value;
            }
        }

        public float PreDistanceForAttack
        {
            get
            {
                return preDistanceForAttack;
            }

            set
            {
                preDistanceForAttack = value;
            }
        }

        public void EnableMayableLookAtPlayer()
        {
            isLookingAtPlayer = true;
        }

        public void DisableMayableLookAtPlayer()
        {
            isLookingAtPlayer = false;
        }

        public void EnableAgent()
        {
            isLookingAtPlayer = true;
            agent.enabled = true;
        }

        public void DisableAgent()
        {
            agent.enabled = false;
        }

        public bool IsLookingAtPlayer
        {
            get
            {
                return isLookingAtPlayer;
            }
            set
            {
                isLookingAtPlayer = value;
            }
        }

        public PlayerCollision PlayerCollisionComponent
        {
            get
            {
                return playerCollisionComponent;
            }

            set
            {
                playerCollisionComponent = value;
            }
        }
        #endregion

        /// <summary>
        /// Задаем скорость
        /// </summary>
        private void RandomSpeedSet()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            agent.speed = LibraryStaticFunctions.GetRangeValue(agentSpeed, 0.2f);
            agentSpeed = agent.speed;
            rotationSpeed = agent.angularSpeed;
        }

        /// <summary>
        /// Зависимость скорости воспроизведения 
        /// анимации врага от скорости его движения
        /// </summary>
        public void DependenceAnimatorSpeedOfVelocity(bool ourValue=false)
        {
            if (!isStopped)
            {
                currentVector = transform.position;
                if (ourValue)
                {
                    tempSpeed = Vector3.Distance
                        (currentVector, tempVector) / 2;
                    if (tempSpeed > 1)
                        enemyAnimationsController.HighSpeedAnimation();
                    else if (tempSpeed < 0.1f)
                        enemyAnimationsController.SetSpeedAnimationByRunSpeed(0.1f);
                    else
                        enemyAnimationsController.SetSpeedAnimationByRunSpeed(tempSpeed);
                }
                else
                {
                    tempSpeed = Vector3.Distance
                        (currentVector, tempVector) * speedCoefficient;
                    if (tempSpeed > 1)
                        enemyAnimationsController.HighSpeedAnimation();
                    else if (tempSpeed < 0.1f)
                        enemyAnimationsController.SetSpeedAnimationByRunSpeed(0.1f);
                    else
                        enemyAnimationsController.SetSpeedAnimationByRunSpeed(tempSpeed);
                }
                tempVector = currentVector;
            }
        }

        /// <summary>
        /// Проверить, достиг ли враг точки прибытия
        /// </summary>
        /// <returns></returns>
        public bool CheckStopped(bool isRandom = false)
        {
            if (isRandom)
            {
                randomPosition.y = transform.position.y;
                isStopped = Vector3.Distance(transform.position, randomPosition)
                    <= agent.stoppingDistance ? true : false;
                if (!isStopped)
                {
                    EnableAgent();
                }
            }
            else
            {
                isStopped = Vector3.Distance
                    (transform.position, playerObjectTransformForFollow.position)
                    <= agent.stoppingDistance + preDistanceForAttack ? true : false;

                // если не остановились - включаем агент
                if (!isStopped && playerObjectTransformForFollow)
                {
                    EnableAgent();
                }
            }
            return isStopped;
        }


        /// <summary>
        /// Установить новую скорость для агента
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="newRotSpeed"></param>
        public void SetNewSpeedOfNavMeshAgent(float newValue,float newRotSpeed=0)
        {
            if (!enemyConditions.IsAlive) return;
            agent.speed = newValue;
            if (newRotSpeed != 0)
                agent.angularSpeed = newRotSpeed;
        }

        /// <summary>
        /// Получить игрока и его компонент
        /// </summary>
        public bool GetPlayerAndComponent()
        {
            playerObjectTransformForFollow = 
                abstractEnemy.EnemyOpponentChoiser.GetRandomTransformOfPlayer();

            if (playerObjectTransformForFollow != null)
            {
                PlayerComponentsControl pCC = playerObjectTransformForFollow.
                    GetComponent<PlayerComponentsControl>();

                enemyAttack.InFightMode = true;
                enemyAttack.PlayerTarget = pCC.PlayerAttack;

                playerConditionsComponent = pCC.PlayerConditions;
                playerCollisionComponent = pCC.PlayerCollision;
                playerObjectTransformForFollow = pCC.PlayerModel;

                return true;
            }
            else
            {
                enemyAttack.InFightMode = false;
                enemyAnimationsController.DisableAllStates();
                return false;
            }
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public void Start()
        {
            enemyAttack = abstractEnemy.EnemyAttack;
            enemyConditions = abstractEnemy.EnemyConditions;
            enemyAnimationsController = abstractEnemy.EnemyAnimationsController;

            agent = GetComponent<NavMeshAgent>();
            angularLookSpeed = Time.deltaTime * agent.angularSpeed/100;
            randomPosition.y = 3;
        }

        /// <summary>
        /// Рестартировать компонент врага, отвечаюшщий за его движение
        /// </summary>
        public virtual void RestartEnemyMove()
        {
            if (enemyConditions == null
                || enemyAttack == null
                    || enemyAnimationsController == null)
            {
                enemyAttack = abstractEnemy.EnemyAttack;
                enemyConditions = abstractEnemy.EnemyConditions;
                enemyAnimationsController = abstractEnemy.EnemyAnimationsController;
            }

            GetPlayerAndComponent();
            RandomSpeedSet();
            tempVector = transform.position;
            Agent.enabled = true;
            isLookingAtPlayer = true;

            Timing.RunCoroutine(CoroutineForSearchingByPlayerObject());
            Timing.RunCoroutine(CoroutineForRotating());
        }

        /// <summary>
        /// Обнулять поворот модели
        /// </summary>
        private void LookAtNullRotation()
        {
            lerpRotationQuar.y = 0;
        }

        /// <summary>
        /// Поворот
        /// </summary>
        public void LookAtPlayerObject()
        {
            if (!enemyConditions.IsFrozen)
            {
                lerpRotationQuar = Quaternion.LookRotation
                (playerObjectTransformForFollow.position - transform.position);
                lerpRotationQuar.z = 0;
                lerpRotationQuar.x = 0;
            }
        }

        /// <summary>
        /// Установить случайную позицию для врага во время отдыха
        /// </summary>
        public void SetRandomPosition()
        {
            if (agent != null)
            {
                if (!agent.enabled) agent.enabled = true;
            }
            else
            {
                return;
            }

            agent.speed = agentSpeed;
            enemyAnimationsController.DisableAllStates();
            enemyAnimationsController.SetState(0, true);
            enemyAttack.IsMayToPlayAttackAnimation = false;
            enemyAnimationsController.SetSpeedAnimationByRunSpeed(agentSpeed / 3);

            randomPosition.x = LibraryStaticFunctions.
                GetPlusMinusValue(radiusForRandomWalk);
            randomPosition.z = LibraryStaticFunctions.GetPlusMinusValue
                (radiusForRandomWalk - Math.Abs(randomPosition.x));

            agent.SetDestination(randomPosition);
        }

        /// <summary>
        /// Поворот
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<float> CoroutineForRotating()
        {
            while (enemyConditions.IsAlive)
            {
                if (PlayerObjectTransformForFollow != null)
                {
                    /* если мы остановились, не заморожены и можем
                    смотреть на игрока */
                    if (isStopped
                        && !enemyConditions.IsFrozen
                        && isLookingAtPlayer)
                    {
                        transform.rotation =
                        Quaternion.Lerp(transform.rotation
                            , lerpRotationQuar, Time.deltaTime * angularBooster);
                        yield return Timing.WaitForOneFrame;
                    }
                    else if (modelEnemy.localEulerAngles.y != 0)
                    {
                        modelEnemy.localRotation =
                        Quaternion.Lerp(modelEnemy.localRotation
                            , Quaternion.identity, Time.deltaTime * angularBooster);
                        yield return Timing.WaitForOneFrame;
                    }
                    else
                    {
                        yield return Timing.WaitForSeconds(frequencyResting);
                    }
                }
                else
                {
                    CheckStopped(true);
                    if (modelEnemy.localEulerAngles.y != 0)
                    {
                        modelEnemy.localRotation =
                        Quaternion.Lerp(modelEnemy.localRotation
                            , Quaternion.identity, Time.deltaTime * angularBooster);
                        yield return Timing.WaitForOneFrame;
                    }
                    else
                    {
                        yield return Timing.WaitForSeconds(frequencyResting);
                    }
                }
            }
        }

        /// <summary>
        /// Движение противника. Корутина.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<float> CoroutineForSearchingByPlayerObject()
        {
            while (enemyConditions.IsAlive)
            {
                if (agent == null) yield break; // если враг удален. но в этом нет необходимости

                if (playerObjectTransformForFollow)
                {
                    LookAtNullRotation(); // корректировка позиции
                    CheckStopped(); // проверить не остановились ли мы
                    if (playerConditionsComponent.IsAlive)
                    {
                        if (isStopped && isLookingAtPlayer)
                        {
                            if (!isAlwaysMoving) agent.speed = 0;
                            LookAtPlayerObject();
                        }
                        else
                        {
                            agent.speed = agentSpeed;
                        }

                        // идем за игроком
                        if (!enemyConditions.IsFrozen
                            && agent.enabled)
                        {
                            if (!isAlwaysMoving)
                            {
                                if (!enemyAttack.IsMayToDamage)
                                {
                                    agent.SetDestination
                                            (playerObjectTransformForFollow.position);
                                }
                            }
                            else
                            {
                                agent.SetDestination
                                    (playerObjectTransformForFollow.position);
                            }
                        }
                        yield return Timing.WaitForSeconds(frequencySearching);
                    }
                    else
                    {
                        agent.speed = agentSpeed;
                        if (!GetPlayerAndComponent())
                        {
                            SetRandomPosition();
                            yield return Timing.WaitForSeconds
                                (UnityEngine.Random.Range(0, 1f)
                                * 10 + 10);
                        }
                        else
                        {
                            yield return Timing.WaitForSeconds(frequencySearching);
                        }
                    }
                }
                else
                {
                    SetRandomPosition();
                    CheckStopped(true);
                    yield return Timing.WaitForSeconds
                        (UnityEngine.Random.Range(0, 1f)
                        * 10 + 10);
                }
            }
            isStopped = true;
        }
    }
}
