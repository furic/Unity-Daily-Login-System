using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RFGames.UI
{
	[RequireComponent(typeof(Button))]
	public class DailyLoginButton : UIBehaviour
	{

		[SerializeField] private GameObject _notificationIconGO;

		protected override void Awake()
		{
			DailyLoginManager.rewardObtained += OnEnable;
			GetComponent<Button>().onClick.AddListener(OnClick);
		}

		protected override void OnDestroy()
		{
			DailyLoginManager.rewardObtained -= OnEnable;
		}

		protected override void OnEnable()
		{
			_notificationIconGO?.SetActive(!DailyLoginManager.isTodayRewardObtained);
		}

		private void OnClick()
		{
			if (DateTimeUtils.serverTimeObtained) {
				DailyLoginManager.ToggleUI();
			} else {
				new Message {
					title = CommonTexts.MSG_CHECK_INTERNET_TITLE.Localize(TermCategory.Message),
					content = "We need internet to check the daily login rewards.".Localize(TermCategory.DailyLogin) + "\n" +
					CommonTexts.MSG_CHECK_INTERNET_CONTENT.Localize(TermCategory.Message),
					confirmCallback = UnbiasedTime.Instance.UpdateNetworkTimeOffset
				}.Show();
			}
		}

	}

}