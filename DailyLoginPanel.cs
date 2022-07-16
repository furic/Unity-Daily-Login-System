using RFGames.StateManagement;
using UnityEngine;

namespace RFGames.UI
{

	/// <summary>
	/// Daily login panel.
	/// </summary>
	public class DailyLoginPanel : ScrollSlotPanel<Reward>
	{

		private static bool _isStateMatched; // Boolean for showing if second state match is enabled

		[Header("Settings")]
		[Tooltip("Auto-show state, set to none if no auto-show.")]
		[SerializeField] private State _showState = State.Menu;
		[Tooltip("Auto-show on 2nd state match, otherewise check on 1st by default. For example, show on 2nd time changed to Menu state (don't show on app launch by only when going back to menu after a game).")]
		[SerializeField] private bool _showOnSecondStateMatch;

		public override void Init()
		{
			base.Init();
			DailyLoginManager.uiToggled += Toggle;
			OnStateChanged(StateManager.currentState);
		}

		public override void Reset()
		{
			base.Reset();
			DailyLoginManager.uiToggled -= Toggle;
		}

		protected override void OnStateChanged(State state)
		{
			if (state == _showState && DailyLoginManager.hasRewards) { // Try to show when in Menu and still has rewards
				if (_showOnSecondStateMatch && !_isStateMatched) {
					_isStateMatched = true;
					return;
				}
				DailyLoginManager.CheckToggleUI(state);
			}
		}

		public override void Toggle(bool show)
		{
			base.Toggle(show);
			if (show)
				InitSlots(DailyLoginManager.rewards);
		}

		protected override void InitSlots(Reward[] rewards)
		{
			base.InitSlots(rewards);
			int cycleDay = DailyLoginManager.cycleDay;
			for (int i = 0, imax = rewards.Length; i < imax; i++) {
				DailyLoginSlot slot = (DailyLoginSlot)GetSlot(i);
				if (slot != null) {
					if (i == cycleDay) {
						if (DailyLoginManager.isTodayRewardObtained)
							slot.SetObtained();
						else
							slot.SetObtainable();
						CenterSlot(slot);
					} else if (i < cycleDay) {
						slot.SetObtained();
					} else if (i > cycleDay) {
						slot.SetMask();
					}
					slot.SetDay(i + 1, i == cycleDay);
				}
			}
		}

		#region Button Controls

		public void OnCollectClick()
		{
			DailyLoginManager.ObtainTodayReward();
		}

		private float _lastDebugSetCanObtainClickTime;
		private int _debugSetCanObtainClickCount;
		private bool _debugSetCanObtainAClicked;

		public void OnDebugSetCanObtainClick()
		{
			DailyLoginManager.ObtainTodayReward();
		}

		public void OnDebugSetCanObtainAClick()
		{
			if (!_debugSetCanObtainAClicked)
				CheckDebugSetCanObtain();
			else
				_debugSetCanObtainClickCount = 0;
			_debugSetCanObtainAClicked = true;
		}

		public void OnDebugSetCanObtainBClick()
		{
			if (_debugSetCanObtainAClicked)
				CheckDebugSetCanObtain();
			else
				_debugSetCanObtainClickCount = 0;
			_debugSetCanObtainAClicked = false;
		}

		private void CheckDebugSetCanObtain()
        {
			if (Time.unscaledTime - _lastDebugSetCanObtainClickTime > 2) {
				_lastDebugSetCanObtainClickTime = Time.unscaledTime;
				_debugSetCanObtainClickCount = 0;
			}
			if (_debugSetCanObtainClickCount++ >= 6)
				DailyLoginManager.DebugSetCanCollectToday();
		}

		#endregion

	}

}