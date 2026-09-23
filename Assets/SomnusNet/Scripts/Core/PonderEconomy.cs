using System;
using UnityEngine;

namespace SomnusNet.Core
{
    public class PonderEconomy : MonoBehaviour
    {
        [SerializeField] int startingPonders = PonderEconomyRules.StandardStartingPonders;
        [SerializeField] int passiveIncome = PonderEconomyRules.PassiveIncomeAmount;
        [SerializeField] float incomeInterval = PonderEconomyRules.PassiveIncomeIntervalSeconds;

        public int Current { get; private set; }
        public event Action<int> OnPondersChanged;

        public static PonderEconomy Instance { get; private set; }

        float _incomeTimer;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            Current = StoryModeRules.Active
                ? StoryModeRules.StartingPonders
                : LevelLayoutSession.IsLayoutMaker
                    ? LayoutMakerRules.StartingPonders
                    : startingPonders;
            _incomeTimer = incomeInterval;
            OnPondersChanged?.Invoke(Current);
        }

        public void ApplyLayoutMakerStart()
        {
            Current = LayoutMakerRules.StartingPonders;
            OnPondersChanged?.Invoke(Current);
        }

        void Update()
        {
            if (GameManager.Instance != null
                && GameManager.Instance.Phase is not GamePhase.Playing and not GamePhase.RoundIntermission)
                return;

            if (LevelStartRules.ShouldPausePassivePonderIncome())
                return;

            if (LevelLayoutSession.IsLayoutMaker)
                return;

            _incomeTimer -= Time.deltaTime;
            if (_incomeTimer > 0f) return;

            _incomeTimer = incomeInterval;
            Add(passiveIncome);
        }

        public bool CanAfford(int cost) => Current >= cost;

        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost)) return false;
            Current -= cost;
            OnPondersChanged?.Invoke(Current);
            return true;
        }

        public void Add(int amount)
        {
            Current += amount;
            OnPondersChanged?.Invoke(Current);
        }
    }
}
