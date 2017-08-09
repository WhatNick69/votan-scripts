﻿using PlayerBehaviour;
using UnityEngine;
using UnityEngine.UI;

namespace CraftSystem
{
    /// <summary>
    /// Описывает гем
    /// </summary>
    public class Gem
        : MonoBehaviour
    {
        #region Переменные
        [SerializeField, Tooltip("Изображение камня")]
        private Sprite imageGem;
        [SerializeField,Tooltip("Тип атаки камня")]
        private GemType damageTypeGem;
        [SerializeField, Range(1, 100f),Tooltip("Сила камня")]
        private float gemPower;
        [SerializeField,Tooltip("Имя камня")]
        private string gemName;
		private string prefabName;
		private string linkPrefab = "Prefabs/Weapon/Grip/";
		#endregion

		#region Свойства
		public string PrefabName
		{
			get
			{
				return prefabName + linkPrefab;
			}
		}
		public GemType DamageTypeGem
        {
            get
            {
                return damageTypeGem;
            }

            set
            {
                damageTypeGem = value;
            }
        }

        public float GemPower
        {
            get
            {
                return gemPower;
            }

            set
            {
                gemPower = value;
            }
        }

        public string GemName
        {
            get
            {
                return gemName;
            }

            set
            {
                gemName = value;
            }
        }

		public Sprite ItemImage
		{
			get
			{
				return imageGem;
			}
		}
		#endregion
	}
}
