# Changelog

## [1.6.2]
- fixed cursing gaze X plague hex #284

## [1.6.1]
- added ProphetOfPestilence to cursing gaze logic #284
- added 3 buffs PatchHideBuffs #280
- fixed Boundless Injury caster level unlock #273

## [1.6.0]
- fixed multiple patches for update (watch out for more bugs)

## [1.5.40]
- fixed Elemental Ascetic AC not counting towards monk levels
- fixed Elemental Ascetic not getting extra flurry at level 11

## [1.5.39]
- fixed Elemental Ascetic: Powerful Fist crashing the game

## [1.5.38]
- fixed Elemental Ascetic: Powerful Fist not dealing extra damage

## [1.5.37]
- fixed stacking of Master Shapeshifter bonus #269
- added Fix Wolverine Rage, unlock Limitless Rage for Wolverine (Shifter) #268
- merged fix for limitless hexes and TTT #270, closes #267

## [1.5.36]
- changed Patch_KineticistAllowOpportunityAttack for compatibility

## [1.5.35]
- fixed FixBloodlineArcane removing Metamagic Adept at level 9 #262
- changed how cackle is applied which perhaps fixes inconsistencies #264
- fixed fix for quicken command type #258
- added combat feat Two-Weapon Rend #265

## [1.5.34]
- added Elemental Ascetic (needs testing) #130
- fixed a compatibility issue with BubbleBuff #261

## [1.5.33]
- fixed issue during RuleCalculateAbilityParams, if spell is null #259
- merge pull request #257

## [1.5.32]
- fixed bug that all physical kinetic blasts applied spell resistance #246

## [1.5.31]
- compatibility patch
- fixed bug that would prevented limitless abilities to turn off #254
- added DontScale flag to summoned weapons #252

## [1.5.30]
- added new spell: Frostbite #235
- fixed Mad Magic getting lost after reloading a save #244
- added druid spells to hunter list #248
- removed limit to metamagic backgrounds #249

## [1.5.29]
- added even more localization
- fixed Chill Touch and touch attacks #239 #243

## [1.5.28]
- added localization to menu

## [1.5.27]
- fixes for update
- fixed Feral Combat Training with Mythic Improved Unarmed Strike
- honor toybox gather power cheat
- rephrased patch metadata popup

## [1.5.26]
- fixed Chill Touch not using touch weapon
- fixed Produce Flame causing a crash because weapon model was empty (uses torch model now)
- Channel Form, basic feat: collection of abilities to shape channel energy into new forms #181

## [1.5.25]
- limitless activatables ignore OnlyInCombat condition #230
- added Chill Touch spell
- added save metadata to keep track of enabled patches
- updated chinese translation by nixgnot #225

## [1.5.24]
- added demon rage to limitless rage prerequisites
- localized string cleanup #225
- fixed spell weapons crashing during load, causing all equipment visuals to not load #224

## [1.5.23]
- fixed Limitless Alchemist prerequisites #220
- fixed cache for mods that load before the game's blueprints #222
- added HarmoniousMage, mythic feat: ignore opposition schools #221

## [1.5.22]
- update

## [1.5.21]
- made limitless domain more inclusive regarding 3rd party mods
- fixed so Flame Blade Dervish Combat checks for the actual spell, instead of classes #216

## [1.5.20]
- fixed translation logic

## [1.5.19]
- added Oathbreakers Bane Ability to Limitless Smite
- changed project
- added Paladin Virtuous Bravo for testing only #87

## [1.5.18]
- removed broken logic from KineticArtifact #210
- removed recommendation from Piranha Strike #208
- changed weapon type 'Flame Blade' to scimitar, 'Divine Trident' to trident #197
- added Limitless Animal Focus, mythic ability: gain the Hunter capstone 'Master Hunter' #209
- added Limitless Inquisitor Bane, mythic ability: infinite inquisitor bane #203

## [1.5.17]
- fixed Ki Strikes and Shattering Punch with Feral Combat Training

