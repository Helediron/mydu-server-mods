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
    private ILogger logger;
    private IPub pub;
    private IClusterClient orleans;
    private readonly Dictionary<ulong, ulong> playerPlanetCat = new();
    public string GetName()
    {
        return "DynamicInterestPoint";
    }
    public Task Initialize(IServiceProvider isp)
    {
        this.isp = isp;
        this.logger = isp.GetRequiredService<ILogger<MyDuMod>>();
        this.pub = isp.GetRequiredService<IPub>();
        this.orleans = isp.GetRequiredService<IClusterClient>();
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
                    id = 1000,
                    label = "My cat is here!",
                    context = ModActionContext.Global,
                },
                new ModActionDefinition
                {
                    id = 1001,
                    label = "My cat is stealth!",
                    context = ModActionContext.Global,
                },
            }
        });
    }
    public async Task TriggerAction(ulong playerId, ModAction action)
    {
        if (action.actionId == 1001)
        {
            if (!playerPlanetCat.ContainsKey(playerId))
                return;
            var dip = new DynamicInterestPoint
            {
                planetId = playerPlanetCat[playerId],
                markType = 1000,
                uniqueId = 1000000,
                location = new DetailedLocation
                {
                    absolute = new(),
                    relative = new(),
                }
            };
            var dips = JsonConvert.SerializeObject(dip);
            logger.LogInformation("GO FOR REMOVE {mark}", dips);
            var s = new VoxelEdit
            {
                flags = 10,
                hashContext = dips,
            };
            await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.VoxelCsgApplied(s));
            return;
        }
        if (action.actionId == 1000)
        {
            var (rloc, aloc) = await isp.GetRequiredService<IScenegraph>().GetPlayerWorldPosition(playerId);
            ulong planetId = 0;
            if (rloc.constructId != 0)
            {
                var ctree = await isp.GetRequiredService<IScenegraph>().GetTree(rloc.constructId);
                foreach (var crd in ctree)
                {
                    if (crd.constructId < 10000)
                    {
                        planetId = crd.constructId;
                        break;
                    }
                }
            }
            playerPlanetCat[playerId] = planetId;
            var dip = new DynamicInterestPoint
            {
                planetId = planetId,
                markType = 1000,
                uniqueId = 1000000,
                location = new DetailedLocation
                {
                    absolute = aloc,
                    relative = rloc,
                },
                name = "My CAT",
                description = "He's so cute and fluffy!",
            };
            var dips = JsonConvert.SerializeObject(dip);
            logger.LogInformation("GO FOR {mark}", dips);
            var s = new VoxelEdit
            {
                flags = 9,
                hashContext = dips,
            };
            await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.VoxelCsgApplied(s));
        }
    }
}