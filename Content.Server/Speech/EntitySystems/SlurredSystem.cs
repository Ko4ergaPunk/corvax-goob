// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Waylon Cude <waylon.cude@finzdani.net>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text;
using Content.Server.Speech.Components;
using Content.Shared.Drunk;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Speech.EntitySystems;

public sealed class SlurredSystem : SharedSlurredSystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;



    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string SlurKey = "SlurredSpeech";

    public override void Initialize()
    {
        SubscribeLocalEvent<SlurredAccentComponent, AccentGetEvent>(OnAccent);
    }

    public override void DoSlur(EntityUid uid, TimeSpan time, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (!_statusEffectsSystem.HasStatusEffect(uid, SlurKey, status))
            _statusEffectsSystem.TryAddStatusEffect<SlurredAccentComponent>(uid, SlurKey, time, true, status);
        else
            _statusEffectsSystem.TryAddTime(uid, SlurKey, time, status);
    }

    /// <summary>
    ///     Slur chance scales with "drunkeness", which is just measured using the time remaining on the status effect.
    /// </summary>
    private float GetProbabilityScale(EntityUid uid)
    {
        if (!_statusEffectsSystem.TryGetTime(uid, SharedDrunkSystem.DrunkKey, out var time))
            return 0;

        var curTime = _timing.CurTime;
        var timeLeft = (float) (time.Value.Item2 - curTime).TotalSeconds;
        return Math.Clamp((timeLeft - 80) / 1100, 0f, 1f);
    }

    private void OnAccent(EntityUid uid, SlurredAccentComponent component, AccentGetEvent args)
    {
        var scale = GetProbabilityScale(uid);
        args.Message = Accentuate(args.Message, scale);
    }

    private string Accentuate(string message, float scale)
    {
        var sb = new StringBuilder();

        // This is pretty much ported from TG.
        foreach (var character in message)
        {
            if (_random.Prob(scale / 3f))
            {
                var lower = char.ToLowerInvariant(character);
                var newString = lower switch
                {
                    'o' => "u",
                    's' => "ch",
                    'a' => "ah",
                    'u' => "oo",
                    'c' => "k",
                    // CorvaxGoob-Localization Start
                    'о' => "а",
                    'к' => "кх",
                    'щ' => "шч",
                    'ц' => "тс",
                    // CorvaxGoob-Localization End
                    _ => $"{character}",
                };

                sb.Append(newString);
            }

            if (_random.Prob(scale / 20f))
            {
                if (character == ' ')
                {
                    sb.Append(Loc.GetString("slur-accent-confused"));
                }
                else if (character == '.')
                {
                    sb.Append(' ');
                    sb.Append(Loc.GetString("slur-accent-burp"));
                }
            }

            if (!_random.Prob(scale * 3/20))
            {
                sb.Append(character);
                continue;
            }

            var next = _random.Next(1, 3) switch
            {
                1 => "'",
                2 => $"{character}{character}",
                _ => $"{character}{character}{character}",
            };

            sb.Append(next);
        }

        return sb.ToString();
    }
}
