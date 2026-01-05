using Orleans;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using Backend;
using Backend.Business;
using Backend.Database;
using NQutils.Config;
using Backend.Construct;
using Backend.Storage;
using Backend.Scenegraph;
using NQ;
using NQ.RDMS;
using NQ.Interfaces;
using NQ.Visibility;
using NQ.Grains.Core;
// in Grains!! using Services;
using NQutils;
using NQutils.Exceptions;
using NQutils.Net;
using NQutils.Serialization;
using NQutils.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MathNet.Spatial.Euclidean;
using MathNet.Numerics;



public class MyDuMod : IMod
{
    private IServiceProvider isp;
    private IClusterClient orleans;
    private ILogger logger;
    private IGameplayBank bank;
    private IPub pub;

    public string GetName()
    {
        return "ModDoorToll";
    }

    public Task Initialize(IServiceProvider isp)
    {
        this.isp = isp;
        this.orleans = isp.GetRequiredService<IClusterClient>();
        this.logger = isp.GetRequiredService<ILogger<MyDuMod>>();
        this.bank = isp.GetRequiredService<IGameplayBank>();
        this.pub = isp.GetRequiredService<IPub>();
        return Task.CompletedTask;
    }
    public Task<ModInfo> GetModInfoFor(ulong playerId, bool admin)
    {
        return Task.FromResult<ModInfo>(new ModInfo
        {
            name = GetName(),
            actions = new List<ModActionDefinition>
            {
                new ModActionDefinition
                {
                    id = 10001,
                    label = "Toll\\set to 10",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 10002,
                    label = "Toll\\set to 100",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 10003,
                    label = "Toll\\set to 1000",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 10009,
                    label = "Toll\\set to ONE BILLION",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 10000,
                    label = "Toll\\disable",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 9,
                    label = "Toll\\no close",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 1,
                    label = "Toll\\opt in",
                    context = ModActionContext.Global,
                },
            }
        });
    }

    public async Task PayToll(ulong playerId, string payload)
    {
        var tokAndAmount = payload.Split('@');
        var amount = long.Parse(tokAndAmount[1]);
        var token = tokAndAmount[0];
        var ok = false;
        var sql = isp.GetRequiredService<ISql>();
        try
        {
            if (await sql.UpdateWallet(playerId, -amount*100))
            {
                ok = true;
                var money = await sql.GetWallet(playerId);
                var c = new Currency() { amount = money };
                await pub.NotifyPlayer(playerId, new NQutils.Messages.WalletUpdated(c));

                //await isp.GetRequiredService<IInventoryService>().addToWallet(playerId, -amount * 100);
                //ok = true;
            }
        }
        catch (Exception)
        {

        }
        await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = "CPPMod.authorizeInteraction(\""+token+"\"," + 
                (ok ? "true":"false") +
                ",\"\");",
            }
                ));
    }
    public async Task SetProp(IConstructElementsGrain ceg, ulong cid, ulong eid, string name, string value)
    {
        await ceg.UpdateElementProperty(new ElementPropertyUpdate
        {
            constructId = cid,
            elementId = eid,
            name = name,
            value = new PropertyValue(value),
            timePoint = TimePoint.Now(),
        });
    }
    public async Task SetupToll(ulong cid, ulong eid, ulong amountCode)
    {
        var ceg = orleans.GetConstructElementsGrain(cid);
        var amounts = new long[] { 0, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };
        if (amountCode == 10000)
        {
            await SetProp(ceg, cid, eid, "interactionParameters", "");
            await SetProp(ceg, cid, eid, "interactionAuthorize", "");
        }
        else
        {
            var amount = amounts[amountCode - 10000].ToString();
            await SetProp(ceg, cid, eid, "interactionAuthorize", "toll");
            await SetProp(ceg, cid, eid, "deactivateLabel", "");
            await SetProp(ceg, cid, eid, "interactionParameters", amount);
            await SetProp(ceg, cid, eid, "activateLabel", $"Pay {amount} to open");
        }
        
    }
    public async Task TriggerAction(ulong playerId, ModAction action)
    {
        if (action.actionId == 1)
        {
            var js = @"
             if (!window.tollControl)
             {
                window.tollControl = function(token, enable, parms, defId, cid, eid)
                {
                    if (parms == 'noclose')
                    {
                        CPPMod.authorizeInteraction(token, enable, '');
                    }
                    else
                        CPPMod.sendModAction(""ModDoorToll"", 10, [cid, eid, 0], token + ""@"" + parms);
                }
                engine.on(""toll"", window.tollControl);
             }
            ";
            await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = js,
                    }
                        ));
            return;
        }
        if (action.actionId == 9)
        {
            var ceg = orleans.GetConstructElementsGrain(action.constructId);
            await SetProp(ceg, action.constructId, action.elementId, "interactionParameters", "noclose");
            await SetProp(ceg, cid, eid, "deactivateLabel", "Jammed!");
            await SetProp(ceg, cid, eid, "activateLabel", "");
            await SetProp(ceg, cid, eid, "interactionAuthorize", "toll");
            return;     
        }
        if (action.actionId == 10)
            {
                await PayToll(playerId, action.payload);
                return;
            }
        await SetupToll(action.constructId, action.elementId, action.actionId);
    }
}