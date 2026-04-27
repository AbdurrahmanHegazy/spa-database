using IndustrialMonitoring.Api.Models.Tags;

namespace IndustrialMonitoring.Api.Services;

public class TagDetailsService : ITagDetailsService
{
    public TagDetailsResponse GetTagDetails(string tagId)
    {
        var tagDetailsMap = new Dictionary<string, TagDetailsResponse>
        {
            ["motor-m030"] = new()
            {
                DisplayName = "Motori_BM_Pompa_i_M030.frequenza attuale",
                Group = "Pump Group M030",
                DataType = "Real",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Motori_BM_Pompa_i_M030.frequenza attuale",
                CurrentValue = "123.45",
                Quality = "Good",
                LastUpdate = "2s ago",
                DeviceState = "Online",
                Minimum = "118.20 Hz",
                Maximum = "126.90 Hz",
                Average = "123.45 Hz",
                Samples = "1,248"
            },
            ["motor-m030-frequency"] = new()
            {
                DisplayName = "Motori_BM_Pompa_i_M030.frequenza attuale",
                Group = "Pump Group M030",
                DataType = "Real",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Motori_BM_Pompa_i_M030.frequenza attuale",
                CurrentValue = "123.45",
                Quality = "Good",
                LastUpdate = "2s ago",
                DeviceState = "Online",
                Minimum = "118.20 Hz",
                Maximum = "126.90 Hz",
                Average = "123.45 Hz",
                Samples = "1,248"
            },
            ["motor-m030-current"] = new()
            {
                DisplayName = "Motori_BM_Pompa_i_M030.corrente attuale",
                Group = "Pump Group M030",
                DataType = "Real",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Motori_BM_Pompa_i_M030.corrente attuale",
                CurrentValue = "17.80",
                Quality = "Good",
                LastUpdate = "3s ago",
                DeviceState = "Online",
                Minimum = "15.10 A",
                Maximum = "19.40 A",
                Average = "17.80 A",
                Samples = "1,240"
            },
            ["boiler-main-pressure"] = new()
            {
                DisplayName = "Boiler_Main.Pressure",
                Group = "Boiler Circuit",
                DataType = "Real",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Boiler_Main.Pressure",
                CurrentValue = "7.80",
                Quality = "Warning",
                LastUpdate = "5s ago",
                DeviceState = "Online",
                Minimum = "7.10 bar",
                Maximum = "8.20 bar",
                Average = "7.62 bar",
                Samples = "980"
            },
            ["mixer-line-2-temperature"] = new()
            {
                DisplayName = "Mixer_Line_2.Temperature",
                Group = "Mixer Line 2",
                DataType = "Real",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Mixer_Line_2.Temperature",
                CurrentValue = "88.20",
                Quality = "Good",
                LastUpdate = "2s ago",
                DeviceState = "Online",
                Minimum = "79.30 °C",
                Maximum = "91.80 °C",
                Average = "86.70 °C",
                Samples = "1,305"
            },
            ["conveyor-line-1-speed"] = new()
            {
                DisplayName = "Line_1.Conveyor.Speed",
                Group = "Conveyor Line 1",
                DataType = "Real",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Line_1.Conveyor.Speed",
                CurrentValue = "65.20",
                Quality = "Good",
                LastUpdate = "1s ago",
                DeviceState = "Online",
                Minimum = "60.10 rpm",
                Maximum = "70.40 rpm",
                Average = "65.20 rpm",
                Samples = "1,115"
            },
            ["motor-m800-status"] = new()
            {
                DisplayName = "Motori_CO_Cond_M800.stato attuale",
                Group = "Pump Group M800",
                DataType = "Byte",
                Source = "WinCC Unified / OPC UA",
                MqttTopic = "industrial/tags",
                RedisKey = "industrial:latest:Motori_CO_Cond_M800.stato attuale",
                CurrentValue = "Unavailable",
                Quality = "Bad",
                LastUpdate = "45s ago",
                DeviceState = "Offline",
                Minimum = "N/A",
                Maximum = "N/A",
                Average = "N/A",
                Samples = "0"
            }
        };

        if (tagDetailsMap.TryGetValue(tagId, out var result))
        {
            return result;
        }

        return new TagDetailsResponse
        {
            DisplayName = "Unknown Tag",
            Group = "Unknown Group",
            DataType = "Unknown",
            Source = "Unavailable",
            MqttTopic = "industrial/tags",
            RedisKey = "industrial:latest:unknown",
            CurrentValue = "--",
            Quality = "Unknown",
            LastUpdate = "--",
            DeviceState = "Unknown",
            Minimum = "--",
            Maximum = "--",
            Average = "--",
            Samples = "--"
        };
    }
}