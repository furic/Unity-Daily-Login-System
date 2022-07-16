using RFGames.StateManagement;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RFGames
{

	/// <summary>
	/// Manager to manage all daily logins.
	/// </summary>
	public static class DailyLoginManager
	{

		private const string PREFS_START_DATE = "DailyLoginStartDate";
		private const string PREFS_DAILY_LOGIN_COUNT = "DailyLoginCount";
		private const string PREFS_LAST_DAILY_LOGIN_DATE = "LastDailyLoginDate";
		private const string PREFS_DAILY_LOGIN_ENABLED = "DailyLoginEnabled";
		private const string PREFS_DAILY_LOGIN_SCENE_LOADED_COUNT = "DailyLoginSceneLoadedCount";

		public static event UnityAction<bool> uiToggled;
		public static event UnityAction rewardObtained;

		private static DateTime _lastDailyLoginDate;
		private static bool _isEnabled;

		private static DateTime _startDateTime;

		private static int _dailyLoginCount
		{
			get { return PlayerPrefs.GetInt(PREFS_DAILY_LOGIN_COUNT); }
			set { PlayerPrefs.SetInt(PREFS_DAILY_LOGIN_COUNT, value); }
		}

		private static DailyLoginSettings _settings => DailyLoginSettings.current;

		private static bool _isInFirstCycle => totalDays / _settings.cycleDays == 0;

		/// <summary>
		/// Total days passed since the fist day (miss reward if not login), or the total days that obtained rewards.
		/// </summary>
		public static int totalDays => _settings.missRewardIfNotLogin ? (DateTime.Now.Date - _startDateTime.Date).Days : (_dailyLoginCount - (isTodayRewardObtained ? 1 : 0));

		public static Reward currentReward => rewards[cycleDay];

		/// <summary>
		/// If all rewards are already been obtained.
		/// </summary>
		public static bool hasRewards => !_settings.oneCycleOnly || _isInFirstCycle; // Not one cycle only or in first cycle

		public static Reward[] rewards => _isInFirstCycle && _settings.differentFirstCycle ? _settings.firstCycleRewards : _settings.rewards;

		public static int cycleDay => totalDays % _settings.cycleDays;

		public static bool isTodayRewardObtained => DateTime.Now.Date <= _lastDailyLoginDate;

		private static bool _hasPromptedThisLaunch = false;

		static DailyLoginManager()
		{
			if (!PlayerPrefs.HasKey(PREFS_START_DATE))
				DateTimeUtils.SetPlayerPrefsDateTime(PREFS_START_DATE, DateTime.Now.Date);
			_startDateTime = DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE);
			_lastDailyLoginDate = DateTimeUtils.GetPastPlayerPrefs(PREFS_LAST_DAILY_LOGIN_DATE, DateTime.Now.AddHours(-24));
			_isEnabled = PlayerPrefs.GetInt(PREFS_DAILY_LOGIN_ENABLED) == 1;
		}

		public static void Enable()
		{
			// A/B testing
			if (!RemoteSettings.GetBool("daily_login_enabled", true)) return;
			_isEnabled = true;
			PlayerPrefs.SetInt(PREFS_DAILY_LOGIN_ENABLED, 1);
		}

		public static void CheckToggleUI(State state)
		{
			// If we are prompt on state, and we are at our check state
			if (_settings.promptOnState && state == _settings.checkState) {

				// Check how many times we have prompted our panel
				if (_settings.promptAfterSceneLoadCount >= 0 && !_isEnabled) {
					int sceneLoadedCount = PlayerPrefs.GetInt(PREFS_DAILY_LOGIN_SCENE_LOADED_COUNT);
					if (sceneLoadedCount > _settings.promptAfterSceneLoadCount)
						Enable();
					sceneLoadedCount++;
					PlayerPrefs.SetInt(PREFS_DAILY_LOGIN_SCENE_LOADED_COUNT, sceneLoadedCount);
				}
				// If the server time not yet obtained yet, don't auto prompt
				if (!DateTimeUtils.serverTimeObtained)
					return;
				if (_isEnabled && !isTodayRewardObtained && hasRewards) {
					// Do a check for how many times we have auto prompted
					// We do this to stop issues where daily login prompt would toggle on every time the game changed state.
					// So essentially every time they go to the shop. Which causes flow issues when we want to return to our previous UI
					if (_hasPromptedThisLaunch)
						return;
					ToggleUI();
					_hasPromptedThisLaunch = true;
				}
			}
		}

		public static void ToggleUI(bool doShow = true)
		{
			uiToggled?.Invoke(doShow);
		}

		public static void ObtainTodayReward()
		{
			if (isTodayRewardObtained) return; // Just in case

			currentReward.Obtain("daily_login");

			++_dailyLoginCount;
			_lastDailyLoginDate = DateTime.Now.Date;
			DateTimeUtils.SetPlayerPrefs(PREFS_LAST_DAILY_LOGIN_DATE, _lastDailyLoginDate);
			rewardObtained?.Invoke();
		}

		/// <summary>
		/// Set can collect today, used for admin debug only
		/// </summary>
		public static void DebugSetCanCollectToday()
		{
			PlayerPrefs.DeleteKey(PREFS_LAST_DAILY_LOGIN_DATE);
			_lastDailyLoginDate = DateTime.Now.AddHours(-24);
			ToggleUI(true);
		}

#if UNITY_EDITOR

		[UnityEditor.MenuItem("Debug/Daily Login/Print Parameters")]
		private static void PrintParameters()
		{
			if (Application.isPlaying) {
				DebugUtils.Log(_startDateTime, "_startDateTime");
				DebugUtils.Log(_dailyLoginCount, "_dailyLoginCount");
				DebugUtils.Log(_lastDailyLoginDate, "_lastDailyLoginDate");
				DebugUtils.Log(_isEnabled, "_isEnabled");
				DebugUtils.Log(_isInFirstCycle, "_isInFirstCycle");
				DebugUtils.Log(totalDays, "_totalDay");
				DebugUtils.Log(rewards, "rewardItems");
				DebugUtils.Log(cycleDay, "cycleDay");
				DebugUtils.Log(isTodayRewardObtained, "isTodayRewardObtained");
			}
			else {
				DebugUtils.LogPlayerPrefs<DateTime>(PREFS_START_DATE);
				DebugUtils.LogPlayerPrefs<int>(PREFS_DAILY_LOGIN_COUNT);
				DebugUtils.LogPlayerPrefs<DateTime>(PREFS_LAST_DAILY_LOGIN_DATE);
				DebugUtils.LogPlayerPrefs<int>(PREFS_DAILY_LOGIN_ENABLED);
			}
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Enable or Disable")]
		private static void EnableOrDisable()
		{
			_isEnabled = !_isEnabled;
			PlayerPrefs.SetInt(PREFS_DAILY_LOGIN_ENABLED, 1);
			Debug.LogFormat("DailyLoginManager:EnableOrDisable - DailyLoginManager is now {0}", _isEnabled ? "enabled" : "disabled");
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Show UI")]
		private static void ShowUI()
		{
			DebugUtils.CheckPlaying(() => ToggleUI(true));
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Set Can Collect Today")]
		private static void SetCanCollectToday()
		{
			PlayerPrefs.DeleteKey(PREFS_LAST_DAILY_LOGIN_DATE);
			if (Application.isPlaying) {
				_lastDailyLoginDate = DateTime.Now.AddHours(-24);
				ToggleUI(true);
			}
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Add 1 Daily Day")]
		private static void Add1DailyDay()
		{
			if (_settings.missRewardIfNotLogin) {
				DateTimeUtils.SetPlayerPrefs(PREFS_START_DATE, DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE).AddDays(-1));
				_startDateTime = DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE);
				DebugUtils.Log(_startDateTime, "_startDateTime");
			}
			else {
				++_dailyLoginCount;
				DebugUtils.Log(_dailyLoginCount, "_dailyLoginCount");
			}
			if (Application.isPlaying)
				ToggleUI(true);
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Reduce 1 Daily Day")]
		private static void Reduce1DailyDay()
		{
			if (_settings.missRewardIfNotLogin) {
				_startDateTime = DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE);
				if (_startDateTime.AddDays(1) <= DateTime.Now.Date) { // Make sure start date is not after today
					DateTimeUtils.SetPlayerPrefs(PREFS_START_DATE, DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE).AddDays(1));
					_startDateTime = DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE);
				}
				DebugUtils.Log(_startDateTime, "_startDateTime");
			}
			else {
				_dailyLoginCount = Mathf.Max(0, _dailyLoginCount - 1);
				DebugUtils.Log(_dailyLoginCount, "_dailyLoginCount");
			}
			if (Application.isPlaying)
				ToggleUI(true);
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Set To 6th Login Day")]
		private static void SetToSixthLoginDay()
		{
			if (_settings.missRewardIfNotLogin) {
				DateTimeUtils.SetPlayerPrefs(PREFS_START_DATE, DateTime.Now.Date.AddDays(-6));
				_startDateTime = DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE);
			}
			else {
				_dailyLoginCount = 6;
			}
			if (Application.isPlaying)
				ToggleUI(true);
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Set To 13th Login Day")]
		private static void SetToThirteenthLoginDay()
		{
			if (_settings.missRewardIfNotLogin) {
				DateTimeUtils.SetPlayerPrefs(PREFS_START_DATE, DateTime.Now.Date.AddDays(-13));
				_startDateTime = DateTimeUtils.GetPlayerPrefs(PREFS_START_DATE);
			}
			else {
				_dailyLoginCount = 13;
			}
			if (Application.isPlaying)
				ToggleUI(true);
		}

		[UnityEditor.MenuItem("Debug/Daily Login/Reset")]
		private static void Reset()
		{
			DebugUtils.CheckNotPlaying(() => {
				PlayerPrefs.DeleteKey(PREFS_START_DATE);
				PlayerPrefs.DeleteKey(PREFS_DAILY_LOGIN_COUNT);
				PlayerPrefs.DeleteKey(PREFS_LAST_DAILY_LOGIN_DATE);
				PlayerPrefs.DeleteKey(PREFS_DAILY_LOGIN_ENABLED);
				PlayerPrefs.DeleteKey(PREFS_DAILY_LOGIN_SCENE_LOADED_COUNT);
			});
		}

#endif

	}

}