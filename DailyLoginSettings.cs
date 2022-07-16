using UnityEngine;

namespace RFGames
{

	/// <summary>
	/// Daily Login module settings.
	/// </summary>
	[CreateAssetMenu(fileName = "DailyLoginSettings", menuName = "RF Games/Settings/Daily Login")]
	public class DailyLoginSettings : ScriptableObjectSingleton<DailyLoginSettings>
	{

		// Prompt UI on state automatically
		public bool promptOnState = true;
		// What state we should attempt to prompt on
		public State checkState = State.Menu;
		// Prompt only after scene loaded x times, used to avoid prompting in tutorial for example
		public int promptAfterSceneLoadCount = 3;
		// The cycle days, normally 7 days a cycle
		public int cycleDays = 7;
		// Rewards for one cycle only and never shown, or keep looping the cycle
		public bool oneCycleOnly = false;
		// Has a different cycle after 1st cycle
		public bool differentFirstCycle = true;
		// If missing the reward if not login that day; false if keep consecutive rewards no-matter want day logging in
		public bool missRewardIfNotLogin = false;

		// The rewards for 1st cycle
		public Reward[] firstCycleRewards = new Reward[0];
		// The rewards for the non-1st cycle, only used when differentFirstCycle is true
		public Reward[] rewards = new Reward[0];

#if UNITY_EDITOR
		[UnityEditor.MenuItem("RF Games/Settings/Daily Login")]
		private static void OpenSettings()
		{
			current.OpenAssetsFile();
		}
#endif

	}

}