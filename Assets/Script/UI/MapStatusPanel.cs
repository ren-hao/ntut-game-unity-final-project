﻿using NTUT.CSIE.GameDev.Game;
using NTUT.CSIE.GameDev.Scene;
using NTUT.CSIE.GameDev.CardSelect;
using NTUT.CSIE.GameDev.Component.Map;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NTUT.CSIE.GameDev.Component;

namespace NTUT.CSIE.GameDev.UI
{
    public class MapStatusPanel : CommonObject
    {
        private enum Mode { None, Buy, Select, Upgrade, Master}
        private delegate void PanelCloseEventHandler();
        List<Monster.Info> _cardSet = new List<Monster.Info>();
        public GameObject cardPrefab;
        [SerializeField]
        Sprite[] _buildingLevel = new Sprite[3];
        public Text upgradeAttackText, upgradeHPText, upgradeSpeedText, upAttackText, upHpText, upSpeedText, disCard;

        [SerializeField]
        protected Button _buyOK, _buyCancel, _upgAtt, _upgHP, _upgSpeed, _disCard, _uniAttBtn, _uniDefBtn;
        [SerializeField]
        protected SelectCardButton[] _selectCard;
        [SerializeField]
        private GameObject _pictureObj = null, _describeObj = null, _miniMapObj = null,
                           _buyPanel = null, _selectPanel = null, _upgradePanel = null,
                           _masterPanel = null;
        private Mode _mode;

        [SerializeField]
        private Image _picImage = null;
        [SerializeField]
        private Text _picHpText = null,
                     _picNameText = null,
                     _remSecText = null;

        [SerializeField]
        private Text _monsterInfoText = null,
                     _monsterDescText = null;

        private FightSceneLogic _scene;
        private event PanelCloseEventHandler OnPanelClosed;

        protected override void Awake()
        {
            base.Awake();
            _cardSet.AddRange(
                Manager.GetPlayerAt(Manager.DEFAULT_PLAYER_ID)
                .GetCardIds()
                .Select(mobID => this.Manager.MonsterInfoCollection[mobID])
            );
            _scene = GetSceneLogic<FightSceneLogic>();
            Debug.Assert(_miniMapObj != null);
            BindButtonEvent();
        }

        // Use this for initialization
        void Start()
        {
            this.gameObject.SetActive(false);
            _mode = Mode.None;
        }

        private void BindButtonEvent()
        {
            var player = _scene.GetPlayerAt(Manager.DEFAULT_PLAYER_ID);
            System.Action noMoneyMsg = () => new DialogBuilder().SetContent("你沒錢┐(￣ヘ￣)┌").Show(_scene.Window);
            System.Action<System.Action> doActionOrShowErrMsg = (System.Action a) =>
            {
                try
                {
                    a();
                }
                catch (System.Exception e)
                {
                    new DialogBuilder()
                    .SetIcon(Dialog.Icon.Error)
                    .SetTitle("錯誤")
                    .SetContent(e.Message)
                    .Show(_scene.Window);
                }
            };
            System.Action<HouseInfo> checkInfoAndShow = (houseInfo) =>
            {
                if (houseInfo != null)
                {
                    CloseAllPanel();
                    DisplayInfo(houseInfo);
                }
                else
                    noMoneyMsg.Invoke();
            };
            _buyOK.onClick.AddListener(() =>
            {
                var houseInfo = player.BuyHouse(_scene.MapGridGenerator.CurPoint);
                checkInfoAndShow.Invoke(houseInfo);
            });
            _buyCancel.onClick.AddListener(() => Hide());

            for (int i = 0; i < _selectCard.Length; i++)
            {
                int finalI = i; // Important, if use i, i always equals to _selectCard.Length
                _selectCard[i].Button.onClick.AddListener(() =>
                {
                    var houseInfo = player.SetHouseMonster(_scene.MapGridGenerator.CurPoint, finalI);
                    checkInfoAndShow.Invoke(houseInfo);
                });
                _selectCard[i].OnMouseOver += () =>
                {
                    var mobID = player.Info.GetCardIds()[finalI];
                    Monster.Info mobInfo = Manager.Manager.MonsterInfoCollection[mobID];
                    _monsterInfoText.text = $"名稱: {mobInfo.Name}, 價錢: {mobInfo.Cost}";
                    _monsterDescText.text = mobInfo.Description;
                };
                _selectCard[i].OnMouseLeave += () =>
                {
                    _monsterInfoText.text = "";
                    _monsterDescText.text = "";
                };
            }

            _upgAtt.onClick.AddListener(() =>
            {
                checkInfoAndShow.Invoke(
                    player.UpgradeHouse(_scene.MapGridGenerator.CurPoint, HouseInfo.UpgradeType.Attack)
                );
            });
            _upgHP.onClick.AddListener(() =>
            {
                checkInfoAndShow.Invoke(
                    player.UpgradeHouse(_scene.MapGridGenerator.CurPoint, HouseInfo.UpgradeType.HP)
                );
            });
            _upgSpeed.onClick.AddListener(() =>
            {
                checkInfoAndShow.Invoke(
                    player.UpgradeHouse(_scene.MapGridGenerator.CurPoint, HouseInfo.UpgradeType.Speed)
                );
            });
            _disCard.onClick.AddListener(() =>
            {
                var houseInfo = player.DiscardHouseMonster(_scene.MapGridGenerator.CurPoint);
                checkInfoAndShow.Invoke(houseInfo);
            });
            _uniAttBtn.onClick.AddListener(() =>
            {
                doActionOrShowErrMsg(() => player.DoUniqueSkill(Player.Player.UniqueSkill.Attack));
            });
            _uniDefBtn.onClick.AddListener(() =>
            {
                doActionOrShowErrMsg(() => player.DoUniqueSkill(Player.Player.UniqueSkill.Defense));
            });
        }

