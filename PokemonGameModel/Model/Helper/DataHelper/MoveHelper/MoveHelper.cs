using PokemonGame.Services.Enums.MovesEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Core.Model.Helper.DataHelper.MoveHelper
{
	public static class MoveHelper 
	{
		public static bool HasSecondaryEffects(MoveEffectType effect)
		{
			switch (effect)
			{
				case MoveEffectType.Hit__MaybeBurn:
				case MoveEffectType.Hit__MaybeBurn__10PercentFlinch:
				case MoveEffectType.Hit__MaybeConfuse:
				case MoveEffectType.Hit__MaybeFlinch:
				case MoveEffectType.Hit__MaybeFreeze:
				case MoveEffectType.Hit__MaybeFreeze__10PercentFlinch:
				case MoveEffectType.Hit__MaybeLowerTarget_ACC_By1:
				case MoveEffectType.Hit__MaybeLowerTarget_ATK_By1:
				case MoveEffectType.Hit__MaybeLowerTarget_DEF_By1:
				case MoveEffectType.Hit__MaybeLowerTarget_SPATK_By1:
				case MoveEffectType.Hit__MaybeLowerTarget_SPDEF_By1:
				case MoveEffectType.Hit__MaybeLowerTarget_SPDEF_By2:
				case MoveEffectType.Hit__MaybeLowerTarget_SPE_By1:
				case MoveEffectType.Hit__MaybeLowerUser_ATK_DEF_By1:
				case MoveEffectType.Hit__MaybeLowerUser_DEF_SPDEF_By1:
				case MoveEffectType.Hit__MaybeLowerUser_SPATK_By2:
				case MoveEffectType.Hit__MaybeLowerUser_SPE_By1:
				case MoveEffectType.Hit__MaybeLowerUser_SPE_DEF_SPDEF_By1:
				case MoveEffectType.Hit__MaybeParalyze:
				case MoveEffectType.Hit__MaybeParalyze__10PercentFlinch:
				case MoveEffectType.Hit__MaybePoison:
				case MoveEffectType.Hit__MaybeRaiseUser_ATK_By1:
				case MoveEffectType.Hit__MaybeRaiseUser_ATK_DEF_SPATK_SPDEF_SPE_By1:
				case MoveEffectType.Hit__MaybeRaiseUser_DEF_By1:
				case MoveEffectType.Hit__MaybeRaiseUser_SPATK_By1:
				case MoveEffectType.Hit__MaybeRaiseUser_SPE_By1:
				case MoveEffectType.Hit__MaybeToxic:
				case MoveEffectType.Snore: return true;
				default: return false;
			}
		}
		public static bool IsHPDrainMove(MoveEffectType effect)
		{
			switch (effect)
			{
				case MoveEffectType.HPDrain:
				case MoveEffectType.HPDrain__RequireSleep: return true;
				default: return false;
			}
		}
		public static bool IsHPRestoreMove(MoveEffectType effect)
		{
			switch (effect)
			{
				case MoveEffectType.Rest:
				case MoveEffectType.RestoreTargetHP: return true;
				default: return false;
			}
		}
		public static bool IsMultiHitMove(MoveEffectType effect) // TODO: TripleKick
		{
			switch (effect)
			{
				case MoveEffectType.Hit__2Times:
				case MoveEffectType.Hit__2Times__MaybePoison:
                case MoveEffectType.Hit__2To5Times: return true;
				default: return false;
			}
		}
		public static bool IsRecoilMove(MoveEffectType effect) // TODO: JumpKick/HiJumpKick
		{
			switch (effect)
			{
				case MoveEffectType.Recoil:
				case MoveEffectType.Recoil__10PercentBurn:
				case MoveEffectType.Recoil__10PercentParalyze: return true;
				default: return false;
			}
		}
		public static bool IsSetDamageMove(MoveEffectType effect)
		{
			switch (effect)
			{
				case MoveEffectType.Endeavor:
				case MoveEffectType.FinalGambit:
				case MoveEffectType.OneHitKnockout:
				case MoveEffectType.Psywave:
				case MoveEffectType.SeismicToss:
				case MoveEffectType.SetDamage:
				case MoveEffectType.SuperFang: return true;
				default: return false;
			}
		}
		public static bool IsSpreadMove(MoveTargetType targets)
		{
			switch (targets)
			{
				case MoveTargetType.All:
				case MoveTargetType.AllFoes:
				case MoveTargetType.AllFoesSurrounding:
				case MoveTargetType.AllSurrounding:
				case MoveTargetType.AllTeam: return true;
				default: return false;
			}
		}
		public static bool IsWeatherMove(MoveEffectType effect)
		{
			switch (effect)
			{
				case MoveEffectType.Hail:
				case MoveEffectType.RainDance:
				case MoveEffectType.Sandstorm:
				case MoveEffectType.SunnyDay: return true;
				default: return false;
			}
		}

		/// <summary>Temporary check to see if a move is usable, can be removed once all moves are added</summary>
		//public static bool IsMoveUsable(MoveData move)
		//{
		//	return PBEDataProvider.Instance.GetMoveData(move, cache: false).IsMoveUsable();
		//}
		/// <summary>Temporary check to see if a move is usable, can be removed once all moves are added</summary>
		public static bool IsMoveUsable(MoveEffectType effect)
		{
			return effect != MoveEffectType.TODOMOVE && effect != MoveEffectType.Sketch;
		}
	}
}
