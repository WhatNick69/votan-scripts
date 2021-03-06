﻿using AbstractBehaviour;
using MovementEffects;
using PlayerBehaviour;
using System.Collections.Generic;
using UnityEngine;
using VotanInterfaces;
using VotanLibraries;
using System;

namespace GameBehaviour
{
    /// <summary>
    /// Менеджер ледяного эффекта
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class IceEffectManager
        : MonoBehaviour, IIceEffect
    {
        #region Переменные
        [SerializeField,Tooltip("Враг")]
        private AbstractEnemy abstractEnemy;
        [SerializeField, Tooltip("Враг является боссом?")]
        private bool isBoss;
        [SerializeField, Tooltip("Босья сила сокращения эффекта"), Range(0, 1f)]
        private float bossEffectDecreaseMultiplier;
        [SerializeField, Tooltip("Разброс ледяных объектов"), Range(0.5f, 2f)]
        private float iceObjectsDistanceBetweenOther;
        [SerializeField, Tooltip("Лист ледяных мешей")]
        private Transform[] listIceObjects;
        [SerializeField, Tooltip("Лист трэилов")]
        private Transform[] listTrailObjects;
        [SerializeField, Tooltip("Материал ледяных глыб")]
        private Material iceObjectMaterial;
        [SerializeField, Tooltip("Громкость льда")]
        private float iceSoundVolume;
        private AudioSource audioSource;
        private IWeapon weapon;

        private List<Vector3> listPositionIceObjects;
        private List<Vector3> listPositionTrailObjects;

        private bool isRandomTrailPosition;
        private float timeToDisable;

        private bool isOneCoroutine;
        private float damage;
        private bool isActiveSound;
        #endregion

        #region Свойства
        public float TimeToDisable
        {
            get
            {
                return timeToDisable;
            }

            set
            {
                timeToDisable = value;
            }
        }
        #endregion

        /// <summary>
        /// Инициализация
        /// </summary>
        public void Start()
        {
            audioSource = 
                GetComponent<AudioSource>();
        }

        /// <summary>
        /// Зажечь событие для ледяного эффекта.
        /// Ледяные объекты возникают под игровым объектом из под земли
        /// поднимаясь снизу вверх. Кончики ледяных объектов блестят, при помощи
        /// трэилов. Затем, по истечению определенного времени, эти ледяные глыбы
        /// вместе с трэилами исчезают, посредством сведения к нулю их альфа-канала.
        /// </summary>
        public float EventEffect(float damage,IWeapon weapon)
        {
            weapon.GetPlayer.PlayerCameraSmooth.
                DoNoize((weapon.SpinSpeed / weapon.OriginalSpinSpeed)+0.5f);

            if (!isOneCoroutine)
            {
                this.weapon = weapon;
                this.damage = LibraryStaticFunctions.IceDamagePerPeriod(damage, weapon);
                timeToDisable = LibraryStaticFunctions.TimeToFreezy(weapon.GemPower);

                if (isBoss)
                    timeToDisable = timeToDisable*(1-bossEffectDecreaseMultiplier) - 0.5f;
                else
                    timeToDisable = timeToDisable - 0.5f;

                isOneCoroutine = true;
                SetColorOfMaterial();
                SetActiveForAudioSource(true, true);
                SetActiveForTrailObjects(true);
                SetActiveForIceObjects(true);
                InitialisationVariables();
                RandomSetScaleAndPosition();
                RunAllCoroutines();
            }
            return timeToDisable + 0.5f;
        }

        /// <summary>
        /// Велючение. либо отключение ледяного эффекта
        /// </summary>
        /// <param name="flag"></param>
        private void SetActiveForAudioSource(bool isStart,bool active)
        {
            audioSource.enabled = active;
            if (active)
            {
                if (isStart)
                {
                    isActiveSound = true;
                    audioSource.loop = false;
                    AbstractSoundStorage.PlayIceAudio
                        (isStart, iceSoundVolume / 2.5f, audioSource);
                    audioSource.loop = false;
                }
                else if (isActiveSound)
                {
                    audioSource.loop = true;
                    AbstractSoundStorage.PlayIceAudio
                        (isStart, iceSoundVolume, audioSource);
                    audioSource.loop = true;
                }
            }
            else
            {
                isActiveSound = false;
            }
        }

        /// <summary>
        /// Инициализация переменных
        /// </summary>
        private void InitialisationVariables()
        {
            listPositionIceObjects = new List<Vector3>();
            listPositionTrailObjects = new List<Vector3>();
            isRandomTrailPosition = true;
        }

        /// <summary>
        /// Запуск всех корутин
        /// </summary>
        private void RunAllCoroutines()
        {
            Timing.RunCoroutine(CoroutineForGetDamagePerPeriod());
            Timing.RunCoroutine(CoroutineForFireDisableIceObjects());
            Timing.RunCoroutine(CoroutineForMoveIceObjects());
            Timing.RunCoroutine(CoroutineSetRandomPositionForTrailIce());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineForGetDamagePerPeriod()
        {
            int i = 0;
            int maxI = Convert.ToInt32(timeToDisable / 0.25f);

            while (i < maxI)
            {
                if (!abstractEnemy ||
                    abstractEnemy.EnemyConditions.HealthValue <= 0) yield break;
                abstractEnemy.EnemyConditions.HealthValue -=
                    LibraryStaticFunctions.GetRangeValue(damage, 0.05f);
                if (abstractEnemy.EnemyConditions.HealthValue <= 0)
                {
                    abstractEnemy.ScoreAddingEffect.EventEffect(weapon);
                    yield break;
                }
                yield return Timing.WaitForSeconds(0.25f);
                i++;
            }
        }

        /// <summary>
        /// Установить цвет материала льда, исходя из силы гема
        /// </summary>
        /// <param name="weapon"></param>
        private void SetColorOfMaterial()
        {
            Color color = weapon.TrailRenderer.endColor;
            color.a = 0.45f;
            iceObjectMaterial.color = color;
        }
    
        /// <summary>
        /// Сбросить изменения трансформа
        /// </summary>
        /// <param name="flag"></param>
        private void SetActiveForIceObjects(bool flag)
        {
            foreach (Transform iceObject in listIceObjects)
            {
                iceObject.localPosition = Vector3.zero;
                iceObject.localScale = new Vector3(1,1,1);
                iceObject.gameObject.SetActive(flag);
            }
        }

        /// <summary>
        /// Активировать трэилы
        /// </summary>
        /// <param name="flag"></param>
        private void SetActiveForTrailObjects(bool flag)
        {
            foreach (Transform trailObject in listTrailObjects)
                trailObject.gameObject.SetActive(flag);
        }

        /// <summary>
        /// Устанавливаем случайное положение ледяных объектов, 
        /// их размер, поворот.
        /// Затем кешируем позиции объектов
        /// </summary>
        private void RandomSetScaleAndPosition()
        {
            float scale = 0;
            for (int i = 0; i < listIceObjects.Length; i++)
            {
                // Позиция
                listIceObjects[i].position =
                    new Vector3(listIceObjects[i].position.x +
                        LibraryStaticFunctions.GetPlusMinusValue
                        (iceObjectsDistanceBetweenOther), 
                    listIceObjects[i].position.y + 0,
                    listIceObjects[i].position.z + 
                        LibraryStaticFunctions.GetPlusMinusValue
                        (iceObjectsDistanceBetweenOther));

                // Размер
                scale = LibraryStaticFunctions.GetRangeValue(1+(weapon.GemPower/200),0.2f);
                listIceObjects[i].localScale = new Vector3(scale,scale, scale);

                // Поворот
                listIceObjects[i].rotation = Quaternion.Euler
                    (0, UnityEngine.Random.Range(0,360), 0);

                // Кеширование позиции
                listPositionIceObjects.Add(new Vector3(listIceObjects[i].position.x,
                    listIceObjects[i].position.y +
                     UnityEngine.Random.Range(0,1f) / 4+1.75f,
                    listIceObjects[i].position.z));
            }
        }

        /// <summary>
        /// Корутина для движения 
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineForMoveIceObjects()
        {
            int i = 0;
            while (i < 25)
            {
                for (int j = 0; j < listIceObjects.Length; j++)
                {
                    if (this == null) yield break;

                    listIceObjects[j].position = Vector3.Lerp(listIceObjects[j].position,
                        listPositionIceObjects[j], 0.3f);
                }
                yield return Timing.WaitForSeconds(0.05f);
                i++;
            }
            SetActiveForAudioSource(false,true);
        }

        /// <summary>
        /// Корутина для установления случайной позиции для трэила льда
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineSetRandomPositionForTrailIce()
        {
            // Лист позиций с ледяными трэилами
            foreach (Transform iceTrailPos in listTrailObjects)
                listPositionTrailObjects.Add(iceTrailPos.localPosition);

            while (isRandomTrailPosition)
            {
                for (int i = 0; i < listTrailObjects.Length; i++)
                {
                    if (this == null) yield break;
                    listTrailObjects[i].localPosition =
                        new Vector3(LibraryStaticFunctions.
                            GetRangeValue(listPositionTrailObjects[i].x, 0.05f),
                        LibraryStaticFunctions.
                            GetRangeValue(listPositionTrailObjects[i].y, 0.05f),
                        LibraryStaticFunctions.
                            GetRangeValue(listPositionTrailObjects[i].z, 0.05f));
                }
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Корутина для отключения ледяного эффекта
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineForFireDisableIceObjects()
        {
            yield return Timing.WaitForSeconds(timeToDisable);
            isRandomTrailPosition = false;

            if (this == null) yield break;
            Timing.RunCoroutine(CoroutineDisableIceObjects());
        }

        /// <summary>
        /// Корутина на выключение эффекта
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineDisableIceObjects()
        {
            SetActiveForTrailObjects(false);
            Vector3 tempVector = new Vector3(0, 2, 0);
            for (int j = 0; j < listPositionIceObjects.Count; j++)
                listPositionIceObjects[j] -= tempVector;

            int i = 0;
            Vector3 localScaleTemp = new Vector3(0.5f,0.5f,0.5f);
            while (i < 15)
            {
                for (int j = 0; j < listIceObjects.Length; j++)
                {
                    if (this == null) yield break;

                    listIceObjects[j].position = 
                        Vector3.Lerp(listIceObjects[j].position,
                        listPositionIceObjects[j], 0.3f);
                   listIceObjects[j].localScale = 
                       Vector3.Lerp(listIceObjects[j].localScale, localScaleTemp, 0.1f);
                }
                yield return Timing.WaitForSeconds(0.05f);
                i++;
            }
            SetActiveForAudioSource(false, false);
            SetActiveForIceObjects(false);
            isOneCoroutine = false;
            timeToDisable = 0;
        }
    }
}