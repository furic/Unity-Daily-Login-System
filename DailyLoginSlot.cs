using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RFGames.UI
{

	/// <summary>
	/// An daily login slot in daily login panel, each slot represent a day. 
	/// </summary>
	public class DailyLoginSlot : Slot<Reward>
	{

		#region Variables

		[Header("UI")]
		[SerializeField] protected Transform _rewardHolderTF;
		[SerializeField] protected Transform _bonusRewardHolderTF;

		[SerializeField] protected Animation _rewardReadyAnim;
		[SerializeField] protected GameObject _rewardEffectGO;

		[SerializeField] protected GameObject _obtainedGO;
		[SerializeField] protected GameObject _maskGO;

		[SerializeField] protected GameObject _doubleRewardGO;

		[SerializeField] protected Text _dayText;

		[Header("Settings")]
		[SerializeField] protected bool _closePanelOnClaim = true;

		// Internal Cached
		[NonSerialized] protected Reward _cachedReward;
		[NonSerialized] protected List<Sprite> _cachedSprites = new List<Sprite>();

		#endregion

		#region OnEnable / OnDisable

		protected override void OnEnable()
		{
			_rewardReadyAnim.Stop(); // Make sure animation stoped
		}

		protected override void OnDisable()
		{
			if (_doubleRewardGO != null)
				_doubleRewardGO.SetActive(false);

			if (_obtainedGO.activeSelf) // Just make sure obtained image is enabled
				_obtainedGO.GetComponent<Image>().enabled = true;

			// Make sure we delete any lingering Textures / Sprites
			DestroyPrefabIcons();
		}

		#endregion

		#region Set

		public override void Set(Reward reward)
		{
			// Populate our reward parent
			PopulateRewardParent(_rewardHolderTF, reward);
			PopulateRewardParent(_bonusRewardHolderTF, reward);

			_obtainedGO.SetActive(false);
			_maskGO.SetActive(false);
			_rewardEffectGO.SetActive(false);
		}

		private void PopulateRewardParent(Transform parent, Reward reward)
		{
			// Disable all of our reward children
			parent.DisableChildren();

			foreach (Item item in reward.items) {
				Transform tf = parent.Find(item.type.ToString());
				if (tf != null) {
					tf.gameObject.SetActive(true);
					Transform amountTF = tf.Find("Amount");
					if (amountTF != null) {
						Text amountText = amountTF.GetComponent<Text>();
						if (amountText != null)
							amountText.text = item.amountString;
					}
					if (item.type != ItemType.Coin && item.type != ItemType.Gem) { // Try get the icon
						if (item.iconSprite != null) {
							Transform iconImageTF = tf.Find("ImageIcon");
							if (iconImageTF != null) {
								Image iconImage = iconImageTF.GetComponent<Image>();
								if (iconImage != null)
									iconImage.sprite = item.iconSprite;
							}
						} else if (item.iconPrefab != null) {
							Transform iconTF = tf.Find("PrefabIcon");
							if (iconTF != null) {
								// For now we just take a photo of our icon prefab rather than actually placing in the UI.
								// Solves alot of issues with layering and showing in canvas space when in overlay mode
								Image iconImage = iconTF.GetOrAddComponent<Image>();
								TimeManager.Invoke(() => {
									CreatePrefabIcon(item.iconPrefab, iconImage);
								}, transform.GetSiblingIndex() * 0.25f);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Other Sets

		public virtual void SetObtained()
		{
			_obtainedGO.SetActive(true);
			_obtainedGO.GetComponent<Image>().enabled = true; // Just to make sure
			_bonusRewardHolderTF.gameObject.SetActive(true);
		}

		public virtual void SetMask()
		{
			_maskGO.SetActive(true);
		}

		public virtual void SetObtainable()
		{
			_rewardReadyAnim.Play();
			_bonusRewardHolderTF.gameObject.SetActive(false);
		}

		public virtual void SetDoubleRewardEnabled()
		{
			if (_doubleRewardGO != null)
				_doubleRewardGO.SetActive(true);

			if (_bonusRewardHolderTF != null)
				_bonusRewardHolderTF.gameObject.SetActive(true);
		}

		public virtual void SetDay(int day, bool isToday)
		{
			if (isToday)
				_dayText.text = "Today".Localize(TermCategory.DailyLogin);
			else
				_dayText.text = "Day {0}".LocalizeFormat(TermCategory.DailyLogin, day);
		}

		#endregion

		#region UI Callbacks

		public virtual void OnClick()
		{
			// Do a sound and animation
			_rewardReadyAnim.Stop();
			SoundManager.PlaySFX("Ding2");
			_rewardEffectGO.SetActive(true);

			if (_doubleRewardGO != null && _doubleRewardGO.activeSelf) {
				// If we are able to claim our reward again for a double reward gain
				ObtainRewardDouble();
			} else {
				// Ignore if clicking this's previous or future reward
				if (_obtainedGO.activeSelf || _maskGO.activeSelf) return;
				// Attempt to obtain our normal reward and set up for a double reward
				ObtainRewardDefault();
				AchievementManager.Report("DailyLogin", 1);
			}
		}

		protected virtual void ObtainRewardDefault()
		{
			// Cache the reward for double reward, then Claim our reward
			_cachedReward = DailyLoginManager.currentReward;
			DailyLoginManager.ObtainTodayReward();

			SetObtained();

			// Close the daily login panel if needed
			if (_closePanelOnClaim)
				DailyLoginManager.ToggleUI(false);

			// Analytics
			GameAnalytics.ClaimDailyLogin(DailyLoginManager.totalDays, _cachedReward, false);
		}

		protected virtual void ObtainRewardDouble()
		{
			AdsManager.ShowRewardedVideo(AdTag.DailyLogin, () => {
				_cachedReward.Obtain("daily_login"); // Obtain again
				if (_closePanelOnClaim)
					DailyLoginManager.ToggleUI(false);
				SetObtained();
				if (_doubleRewardGO != null)
					_doubleRewardGO.SetActive(false);
				GameAnalytics.ClaimDailyLogin(DailyLoginManager.totalDays, _cachedReward, true);
			});
		}

		#endregion

		#region Prefab Icon

		protected void CreatePrefabIcon(GameObject prefab, Image image)
		{
			// Then get our sprite
			Sprite prefabSprite = PrefabToImage.GetSpriteWithAlphaInstant(prefab, 64, 64, false, Vector3.back);

			// Initialize
			_cachedSprites.Add(prefabSprite);
			image.sprite = prefabSprite;
		}

		protected void DestroyPrefabIcons()
		{
			foreach (Sprite cachedSprite in _cachedSprites)
				PrefabToImage.DestroySprite(cachedSprite);
			_cachedSprites.Clear();
		}

		#endregion

	}

}