        /// <summary>   Sets an image. </summary>
        ///
        /// <param name="img">          The image. </param>
        /// <param name="hpText">       The hp text. </param>
        /// <param name="houseName">    Name of the house. </param>
        protected void SetImage(Sprite img, string hpText, string houseName)
        {
            if (img) _picImage.sprite = img;

            if (hpText != null) _picHpText.text = hpText;

            if (houseName != null) _picNameText.text = houseName;
        }


        protected void SetImage(Sprite img, string houseName)
        {
            SetImage(img, houseName, "");
        }

        // display about mapgrid
        public void DisplayInfo(HouseInfo houseInfo)
        {
            Show();

            for (int i = 0; i < 3; i++)
                _pictureObj.transform.GetChild(i).gameObject.SetActive(true);

            SetImage(_buildingLevel[(int)houseInfo.Type], $"{houseInfo.HP}/{houseInfo.MAX_HP}", $"{houseInfo.houseName} {houseInfo.ID:00}");
            PanelCloseEventHandler removeHpUpdateEvent = null;
            removeHpUpdateEvent = () =>
            {
                houseInfo.OnHPChanged -= UpdateHpText;
                OnPanelClosed -= removeHpUpdateEvent;
            };

            switch (houseInfo.Type)
            {
                case HouseInfo.HouseType.Empty:
                    this.Buy();
                    break;

                case HouseInfo.HouseType.Building:
                    this.Select();
                    houseInfo.OnHPChanged += UpdateHpText;
                    OnPanelClosed += removeHpUpdateEvent;
                    break;

                case HouseInfo.HouseType.Summon:
                    this.Upgrade();
                    houseInfo.OnHPChanged += UpdateHpText;
                    OnPanelClosed += removeHpUpdateEvent;
                    break;

                case HouseInfo.HouseType.Master:
                    this.ShowMaster();
                    break;

                default:
                    Debug.Log("Type error");
                    break;
            }
        }

        private void UpdateHpText(HouseInfo houseInfo)
        {
            SetImage(null, $"{houseInfo.HP}/{houseInfo.MAX_HP}", null);

            if (!houseInfo.Alive && _scene.MapGridGenerator.CurPoint == houseInfo.Position)
            {
                Hide();
            }
        }

        // buy
        public void Buy()
        {
            this.CloseDescribePanel();
            _buyPanel.SetActive(true);
            SetImage(_buildingLevel[0], "空地");
            _mode = Mode.Buy;
            // _buyPanel.transform.Find("BuildImage").Find("Image").GetComponent<Image>().sprite = _buildingLevel[1];
        }

        public void Select()
        {
            this.CloseDescribePanel();
            _selectPanel.gameObject.SetActive(true);

            for (int i = 0; i < 6; i++)
            {
                _selectCard[i].gameObject
                .GetComponent<Image>()
                .sprite = Resources.Load<Sprite>("Card/Card" + _cardSet[i].IDStr);
            }

            _mode = Mode.Select;
        }

        public void Upgrade()
        {
            var player = _scene.GetPlayerAt(Manager.DEFAULT_PLAYER_ID);
            var houseInfo = _scene.MapGridGenerator.CurHouseInfo;
            this.CloseDescribePanel();
            _upgradePanel.SetActive(true);
            _upgradePanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Monster/" + houseInfo.MonsterInfo.IDStr);
            upgradeAttackText.text = string.Format("攻擊：{0}", houseInfo.RealAttack);
            upgradeHPText.text = string.Format("血量：{0}", houseInfo.RealHP);
            upgradeSpeedText.text = string.Format("速度：{0}", houseInfo.RealSpeed);
            upAttackText.text = string.Format("提升攻擊({0:C0})", player.GetUpgradePrice(_scene.MapGridGenerator.CurPoint, HouseInfo.UpgradeType.Attack));
            upHpText.text = string.Format("提升血量({0:C0})", player.GetUpgradePrice(_scene.MapGridGenerator.CurPoint, HouseInfo.UpgradeType.HP));
            upSpeedText.text = string.Format("提升移速({0:C0})", player.GetUpgradePrice(_scene.MapGridGenerator.CurPoint, HouseInfo.UpgradeType.Speed));
            disCard.text = string.Format("解除卡片({0:C0})", Config.DISCARD_MONSTER_PUNISH);
            _mode = Mode.Upgrade;
        }

        public void ShowMaster()
        {
            this.CloseDescribePanel();
            _masterPanel.SetActive(true);
            SetImage(_buildingLevel[10 + _scene.MapGridGenerator.CurHouseInfo.PlayerID], "主塔");
            _mode = Mode.Master;
        }

        public void CloseDescribePanel()
        {
            // 把描述介面全關閉 需要再另開起
            foreach (Transform t in _describeObj.transform)
            {
                t.gameObject.SetActive(false);
            }

            OnPanelClosed?.Invoke();
        }

        public void CloseAllPanel()
        {
            for (int i = 0; i < 3; i++)
                _pictureObj.transform.GetChild(i).gameObject.SetActive(false);

            CloseDescribePanel();
            OnPanelClosed?.Invoke();
        }

        // Update is called once per frame
        protected void Update()
        {
            if (_mode == Mode.Upgrade)
            {
                HouseInfo h = _scene.MapGridGenerator.CurHouseInfo;

                if (h)
                {
                    _remSecText.text = $"剩餘: {h.RemainingNextSpawnTime}秒";
                }
            }
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            OnPanelClosed?.Invoke();
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
        }
    }
}
