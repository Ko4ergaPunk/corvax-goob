// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.UserInterface;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.NameIdentifier;
using Content.Shared.PowerCell.Components;
using Content.Shared.Preferences;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Configuration;

namespace Content.Server.Silicons.Borgs;

/// <inheritdoc/>
public sealed partial class BorgSystem
{
    // CCvar.
    private int _maxNameLength;

    public void InitializeUI()
    {
        SubscribeLocalEvent<BorgChassisComponent, BeforeActivatableUIOpenEvent>(OnBeforeBorgUiOpen);
        SubscribeLocalEvent<BorgChassisComponent, BorgEjectBrainBuiMessage>(OnEjectBrainBuiMessage);
        SubscribeLocalEvent<BorgChassisComponent, BorgEjectBatteryBuiMessage>(OnEjectBatteryBuiMessage);
        SubscribeLocalEvent<BorgChassisComponent, BorgSetNameBuiMessage>(OnSetNameBuiMessage);
        SubscribeLocalEvent<BorgChassisComponent, BorgRemoveModuleBuiMessage>(OnRemoveModuleBuiMessage);

        Subs.CVar(_cfgManager, CCVars.MaxNameLength, value => _maxNameLength = value, true);
    }

    private void OnBeforeBorgUiOpen(EntityUid uid, BorgChassisComponent component, BeforeActivatableUIOpenEvent args)
    {
        UpdateUI(uid, component);
    }

    private void OnEjectBrainBuiMessage(EntityUid uid, BorgChassisComponent component, BorgEjectBrainBuiMessage args)
    {
        if (component.BrainEntity is not { } brain)
            return;

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} removed brain {ToPrettyString(brain)} from borg {ToPrettyString(uid)}");
        _container.Remove(brain, component.BrainContainer);
        _hands.TryPickupAnyHand(args.Actor, brain);
        UpdateUI(uid, component);
    }

    private void OnEjectBatteryBuiMessage(EntityUid uid, BorgChassisComponent component, BorgEjectBatteryBuiMessage args)
    {
        if (TryEjectPowerCell(uid, component, out var ents))
            _hands.TryPickupAnyHand(args.Actor, ents.First());
    }

    private void OnSetNameBuiMessage(EntityUid uid, BorgChassisComponent component, BorgSetNameBuiMessage args)
    {
        if (args.Name.Length > _maxNameLength ||
            args.Name.Length == 0 ||
            string.IsNullOrWhiteSpace(args.Name) ||
            string.IsNullOrEmpty(args.Name))
        {
            return;
        }

        var name = args.Name.Trim();

        var metaData = MetaData(uid);

        // don't change the name if the value doesn't actually change
        if (metaData.EntityName.Equals(name, StringComparison.InvariantCulture))
            return;

        _adminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):player} set borg \"{ToPrettyString(uid)}\"'s name to: {name}");
        _metaData.SetEntityName(uid, name, metaData);
    }

    private void OnRemoveModuleBuiMessage(EntityUid uid, BorgChassisComponent component, BorgRemoveModuleBuiMessage args)
    {
        var module = GetEntity(args.Module);

        if (!component.ModuleContainer.Contains(module))
            return;

        if (!CanRemoveModule((uid, component), (module, Comp<BorgModuleComponent>(module)), args.Actor))
            return;

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} removed module {ToPrettyString(module)} from borg {ToPrettyString(uid)}");
        _container.Remove(module, component.ModuleContainer);
        _hands.TryPickupAnyHand(args.Actor, module);

        UpdateUI(uid, component);
    }

    public void UpdateUI(EntityUid uid, BorgChassisComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var chargePercent = 0f;
        var hasBattery = false;
        if (_powerCell.TryGetBatteryFromSlot(uid, out var battery))
        {
            hasBattery = true;
            chargePercent = battery.CurrentCharge / battery.MaxCharge;
        }

        var state = new BorgBuiState(chargePercent, hasBattery);
        _ui.SetUiState(uid, BorgUiKey.Key, state);
    }
}