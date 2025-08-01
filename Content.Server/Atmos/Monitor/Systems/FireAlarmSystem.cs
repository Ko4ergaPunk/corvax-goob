// SPDX-FileCopyrightText: 2022 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Monitor.Components;
using Content.Server.DeviceNetwork.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Access.Systems;
using Content.Shared.Atmos.Monitor;
using Content.Shared.CCVar;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.Interaction;
using Content.Shared.Emag.Systems;
using Robust.Shared.Configuration;

namespace Content.Server.Atmos.Monitor.Systems;

public sealed class FireAlarmSystem : EntitySystem
{
    [Dependency] private readonly AtmosDeviceNetworkSystem _atmosDevNet = default!;
    [Dependency] private readonly AtmosAlarmableSystem _atmosAlarmable = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FireAlarmComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<FireAlarmComponent, DeviceListUpdateEvent>(OnDeviceListSync);
        SubscribeLocalEvent<FireAlarmComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnDeviceListSync(EntityUid uid, FireAlarmComponent component, DeviceListUpdateEvent args)
    {
        var query = GetEntityQuery<DeviceNetworkComponent>();
        foreach (var device in args.OldDevices)
        {
            if (!query.TryGetComponent(device, out var deviceNet))
            {
                continue;
            }

            _atmosDevNet.Deregister(uid, deviceNet.Address);
        }

        _atmosDevNet.Register(uid, null);
        _atmosDevNet.Sync(uid, null);
    }

    private void OnInteractHand(EntityUid uid, FireAlarmComponent component, InteractHandEvent args)
    {
        if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target))
            return;

        if (!_configManager.GetCVar(CCVars.FireAlarmAllAccess) && !_access.IsAllowed(args.User, args.Target))
            return;

        if (this.IsPowered(uid, EntityManager))
        {
            if (!_atmosAlarmable.TryGetHighestAlert(uid, out var alarm))
            {
                alarm = AtmosAlarmType.Normal;
            }

            if (alarm == AtmosAlarmType.Normal)
            {
                _atmosAlarmable.ForceAlert(uid, AtmosAlarmType.Danger);
            }
            else
            {
                _atmosAlarmable.ResetAllOnNetwork(uid);
            }
        }
    }

    private void OnEmagged(EntityUid uid, FireAlarmComponent component, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        if (!TryComp<AtmosAlarmableComponent>(uid, out var alarmable))
            return;

        // Remove the atmos alarmable component permanently from this device.
        _atmosAlarmable.ForceAlert(uid, AtmosAlarmType.Emagged, alarmable);
        RemCompDeferred<AtmosAlarmableComponent>(uid);
        args.Handled = true;
    }
}