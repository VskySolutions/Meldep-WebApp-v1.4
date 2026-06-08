using System;
using System.Collections.Generic;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;

namespace Vsky.Models;

public class DashboardEmbedConfig
{
    public Guid DashboardId { get; set; }

    public string EmbedUrl { get; set; }

    public EmbedToken EmbedToken { get; set; }
}