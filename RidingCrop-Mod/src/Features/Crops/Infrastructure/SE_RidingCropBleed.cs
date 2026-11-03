using KydlCore.Common;
using UnityEngine;

namespace RidingCrop
{
    /// <summary>
    /// Bleeding: deals a fixed amount of damage once per second until the effect runs out.
    /// Modelled on the game's own burning/poison effects: it hurts the target through Character.ApplyDamage.
    /// The damage is the typeless "damage" kind, so armor and elemental resistances don't reduce it.
    /// Bleeds never stack, not even between different crops: while a target is still bleeding, further hits neither start a
    /// second bleed (see CanAdd) nor restart or refresh the running one (see ResetTime). Once the last tick has been dealt,
    /// the next hit starts a new bleed.
    /// </summary>
    public class SE_RidingCropBleed : StatusEffect
    {
        // Public on purpose: the game clones status effects when it applies them, and public fields are copied along.
        public float m_damagePerTick = 10f;
        public float m_tickInterval = 1f;
        public int m_ticksTotal = 5;

        private float _tickTimer;
        private int _ticksDone;
        private Character _attacker;   // who made the target bleed (set by the game right after the effect is applied)

        // True while this bleed still has damage ticks left to deal.
        private bool IsBleeding
        {
            get { return _ticksDone < m_ticksTotal; }
        }

        // Without this the bleed ticks have no attacker, so a kill by bleeding gave the player no credit (kill/XP/aggro).
        public override void SetAttacker(Character attacker)
        {
            base.SetAttacker(attacker);
            _attacker = attacker;
        }

        public void Configure(float damagePerSecond, float durationSeconds)
        {
            m_tickInterval = 1f;
            m_damagePerTick = damagePerSecond * m_tickInterval;
            m_ticksTotal = Mathf.Max(1, Mathf.RoundToInt(durationSeconds / m_tickInterval));
            // A little longer than the last tick so floating point never removes the effect one frame before its final tick.
            m_ttl = m_ticksTotal * m_tickInterval + 0.5f * m_tickInterval;
        }

        public override void Setup(Character character)
        {
            base.Setup(character);
            _tickTimer = m_tickInterval;
            _ticksDone = 0;
            ModLog.Verbose("[bleed] '" + name + "' started on '" + (character != null ? character.name : "null") + "': " + m_damagePerTick.ToString("0.#") + " x " + m_ticksTotal);
        }

        // The game calls this when a target that already has THIS crop's bleed is hit again. A bleed that is still running is
        // left alone (no restart, no refresh). The effect object lingers a short moment after its last tick; a hit in that
        // moment starts a fresh bleed.
        public override void ResetTime()
        {
            if (IsBleeding)
            {
                ModLog.Verbose("[bleed] '" + name + "' hit again while bleeding: ignored.");
                return;
            }

            base.ResetTime();
            _ticksDone = 0;
            _tickTimer = m_tickInterval;
        }

        // The game calls this before adding the effect to a target that does NOT have this exact effect yet. Every crop tier
        // has its own effect, so without this check a different crop could start a second, parallel bleed.
        public override bool CanAdd(Character character)
        {
            if (character == null)
            {
                return base.CanAdd(character);
            }

            SEMan seMan = character.GetSEMan();
            var active = seMan != null ? seMan.GetStatusEffects() : null;
            if (active != null)
            {
                foreach (StatusEffect effect in active)
                {
                    var other = effect as SE_RidingCropBleed;
                    if (other != null && other.IsBleeding)
                    {
                        ModLog.Verbose("[bleed] '" + name + "' not applied to '" + character.name + "': '" + other.name + "' is still bleeding.");
                        return false;
                    }
                }
            }

            return base.CanAdd(character);
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            if (m_character == null || _ticksDone >= m_ticksTotal)
            {
                return;
            }

            _tickTimer -= dt;
            if (_tickTimer > 0f)
            {
                return;
            }

            _tickTimer += m_tickInterval;
            _ticksDone++;
            if (ModSettings.DebugOn)
            {
                ModLog.Verbose("[bleed] '" + name + "' tick " + _ticksDone + "/" + m_ticksTotal + " on '" + m_character.name + "' for " + m_damagePerTick.ToString("0.#"));
            }

            var hit = new HitData();
            hit.m_point = m_character.GetCenterPoint();
            hit.m_damage.m_damage = m_damagePerTick;
            hit.m_hitType = HitData.HitType.Undefined;
            if (_attacker != null)
            {
                hit.SetAttacker(_attacker);
            }

            m_character.ApplyDamage(hit, true, false, HitData.DamageModifier.Normal);
        }
    }
}
