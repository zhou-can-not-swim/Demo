
using System.Data;
using TestMethod;


var rule = new ScrewBoltRule
{
    Details = new Dictionary<string, ScrewBoltRuleItem>
    {
        ["M6_Bolt"] = new ScrewBoltRuleItem
        {
            IsEnabled = true,
            IsLocalCheck = true,
            ProgNo = 1001,
            BackwardProgNo = null,
            SockNo = 1,
            TorqueMin = 8.5m,
            TorqueMax = 10.5m,
            AngleMin = 45m,
            AngleMax = 60m,
            StartOrder = 1,
            ScrewCount = 6
        },
        ["M8_Bolt"] = new ScrewBoltRuleItem
        {
            IsEnabled = true,
            IsLocalCheck = false,
            ProgNo = 1002,
            BackwardProgNo = 1001,
            SockNo = 2,
            TorqueMin = 18.0m,
            TorqueMax = 22.0m,
            AngleMin = 50m,
            AngleMax = 70m,
            StartOrder = 2,
            ScrewCount = 8
        }
    }
};

var res = rule.Details
    .Where(gs => gs.Value?.IsEnabled == true)
    .Select(gr =>
    {
        return Enumerable.Range(gr.Value.StartOrder, gr.Value.ScrewCount)
            .Select(order => order);
    })
    .SelectMany(s => s)
    .DistinctBy(s => s);

Console.WriteLine(string.Join(", ", res));