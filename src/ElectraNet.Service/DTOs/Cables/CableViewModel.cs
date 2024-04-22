﻿using ElectraNet.Domain.Enitites.Commons;

namespace ElectraNet.Service.DTOs.Cables;

public class CableViewModel
{
    public long Id { get; set; }
    public long Voltage { get; set; }
    public string Description { get; set; }
    public long AssetId { get; set; }
    public Asset Asset { get; set; }
}