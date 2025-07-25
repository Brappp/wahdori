using System;
using WahBox.Core;
using WahBox.Core.Interfaces;
using ImGuiNET;

namespace WahBox.Modules.Weekly;

public class HuntMarkWeeklyModule : BaseModule
{
    public override string Name => "Weekly Hunt Bills";
    public override ModuleType Type => ModuleType.Weekly;
    
    private DateTime _nextReset;
    private bool _isComplete = false;

    public HuntMarkWeeklyModule(Plugin plugin) : base(plugin)
    {
        IconId = 60729; // Module icon
    }

    public override void Initialize()
    {
        base.Initialize();
        UpdateResetTime();
    }

    public override void Update()
    {
        if (!Plugin.ClientState.IsLoggedIn) return;

        // Check if we've passed reset time
        if (DateTime.UtcNow > _nextReset)
        {
            Reset();
            UpdateResetTime();
        }

        // Update status based on completion
        Status = _isComplete ? ModuleStatus.Complete : ModuleStatus.Incomplete;
    }

    public override void Reset()
    {
        base.Reset();
        _isComplete = false;
    }

    private void UpdateResetTime()
    {
        var now = DateTime.UtcNow;
        
        // Weekly Hunt Bills resets on Tuesday at 8:00 UTC
        var targetDay = DayOfWeek.Tuesday;
        var daysUntilReset = ((int)targetDay - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilReset == 0 && now.Hour >= 8)
        {
            daysUntilReset = 7;
        }
        
        _nextReset = now.Date.AddDays(daysUntilReset).AddHours(8);
    }

    public void MarkComplete()
    {
        _isComplete = true;
        Plugin.Instance.NotificationManager.SendModuleComplete(Name, "Weekly Hunt Bills completed!");
    }

    public override void DrawConfig()
    {
        ImGui.TextUnformatted("Weekly Hunt Bills Settings");
        ImGui.Separator();
        ImGui.TextWrapped("Track completion of weekly elite hunt marks.");
        
        ImGui.Separator();
        var timeUntilReset = _nextReset - DateTime.UtcNow;
        ImGui.TextUnformatted($"Next reset: Tuesday {timeUntilReset.Days}d {timeUntilReset.Hours:D2}h {timeUntilReset.Minutes:D2}m");
    }

    public override void DrawStatus()
    {
        var color = Status switch
        {
            ModuleStatus.Complete => new System.Numerics.Vector4(0, 1, 0, 1),
            _ => new System.Numerics.Vector4(1, 1, 1, 1)
        };

        ImGui.TextColored(color, $"{Name}: {(Status == ModuleStatus.Complete ? "Complete" : "Incomplete")}");
    }
}