## [1.5.16]
- Feral Combat Training unlocks iterative natural attacks #206

## [1.5.15]
- fixed conflicting prerequisites #204
- fixed spell resistance with chain infusion #205
- added patch info to Patch_UnlockClassLevels

## [1.5.14]
- fixed spell resistance with Bladed Dash #198
- fix for selection #189

## [1.5.13]
- added Martyr (Paladin) to Limitless Bardic Performance #196
- added Patch: Fix limitless activatables; makes it so activatables with infinite resources start out of combat and stay on after combat #195

## [1.5.12]
- fixed limitless features ignoring activatables

## [1.5.11]
- fixed Kinetic Knight maneuver bonus while whip is active #185
- added Unlock Spells: unlocks some spells: Transformation
- fixed parametrized selection not showing spells from base game #188

## [1.5.10]
- added Limitless Bloodline Claws, mythic ability: use claws from bloodlines at will, use breath weapon more often, use dragon disciple form II at will
- fixed spell resistance check on Flame Blade
- change Flame Blade to standard action, but you can target enemies for the first attack
- added Evangelist (Homebrew Archetypes) to Limitless Bardic Performance
- fixed metamagic reduction with convertable spells
- fixed spell selection for metamagic traits, spell perfection, preferred spell

## [1.5.9]
- added metamagic to Flame Blade
- changed Flame Blade to move action (like unsheating a weapon), that way you get the free touch attack from casting a spell, but cannot do a full attack
- added Gozreh's Trident (Divine Trident); allowing Flame Blade Dervish Combat with it (since the spell is virtually identical)
- fixed for 2.0.4j

## [1.5.8]
- added Flame Blade, spell: Flame Blade, feat: Flame Blade Dervish Combat

## [1.5.7]
- fixed damage calculation of Energy Channel
- fixed Healing Flames alignment checks
- added Limitless Warpriest Blessing: mythic ability use blessing powers at will #170

## [1.5.6]
- fixed Bestow Hope only working on cleric channel
- added Energy Channel: basic feat channel energy through weapon attacks #168
- added spell Healing Flames #169
- fixed Patch_ParryAlways #156
- changed bleeding logic

## [1.5.5]
- added limitless smite to antipaladin #166
- fixed Purifying Channel hitting all enemies #150

## [1.5.4]
- added Kitsune Foxfire: magical tail also grants Foxfire Bolt #161
- added Bestow Hope: basic feat channel energy reduces fear #160
- added Patch: Azata Favorable Magic, include saving throws from auras (does not work with TableTopTweaks Azata.FavorableMagic enabled) #162

## [1.5.3]
- fixed Patch_NotAChance making all attacks miss #159
- fixed Patch_ChangeSpellElement ambiguous match
- fixed split hex working on some major/grand hexes #157

## [1.5.2]
- added Purifying Channel feat: channel positive energy deals fire damage #150
- added Patch Parry Always: use parry even if attack would have missed anyway default=off #156
- changed Patch_BackgroundChecks which should fix #151
- fixed internal blueprint cache #155

## [1.5.1]
- fixed Bladed Brush for Spell Combat, Duelist Parry, and Duelist's Deflect Arrows #144
- split boundless healing and boundless injury into separate feats #143
- added fix for fortune hex
- added background: Fate’s Favored #146
- fixed ice tomb DC stacking, added to winter witch selection #149
- fixed Patch_ChangeSpellElement overwriting channel energy #148

## [1.5.0]
- fixed Feral Combat with Elemental Fist
- extended Kineticist.PatchVarious with: fixed Negative Energy Mastery #140
- fixes for 2.0 release

## [1.4.3]
- extended Mythic.PatchVarious with: elemental rampage works with limitless rage #138
- fixed issue with loading CodexLib
- fixed life bubble is AOE again

