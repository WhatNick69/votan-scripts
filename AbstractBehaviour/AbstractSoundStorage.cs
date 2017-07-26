﻿using UnityEngine;
using VotanLibraries;

namespace AbstractBehaviour
{
    /// <summary>
    /// Общее звуковое хранилищ.
    /// Объекты могут его наследовать 
    /// и реализовывать абстрактные методы.
    /// </summary>
    public abstract class AbstractSoundStorage 
        : MonoBehaviour
    {
        #region Переменные и ссылки
        protected static AudioClip[] audioCutBody; // Звуки лязга колющего оружия
        protected static AudioClip[] audioHitBody; // Звуки лязка дробящего оружия
        protected static AudioClip[] audioCollisionMetalThings;
        protected static AudioClip[] audioIce;
        protected static AudioClip[] audioBodyFall;
        protected static AudioClip[] audioGameMusic;
        protected static AudioClip[] audioToBurn;
        protected static AudioClip[] audioBurning;
        protected AudioClip[] audioHitArmory; // Звуки лязка дробящего оружия

        [SerializeField, Tooltip("Звуковой компонент на ногах")]
        protected AudioSource audioSourceLegs;
        [SerializeField, Tooltip("Звуковой компонент на оружии")]
        protected AudioSource audioSourceWeapon;
        [SerializeField, Tooltip("Звуковой компонент на игроке")]
        protected AudioSource audioSourceObject;

        [SerializeField, Tooltip("Громкость шагов")]
        protected float volumeStep;
        [SerializeField, Tooltip("Громкость падения")]
        protected float volumeFall;
        [SerializeField, Tooltip("Громкость страданий")]
        protected float volumeHutred;
        [SerializeField, Tooltip("Громкость смерти")]
        protected float volumeDead;

        protected static object[] tempAudioList;
        #endregion

        #region Инициализация
        /// <summary>
        /// Инициализация первостепенных данных
        /// </summary>
        protected virtual void Start()
        {
            InitialisationCutToBodySounds();
            InitialisationHitToBodySounds();
        }

        /// <summary>
        /// Статическая инициализация
        /// </summary>
        public static void LoadAllStaticSounds()
        {
            InitialisationGameMusic();
            InitialisationSoundsForMetalThings();
            InitialisationSoundsIce();
            InitialisationSoundsBodyFall();
            InitialisationSoundsBurnAndBurning();
        }

        /// <summary>
        /// Инициализация звуков горения
        /// </summary>
        private static void InitialisationSoundsBurnAndBurning()
        {
            tempAudioList = Resources.LoadAll("Sounds/Effects/Fire/ToBurn");
            audioToBurn = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioToBurn[i] = (AudioClip)tempAudioList[i];
            }

            tempAudioList = Resources.LoadAll("Sounds/Effects/Fire/Burning");
            audioBurning = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioBurning[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация звуков падающего тела
        /// </summary>
        private static void InitialisationSoundsBodyFall()
        {
            tempAudioList = Resources.LoadAll("Sounds/Common/BodyFall");
            audioBodyFall = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioBodyFall[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация звуков металлических деталей
        /// </summary>
        private static void InitialisationSoundsForMetalThings()
        {
            tempAudioList = Resources.LoadAll("Sounds/Common/MetalThings");
            audioCollisionMetalThings = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioCollisionMetalThings[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация звукову льда
        /// </summary>
        private static void InitialisationSoundsIce()
        {
            tempAudioList = Resources.LoadAll("Sounds/Effects/Ice");
            audioIce = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioIce[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация лязга оружия режущего типа по мясу
        /// </summary>
        private void InitialisationCutToBodySounds()
        {
            tempAudioList = Resources.LoadAll("Sounds/Common/Weapon/CuttingMeat");
            audioCutBody = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioCutBody[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация лязга оружия дробящего типа
        /// </summary>
        private void InitialisationHitToBodySounds()
        {
            tempAudioList = Resources.LoadAll("Sounds/Common/Weapon/Crushing");
            audioHitBody = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioHitBody[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация музыки
        /// </summary>
        private static void InitialisationGameMusic()
        {
            tempAudioList = Resources.LoadAll("Sounds/Music");
            audioGameMusic = new AudioClip[tempAudioList.Length];
            for (int i = 0; i < tempAudioList.Length; i++)
            {
                audioGameMusic[i] = (AudioClip)tempAudioList[i];
            }
        }

        /// <summary>
        /// Инициализация звуков шагов
        /// </summary>
        protected abstract void InitialisationStepsSounds();

        /// <summary>
        ///  Инициализация звуков получения урона
        /// </summary>
        protected abstract void InitialisationHurtSounds();

        /// <summary>
        /// Инициализация звуков смерти
        /// </summary>
        protected abstract void InitialisationDeadSounds();
        #endregion

        #region Открытые методы
        /// <summary>
        /// Звук шагов
        /// </summary>
        public abstract void PlayStepAudio();

        /// <summary>
        /// Работа со звуком ледяного эффекта
        /// </summary>
        /// <param name="auSo"></param>
        public static void WorkWithIce(bool isStart, float volume, AudioSource auSo)
        {
            auSo.volume =
                LibraryStaticFunctions.GetRangeValue(volume, 0.1f);
            if (isStart)
            {
                auSo.clip = audioIce[0];
            }
            else
            {
                auSo.clip =
                    audioIce[LibraryStaticFunctions.rnd.
                    Next(1, audioIce.Length)];
            }
            auSo.pitch = LibraryStaticFunctions.GetRangeValue(1, 0.1f);
            auSo.Play();
        }

        /// <summary>
        /// Работа с воспроизведением звука у металлической вещи
        /// </summary>
        /// <param name="auSo"></param>
        public static void WorkWithMetalThing(AudioSource auSo)
        {
            auSo.clip =
                audioCollisionMetalThings[LibraryStaticFunctions.rnd.
                Next(0, audioCollisionMetalThings.Length)];
            auSo.pitch = LibraryStaticFunctions.GetRangeValue(1, 0.1f);
            auSo.Play();
        }

        public static void WorkWithBurn(AudioSource auSo)
        {
            auSo.clip =
                audioToBurn[LibraryStaticFunctions.rnd.
                Next(0, audioToBurn.Length)];
            auSo.pitch = LibraryStaticFunctions.GetRangeValue(1, 0.1f);
            auSo.Play();
        }

        public static void WorkWithBurning(AudioSource auSo)
        {
            auSo.clip =
                audioBurning[LibraryStaticFunctions.rnd.
                Next(0, audioBurning.Length)];
            auSo.pitch = LibraryStaticFunctions.GetRangeValue(1, 0.1f);
            auSo.Play();
        }

        /// <summary>
        /// Музыка игрового процесса
        /// </summary>
        /// <param name="auSo"></param>
        public static void GameplayMusic(AudioSource auSo)
        {
            auSo.clip =
                audioGameMusic[LibraryStaticFunctions.rnd.
                Next(1, audioGameMusic.Length)];
            auSo.Play();
        }

        /// <summary>
        /// Музыка окончания игры
        /// </summary>
        /// <param name="auSo"></param>
        public static void GameOverMusic(AudioSource auSo)
        {
            auSo.clip = audioGameMusic[0];
            auSo.Play();
        }

        public void WorkWithSoundsBodyFall(AudioSource audioSource)
        {
            audioSource.clip =
               audioBodyFall[LibraryStaticFunctions.rnd.
               Next(0, audioBodyFall.Length)];
            audioSource.pitch = LibraryStaticFunctions.GetRangeValue(1, 0.1f);
            audioSource.volume = LibraryStaticFunctions.GetRangeValue(volumeFall, 0.1f);
            audioSource.Play();
        }

        /// <summary>
        /// Звук смерти
        /// </summary>
        public abstract void PlayDeadAudio();

        public abstract void FallObject();

        /// <summary>
        /// Звук получения урона
        /// </summary>
        /// <param name="isArmory"></param>
        public abstract void PlayGetDamageAudio(bool isArmory=false);

        public abstract void PlayWeaponHitAudio(int value);

        public abstract void PlaySpinAudio(float speed);

        public abstract void PlaySpinAudio(float speed, bool value=false);
        #endregion
    }
}