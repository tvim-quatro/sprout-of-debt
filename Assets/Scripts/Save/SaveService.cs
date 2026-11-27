using SproutOfDebt.Core;
using SproutOfDebt.Core.Save;
using SproutOfDebt.Core.Stubs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SproutOfDebt.Save
{
    /// <summary>
    /// Saves the game automatically (GDD: after changing location, selling a potion, brewing, depositing into the Treasury,
    /// and on quit) and loads it again for Continue. Changes are collected for a moment and written once, so a burst of
    /// inventory events is a single write. It follows whichever wallet, inventory and debt service is registered.
    /// </summary>
    public class SaveService : MonoBehaviour, ISaveService
    {
        private const string LogPrefix = "[Save] ";
        private const string MainMenuScene = "MainMenu";

        [SerializeField, Min(0f)] private float autosaveDelaySeconds = 1f;

        private IWallet watchedWallet;
        private IInventory watchedInventory;
        private IDebtService watchedDebt;
        private ITutorialService watchedTutorial;
        private bool dirty;
        private float dirtySince;
        private string location;

        public bool HasSave => SaveFile.Exists;

        private bool GameStarted => GameServices.Wallet != null && GameServices.Inventory != null;

        private void OnEnable()
        {
            GameServices.Save = this;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            Application.quitting += OnQuitting;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(GameServices.Save, this))
            {
                GameServices.Save = null;
            }
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Application.quitting -= OnQuitting;
            Unwatch();
        }

        private void Update()
        {
            Watch();
            if (dirty && Time.realtimeSinceStartup - dirtySince >= autosaveDelaySeconds)
            {
                SaveNow();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveNow();
            }
        }

        public void SaveNow()
        {
            dirty = false;
            if (!GameStarted)
            {
                return;
            }

            IBrewedPotionLog brewed = GameServices.BrewedPotions;
            SaveData data = SaveData.Create(
                location,
                GameServices.Wallet.Coins,
                GameServices.Inventory.Counts,
                brewed != null ? brewed.BrewedPotionIds : System.Array.Empty<string>(),
                GameServices.Progress);

            if (SaveFile.TryWrite(JsonUtility.ToJson(data, true)))
            {
                Debug.Log(LogPrefix + "Saved to " + SaveFile.Path);
            }
        }

        public bool TryLoad(out string openLocation)
        {
            openLocation = null;
            if (!SaveFile.TryRead(out string json))
            {
                return false;
            }

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(LogPrefix + "The save file is damaged: " + exception.Message);
                return false;
            }
            if (data == null || data.Version > SaveData.CurrentVersion)
            {
                Debug.LogWarning(LogPrefix + "The save file cannot be used (version " + (data == null ? "?" : data.Version.ToString()) + ")");
                return false;
            }

            Apply(data);
            openLocation = data.Location;
            location = data.Location;
            Debug.Log(LogPrefix + "Loaded the save from " + data.SavedAtUtc);
            return true;
        }

        public void StartNewGame()
        {
            ResetServices();
            location = null;
            dirty = false;
            Debug.Log(LogPrefix + "New game");
        }

        private static void Apply(SaveData data)
        {
            ResetServices();
            GameServices.Progress = data.Progress;
            GameServices.Wallet = new StubWallet(data.Coins);

            StubInventory inventory = new StubInventory();
            foreach (ItemStack stack in data.Items)
            {
                inventory.Add(stack.ItemId, stack.Count);
            }
            GameServices.Inventory = inventory;

            StubBrewedPotionLog brewed = new StubBrewedPotionLog();
            foreach (string potionId in data.BrewedPotions)
            {
                brewed.MarkBrewed(potionId);
            }
            GameServices.BrewedPotions = brewed;
        }

        private static void ResetServices()
        {
            GameServices.Progress = new GameProgress();
            GameServices.Wallet = null;
            GameServices.Inventory = null;
            GameServices.BrewedPotions = null;
            GameServices.Debt = null;
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            if (next.name != MainMenuScene)
            {
                location = next.name;
            }
            SaveNow();
        }

        private void OnQuitting()
        {
            SaveNow();
        }

        private void MarkDirty()
        {
            if (!dirty)
            {
                dirty = true;
                dirtySince = Time.realtimeSinceStartup;
            }
        }

        private void Watch()
        {
            if (!ReferenceEquals(watchedWallet, GameServices.Wallet))
            {
                if (watchedWallet != null) watchedWallet.CoinsChanged -= OnCoinsChanged;
                watchedWallet = GameServices.Wallet;
                if (watchedWallet != null) watchedWallet.CoinsChanged += OnCoinsChanged;
            }
            if (!ReferenceEquals(watchedInventory, GameServices.Inventory))
            {
                if (watchedInventory != null) watchedInventory.ItemChanged -= OnItemChanged;
                watchedInventory = GameServices.Inventory;
                if (watchedInventory != null) watchedInventory.ItemChanged += OnItemChanged;
            }
            if (!ReferenceEquals(watchedDebt, GameServices.Debt))
            {
                if (watchedDebt != null) watchedDebt.Changed -= MarkDirty;
                watchedDebt = GameServices.Debt;
                if (watchedDebt != null) watchedDebt.Changed += MarkDirty;
            }
            if (!ReferenceEquals(watchedTutorial, GameServices.Tutorial))
            {
                if (watchedTutorial != null) watchedTutorial.StepChanged -= OnTutorialStepChanged;
                watchedTutorial = GameServices.Tutorial;
                if (watchedTutorial != null) watchedTutorial.StepChanged += OnTutorialStepChanged;
            }
        }

        private void Unwatch()
        {
            if (watchedWallet != null) watchedWallet.CoinsChanged -= OnCoinsChanged;
            if (watchedInventory != null) watchedInventory.ItemChanged -= OnItemChanged;
            if (watchedDebt != null) watchedDebt.Changed -= MarkDirty;
            if (watchedTutorial != null) watchedTutorial.StepChanged -= OnTutorialStepChanged;
            watchedWallet = null;
            watchedInventory = null;
            watchedDebt = null;
            watchedTutorial = null;
        }

        private void OnCoinsChanged(int coins)
        {
            MarkDirty();
        }

        private void OnItemChanged(string itemId, int count)
        {
            MarkDirty();
        }

        private void OnTutorialStepChanged(TutorialStep step)
        {
            MarkDirty();
        }
    }
}
