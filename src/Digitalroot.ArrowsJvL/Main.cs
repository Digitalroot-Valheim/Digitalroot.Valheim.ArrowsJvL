using BepInEx;
using BepInEx.Configuration;
using Digitalroot.Modding.Framework.Logging;
using Digitalroot.Modding.Framework.Names.Vanilla;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Digitalroot.ArrowsJvL
{
  /// <summary>
  /// Combines BoneArrowsJvL, LightningArrowsJvL and AtosArrowsJvL
  /// PR: https://github.com/Atokal/AtosArrows/pull/3
  /// Assets belong to Atokal and are used with permission because of:
  ///  - Asset use permission You are allowed to use the assets in this file without permission or crediting me. https://www.nexusmods.com/valheim/mods/969 (June 12, 2021)
  /// Original Mod: https://www.nexusmods.com/valheim/mods/969
  /// Code is a complete rewrite.
  /// </summary>
  [BepInPlugin(Guid, Name, Version)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  [BepInIncompatibility("com.bepinex.plugins.atosarrows")]
  [BepInIncompatibility("digitalroot.valheim.mods.atosarrows.jvl")]
  [BepInIncompatibility("digitalroot.mods.lightningarrows.jvl")]
  [BepInIncompatibility("digitalroot.mods.bonearrows.jvl")]
  public partial class Main : BaseUnityPlugin, ITraceableLogging
  {
    // ReSharper disable once MemberCanBePrivate.Global
    public static Main Instance;
    private AssetBundle _assetBundle;
    public static ConfigEntry<int> NexusId;

    #region Implementation of ITraceableLogging

    /// <inheritdoc />
    public string Source => Namespace;

    /// <inheritdoc />
    public bool EnableTrace { get; }

    #endregion

    public Main()
    {
      try
      {
        #if DEBUG
        EnableTrace = true;
        #else
        EnableTrace = false;
        #endif
        Instance = this;
        NexusId = Config.Bind("General", nameof(NexusId), 3620, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));
        InitConfig();
        Log.RegisterSource(Instance);
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
      }
      catch (Exception e)
      {
        ZLog.LogError(e);
      }
    }

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public void Awake()
    {
      Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");

      PrefabManager.OnVanillaPrefabsAvailable += AddClonedItems;
    }

    private void AddClonedItems()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");

        _assetBundle = AssetUtils.LoadAssetBundleFromResources("atoarrows", typeof(Main).Assembly);

        #if DEBUG
        foreach (var assetName in _assetBundle.GetAllAssetNames())
        {
          Jotunn.Logger.LogInfo(assetName);
        }
        #endif

        AddStoneArrows();
        AddBluntedArrows();
        AddBoneArrows();
        AddFlintArrows();
        AddObsidianArrows();
        AddNeedleArrows();
        AddFireArrows();
        AddIceArrows();
        AddPoisonArrows();
        AddBombs();
        AddXBow();
        AddBoneArrow();
        AddLightningArrow();

        _assetBundle.Unload(false);

        // You want that to run only once, Jotunn has the item cached for the game session
        PrefabManager.OnVanillaPrefabsAvailable -= AddClonedItems;
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableFireBomb;
    public static ConfigEntry<bool> EnableIceBomb;

    private void AddBombs()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");

        if (EnableFireBomb.Value)
        {
          var bombFire = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/items/firebomb.prefab"), false, new ItemConfig
          {
            Amount = 5
            , CraftingStation = "piece_workbench"
            , Enabled = true
            , MinStationLevel = 3
            , Name = "FireBomb"
            , Requirements = new[]
            {
              new RequirementConfig
              {
                Item = "Coal"
                , Amount = 10
              }
              , new RequirementConfig
              {
                Item = "Resin"
                , Amount = 8
              }
              , new RequirementConfig
              {
                Item = "LeatherScraps"
                , Amount = 8
              }
              , new RequirementConfig
              {
                Item = "Entrails", Amount = 2
              }
            }
          });

          bombFire.ItemDrop.m_itemData.m_shared.m_name = "$item_ato_firebomb";
          ItemManager.Instance.AddItem(bombFire);
        }

        if (EnableIceBomb.Value)
        {
          var bombIce = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/items/icebomb.prefab"), false, new ItemConfig
          {
            Amount = 5, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 3, Name = "IceBomb", Requirements = new[]
            {
              new RequirementConfig { Item = "FreezeGland", Amount = 10 }, new RequirementConfig { Item = "Resin", Amount = 8 }, new RequirementConfig { Item = "LeatherScraps", Amount = 8 }, new RequirementConfig { Item = "Entrails", Amount = 2 }
            }
          });
          bombIce.ItemDrop.m_itemData.m_shared.m_name = "$item_ato_icebomb";
          ItemManager.Instance.AddItem(bombIce);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableBoneArrow;
    public static ConfigEntry<bool> EnableHeavyBoneArrow;

    private void AddBoneArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableBoneArrow.Value)
        {
          var arrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowbone.prefab"), false, new ItemConfig
          {
            Amount = 20
            , CraftingStation = "piece_workbench"
            , Enabled = true
            , MinStationLevel = 3
            , Name = "BoneArrow"
            , Requirements = new[]
            {
              new RequirementConfig
              {
                Item = "BoneFragments"
                , Amount = 4
              }
              , new RequirementConfig
              {
                Item = "Wood"
                , Amount = 8
              }
              , new RequirementConfig
              {
                Item = "Feathers", Amount = 2
              }
            }
          });
          arrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_bone";
          arrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          arrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 24f;
          arrow.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 30f;
          ItemManager.Instance.AddItem(arrow);
        }

        if (EnableHeavyBoneArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavybone.prefab"), false, new ItemConfig
          {
            Amount = 10
            , CraftingStation = "piece_workbench"
            , Enabled = true
            , MinStationLevel = 3
            , Name = "HeavyBoneArrow"
            , Requirements = new[]
            {
              new RequirementConfig
              {
                Item = "BoneFragments"
                , Amount = 6
              }
              , new RequirementConfig
              {
                Item = "RoundLog"
                , Amount = 10
              }
              , new RequirementConfig
              {
                Item = "Feathers"
                , Amount = 5
              }
            }
          });
          heavyArrow.ItemDrop.m_itemData.m_shared.m_description = "$item_atoarrow_heavy_bone_description";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_heavy_bone";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 36f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 36f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 14f;
          ItemManager.Instance.AddItem(heavyArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }


    public static ConfigEntry<bool> EnableBluntedArrow;
    public static ConfigEntry<bool> EnableHeavyBluntedArrow;

    private void AddBluntedArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableBluntedArrow.Value)
        {
          var arrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowcore.prefab"), false, new ItemConfig
          {
            Amount = 20, CraftingStation = "forge", Enabled = true, MinStationLevel = 2, Name = "CoreArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Bronze", Amount = 1 }, new RequirementConfig { Item = "Stone", Amount = 2 }, new RequirementConfig { Item = "RoundLog", Amount = 8 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });
          arrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_core";
          arrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(arrow);
        }

        if (EnableHeavyBluntedArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavycore.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "forge", Enabled = true, MinStationLevel = 3, Name = "HeavyCoreArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Bronze", Amount = 2 }, new RequirementConfig { Item = "Stone", Amount = 4 }, new RequirementConfig { Item = "RoundLog", Amount = 12 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });

          heavyArrow.ItemDrop.m_itemData.m_shared.m_description = "$item_atoarrow_heavy_core_description";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_heavy_core";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 64f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          ItemManager.Instance.AddItem(heavyArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableObsidianFireArrow;
    public static ConfigEntry<bool> EnableHeavyFireArrow;
    public static ConfigEntry<bool> EnableExplodingFireArrow;

    private void AddFireArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableObsidianFireArrow.Value)
        {
          var arrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowobsidianfire.prefab"), false, new ItemConfig
          {
            Amount = 20, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 4, Name = "BigFireArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Obsidian", Amount = 4 }, new RequirementConfig { Item = "Resin", Amount = 8 }, new RequirementConfig { Item = "Wood", Amount = 8 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });
          arrow.ItemDrop.m_itemData.m_shared.m_name = "$item_arrow_obsidianfire";
          arrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(arrow);
        }

        if (EnableHeavyFireArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavyfire.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 4, Name = "HeavyFireArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Obsidian", Amount = 6 }, new RequirementConfig { Item = "Resin", Amount = 10 }, new RequirementConfig { Item = "FineWood", Amount = 10 }, new RequirementConfig { Item = "Feathers", Amount = 5 }
            }
          });
          heavyArrow.ItemDrop.m_itemData.m_shared.m_description = "$item_arrow_heavyfire_description";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_arrow_heavyfire";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 32f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_fire = 72f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          ItemManager.Instance.AddItem(heavyArrow);
        }

        if (EnableExplodingFireArrow.Value)
        {
          var aoeArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowfireaoe.prefab"), false, new ItemConfig
          {
            Amount = 5, CraftingStation = "forge", Enabled = true, MinStationLevel = 6, Name = "FireAoeArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Crystal", Amount = 4 }, new RequirementConfig { Item = "FireBomb", Amount = 1 }, new RequirementConfig { Item = "FineWood", Amount = 8 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });
          aoeArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_bigfire";
          aoeArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(aoeArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableHeavyFlintArrow;

    private void AddFlintArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableHeavyFlintArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavyflint.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 3, Name = "HeavyFlintArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Flint", Amount = 6 }, new RequirementConfig { Item = "RoundLog", Amount = 12 }, new RequirementConfig { Item = "Feathers", Amount = 5 }
            }
          });
          heavyArrow.ItemDrop.m_itemData.m_shared.m_description = "$item_atoarrow_heavy_flint_description";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_heavy_flint";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 47f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          ItemManager.Instance.AddItem(heavyArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableHeavyIceArrow;
    public static ConfigEntry<bool> EnableExplodingIceArrow;

    private void AddIceArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableHeavyIceArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavyfrost.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 4, Name = "HeavyIceArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Obsidian", Amount = 6 }, new RequirementConfig { Item = "FreezeGland", Amount = 10 }, new RequirementConfig { Item = "FineWood", Amount = 10 }, new RequirementConfig { Item = "Feathers", Amount = 5 }
            }
          });

          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_arrow_heavy_frost";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 32f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_frost = 72f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          ItemManager.Instance.AddItem(heavyArrow);
        }

        if (EnableExplodingIceArrow.Value)
        {
          var aoeArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowiceaoe.prefab"), false, new ItemConfig
          {
            Amount = 5, CraftingStation = "forge", Enabled = true, MinStationLevel = 6, Name = "IceAoeArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Crystal", Amount = 4 }, new RequirementConfig { Item = "IceBomb", Amount = 1 }, new RequirementConfig { Item = "FineWood", Amount = 8 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });
          aoeArrow.ItemDrop.m_itemData.m_shared.m_description = "$item_atoarrow_bigice_description";
          aoeArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_bigice";
          aoeArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(aoeArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableHeavyNeedleArrow;

    private void AddNeedleArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableHeavyNeedleArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavyneedle.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 4, Name = "HeavyNeedleArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Needle", Amount = 10 }, new RequirementConfig { Item = "FineWood", Amount = 10 }, new RequirementConfig { Item = "Feathers", Amount = 5 }
            }
          });
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_arrow_heavyneedle";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 72f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          ItemManager.Instance.AddItem(heavyArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableHeavyObsidianArrow;

    private void AddObsidianArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableHeavyObsidianArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavyobsidian.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 4, Name = "HeavyObsidianArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Obsidian", Amount = 6 }, new RequirementConfig { Item = "FineWood", Amount = 12 }, new RequirementConfig { Item = "Feathers", Amount = 5 }
            }
          });
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_arrow_heavyobsidian";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 67f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(heavyArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableHeavyPoisonArrow;
    public static ConfigEntry<bool> EnableExplodingPoisonArrow;

    private void AddPoisonArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableHeavyPoisonArrow.Value)
        {
          var heavyArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/heavy/arrowheavypoison.prefab"), false, new ItemConfig
          {
            Amount = 10, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 4, Name = "HeavyPoisonArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Obsidian", Amount = 6 }, new RequirementConfig { Item = "Ooze", Amount = 10 }, new RequirementConfig { Item = "FineWood", Amount = 10 }, new RequirementConfig { Item = "Feathers", Amount = 5 }
            }
          });
          heavyArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_arrow_heavy_poison";
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 32f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_damages.m_poison = 72f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 10f;
          heavyArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(heavyArrow);
        }

        if (EnableExplodingPoisonArrow.Value)
        {
          var aoeArrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowpoisonaoe.prefab"), false, new ItemConfig
          {
            Amount = 5, CraftingStation = "forge", Enabled = true, MinStationLevel = 6, Name = "PoisonAoeArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Crystal", Amount = 4 }, new RequirementConfig { Item = "BombOoze", Amount = 1 }, new RequirementConfig { Item = "FineWood", Amount = 8 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });
          aoeArrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_aoepoison";
          aoeArrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(aoeArrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableStoneArrow;

    private void AddStoneArrows()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableStoneArrow.Value)
        {
          var arrow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/arrows/arrowstone.prefab"), false, new ItemConfig
          {
            Amount = 20, CraftingStation = "piece_workbench", Enabled = true, MinStationLevel = 1, Name = "StoneArrow", Requirements = new[]
            {
              new RequirementConfig { Item = "Stone", Amount = 2 }, new RequirementConfig { Item = "Wood", Amount = 8 }, new RequirementConfig { Item = "Feathers", Amount = 2 }
            }
          });
          arrow.ItemDrop.m_itemData.m_shared.m_name = "$item_atoarrow_stone";
          arrow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(arrow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableXBow;

    private void AddXBow()
    {
      try
      {
        Log.Trace(Instance, $"{GetType().Namespace}.{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}()");
        if (EnableXBow.Value)
        {
          var xbow = new CustomItem(_assetBundle.LoadAsset<GameObject>("assets/atosarrows/bows/xbow.prefab"), false, new ItemConfig
          {
            Amount = 1, CraftingStation = "forge", RepairStation = "forge", Enabled = true, MinStationLevel = 1, Name = "XBow", Requirements = new[]
            {
              new RequirementConfig { Item = "Crystal", Amount = 10, AmountPerLevel = 2 }, new RequirementConfig { Item = "BlackMetal", Amount = 60, AmountPerLevel = 10 }, new RequirementConfig { Item = "FineWood", Amount = 8 }, new RequirementConfig { Item = "LinenThread", Amount = 20, AmountPerLevel = 2 }
            }
          });
          xbow.ItemDrop.m_itemData.m_shared.m_name = "$item_xbow";
          xbow.ItemDrop.m_itemData.m_shared.m_attackForce = 0f;
          xbow.ItemDrop.m_itemData.m_shared.m_attack.m_projectileVel = 65f;
          xbow.ItemDrop.m_itemData.m_shared.m_maxDurability = 600f;
          xbow.ItemDrop.m_itemData.m_shared.m_blockPower = 10f;
          xbow.ItemDrop.m_itemData.m_shared.m_maxStackSize = 1;
          xbow.ItemDrop.m_itemData.m_shared.m_ammoType = "$ammo_arrows";
          ItemManager.Instance.AddItem(xbow);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableSimpleBoneArrow;

    private void AddBoneArrow()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        if (EnableSimpleBoneArrow.Value)
        {
          var customItem = new CustomItem("ArrowBoneSimple"
                                          , ItemDropNames.ArrowFlint
                                          , new ItemConfig
                                          {
                                            CraftingStation = CraftingStationNames.Workbench
                                            , MinStationLevel = 1
                                            , Amount = 20
                                            , Requirements = new[]
                                            {
                                              new RequirementConfig
                                              {
                                                Item = ItemDropNames.Wood
                                                , Amount = 8
                                              }
                                              , new RequirementConfig
                                              {
                                                Item = ItemDropNames.BoneFragments
                                                , Amount = 3
                                              }
                                            }
                                          });

          var prefab = customItem.ItemPrefab;

          if (prefab == null)
          {
            throw new NullReferenceException(nameof(prefab));
          }

          var itemDrop = prefab.GetComponent<ItemDrop>();

          if (itemDrop == null)
          {
            throw new NullReferenceException(nameof(itemDrop));
          }

          itemDrop.m_itemData.m_shared.m_name = "$item_bone_arrow";
          itemDrop.m_itemData.m_shared.m_description = "$item_bone_arrow_description";
          itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Ammo;
          itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
          itemDrop.m_itemData.m_shared.m_weight = 0.1f;
          itemDrop.m_itemData.m_shared.m_backstabBonus = 4f;
          itemDrop.m_itemData.m_shared.m_damages.m_pierce = 20f;
          itemDrop.m_itemData.m_shared.m_damages.m_slash = 15f;

          ItemManager.Instance.AddItem(customItem);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static ConfigEntry<bool> EnableLightningArrow;

    private void AddLightningArrow()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        if (EnableLightningArrow.Value)
        {
          var customItem = new CustomItem("ArrowLightning"
                                          , ItemDropNames.ArrowSilver
                                          , new ItemConfig
                                          {
                                            CraftingStation = CraftingStationNames.Workbench
                                            , MinStationLevel = 1
                                            , Amount = 20
                                            , Requirements = new[]
                                            {
                                              new RequirementConfig
                                              {
                                                Item = ItemDropNames.Wood
                                                , Amount = 8
                                              }
                                              , new RequirementConfig
                                              {
                                                Item = ItemDropNames.Feathers
                                                , Amount = 2
                                              }
                                              , new RequirementConfig
                                              {
                                                Item = ItemDropNames.HardAntler
                                                , Amount = 1
                                              }
                                            }
                                          });

          var prefab = customItem.ItemPrefab;

          if (prefab == null)
          {
            throw new NullReferenceException(nameof(prefab));
          }

          var itemDrop = prefab.GetComponent<ItemDrop>();

          if (itemDrop == null)
          {
            throw new NullReferenceException(nameof(itemDrop));
          }

          itemDrop.m_itemData.m_shared.m_name = "$item_lightning_arrow";
          itemDrop.m_itemData.m_shared.m_description = "$item_lightning_arrow_description";
          itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Ammo;
          itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
          itemDrop.m_itemData.m_shared.m_weight = 0.1f;
          itemDrop.m_itemData.m_shared.m_backstabBonus = 4f;
          itemDrop.m_itemData.m_shared.m_damages.m_pierce = 28f;
          itemDrop.m_itemData.m_shared.m_damages.m_lightning = 40f;
          itemDrop.m_itemData.m_shared.m_damages.m_spirit = 0f;
          itemDrop.m_itemData.m_shared.m_icons[0] = LoadResourceIcon("Lightning_Arrow");

          ItemManager.Instance.AddItem(customItem);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    private void InitConfig()
    {
      EnableFireBomb = Config.Bind("Toggles", nameof(EnableFireBomb), true, new ConfigDescription("Enable Fire Bomb", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableIceBomb = Config.Bind("Toggles", nameof(EnableIceBomb), true, new ConfigDescription("Enable Ice Bomb", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableBoneArrow = Config.Bind("Toggles", nameof(EnableBoneArrow), true, new ConfigDescription("Enable Bone Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyBoneArrow = Config.Bind("Toggles", nameof(EnableHeavyBoneArrow), true, new ConfigDescription("Enable Heavy Bone Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableBluntedArrow = Config.Bind("Toggles", nameof(EnableBluntedArrow), true, new ConfigDescription("Enable Blunted Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyBluntedArrow = Config.Bind("Toggles", nameof(EnableHeavyBluntedArrow), true, new ConfigDescription("Enable Heavy Blunted Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableObsidianFireArrow = Config.Bind("Toggles", nameof(EnableObsidianFireArrow), true, new ConfigDescription("Enable Obsidian Fire Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyFireArrow = Config.Bind("Toggles", nameof(EnableHeavyFireArrow), true, new ConfigDescription("Enable Heavy Fire Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableExplodingFireArrow = Config.Bind("Toggles", nameof(EnableExplodingFireArrow), true, new ConfigDescription("Enable Exploding Fire Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyFlintArrow = Config.Bind("Toggles", nameof(EnableHeavyFlintArrow), true, new ConfigDescription("Enable Heavy Flint Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyIceArrow = Config.Bind("Toggles", nameof(EnableHeavyIceArrow), true, new ConfigDescription("Enable Heavy Ice Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableExplodingIceArrow = Config.Bind("Toggles", nameof(EnableExplodingIceArrow), true, new ConfigDescription("Enable Exploding Ice Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyNeedleArrow = Config.Bind("Toggles", nameof(EnableHeavyNeedleArrow), true, new ConfigDescription("Enable Heavy Needle Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyObsidianArrow = Config.Bind("Toggles", nameof(EnableHeavyObsidianArrow), true, new ConfigDescription("Enable Heavy Obsidian Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableHeavyPoisonArrow = Config.Bind("Toggles", nameof(EnableHeavyPoisonArrow), true, new ConfigDescription("Enable Heavy Poison Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableExplodingPoisonArrow = Config.Bind("Toggles", nameof(EnableExplodingPoisonArrow), true, new ConfigDescription("Enable Exploding Poison Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableStoneArrow = Config.Bind("Toggles", nameof(EnableStoneArrow), true, new ConfigDescription("Enable Stone Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableXBow = Config.Bind("Toggles", nameof(EnableXBow), true, new ConfigDescription("Enable XBow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableSimpleBoneArrow = Config.Bind("Toggles", nameof(EnableSimpleBoneArrow), true, new ConfigDescription("Enable Simple Bone Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
      EnableLightningArrow = Config.Bind("Toggles", nameof(EnableLightningArrow), true, new ConfigDescription("Enable Lightning Arrow", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false }));
    }

    private static Sprite LoadResourceIcon(string name)
    {
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      var resource = GetResource(Assembly.GetCallingAssembly(), $"Digitalroot.ArrowsJvL.Assets.{name}.png");
      return LoadSpriteFromTexture(AssetUtils.LoadImage(resource));
    }

    private static Sprite LoadSpriteFromTexture(Texture2D spriteTexture, float pixelsPerUnit = 100f)
    {
      return spriteTexture ? Sprite.Create(spriteTexture, new Rect(0f, 0f, spriteTexture.width, spriteTexture.height), new Vector2(0f, 0f), pixelsPerUnit) : null;
    }

    private static byte[] GetResource(Assembly asm, string resourceName)
    {
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      var manifestResourceStream = asm.GetManifestResourceStream(resourceName);
      Log.Trace(Instance, $"[{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}] manifestResourceStream == null : {manifestResourceStream == null}");
      if (manifestResourceStream == null)
      {
        throw new Exception($"Unable to load the manifestResourceStream from {asm.FullName} for {resourceName}");
      }

      Log.Trace(Instance, $"[{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}] manifestResourceStream.Length : {manifestResourceStream.Length}");
      var array = new byte[manifestResourceStream.Length];
      _ = manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);
      Log.Trace(Instance, $"[{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}] array.Length : {array.Length}");
      return array;
    }
  }
}