## [1.4.2]
- added 'body forms become immune to horrid wilting' to General: Various Tweaks
- added Bladed Brush: new combat feat #87
- extended Mythic.PatchVarious with: allow Elemental Barrage on any damage trigger #136
- included feats in gold dragon bonus feat
- added Mythic Animal Companion, mythic feat: unlocks companion equipment slots #134
- added Not A Chance: mythic ability: immunity to crits #133
- fixed PatchAlwaysAChance not being called #137
- added Split Hex

## [1.4.1]
- fixed serialization issue
- fixed auto metakinesis applying maximized before level 9
- added Prodigious Two-Weapon Fighting feat
- fixed selective metakinesis not applying to some area blasts and applying to single target blasts
- fixed missing talents and mastery when picking the same kineticist element focus multiple times

## [1.4.0]
- fixed Limitless Azata Song prerequisite
- fixed Scion archetype getting all composite blasts
- update for 1.4

## [1.3.24]
- fixed bug from spellstrike patch

## [1.3.23]
- removed Kinetic Artifact "Catalyst", because it causes bugs I can't seem to fix
- fixed kinetic fist and energize weapon dealing damage even if burn cannot be paid
- fixed Blood Kineticist cached damage info
- fixed Kinetic Whip with 3rd party elements
- fixed Selective Metakinesis with 3rd party elements
- added fix for Eldritch Archer Spellstrike

## [1.3.22]
- added infusion Kinetic Fist
- added infusion Energize Weapon
- fixed Spell Perfection not working with Spell Specialization and some interactions with TableTopTweaks
- fixed Metamagic Master &co not working with Bladed Dash
- added Hexcrafter to Cursing Gaze

## [1.3.21]
- changed bonus Ability Range to a 3% bonus per level, reducing overall bonus range (before +5 ft per two levels)
- fixed expanded element with 3rd party elements

## [1.3.20]
- hopefully fixed bleed
- vampiric infusion will now just trigger kinetic healer, instead of reducing kinetic healer burn
- fixed kinetic blade interactions with abilities (like charge, blade whirlwind, coup de grace) #97
- fixed Elemental Scion not getting composites, if she didn't pick ElementalScionSecondBlast
- added Patch: Ability Range; bonus spell range equal to 5 feet per 2 caster levels

## [1.3.19]
- fixed Expanded Element allow selection of already acquired elements
- fixed Venom Infusion context data
- added Mythic Eschew Materials, mythic ability: you cast spells without expending material components

## [1.3.18]
- added Kineticist archetype Elemental Scion

## [1.3.17]
- fixed Arcanist not able to memorize the same spell with different metamagics
- fixed Arcanist Magical Supremacy not working with spontaneous spells

## [1.3.16]
- added icon to Venom Infusion Poison
- added Opportune Parry, combat feat: duelist parry by expending Attack of Opportunities
- added patch to allow Zippy to work with spell-like abilities

## [1.3.15]
- added references to KineticistExpandedElements; will fix interactions with substance infusions
- fixed Venom Infusion's poison from "2 saves" to "2 consecutive saves"; simplified damage calculation which might fix bugs;
	it deals dex damage, if target is immune to con damage; target is sickended while poisoned; reduced burn by 1;
	fixed DC being dex based, instead of main stat
- added more checks to Spell Perfection
- fixed Dirty Fighting granting all maneuver feats instead of just the action; this might need a respec, if you already took that feat
- added Razortusk to Orc Atavism
- fixed kinetic weapons not registering as melee weapon for some abilities
- fixed Blood Kineticist Bleed Infusion and some strings
- reversed some butchering axe changes until bugs are fixed

## [1.3.14]
- fixed Orc Atavism Ferocity not in UI
- fixed Venom Infusion applying to all attacks, not just blasts
- added Dirty Fighting combat feat
- added Spell Perfection

## [1.3.13]
- fixed sticky spells not working with Boundless Injury
- allowed arcanist to use temporary known spells without preparing them
- added Venom Infusion; infusion: applies sickened or poisons the target

