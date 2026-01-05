using System;
using System.Threading.Tasks;
using Services;
using Backend;
using NQ.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NQ;
using NQutils.Sql;
using NQutils;
using Backend.Analytics;
using Backend.Business;
using NQ.Grains.Core;
using NQ.Grains.Telemetry;
using OpenTelemetry;
using OpenTelemetry.Shims.OpenTracing;
using OpenTracing;
using Orleans;

public class MyDuGrainMod: IGrainMod
{
    public void Initialize(IHostBuilder hb)
    {
        Console.WriteLine("Talent Grain Mod initialized");
    }
}


public class QuantaCostTalentGrain: NQ.Grains.Gameplay.Talent.TalentGrain, ITalentGrain
{
    private ILogger<NQ.Grains.Gameplay.Talent.TalentGrain> logger;
    private ISql sql;
    private IInventoryService inventoryService;
    private ulong playerId;
    public QuantaCostTalentGrain(IPub ips, ILogger<NQ.Grains.Gameplay.Talent.TalentGrain> logger, IItemPermissionCache ipc,
            ITalentTree talentTree, ITracer tracer, ISql sql, IGameplayBank bank,
            IDataAccessor dataAccessor, IAnalytics analytics, IInternalPubSub internalPubSub, IInventoryService inventoryService)
            : base(ips, logger, ipc, talentTree, tracer, sql, bank, dataAccessor, analytics, internalPubSub)
    {
        this.logger = logger;
        this.sql = sql;
        this.inventoryService = inventoryService;
    }
    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();
        playerId = UInt64.Parse(this.GetPrimaryKeyString());
        logger.LogInformation("Mod talent grain activating for {player}", playerId);
    }
    async Task<TalentAndLevelPoints> ITalentGrain.Purchase(TalentAndLevel talentAndLevel)
    {
        logger.LogInformation("Talent purchase order received at level {level}", talentAndLevel.level);
        var cost = talentAndLevel.level * 1000 + 1000;
        await inventoryService.addToWallet(playerId, -cost);
        return await base.Purchase(talentAndLevel);
    }
}