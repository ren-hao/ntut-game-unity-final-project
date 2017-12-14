﻿using NTUT.CSIE.GameDev.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NTUT.CSIE.GameDev.Game;
using UnityEngine.Serialization;
using NTUT.CSIE.GameDev.Scene;
using NTUT.CSIE.GameDev.UI;
using NTUT.CSIE.GameDev.Helpers;

namespace NTUT.CSIE.GameDev.Component.Map
{
    public class HouseInfo : CommonObject, IHurtable
    {
        public int scale = 1;
        [SerializeField]
        private ulong _houseId;
        [SerializeField]
        private SpriteRenderer _houseRenderer;
        // 房屋基本資訊
        [SerializeField]
        private HouseType type;
        public int hp;
        public int maxHp;
        public string houseName;
        public Sprite[] houseImage = new Sprite[3];
        // 出產怪物資訊
        private string _monsterNum;
        private int _extraAttack;
        private int _extraHp;
        private int _extraSpeed;
        [FormerlySerializedAs("Remaining Next Spawn Time (Readonly)")]
        public int RemainingNextSpawnTime;
        private float _lastSpawnTime = 0f;
        [SerializeField]
        private Point _position;
        [SerializeField]
        private Direction _direction = Direction.Right;
        private SpriteRenderer _sprite;
        private int _playerID;
        private int _upgAttackCnt, _upgHpCnt, _upgSpeedCnt;

        public enum HouseType
        {
            Empty, Building, Summon
        }
        public enum UpgradeType
        {
            Attack, HP, Speed
        }

        public HouseInfo()
        {
            hp = maxHp = 0;
            houseName = "空地";
            _extraAttack = _extraHp = _extraSpeed = 0;
            _upgAttackCnt = _upgHpCnt = _upgSpeedCnt = 0;
        }

        public HouseType Type
        {
            set
            {
                type = value;

                if (type == HouseType.Empty)
                {
                    hp = maxHp = 0;
                    houseName = "空地";
                }
                else if (type == HouseType.Building)
                {
                    hp = maxHp = MAX_HP;
                    houseName = "建築";
                }
                else if (type == HouseType.Summon)
                {
                    _lastSpawnTime = Time.time;
                }

                _houseRenderer.sprite = houseImage[(int)type];
            }

            get
            {
                return type;
            }
        }

        public HouseInfo SetPosition(int row, int col)
        {
            this._position = new Point(row, col);
            var gen = GetSceneLogic<FightSceneLogic>().MapGridGenerator;
            var pos = Helper.Clone(gen[row, col].gameObject.transform.localPosition);
            pos.y = 0;
            gameObject.transform.localPosition = pos;
            return this;
        }

        public HouseInfo SetId(ulong id)
        {
            this._houseId = id;
            this.name = string.Format("House #{0}", id);
            return this;
        }

        public HouseInfo SetType(HouseType type)
        {
            this.Type = type;
            return this;
        }

        public string MonsterNumber
        {
            set
            {
                _monsterNum = value;
                this.SetMonsterAbility(value);
            }

            get
            {
                return _monsterNum;
            }
        }

        public Monster.Info MonsterInfo
        {
            get
            {
                if (_monsterNum == null)
                    return null;

                return this.Manager.MonsterInfoCollection[_monsterNum];
            }
        }

        public HouseInfo SetPlayerID(int id)
        {
            this.PlayerID = id;
            return this;
        }

        public int PlayerID
        {
            get
            {
                return _playerID;
            }
            set
            {
                _playerID = value;
            }
        }

        private void SetMonsterAbility(string num)
        {
            // read monster info
        }

        private void Update()
        {
            RemainingNextSpawnTime = -1;

            if (_direction == Direction.Left)
                _sprite.flipX = true;
            else
                _sprite.flipX = false;

            if (type == HouseInfo.HouseType.Summon)
            {
                RemainingNextSpawnTime = System.Convert.ToInt32(_lastSpawnTime + MonsterInfo.SpawnInterval - Time.time);

                if (Time.time - _lastSpawnTime > MonsterInfo.SpawnInterval)
                {
                    Spawn();
                    _lastSpawnTime = Time.time;
                }
            }
        }

        private void Spawn()
        {
            Debug.Log(string.Format("召喚: {0}", MonsterInfo.Name));
            GetSceneLogic<FightSceneLogic>().SpawnMonster(MonsterInfo.ID, _playerID, this);
        }

        public HouseInfo SetDirection(Direction dir)
        {
            this.Direction = dir;
            return this;
        }

        public Direction Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
            }
        }
        internal void _ResetSpawnCounter()
        {
            if (type == HouseInfo.HouseType.Summon)
            {
                _lastSpawnTime = -MonsterInfo.SpawnInterval - 1;
            }
        }

        protected virtual void Start()
        {
            _sprite = transform.Find("Building").GetComponent<SpriteRenderer>();
            Debug.Assert(_sprite != null);
        }

        protected void OnMouseDown()
        {
            // 空地時 顯示hightlight的工作 是給格線MapGrid執行的 之後選卡跟升級給HouseInfo來執行
            if (Type != 0)
            {
                var logic = GetSceneLogic<FightSceneLogic>();
                logic.MapGridGenerator.SetHighLight(_position);
                GetSceneLogic<FightSceneLogic>().ControlPanel.DisplayInfo(this);
            }
        }

        public void Damage(int attack)
        {
            hp -= attack;
        }

        public void Recovery(int recovery)
        {
            hp += recovery;
        }

        public void ResetMonster()
        {
            Type = HouseInfo.HouseType.Building;
            _monsterNum = null;
            _extraAttack = 0;
            _extraHp = 0;
            _extraSpeed = 0;
        }

        public void UpgradeAttack()
        {
            _extraAttack++;
            _upgAttackCnt++;
        }

        public void UpgradeSpeed()
        {
            _extraSpeed++;
            _upgSpeedCnt++;
        }

        public void UpgradeHP()
        {
            _extraHp += 10;
            _upgHpCnt++;
        }

        public int RealAttack => MonsterInfo == null ? 0 : (MonsterInfo.Attack + _extraAttack);
        public int RealHP => MonsterInfo == null ? 0 : (MonsterInfo.MaxHP + _extraHp);
        public int RealSpeed => MonsterInfo == null ? 0 : (MonsterInfo.Speed + _extraSpeed);

        public int MAX_HP => 5000;

        public int UpgradeAttackCount => _upgAttackCnt;
        public int UpgradeSpeedCount => _upgSpeedCnt;
        public int UpgradeHpCount => _upgHpCnt;

        public int GetUpgradeCount(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Attack:
                    return _upgAttackCnt;

                case UpgradeType.HP:
                    return _upgHpCnt;

                case UpgradeType.Speed:
                    return _upgSpeedCnt;
            }

            return 0;
        }

        public void ShowHpChangedNumber(int damage)
        {
        }

        public ulong ID => _houseId;
    }
}