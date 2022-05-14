# Changelog
- ~~added save metadata to keep track of enabled patches~~ not yet

## [1.3.0]
- changed project framework

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