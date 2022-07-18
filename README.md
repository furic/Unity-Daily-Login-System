# Unity-Daily-Login-System
A simple reward system that gets the players coming back every day or so. In this article, I'll show you in detail how to develop a local-stored daily login system in Unity, using the MVC approach.

A overview of the module can be found [here](https://www.richardfu.net/create-a-daily-login-reward-system-in-unity/).

We probably see a daily login reward system in every game nowadays. It's a simple reward system that gets the players coming back every day or so. In this article, I'll show you in detail how to develop a local-stored daily login system in Unity, using the MVC approach.

## What's Daily Login
--------------

Daily logic rewards are the rewards that can be claimed by players each day they start the game. We store the last login (app launch) DateTime of a player and the number of days the player has logged in, then we can determine if the player can claim today, and which reward of the day to be claimed.

There are 3 ways how days are counted:

*   Login streak: Whenever the player misses a day, he will need to restart the reward stack from day 1. The longer continuous days player normally has better rewards.
*   Monthly calendar: Each day is matching the exact day in a calendar month. Missing a day simply means the player is missing the reward of that day.
*   Day-by-day: Similar to login stack but without the stack restriction. Missing a day would not affect anything. For example, if the player didn't log in on the 2nd day, he can still claim the 2nd reward on the 3rd day when he logs in. The simplest and wildly used in most games.

![](https://www.richardfu.net/wp-content/uploads/monthly_calendar_login_streak_dragon_city-1-1024x576.jpg)
*Login streak (right) and monthly calendar (left) in Dragon City*

![](https://www.richardfu.net/wp-content/uploads/day_to_day_no_humanity-1-473x1024.jpg)
*Day-by-day in No Humanity*

> Some games have multiplie login rewards with differenent obtain-ways. E.g. having login stack with monthly calendar, since they can work independently.

We will make some Unity settings so the desired day counting can be used depending on the needs.

## File Overviews
--------------

_DailyLoginSettings.cs_ - `ScriptableObject` containing settings values that can be shown/set in Unity Inspector.

_DailyLoginSettingsEditor.cs_ - A custom inspector to set the values for `DailyLoginSettings`.

_DailyLoginManager.cs_ - Master controller for all save/load/display logic.

_DailyLoginPanel.cs_ - The main UI panel displaying the daily login rewards.

_DailyLoginSlot.cs_ - The UI slot displaying the login reward of a day.

_DailyLoginButton.cs_ - The button that shows the daily login panel on click with notification icon.

MVC Design
----------

In other articles, I've mentioned how we use MVC in our games, this is a great and simple example of how we do it.

**MODEL** - Because there are only two stored values (last login and number of logged-in days), we can keep it simple by using `PlayerPrefs` in Unity and there's no need to create a dedicated class for MODEL. For the larger modules, we would like to keep the values in a class and save them with persistent data. Note that DailyLoginSettings is more like a constant or environment class instead because it's not storing data in user-scope but in application-scope.

**VIEW** - All panel, slot and button classes. Note that UIs/View shouldn't contain any logic and simply make calls to and listen to callback from the controller class.

**CONTROLLER** - The master `DailyLoginManager` in this case without a doubt. It contains all logic such as loading the MODEL (PlayerPrfs in this case), instantiating the rewards and collecting the reward on user click.

Script Highlights
-----------------

_DailyLoginSettings_

```
		// The cycle days, normally 7 days a cycle
		public int cycleDays = 7;
		// Rewards for one cycle only and never shown, or keep looping the cycle
		public bool oneCycleOnly = false;
		// Has a different cycle after 1st cycle
		public bool differentFirstCycle = true;
		// If missing the reward if not login that day; false if keep consecutive rewards no-matter want day logging in
		public bool missRewardIfNotLogin = false;

		// The rewards for 1st cycle
		public Reward\[\] firstCycleRewards = new Reward\[0\];
		// The rewards for the non-1st cycle, only used when differentFirstCycle is true
		public Reward\[\] rewards = new Reward\[0\];
```

With these setting values, we can set up most reward types, e.g.:

*   New player 5 days rewards - 5 logged-in days only, missed a day will still get the reward of the missing days  
    `cycleDays=5`, `oneCycleOnly=true`, `missRewardIfNotLogin=false`
*   Continuous weekly rewards - loop every 7 days, the first week has better rewards.  
    `cycleDays=7`, `oneCycleOnly=false`, `missRewardIfNotLogin=true`

_DailyLoginManager:_

```
		private const string PREFS\_LOGIN\_START\_DATE = "DailyLoginStartDate";
		private const string PREFS\_DAILY\_LOGIN\_COUNT = "DailyLoginCount";
		private const string PREFS\_LAST\_DAILY\_LOGIN\_DATE = "LastDailyLoginDate";
		private const string PREFS\_DAILY\_LOGIN\_ENABLED = "DailyLoginEnabled";
		private const string PREFS\_DAILY\_LOGIN\_SCENE\_LOADED\_COUNT = "DailyLoginSceneLoadedCount";
```

Here we need to save 5 values:

*   `DailyLoginStartDate` - The date (no time) that the app is first launched, so we know what is the day of the reward, only used when `missRewardIfNotLogin` is true.
*   `DailyLoginCount` - The number of days that logged in before, only used when `missRewardIfNotLogin` is false.
*   LastDailyLoginDate - The last date that reward is contained, so no multiple rewards can be contained in one day.
*   DailyLoginEnabled - Used to temporarily disable daily login, in tutorials for example.
*   DailyLoginSceneLoadedCount - Used to auto-enable daily login, in 3rd app load for example.