## [1.3.12]
- added Patch Fix Arcanist Spontaneous Metamagic: allows arcanist to use non memorized metamagic, but increases casting time
- fixed Sacred Summons not showing up

## [1.3.11]
- renamed Hexcrafter to Magus category
- added Sword Saint Any Weapon: allow Sword Saint to pick any weapon focus; default off

## [1.3.10]
- added PatchArcanistBrownFur: allows Share Transmutation to affect any spell
- fixed Metamagic Adept still increasing caster time to standard action, if the original casting time was shorter
- added Mythic Metamagic Adept feat: allow spontaneous spellcasters to apply metamagic without casting time penalty
- added Sacred Summons, basic feat: requires Channel Energy, summons act immediately

## [1.3.9]
- fixed Metamagic Adept to retaining through saves
- fixed fix of Master Shapeshifter ;)

## [1.3.8]
- added fix for Arcane bloodline; Metamagic Adept and Arcane Apotheosis
- fixed crash during save, if a character has Arcane Apotheosis

## [1.3.7]
- added Orc-Atavism; Kindred-Raised Half-Elf regain Elven Immunity
- fixed bug that made metamagic traits unselectable

## [1.3.6]
- added Butchering Axe weapon proficiency, which is now required to use it
- fixed Elemental Focus not applying to spell-like abilities
- added Additional Traits, metamagic traits
- added fix for Master Shapeshifter: ensures polymorph buffs grant boni
- added freebooter to swift hunter's bond
- fixed AC malus of Flensing Strike not updating
- changed installation path, so files should extract correctly for everybody

## [1.3.5]
- changed Bladed Dash, so if the spells target is a unit, it will always be hit; reduced reach again
- fixed null exception in Patch_ChangeSpellElement

## [1.3.4]
- increased Bladed Dash reach a bit, which should fix not hitting some enemies
- fixed Fix Spell Element Change :P

## [1.3.3]
- fixed more Boundless Healing spell levels
- fixed Bladed Dash being too short, will use caster's reach
- fixed Bladed Dash showing the line indicator
- fixed ability tooltip always claiming to target all
- added Fix Spell Element Change: fixes Elemental Bloodline and Spell Focus interaction

## [1.3.2]
- fixed Boundless Healing spell levels
- fixed Greater Bladed Dash not dealing damage
- fixed Bladed Dash attack bonus too high
- fixed Bladed Dash not working with Spell Combat
- fixed Bladed Dash not working with reach metamagic
- fixed Ascendant Summons description
- changed Bladed Dash modifier being less ambiguous

## [1.3.1]
- added Boundless Injury: Boundless Healing also applies to inflict wound spells and grants those to spellbooks
- added spell Bladed Dash and Greater Bladed Dash
- fixed hexes not being available to sylvan trickster
- fixed rogue talents not being available to sylvan trickster
- fixed Cursing Gaze not being available to sylvan trickster
- added Ascendant Summons: buffed Ascendant Summons by +4 stats and DR 10
- fixed metal and blue flame blasts selectable before having their prerequisites

## [1.3.0]
- changed project framework
- removed ability/spell groups
Update Instructions:
On request, Ability/Spell Groups have been made into its own mod here: https://github.com/Truinto/SpellPouch/releases/tag/v1.0.0
If you have used any Groups in DarkCodex, you need to install SpellPouch to keep using this feature.

## [1.2.8]
- added limitless azata songs
- fixed possible memory leak
- added logic to edit ability groups while pressing shift
- added logic to display all abilities while ability groups are unlocked
- added spell groups
- added icon border (ability group is black, spell group is gray)
- changed drag behaviour; action slots dragged into groups will be placed left or right depending on which side is closer

## [1.2.7]
- fixed quickened abilities taking more time than normal, if you already have used your swift action
- fixed freebie feats
- fixed Mad Magic not working under most conditions

## [1.2.6]
- added airborne features to demon wings
- added Mad Magic
- updated to new game version

