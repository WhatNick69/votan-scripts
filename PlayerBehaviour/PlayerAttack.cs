﻿using AbstractBehaviour;
using System.Collections.Generic;

namespace PlayerBehaviour
{
    /// <summary>
    /// Компонент-атака для персонажа
    /// </summary>
    public class PlayerAttack
        : AbstractAttack
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        void Start()
        {
            listEnemy = new List<AbstractEnemy>();
            attackList = new List<AbstractEnemy>();
        }

        /// <summary>
        /// Обновление с заданной частотой
        /// </summary>
        private void FixedUpdate()
        {
            for (int i = 0; i < listEnemy.Count; i++)
                if (listEnemy[i])
                {
                    if (AttackRange(transform.position, listEnemy[i].transform.position) < 3)
                    {
                        if (!attackList.Contains(listEnemy[i])) attackList.Add(listEnemy[i]);
                    }
                }
            EnemyAttack(1);
        }
    }
}
