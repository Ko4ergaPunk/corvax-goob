// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leo <lzimann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Firewatch <54725557+musicmanvr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <koolthunder019@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Corvax.Interfaces.Shared;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Preferences;
using Robust.Client;
using Robust.Client.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Lobby
{
    /// <summary>
    ///     Receives <see cref="PlayerPreferences" /> and <see cref="GameSettings" /> from the server during the initial
    ///     connection.
    ///     Stores preferences on the server through <see cref="SelectCharacter" /> and <see cref="UpdateCharacter" />.
    /// </summary>
    public sealed class ClientPreferencesManager : IClientPreferencesManager
    {
        [Dependency] private readonly IClientNetManager _netManager = default!;
        [Dependency] private readonly IBaseClient _baseClient = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        private ISharedSponsorsManager? _sponsorsManager; // CorvaxGoob-Sponsors

        public event Action? OnServerDataLoaded;

        public GameSettings Settings { get; private set; } = default!;
        public PlayerPreferences Preferences { get; private set; } = default!;

        public void Initialize()
        {
            IoCManager.Instance!.TryResolveType(out _sponsorsManager); // CorvaxGoob-Sponsors
            _netManager.RegisterNetMessage<MsgPreferencesAndSettings>(HandlePreferencesAndSettings);
            _netManager.RegisterNetMessage<MsgUpdateCharacter>();
            _netManager.RegisterNetMessage<MsgSelectCharacter>();
            _netManager.RegisterNetMessage<MsgDeleteCharacter>();

            _baseClient.RunLevelChanged += BaseClientOnRunLevelChanged;
        }

        private void BaseClientOnRunLevelChanged(object? sender, RunLevelChangedEventArgs e)
        {
            if (e.NewLevel == ClientRunLevel.Initialize)
            {
                Settings = default!;
                Preferences = default!;
            }
        }

        public void SelectCharacter(ICharacterProfile profile)
        {
            SelectCharacter(Preferences.IndexOfCharacter(profile));
        }

        public void SelectCharacter(int slot)
        {
            Preferences = new PlayerPreferences(Preferences.Characters, slot, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);
            var msg = new MsgSelectCharacter
            {
                SelectedCharacterIndex = slot
            };
            _netManager.ClientSendMessage(msg);
        }

        public void UpdateCharacter(ICharacterProfile profile, int slot)
        {
            var collection = IoCManager.Instance!;
            // CorvaxGoob-Sponsors-Start
            var sponsorPrototypes = _sponsorsManager?.GetClientPrototypes().ToArray() ?? [];
            profile.EnsureValid(_playerManager.LocalSession!, collection, sponsorPrototypes);
            // CorvaxGoob-Sponsors-End
            var characters = new Dictionary<int, ICharacterProfile>(Preferences.Characters) {[slot] = profile};
            Preferences = new PlayerPreferences(characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);
            var msg = new MsgUpdateCharacter
            {
                Profile = profile,
                Slot = slot
            };
            _netManager.ClientSendMessage(msg);
        }

        public void CreateCharacter(ICharacterProfile profile)
        {
            var characters = new Dictionary<int, ICharacterProfile>(Preferences.Characters);
            var lowest = Enumerable.Range(0, Settings.MaxCharacterSlots)
                .Except(characters.Keys)
                .FirstOrNull();

            if (lowest == null)
            {
                throw new InvalidOperationException("Out of character slots!");
            }

            var l = lowest.Value;
            characters.Add(l, profile);
            Preferences = new PlayerPreferences(characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);

            UpdateCharacter(profile, l);
        }

        public void DeleteCharacter(ICharacterProfile profile)
        {
            DeleteCharacter(Preferences.IndexOfCharacter(profile));
        }

        public void DeleteCharacter(int slot)
        {
            var characters = Preferences.Characters.Where(p => p.Key != slot);
            Preferences = new PlayerPreferences(characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, Preferences.ConstructionFavorites);
            var msg = new MsgDeleteCharacter
            {
                Slot = slot
            };
            _netManager.ClientSendMessage(msg);
        }

        public void UpdateConstructionFavorites(List<ProtoId<ConstructionPrototype>> favorites)
        {
            Preferences = new PlayerPreferences(Preferences.Characters, Preferences.SelectedCharacterIndex, Preferences.AdminOOCColor, favorites);
            var msg = new MsgUpdateConstructionFavorites
            {
                Favorites = favorites
            };
            _netManager.ClientSendMessage(msg);
        }

        private void HandlePreferencesAndSettings(MsgPreferencesAndSettings message)
        {
            Preferences = message.Preferences;
            Settings = message.Settings;

            OnServerDataLoaded?.Invoke();
        }
    }
}