## [1.2.5]
- fixed "Unlock ability groups" not showing up in menu
- fixed oversight in Patch_AOEAttackRolls
- fixed Dismiss Anything

## [1.2.4]
- added dispel effects to Resourceful Caster conditions
- added thunderstorm chain variant
- added logic to organize ability groups via drag&drop (needs unlock in mod menu)
- fixed destrutive dispel not scaling for non-spellcaster classes (like Kineticist)

## [1.2.3]
- fixed Arcane Weapon buff not getting replaced on cast
- fixed ability group not showing up

## [1.2.2]
- added Dark Elementalist soul power to burn boni
- added option to dismiss any spell regardless of who casted it (default off)
- fixed Limitless Arcane Pool not applying to Eldritch Scion
- fixed Enduring Spells not able to apply to enchantment spells (like Magic Weapon)
- rewrote blueprint loading, which should fix previous issues with it and still be quite fast

## [1.2.1.1]
- removed patchDispelMagic as Destructive Dispel has been fixed in 1.2 update
- added more patch comments
- added Chain Infusion
- added Enduring Spells to spell-like abilities and items again
- added option to reduce limitless cost to 0
- added Expanded Element which adds feat to select new elements
- added Patch_UnlockClassLevels which enables all classes to level up to 40 (albeit no features beyond 20)
- added Unlock Kineticist which adds features up to level 40
- added Demon Mastery
- fixed ice impale dealing twice as much damage
- fixed issues with Patch_FixAreaDoubleDamage that had no UnitEnter actions
- fixed cackle/chant not showing up in prediction bar
- fixed area effects being invisible after cutscenes
- fixed loading issue
- fixed Resourceful Caster causing all spell being broken after some cutscenes
- ~~improved loading time significantly~~ reverted to fix a bug
- extended ability groups, now accessible via file "DefGroups.json"

## [1.2.0]
- updated to game version 1.2
- added Patch_AbilityGroups that merges similar abilities into foldable categories

## [1.1.8]
- removed Patch_FixAreaEffectDamage
- added Patch_FixAreaDoubleDamage, which doesn't have overlapping issues
- added Patch_FixAreaEndOfTurn to deal damage at the end of each unit's turn, instead all at once during the caster's turn
- added option to disable new features by default

## [1.1.7]
- added Patch_FixAreaEffectDamage which prevents area effects from running simultaneously OnEnter and OnRound e.g. dealing double damage
- extended Control_AreaEffects functionality to cutscenes (needs testing)
- added events to advanced patch manager
- disabled patches show warnings, if a requirement isn't met

## [1.1.6]
- fixed improved hunter's bond not showing up in feat selection

## [1.1.5]
- added createMindShield: Wild Talent, half Psychokineticist's penalties
- added patchJudgementAura: Everlasting Judgement also applies to Judgement Aura
- added option to change Psychokineticist's main stat (does not update ingame descriptions)

## [1.1.4]
- changed restriction on Kinetic Whip, so it can be used whenever a Kinetic Blade is equipped
- added Butchering Axe and Impact enchantment

## [1.1.3]
- fixed Bleeding Attack's funny damage resolution by apply it to weapon attacks instead of attack rolls

## [1.1.2]
- fixed metamagic variants with preferred spell
- improved ingame menu
- added one time start up warning
- added TableTopTweaks' (TTT from now on) Fighter Advanced Weapon Training for limitless shaman weapon
- added Cursing Gaze | mythic ability: hexes other than grant can be used as a swift action
- added limitless Skald's Raging Song mythic ability

## [1.1.1]
- fixed bleeding attack not counting as bleed for some features
- added General.patchDispelMagic: fix Destructive Dispel doing nothing, apply bonus from Mythic Spell Penetration to dispel attempts
- fixed crash with debug flag 2

