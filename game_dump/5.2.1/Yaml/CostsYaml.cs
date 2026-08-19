using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class CostsYaml : Singleton<CostsYaml>
{
	[JsonProperty(PropertyName = "estate", Required = Required.Always)]
	public EstateCost Estate;

	[JsonProperty(PropertyName = "cargo", Required = Required.Always)]
	public CargoCost Cargo;

	[JsonProperty(PropertyName = "skill_untrain", Required = Required.Always)]
	public SkillUntrainInfo SkillUntrainVoucher;

	[JsonProperty(PropertyName = "balloon_ticket", Required = Required.Always)]
	public Cost BalloonTicket;

	[JsonProperty(PropertyName = "artifact_rename", Required = Required.Always)]
	public Cost ArtifactRename;

	[JsonProperty(PropertyName = "clan_warphole_visit", Required = Required.Always)]
	public Cost ClanWarpholeVisit;

	[JsonProperty(PropertyName = "pet_revert_active_skill", Required = Required.Always)]
	public Cost PetRevertActiveSkill;

	[JsonProperty(PropertyName = "pet_revert_milestone", Required = Required.Always)]
	public Cost PetRevertMilestone;

	[JsonProperty(PropertyName = "pet_revert_rank", Required = Required.Always)]
	public Cost PetRevertRank;

	[JsonProperty(PropertyName = "reset_reform_slot", Required = Required.Always)]
	public Cost ResetReformSlot;

	[JsonProperty(PropertyName = "expert_reform_estimate", Required = Required.Always)]
	public Cost ReformTechSupportEstimate;

	[JsonProperty(PropertyName = "encyclopedia_mastery_swap")]
	public Cost EncyclopediaMasterySwap;

	[JsonProperty(PropertyName = "artifact_extend_floor")]
	public Cost ArtifactExtendFloor;

	[JsonProperty(PropertyName = "purchase_r_piece")]
	public PurchaseRandomPiece RandomPiece;

	[JsonProperty(PropertyName = "revive_immediately")]
	public ReviveImmediatelyCost ReviveImmediately;

	[JsonProperty(PropertyName = "open_map")]
	public OpenMapCost OpenMap;
}
