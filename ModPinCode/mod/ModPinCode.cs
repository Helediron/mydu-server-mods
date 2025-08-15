using Orleans;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using Backend;
using Backend.Business;
using Backend.Database;
using NQutils.Config;
using Backend.Construct;
using NQ;
using NQ.RDMS;
using NQ.Interfaces;
using NQutils;
using NQutils.Exceptions;
using NQutils.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NQutils.Def;

public class PinCodeData
{
    public string pinCode { get; set; } = "";
    public ulong elementId { get; set; }
    public ulong constructId { get; set; }
}

public class MyDuMod : IMod
{
    private IServiceProvider isp;
    private IClusterClient orleans;
    private ILogger logger;
    private IGameplayBank bank;
    private IPub pub;

    public string GetName()
    {
        return "PinCode";
    }

    public Task Initialize(IServiceProvider isp)
    {
        this.isp = isp;
        this.orleans = isp.GetRequiredService<IClusterClient>();
        this.logger = isp.GetRequiredService<ILogger<MyDuMod>>();
        this.bank = isp.GetRequiredService<IGameplayBank>();
        this.pub = isp.GetRequiredService<IPub>();

        logger.LogInformation("ModPinCode initialized successfully");
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
                    label = "Set PIN Code",
                    context = ModActionContext.Element,
                },
                new ModActionDefinition
                {
                    id = 2000,
                    label = "Enter PIN",
                    context = ModActionContext.Element,
                }
            }
        });
    }

    private string EscapeJavaScript(string js)
    {
        return js.Replace("\\", "\\\\")
                 .Replace("\"", "\\\"")
                 .Replace("\r\n", "\\n")
                 .Replace("\n", "\\n")
                 .Replace("\r", "\\n");
    }

    private async Task ShowPinCodeWidget(ulong playerId, ulong constructId, ulong elementId)
    {
        var js = $@"
// PIN Code Widget
if (window.pinCodeWidget) {{
    window.pinCodeWidget.remove();
}}

window.pinCodeWidget = createElement(document.body, 'div');
window.pinCodeWidget.style.position = 'absolute';
window.pinCodeWidget.style.top = '50%';
window.pinCodeWidget.style.left = '50%';
window.pinCodeWidget.style.transform = 'translate(-50%, -50%)';
window.pinCodeWidget.style.backgroundColor = 'rgba(0, 0, 0, 0.9)';
window.pinCodeWidget.style.color = 'white';
window.pinCodeWidget.style.padding = '20px';
window.pinCodeWidget.style.borderRadius = '10px';
window.pinCodeWidget.style.border = '2px solid #00ff00';
window.pinCodeWidget.style.fontFamily = 'monospace';
window.pinCodeWidget.style.fontSize = '16px';
window.pinCodeWidget.style.zIndex = '100000';
window.pinCodeWidget.style.minWidth = '300px';
window.pinCodeWidget.style.textAlign = 'center';

// Title
var title = createElement(window.pinCodeWidget, 'div');
title.style.marginBottom = '20px';
title.style.fontSize = '18px';
title.style.fontWeight = 'bold';
title.innerHTML = '🔒 Set PIN Code';

// Instructions
var instructions = createElement(window.pinCodeWidget, 'div');
instructions.style.marginBottom = '15px';
instructions.style.fontSize = '12px';
instructions.style.color = '#cccccc';
instructions.innerHTML = 'Enter a PIN code (4-12 digits):';

// PIN Input
var pinInput = createElement(window.pinCodeWidget, 'input');
pinInput.type = 'text';
pinInput.placeholder = 'Enter PIN code';
pinInput.style.width = '200px';
pinInput.style.padding = '10px';
pinInput.style.fontSize = '16px';
pinInput.style.backgroundColor = '#333';
pinInput.style.color = 'white';
pinInput.style.border = '1px solid #555';
pinInput.style.borderRadius = '5px';
pinInput.style.textAlign = 'center';
pinInput.style.letterSpacing = '2px';
pinInput.maxLength = 12;

// Only allow digits
pinInput.addEventListener('input', function(e) {{
    e.target.value = e.target.value.replace(/[^0-9]/g, '');
}});

// Button container
var buttonContainer = createElement(window.pinCodeWidget, 'div');
buttonContainer.style.marginTop = '20px';

// Set PIN button
var setButton = createElement(buttonContainer, 'button');
setButton.innerHTML = 'Set PIN';
setButton.style.padding = '10px 20px';
setButton.style.margin = '0 10px';
setButton.style.fontSize = '14px';
setButton.style.backgroundColor = '#00aa00';
setButton.style.color = 'white';
setButton.style.border = 'none';
setButton.style.borderRadius = '5px';
setButton.style.cursor = 'pointer';

// Cancel button
var cancelButton = createElement(buttonContainer, 'button');
cancelButton.innerHTML = 'Cancel';
cancelButton.style.padding = '10px 20px';
cancelButton.style.margin = '0 10px';
cancelButton.style.fontSize = '14px';
cancelButton.style.backgroundColor = '#aa0000';
cancelButton.style.color = 'white';
cancelButton.style.border = 'none';
cancelButton.style.borderRadius = '5px';
cancelButton.style.cursor = 'pointer';

// Status message
var statusMsg = createElement(window.pinCodeWidget, 'div');
statusMsg.style.marginTop = '15px';
statusMsg.style.fontSize = '12px';
statusMsg.style.minHeight = '20px';

// Event handlers
setButton.addEventListener('click', function() {{
    var pin = pinInput.value.trim();
    if (pin.length < 4) {{
        statusMsg.style.color = '#ff6666';
        statusMsg.innerHTML = 'PIN must be at least 4 digits';
        return;
    }}
    if (pin.length > 12) {{
        statusMsg.style.color = '#ff6666';
        statusMsg.innerHTML = 'PIN must be no more than 12 digits';
        return;
    }}
    
    statusMsg.style.color = '#66ff66';
    statusMsg.innerHTML = 'Setting PIN code...';
    
    // Send PIN to server
    CPPMod.sendModAction('PinCode', 1001, [{constructId}], JSON.stringify({{
        pinCode: pin,
        elementId: {elementId},
        constructId: {constructId}
    }}));
    
    setTimeout(function() {{
        window.pinCodeWidget.remove();
        window.pinCodeWidget = null;
    }}, 1000);
}});

cancelButton.addEventListener('click', function() {{
    window.pinCodeWidget.remove();
    window.pinCodeWidget = null;
}});

// Handle Enter key
pinInput.addEventListener('keydown', function(e) {{
    if (e.key === 'Enter' || e.keyCode === 13) {{
        setButton.click();
    }}
    if (e.key === 'Escape' || e.keyCode === 27) {{
        cancelButton.click();
    }}
}});

// Focus input
setTimeout(function() {{
    pinInput.focus();
}}, 100);

console.log('PIN Code widget displayed');
";

        await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = js,
            }));
    }

    private async Task SetElementPinCode(ulong playerId, PinCodeData pinData)
    {
        try
        {
            logger.LogInformation("Setting PIN code for element {elementId} on construct {constructId} by player {playerId}", 
                pinData.elementId, pinData.constructId, playerId);

            // Get the construct elements grain
            var ceg = orleans.GetConstructElementsGrain(pinData.constructId);
            
            // Get element info to verify it exists and is the right type
            var element = await ceg.GetElement(pinData.elementId);
            if (element == null)
            {
                logger.LogWarning("Element {elementId} not found on construct {constructId}", pinData.elementId, pinData.constructId);
                return;
            }
            var pcDef = bank.GetBaseObject<NQutils.Def.PinCodeElement>(element.elementType);


            // Validate PIN code
            if (string.IsNullOrWhiteSpace(pinData.pinCode))
            {
                logger.LogWarning("Empty PIN code provided");
                return;
            }

            if (pinData.pinCode.Length < pcDef.pinCodeMinLength || pinData.pinCode.Length > pcDef.pinCodeMaxLength)
            {
                logger.LogWarning("Invalid PIN code length: {length}", pinData.pinCode.Length);
                return;
            }

            if (!pinData.pinCode.All(char.IsDigit))
            {
                logger.LogWarning("PIN code contains non-digit characters");
                return;
            }

            // Set the PIN code property on the element
            await ceg.UpdateElementProperty(new ElementPropertyUpdate
            {
                constructId = pinData.constructId,
                elementId = pinData.elementId,
                name = "pinCode",
                value = new PropertyValue(pinData.pinCode),
                timePoint = TimePoint.Now(),
            });

            // Reset failed attempts counter
            await ceg.UpdateElementProperty(new ElementPropertyUpdate
            {
                constructId = pinData.constructId,
                elementId = pinData.elementId,
                name = "nFailedAttempt",
                value = new PropertyValue(0L),
                timePoint = TimePoint.Now(),
            });

            // Clear any cooldown
            await ceg.UpdateElementProperty(new ElementPropertyUpdate
            {
                constructId = pinData.constructId,
                elementId = pinData.elementId,
                name = "coolDownUntil",
                value = new PropertyValue(0.0),
                timePoint = TimePoint.Now(),
            });

            logger.LogInformation("Successfully set PIN code for element {elementId}", pinData.elementId);

            // Send confirmation to player
            await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = "CPPHud.addFailureNotification('PIN code set successfully!');",
                }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting PIN code for element {elementId}", pinData.elementId);
        }
    }

    private async Task ShowPinEntryWidget(ulong playerId, ulong constructId, ulong elementId)
    {
        var js = $@"
// PIN Entry Widget
if (window.pinEntryWidget) {{
    window.pinEntryWidget.remove();
}}

window.pinEntryWidget = createElement(document.body, 'div');
window.pinEntryWidget.style.position = 'absolute';
window.pinEntryWidget.style.top = '50%';
window.pinEntryWidget.style.left = '50%';
window.pinEntryWidget.style.transform = 'translate(-50%, -50%)';
window.pinEntryWidget.style.backgroundColor = 'rgba(0, 0, 0, 0.9)';
window.pinEntryWidget.style.color = 'white';
window.pinEntryWidget.style.padding = '20px';
window.pinEntryWidget.style.borderRadius = '10px';
window.pinEntryWidget.style.border = '2px solid #ffaa00';
window.pinEntryWidget.style.fontFamily = 'monospace';
window.pinEntryWidget.style.fontSize = '16px';
window.pinEntryWidget.style.zIndex = '100000';
window.pinEntryWidget.style.minWidth = '300px';
window.pinEntryWidget.style.textAlign = 'center';

// Title
var title = createElement(window.pinEntryWidget, 'div');
title.style.marginBottom = '20px';
title.style.fontSize = '18px';
title.style.fontWeight = 'bold';
title.innerHTML = '🔓 Enter PIN Code';

// Instructions
var instructions = createElement(window.pinEntryWidget, 'div');
instructions.style.marginBottom = '15px';
instructions.style.fontSize = '12px';
instructions.style.color = '#cccccc';
instructions.innerHTML = 'Enter your PIN to unlock:';

// PIN Input
var pinInput = createElement(window.pinEntryWidget, 'input');
pinInput.type = 'password';
pinInput.placeholder = 'Enter PIN';
pinInput.style.width = '200px';
pinInput.style.padding = '10px';
pinInput.style.fontSize = '16px';
pinInput.style.backgroundColor = '#333';
pinInput.style.color = 'white';
pinInput.style.border = '1px solid #555';
pinInput.style.borderRadius = '5px';
pinInput.style.textAlign = 'center';
pinInput.style.letterSpacing = '2px';
pinInput.maxLength = 12;

// Only allow digits
pinInput.addEventListener('input', function(e) {{
    e.target.value = e.target.value.replace(/[^0-9]/g, '');
}});

// Button container
var buttonContainer = createElement(window.pinEntryWidget, 'div');
buttonContainer.style.marginTop = '20px';

// Unlock button
var unlockButton = createElement(buttonContainer, 'button');
unlockButton.innerHTML = 'Unlock';
unlockButton.style.padding = '10px 20px';
unlockButton.style.margin = '0 10px';
unlockButton.style.fontSize = '14px';
unlockButton.style.backgroundColor = '#ff8800';
unlockButton.style.color = 'white';
unlockButton.style.border = 'none';
unlockButton.style.borderRadius = '5px';
unlockButton.style.cursor = 'pointer';

// Cancel button
var cancelButton = createElement(buttonContainer, 'button');
cancelButton.innerHTML = 'Cancel';
cancelButton.style.padding = '10px 20px';
cancelButton.style.margin = '0 10px';
cancelButton.style.fontSize = '14px';
cancelButton.style.backgroundColor = '#aa0000';
cancelButton.style.color = 'white';
cancelButton.style.border = 'none';
cancelButton.style.borderRadius = '5px';
cancelButton.style.cursor = 'pointer';

// Status message
var statusMsg = createElement(window.pinEntryWidget, 'div');
statusMsg.style.marginTop = '15px';
statusMsg.style.fontSize = '12px';
statusMsg.style.minHeight = '20px';

// Event handlers
unlockButton.addEventListener('click', function() {{
    var pin = pinInput.value.trim();
    if (pin.length < 4) {{
        statusMsg.style.color = '#ff6666';
        statusMsg.innerHTML = 'PIN must be at least 4 digits';
        return;
    }}
    
    statusMsg.style.color = '#ffaa66';
    statusMsg.innerHTML = 'Checking PIN...';
    
    // Send PIN to server for verification
    CPPMod.sendModAction('PinCode', 2001, [{constructId}], JSON.stringify({{
        pinCode: pin,
        elementId: {elementId},
        constructId: {constructId}
    }}));
    
    // Clear input for security
    pinInput.value = '';
    
    setTimeout(function() {{
        if (window.pinEntryWidget) {{
            window.pinEntryWidget.remove();
            window.pinEntryWidget = null;
        }}
    }}, 2000);
}});

cancelButton.addEventListener('click', function() {{
    window.pinEntryWidget.remove();
    window.pinEntryWidget = null;
}});

// Handle Enter key
pinInput.addEventListener('keydown', function(e) {{
    if (e.key === 'Enter' || e.keyCode === 13) {{
        unlockButton.click();
    }}
    if (e.key === 'Escape' || e.keyCode === 27) {{
        cancelButton.click();
    }}
}});

// Focus input
setTimeout(function() {{
    pinInput.focus();
}}, 100);

console.log('PIN Entry widget displayed');
";

        await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = js,
            }));
    }

    private async Task VerifyPinAndUnlock(ulong playerId, PinCodeData pinData)
    {
        try
        {
            logger.LogInformation("Verifying PIN for element {elementId} on construct {constructId} by player {playerId}", 
                pinData.elementId, pinData.constructId, playerId);

            // Get the construct elements grain
            var ceg = orleans.GetConstructElementsGrain(pinData.constructId);
            
            // Get element to verify it exists and is the right type
            var element = await ceg.GetElement(pinData.elementId);
            if (element == null)
            {
                logger.LogWarning("Element {elementId} not found on construct {constructId}", pinData.elementId, pinData.constructId);
                return;
            }

            var pcDef = bank.GetBaseObject<NQutils.Def.PinCodeElement>(element.elementType);

            // Get element properties to check PIN and status
            var properties = element.properties;
            
            // Get stored PIN
            properties.TryGetValue("pinCode", out var storedPinProp);
            var storedPin = storedPinProp?.stringValue ?? "";
            
            // Get failed attempts
            properties.TryGetValue("nFailedAttempt", out var failedAttemptsProp);
            var failedAttempts = failedAttemptsProp?.intValue ?? 0;
            
            // Get cooldown time
            properties.TryGetValue("coolDownUntil", out var cooldownProp);
            var cooldownUntil = cooldownProp?.doubleValue ?? 0.0;
            
            // Check if in cooldown
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (cooldownUntil > currentTime)
            {
                var remainingTime = (int)(cooldownUntil - currentTime);
                logger.LogInformation("PIN entry blocked due to cooldown for element {elementId}, {remainingTime}s remaining", 
                    pinData.elementId, remainingTime);
                
                await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = $"CPPHud.addFailureNotification('Access denied: Cooldown active ({remainingTime}s remaining)');",
                    }));
                return;
            }

            // Verify PIN
            if (string.IsNullOrEmpty(storedPin))
            {
                logger.LogWarning("No PIN set for element {elementId}", pinData.elementId);
                await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = "CPPHud.addFailureNotification('Access denied: No PIN configured');",
                    }));
                return;
            }

            if (pinData.pinCode == storedPin)
            {
                // Correct PIN - unlock door
                logger.LogInformation("Correct PIN entered for element {elementId}, unlocking door", pinData.elementId);
                
                // Reset failed attempts
                await ceg.UpdateElementProperty(new ElementPropertyUpdate
                {
                    constructId = pinData.constructId,
                    elementId = pinData.elementId,
                    name = "nFailedAttempt",
                    value = new PropertyValue(0L),
                    timePoint = TimePoint.Now(),
                });

                // Open the door (set element_state to true)
                await ceg.UpdateElementProperty(new ElementPropertyUpdate
                {
                    constructId = pinData.constructId,
                    elementId = pinData.elementId,
                    name = "element_state",
                    value = new PropertyValue(true),
                    timePoint = TimePoint.Now(),
                });

                // Send success message
                await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = "CPPHud.addFailureNotification('Access granted: Door unlocked');",
                    }));
            }
            else
            {
                // Wrong PIN - increment failed attempts
                var newFailedAttempts = failedAttempts + 1;
                logger.LogInformation("Incorrect PIN entered for element {elementId}, failed attempts: {failedAttempts} '{pinCode}'", 
                    pinData.elementId, newFailedAttempts, pinData.pinCode);

                await ceg.UpdateElementProperty(new ElementPropertyUpdate
                {
                    constructId = pinData.constructId,
                    elementId = pinData.elementId,
                    name = "nFailedAttempt",
                    value = new PropertyValue(newFailedAttempts),
                    timePoint = TimePoint.Now(),
                });

                // Check if max attempts reached
                var maxFailAttempts = pcDef.maxFailAttempts; // Default from PinCodeElement
                if (newFailedAttempts >= maxFailAttempts)
                {
                    // Set cooldown (5 minutes = 300 seconds)
                    var cooldownTime = currentTime + pcDef.failCooldownTime;
                    await ceg.UpdateElementProperty(new ElementPropertyUpdate
                    {
                        constructId = pinData.constructId,
                        elementId = pinData.elementId,
                        name = "coolDownUntil",
                        value = new PropertyValue(cooldownTime),
                        timePoint = TimePoint.Now(),
                    });

                    logger.LogInformation("Max attempts reached for element {elementId}, entering cooldown", pinData.elementId);
                    
                    await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                        new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                        {
                            eventName = "modinjectjs",
                            eventPayload = "CPPHud.addFailureNotification('Access denied: Too many failed attempts. Cooldown activated.');",
                        }));
                }
                else
                {
                    var remainingAttempts = maxFailAttempts - newFailedAttempts;
                    await pub.NotifyTopic(Topics.PlayerNotifications(playerId),
                        new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                        {
                            eventName = "modinjectjs",
                            eventPayload = $"CPPHud.addFailureNotification('Access denied: Wrong PIN. {remainingAttempts} attempts remaining.');",
                        }));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying PIN for element {elementId}", pinData.elementId);
        }
    }

    public async Task TriggerAction(ulong playerId, ModAction action)
    {
        try
        {
            logger.LogInformation("ModPinCode action {actionId} triggered by player {playerId}", action.actionId, playerId);

            switch (action.actionId)
            {
                case 1000: // Set PIN Code - show widget
                    if (action.elementId == 0)
                    {
                        logger.LogWarning("No element selected for PIN code action");
                        return;
                    }
                    await ShowPinCodeWidget(playerId, action.constructId, action.elementId);
                    break;

                case 1001: // Set PIN Code - process submitted PIN
                    var pinData = JsonConvert.DeserializeObject<PinCodeData>(action.payload);
                    if (pinData != null)
                    {
                        await SetElementPinCode(playerId, pinData);
                    }
                    break;

                case 2000: // Enter PIN - show entry widget
                    if (action.elementId == 0)
                    {
                        logger.LogWarning("No element selected for PIN entry action");
                        return;
                    }
                    await ShowPinEntryWidget(playerId, action.constructId, action.elementId);
                    break;

                case 2001: // Enter PIN - verify submitted PIN
                    var entryData = JsonConvert.DeserializeObject<PinCodeData>(action.payload);
                    if (entryData != null)
                    {
                        await VerifyPinAndUnlock(playerId, entryData);
                    }
                    break;

                default:
                    logger.LogWarning("Unknown action ID: {actionId}", action.actionId);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ModPinCode action {actionId}", action.actionId);
        }
    }
}