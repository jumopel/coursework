using coursework.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    internal interface IFestivalDataProvider
    {
        string ProviderName { get; }
        IEnumerable<ZoneStateDto> GetZonesSnapshot();
        IEnumerable<ShopStateDto> GetShopsSnapshot();
        event Action DataUpdated;
        void SendCommand(string command);
    }
}
