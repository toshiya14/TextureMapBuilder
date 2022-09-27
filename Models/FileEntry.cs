using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureMapBuilder.Models;

public record FileEntry
{
    public string FullPath { get; set; }

    public string DisplayName { get; set; }
}
