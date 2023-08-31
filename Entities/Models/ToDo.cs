using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class ToDo
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public bool? IsCompleted { get; set; }

    public string UserId { get; set; }
}
