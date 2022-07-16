using UnityEngine;
using UnityEditor;
using System;
using RFGames.StateManagement;

namespace RFGames
{

	[CustomEditor(typeof(DailyLoginSettings))]
	public class DailyLoginSettingsEditor : Editor
	{

		private DailyLoginSettings _dls => target as DailyLoginSettings;

		protected bool _showFirstCycleRewards = true, _showRewards = true;
		protected bool[] _showFirstCycleDayRewards = new bool[0], _showDayRewards = new bool[0];

		public override void OnInspectorGUI()
		{
			bool promptOnStateAutomatically = EditorGUILayout.Toggle(new GUIContent("Prompt On State Automatically", "Prompt UI on certain state automatically"), _dls.promptOnState);
			if (promptOnStateAutomatically != _dls.promptOnState) {
				Undo.RegisterCompleteObjectUndo(_dls, "Edit Prompt On Menu Automatically");
				_dls.promptOnState = promptOnStateAutomatically;
				EditorUtility.SetDirty(_dls);
			}

			if (_dls.promptOnState) {

				EditorGUI.indentLevel++;

				State checkState = (State)EditorGUILayout.EnumPopup(new GUIContent("Check state", "What state we should be checking for to show our menu"), _dls.checkState);
				if (checkState != _dls.checkState) {
					Undo.RegisterCompleteObjectUndo(_dls, "Edit Check State");
					_dls.checkState = checkState;
					EditorUtility.SetDirty(_dls);
				}

				int promptAfterSceneLoadCount = EditorGUILayout.IntField(new GUIContent("Enable On Scene Loaded Count", "Prompt only after scene loaded x times, used to avoid prompting in tutorial"), _dls.promptAfterSceneLoadCount);
				if (promptAfterSceneLoadCount != _dls.promptAfterSceneLoadCount) {
					Undo.RegisterCompleteObjectUndo(_dls, "Edit Auto Enable");
					_dls.promptAfterSceneLoadCount = promptAfterSceneLoadCount;
					EditorUtility.SetDirty(_dls);
				}

				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
			}


			int cycleDays = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Cycle Days", "The day count for a cycle"), _dls.cycleDays)); // At least 1
			if (cycleDays != _dls.cycleDays || cycleDays != _dls.firstCycleRewards.Length || cycleDays != _dls.rewards.Length) {
				Undo.RegisterCompleteObjectUndo(_dls, "Edit Cycle Days");
				Array.Resize(ref _dls.firstCycleRewards, cycleDays);
				Array.Resize(ref _dls.rewards, cycleDays);
				_dls.cycleDays = cycleDays;
				EditorUtility.SetDirty(_dls);
			}

			bool oneCycleOnly = EditorGUILayout.Toggle(new GUIContent("One Cycle Only", "Rewards for one cycle only and never shown, or keep looping the cycle"), _dls.oneCycleOnly);
			if (oneCycleOnly != _dls.oneCycleOnly) {
				Undo.RegisterCompleteObjectUndo(_dls, "Edit One Cycle Only");
				_dls.oneCycleOnly = oneCycleOnly;
				EditorUtility.SetDirty(_dls);
			}

			if (!_dls.oneCycleOnly) {
				bool differentFirstCycle = EditorGUILayout.Toggle(new GUIContent("Different First Cycle", "Use different cycle for first only, only valid when oneCycleOnly is false"), _dls.differentFirstCycle);
				if (differentFirstCycle != _dls.differentFirstCycle) {
					Undo.RegisterCompleteObjectUndo(_dls, "Edit Differnet First Cycle");
					_dls.differentFirstCycle = differentFirstCycle;
					EditorUtility.SetDirty(_dls);
				}
			}

			bool missRewardIfNotLogin = EditorGUILayout.Toggle(new GUIContent("Miss Reward If Not Login", "Player miss the reward for the day not logging in. If false, always continue the day of last login day/."), _dls.missRewardIfNotLogin);
			if (missRewardIfNotLogin != _dls.missRewardIfNotLogin) {
				Undo.RegisterCompleteObjectUndo(_dls, "Edit One Cycle Only");
				_dls.missRewardIfNotLogin = missRewardIfNotLogin;
				EditorUtility.SetDirty(_dls);
			}

			if (_dls.oneCycleOnly || _dls.differentFirstCycle) {

				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

				_showFirstCycleRewards = EditorGUILayout.Foldout(_showFirstCycleRewards, new GUIContent("First Cycle Rewards", "The reward items of first cycle, only shown when different first cycle is enabled"));

				if (_showFirstCycleRewards) {

					if (_showFirstCycleDayRewards.Length != _dls.firstCycleRewards.Length)
						_showFirstCycleDayRewards = _showFirstCycleDayRewards.Resize(_dls.firstCycleRewards.Length, true);

					EditorGUI.indentLevel++;

					for (int i = 0; i < _dls.firstCycleRewards.Length; i++) {

						if (_dls.firstCycleRewards[i] == null)
							_dls.firstCycleRewards[i] = new Reward();
						Reward reward = _dls.firstCycleRewards[i];

						_showFirstCycleDayRewards[i] = EditorGUILayout.Foldout(_showFirstCycleDayRewards[i], "Day " + (i + 1));

						if (_showFirstCycleDayRewards[i]) {

							// Reward count
							int rewardCount = EditorGUILayout.IntField(new GUIContent("Reward Count", "The number of rewarding items after completing it."), reward == null ? 0 : reward.count);
							if (rewardCount != reward.count) {
								Undo.RegisterCompleteObjectUndo(_dls, "Edit First Cycle Day " + (i + 1) + " Reward Count");
								reward.count = rewardCount;
								EditorUtility.SetDirty(_dls);
							}

							EditorGUI.indentLevel++;

							// Rewards
							for (int j = 0; j < reward.count; j++) {

								EditorGUILayout.LabelField(string.Format("Reward {0}", j + 1));

								EditorGUI.indentLevel++;

								// Rewards types
								ItemType itemType = (ItemType)EditorGUILayout.EnumPopup(new GUIContent("Types", "Reward item type."), reward.items[j].type);
								if (itemType != reward.items[j].type) {
									Undo.RegisterCompleteObjectUndo(_dls, "Edit Reward Types");
									reward.items[j].SetItemType(itemType);
									EditorUtility.SetDirty(_dls);
								}

								// Rewards amount
								int amount = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Amount", "Reward item amount."), reward.items[j].amount));
								if (amount != reward.items[j].amount) {
									Undo.RegisterCompleteObjectUndo(_dls, "Edit Reward Amount");
									reward.items[j].SetAmount(amount);
									EditorUtility.SetDirty(_dls);
								}

								// Rewards id
								int id = EditorGUILayout.IntField(new GUIContent("ID", "Item id (Optional)."), reward.items[j].id);
								if (id != reward.items[j].id) {
									Undo.RegisterCompleteObjectUndo(_dls, "Edit Reward ID");
									reward.items[j].SetId(id);
									EditorUtility.SetDirty(_dls);
								}

								EditorGUI.indentLevel--;

							}

							EditorGUI.indentLevel--;

						}

					}

					EditorGUI.indentLevel--;
				}
			}

			if (!_dls.oneCycleOnly) {

				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

				_showRewards = EditorGUILayout.Foldout(_showRewards, new GUIContent("Rewards", "The reward items of all cycle"));

				if (_showRewards) {

					if (_showDayRewards.Length != _dls.rewards.Length)
						_showDayRewards = _showDayRewards.Resize(_dls.rewards.Length, true);

					EditorGUI.indentLevel++;

					for (int i = 0; i < _dls.rewards.Length; i++) {

						if (_dls.rewards[i] == null)
							_dls.rewards[i] = new Reward();
						Reward reward = _dls.rewards[i];

						_showDayRewards[i] = EditorGUILayout.Foldout(_showDayRewards[i], "Day " + (i + 1));

						if (_showDayRewards[i]) {

							// Reward count
							int rewardCount = EditorGUILayout.IntField(new GUIContent("Reward Count", "The number of rewarding items after completing it."), reward == null ? 0 : reward.count);
							if (rewardCount != reward.count) {
								Undo.RegisterCompleteObjectUndo(_dls, "Edit First Cycle Day " + (i + 1) + " Reward Count");
								reward.count = rewardCount;
								EditorUtility.SetDirty(_dls);
							}

							EditorGUI.indentLevel++;

							// Rewards
							for (int j = 0; j < reward.count; j++) {

								EditorGUILayout.LabelField(string.Format("Reward {0}", j + 1));

								EditorGUI.indentLevel++;

								// Rewards types
								ItemType itemType = (ItemType)EditorGUILayout.EnumPopup(new GUIContent("Types", "Reward item type."), reward.items[j].type);
								if (itemType != reward.items[j].type) {
									Undo.RegisterCompleteObjectUndo(_dls, "Edit Reward Types");
									reward.items[j].SetItemType(itemType);
									EditorUtility.SetDirty(_dls);
								}

								// Rewards amount
								int amount = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Amount", "Reward item amount."), reward.items[j].amount));
								if (amount != reward.items[j].amount) {
									Undo.RegisterCompleteObjectUndo(_dls, "Edit Reward Amount");
									reward.items[j].SetAmount(amount);
									EditorUtility.SetDirty(_dls);
								}

								// Rewards id
								int id = EditorGUILayout.IntField(new GUIContent("ID", "Item id (Optional)."), reward.items[j].id);
								if (id != reward.items[j].id) {
									Undo.RegisterCompleteObjectUndo(_dls, "Edit Reward ID");
									reward.items[j].SetId(id);
									EditorUtility.SetDirty(_dls);
								}

								EditorGUI.indentLevel--;

							}

							EditorGUI.indentLevel--;

						}

					}

					EditorGUI.indentLevel--;
				}

			}

		}

	}

}