## [1.1.0]
- fix for missing area effects
- added Kineticist.createHurricaneQueen
- added Mythic.patchBoundlessHealing: Boundless Healing also grants healing spells to spellbooks
- added Mythic.patchVarious: allow quicken metamagic on demon teleport spell
- added Patch_AlwaysAChance: feat 'Always A Chance' will critical succeed on a natural 1 and apply to more checks (saving throws, skill checks)
- added Mythic.patchRangingShots: doesn't get weaker when hitting
- added Mythic.patchWanderingHex: can swap hex at will, instead of once per day (which wasn't in the description anyway)
- updated for game version 1.1.0i
- added debugflag 2 that removes trap data from save games (without this my save wouldn't load)

## [1.0.9]
- fixed duplicate undead immunity that blocked dirge bard archetype and undead bloodline
- added Mythic.createSwiftHuntersBond
- added Ranger.createImprovedHuntersBond
- added metamagic to preferred spell
- fixed LimitlessDemonRage not working
- fixed limitless hexes only working when targeting self
- fixed BoundlessHealing not working with witch healing hexes
- fixed all enemies getting notably stronger from freebie feats
- added option to disable area effects during cutscenes
- fixed blade rush this time

## [1.0.8]
- added General.patchVarious
- added General.patchHideBuffs
- added Kineticist.fixBlastsAreSpellLike
- added Mythic.patchUnstoppable
- added Kineticist.patchVarious: bowling sandstorm
- General various: extend protection from X to 10 minutes
- fixed gather power blocked during polymorph, as well as kitsune human form being unable to use kinetic blade
- fixed feral combat applying to all weapon checks, instead of only unarmed checks
- ~~fixed blade rush attacking at the start point, instead of the end point~~
- fixed preferred spell
- added debug options to enable inventory while polymorphed
- fixed resourceful caster now refunds converted spells
- fixed quicken blade rush not working for regular kineticist

## [1.0.7]
- fix impale rolling individually
- fix bleed working on bleed immune creatures
- fix limitless hexes
- added General.createPreferredSpell
- added Mythic.createResourcefulCaster
- added Monk.createFeralCombatTraining
- createAutoMetakinesis also removes quicken metakinesis after use

## [1.0.6]
- added Mythic.createMagicItemAdept
- added Items.createKineticArtifact
- added Kineticist.createAutoMetakinesis
- set freeby feats as default off
- added Mythic.createExtraMythicFeats
- added Mythic.patchLimitlessDemonRage
- added Kineticist.createWhipInfusion
- added Kineticist.createBladeRushInfusion
- expanded Kinetic Knight to get whip, blade rush, quicken blade rush, and maneuver bonuses

## [1.0.5]
- expanded createLimitlessBombs to include IncenseFog
- added Mythic.createLimitlessShaman
- added Mythic.createLimitlessWarpriest
- fixed Selective Metakinesis
- added Witch.createExtraHex
- added General.patchBasicFreebieFeats
- added Kineticist.patchDemonCharge

## [1.0.4]
- added patchKineticOvercharge
- removed debug functions
- added Mythic.createLimitlessBombs
- added Mythic.createLimitlessArcanePool
- added Mythic.createLimitlessArcaneReservoir
- added Mythic.createLimitlessKi
- added Mythic.createLimitlessDomain
- added Kineticist.createSelectiveMetakinesis

## [1.0.3]
- added bleeding attack
- added flensing strike
- added missing gui elements
- added mythic limitless smite
- added mythic kinetic mastery
- fixed wall infusion
- bugfixes

## [1.0.2]
- fix hexcrafter lv3 hex arcana selection; lv11 spell recall
- cold iron arrows refresh after combat
- a kineticist background
- kinetic blasts apply gather power, even if a weaker mode is picked
- mobile gathering
- made gather power ability visible
- extra wild talent
- option to allow achievements; this also clears the corresponding flag from future save files (otherwise uninstalling all mods would prevent achievements again)
- impale infusion
- ice tomb
- extra rogue talent
- buffed angel's light
- cackle/shant passive activatable
- ability focus

## [1.0.1]
- first release