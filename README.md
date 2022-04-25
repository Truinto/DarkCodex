# DarkCodex
Mod for Pathfinder: Wrath of the Righteous

:construction: <span style="color:red">**This mod is under development! Some features are untested and work not properly, cause crashes, or corrupt your save.**</span> :construction:

Index
-----------
* [Disclaimers](#disclaimers)
* [Installation](#installation)
* [Contact](#contact)
* [Highlight: Ability Groups](#highlight-ability-groups)
* [Content](#content)
* [FAQ](#faq)

Disclaimers
-----------
* This mod will affect your save! Uninstalling it will break your save.
* I do not take any responsibility for broken saves or any other damage. Use this software at your own risk.
* Please DON'T REPORT BUGS you encounter to Owlcat Games while mods are active.
* BE AWARE that all mods are WIP and may fail.

Installation
-----------
* You will need [Unity Mod Manager](https://www.nexusmods.com/site/mods/21).
* Follow the installation procedure (tl;dr; select game, select folder, press install).
* Download a release or rebuild your own [https://github.com/Truinto/DarkCodex/releases](https://github.com/Truinto/DarkCodex/releases).
* Switch to the mod tab and drop the zip file into the manager.

Contact
-----------
@Fumihiko me on the Owlcat Pathfinder discord: [https://discord.com/invite/wotr](https://discord.com/invite/wotr)

Highlight: Ability Groups
-----------
Ability Groups let you bundle abilities and activatables into a single foldable actionbar slot. \
![example group](/resources/example-group.jpg) \
There are already some predefined groups for class features that are related to each other or use the same resource (like bard songs).
You can place the group on your action bar or open the folding view to place the abilities on your action bar directly. \
The groups are defined in this file: "Mods/DarkCodex/DefGroups.json"

Each group has these properties:
- Title: Name of the group; must be unique
- Description: Text displayed in the group's body; can be empty/null
- Icon: Icon to use for this group; if icon is null, it will display the first active activatable or, if non are active, the first ability's icon
- Guids: a identifier list of all abilities/activatables that are related to that group; if a guid is used in multiple groups, only the first group will apply

You may edit this file and reload the groups with the button in the mod's menu. There is also a button to unlock the groups. 
This will display all groups for all characters, even if they have no matching abilities. It also allows you to add/remove abilities with drag&drop mechanic.
Simply drag any ability on the group's icon to add it. \
![add ability](/resources/adding-ability.jpg) \
Unfold a group and drag an ability from it onto the map to remove it again. \
![remove ability](/resources/remove-ability.jpg) \
Drag an ability onto another ability to create a new group. You will be prompted to give a new unique title name. \
![creating group](/resources/creating-group.jpg) \
To delete a group altogether, drag the group onto the map. This will also prompt a confirmation. \
![delete group](/resources/delete-group.jpg) \
Changes will affect all party members equally. Remember to disable 'Unlock Groups' again, otherwise you might edit them unintentionally.

If you want to use *only* this feature from DarkCodex do the following:
* install the mod like normal
* boot up the game
* open the mod's menu
* set 'New features default on' to ✖ which should change all patches to ✖ as well
* scroll down and enable 'Patch: Ability Groups' ✔
* reboot the game

Content
-----------
| Option | Description | HB | Status |
|--------|-------------|----|--------|
|General.CreateAbilityFocus|basic feat: Ability Focus, increase DC of one ability by +2|:book:|:x:|
|General.CreateBardStopSong|adds ability to stop any activatable immediately|:book:|:heavy_check_mark:|
|General.CreateMadMagic|combat feat: allows spell casting during a rage|:book:|:heavy_check_mark:|
|General.CreatePreferredSpell|basic feat: Preferred Spell, spontaneously cast a specific spell|:book:|:heavy_check_mark:|
|General.PatchAngelsLight|'Light of the Angels' give temporary HP equal to character level|:house:|:heavy_check_mark:|
|General.PatchBasicFreebieFeats|reduced feat tax, inspired from https://michaeliantorno.com/feat-taxes-in-pathfinder/|:house:|:heavy_check_mark:|
|General.PatchHideBuffs|unclogs UI by hiding a few buffs|:book:|:construction:|
|General.PatchVarious|removed PreciousTreat penalty, extend protection from X to 10 minutes|:house:|:heavy_check_mark:|
|Hexcrafter.FixProgression|allows hex selection with any arcana, add missing spell recall at level 11|:book:|:heavy_check_mark:|
|Items.CreateButcheringAxe|new weapon type Butchering Axe|:book:|:heavy_check_mark:|
|Items.CreateImpactEnchantment|new enchantment Impact|:book:|:heavy_check_mark:|
|Items.CreateKineticArtifact|new weapon for Kineticists|:house:|:heavy_check_mark:|
|Items.PatchArrows|will pick up non-magical arrows after combat|:book:|:heavy_check_mark:|
|Items.PatchTerendelevScale|make the revive scale usable once per day|:house:|:heavy_check_mark:|
|Kineticist.CreateAutoMetakinesis|activatable to automatically empower and maximize blasts, if you have unused burn|:book:|:heavy_check_mark:|
|Kineticist.CreateBladeRushInfusion|infusion: Blade Rush, expands Kinetic Knight|:book:|:heavy_check_mark:|
|Kineticist.CreateChainInfusion|infusion: Chain|:book:|:heavy_check_mark:|
|Kineticist.CreateExpandedElement|basic feat: select extra elements|:house:|:heavy_check_mark:|
|Kineticist.CreateExtraWildTalentFeat|basic feat: Extra Wild Talent|:book:|:heavy_check_mark:|
|Kineticist.CreateHurricaneQueen|Wild Talent: Hurricane Queen|:book:|:heavy_check_mark:|
|Kineticist.CreateImpaleInfusion|infusion: Impale|:book:|:heavy_check_mark:|
|Kineticist.CreateKineticistBackground|regional background: gain +1 Kineticist level for the purpose of feat prerequisites|:house:|:heavy_check_mark:|
|Kineticist.CreateMindShield|Wild Talent: half Psychokineticist's penalties|:house:|:heavy_check_mark:|
|Kineticist.CreateMobileGatheringFeat|basic feat: Mobile Gathering|:book:|:heavy_check_mark:|
|Kineticist.CreateSelectiveMetakinesis|gain selective metakinesis at level 7|:house:|:heavy_check_mark:|
|Kineticist.CreateWhipInfusion|infusion: Kinetic Whip, expands Kinetic Knight|:book:|:heavy_check_mark:|
|Kineticist.PatchDarkElementalist|faster animation and use anywhere, but only out of combat|:house:|:heavy_check_mark:|
|Kineticist.PatchDemonCharge|Demon Charge also gathers power|:house:|:heavy_check_mark:|
|Kineticist.PatchGatherPower|Kineticist Gather Power can be used manually|:book:|:heavy_check_mark:|
|Kineticist.PatchVarious|bowling works with sandstorm blast, apply PsychokineticistStat setting|:house:|:heavy_check_mark:|
|Kineticist.FixBlastsAreSpellLike|makes blasts register as spell like, instead of supernatural|:book:|:heavy_check_mark:|
|Kineticist.FixWallInfusion|fix Wall Infusion not dealing damage while standing inside|:book:|:x:|
|Monk.CreateFeralCombatTraining|basic feat: Feral Combat Training|:house:|:heavy_check_mark:|
|Mythic.CreateDemonLord|adds features of Demon Lords to the mythic Demon progression: teleport at will, ...|:house:|:construction:|
|Mythic.CreateDemonMastery|mythic feat: requires demon lv6; change the rage effect of an demon aspect into a passive effect|:house:|:heavy_check_mark:|
|Mythic.CreateExtraMythicFeats|mythic feat: can pick mythic abilities as feats and vice versa|:house:|:heavy_check_mark:|
|Mythic.CreateKineticMastery|mythic feat: physical Kinetic Blasts gain attack bonus equal to mythic level, or half with energy Blasts|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessArcanePool|mythic ability: infinite arcane pool, expect spell recall|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessArcaneReservoir|mythic ability: infinite arcane reservoir|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessBardicPerformance|mythic ability: Bardic Performances cost no resources mythic ability: Skald's Raging Song cost no resources|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessBombs|mythic ability: infinite alchemist bombs and incenses|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessDomain|mythic ability: use domain powers at will|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessKi|mythic ability: reduce ki costs by 1|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessShaman|mythic ability: infinite spirit weapon uses (shaman, spirit hunter)|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessSmite|mythic ability: infinite Smites (chaotic and evil), requires Abundant Smite|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessWarpriest|mythic ability: infinite scared weapon uses|:house:|:heavy_check_mark:|
|Mythic.CreateLimitlessWitchHexes|mythic ability: Hexes ignore their cooldown|:house:|:heavy_check_mark:|
|Mythic.CreateMagicItemAdept|mythic feat: trinket items use character level as caster level|:house:|:heavy_check_mark:|
|Mythic.CreateResourcefulCaster|mythic ability: regain spells that fail because of spell failure, concentration, SR, saving throws|:house:|:heavy_check_mark:|
|Mythic.CreateSwiftHex|mythic ability: hexes other than grant can be used as a swift action|:house:|:heavy_check_mark:|
|Mythic.CreateSwiftHuntersBond|mythic ability: ranger's Hunter's Bond can be used as a swift action|:house:|:heavy_check_mark:|
|Mythic.PatchBoundlessHealing|Boundless Healing also grants healing spells to spellbooks|:house:|:heavy_check_mark:|
|Mythic.PatchJudgementAura|Everlasting Judgement also applies to Judgement Aura|:house:|:heavy_check_mark:|
|Mythic.PatchKineticOvercharge|Kinetic Overcharge works always, not only while gathering power|:house:|:heavy_check_mark:|
|Mythic.PatchLimitlessDemonRage|Limitless Rage also applies to Demon Rage|:house:|:heavy_check_mark:|
|Mythic.PatchRangingShots|doesn't get weaker when hitting|:house:|:construction:|
|Mythic.PatchUnstoppable|Unstoppable works against more conditions like stun, daze, and confusion|:house:|:heavy_check_mark:|
|Mythic.PatchVarious|allow quicken on Demon Teleport|:house:|:heavy_check_mark:|
|Mythic.PatchWanderingHex|can swap hex at will|:house:|:heavy_check_mark:|
|Ranger.CreateImprovedHuntersBond|combat feat: Improved Hunter's Bond|:book:|:heavy_check_mark:|
|Rogue.CreateBleedingAttack|rogue talent: Bleeding Attack; basic talent: Flensing Strike|:book:|:heavy_check_mark:|
|Rogue.CreateExtraRogueTalent|basic feat: Extra Rogue Talent|:book:|:heavy_check_mark:|
|Unlock.UnlockAnimalCompanion|allows animal companions to reach up to level 40|:house:|:construction:|
|Unlock.UnlockKineticist|adds infusion, wild talent, and element focus up to level 40|:house:|:construction:|
|Witch.CreateCackleActivatable|Cackle/Chant can be toggled to use move action passively|:house:|:heavy_check_mark:|
|Witch.CreateExtraHex|basic feat: Extra Hex|:book:|:heavy_check_mark:|
|Witch.CreateIceTomb|Hex: Ice Tomb|:book:|:heavy_check_mark:|
|Witch.FixBoundlessHealing|boundless healing applies to healing hex|:book:|:heavy_check_mark:|
|Patch.Patch_AbilityAtWill|provides logic for at will spells|:house:|:heavy_check_mark:|
|Patch.Patch_AbilityGroups|merges similar abilities into foldable categories|:book:|:heavy_check_mark:|
|Patch.Patch_ActivatableActionBar|adds logic for automatic-only activatable|:book:|:heavy_check_mark:|
|Patch.Patch_ActivatableHandleUnitRunCommand|fixes move actions disabling the activatable (since we have 2 of them)|:book:|:heavy_check_mark:|
|Patch.Patch_ActivatableOnNewRound|uses up move action when triggered; deactivates activatable if no action left|:book:|:heavy_check_mark:|
|Patch.Patch_ActivatableOnTurnOn|fixes activatable not being allowed to be active when they have the same action (like 2 move actions)|:book:|:heavy_check_mark:|
|Patch.Patch_ActivatableTryStart|fixes activatable not starting the second time, while being outside of combat|:book:|:heavy_check_mark:|
|Patch.Patch_AllowAchievements|clears the 'has used mods before' flag and also pretends that no mods are active|:book:|:heavy_check_mark:|
|Patch.Patch_AlwaysAChance|Always A Chance succeeds on a natural one and applies to most d20 rolls|:house:|:heavy_check_mark:|
|Patch.Patch_AOEAttackRolls|allows Impale Infusion and other AOE attacks to roll once for all|:book:|:heavy_check_mark:|
|Patch.Patch_ConditionExemption|Adds logic to ignore status effects under certain conditions.|:book:|:heavy_check_mark:|
|Patch.Patch_DarkElementalistBurn|for Wild Talents your current amount of burn includes the number of successful Soul Power uses|:house:|:heavy_check_mark:|
|Patch.Patch_DebugReport|fixes error log crashes due to unnamed components|:book:|:heavy_check_mark:|
|Patch.Patch_DismissAnything|dismiss any spell regardless of who the caster is|:house:|:heavy_check_mark:|
|Patch.Patch_EnduringSpells|allows Enduring Spell to apply to spells from any source; fix for Magic Weapon|:book:|:heavy_check_mark:|
|Patch.Patch_EnvelopingWindsCap|removes 50% evasion cap for Hurricane Queen|:book:|:heavy_check_mark:|
|Patch.Patch_FeralCombat|collection of patches for Feral Combat Training|:book:|:heavy_check_mark:|
|Patch.Patch_FixAreaDoubleDamage|fixes area effects triggering twice when cast|:book:|:heavy_check_mark:|
|Patch.Patch_FixAreaEndOfTurn|in turn-based mode area effects happen at the end of each unit's round, instead of all at once at the start of the caster's round|:book:|:heavy_check_mark:|
|Patch.Patch_FixPolymorphGather|makes it so polymorphed creatures can use Gather Power and creatures with hands Kinetic Blade|:book:|:heavy_check_mark:|
|Patch.Patch_KineticistAllowOpportunityAttack|allows Attack of Opportunities with anything but standard Kinetic Blade; so that Kinetic Whip works; also allows natural attacks to be used, if Whip isn't available|:book:|:heavy_check_mark:|
|Patch.Patch_MagicItemAdept|patches for Magic Item Adept|:house:|:heavy_check_mark:|
|Patch.Patch_Polymorph|allows debug flags to keep inventory or model during polymorph|:book:|:heavy_check_mark:|
|Patch.Patch_PreferredSpellMetamagic|necessary patches for Preferred Spell|:book:|:heavy_check_mark:|
|Patch.Patch_ResourcefulCaster|patches for Resourceful Caster|:house:|:heavy_check_mark:|
|Patch.Patch_SpellSelectionParametrized|fix spell selection for Preferred Spell|:book:|:heavy_check_mark:|
|Patch.Patch_TrueGatherPowerLevel|Normal: The level of gathering power is determined by the mode (none, low, medium, high) selected. If the mode is lower than the already accumulated gather level, then levels are lost. Patched: The level of gathering is true to the accumulated level or the selected mode, whatever is higher.|:book:|:heavy_check_mark:|

:heavy_check_mark: works, please report bugs you find \
:construction: not tested, please let me know if this works or not \
:x: does not work, avoid taking these feats \
:house: homebrew \
:book: from the books

FAQ
-----------
Q: I don't like feature X, can you remove it? \
A: The ingame menu has a list of all patches. You can click them and restart the game to effectively get rid of any feature. You may not be able to load your save, if you already picked feats which are generated by said patch. In case the menu is not up to date, you can also read the patch names from the log and add them manually in the settings.json file. You can disable a whole category with the asterisk symbol like so "General.*".

Q: Can you make feature X? \
A: At the moment I have enough ideas to fill out my coding hobby. If the scope is small or it is directly related to one of my existing features, then go ahead a open a Github ticket.

Q: I cannot find my question in the FAQ, what now? \
A: Ask me about it :smile:

Q: What are the options in red? \
A: These cannot be disabled during a playthrough. They become save permanent. I am working to make the menu more clear.
