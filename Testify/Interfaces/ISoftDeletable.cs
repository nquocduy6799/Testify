using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Interfaces
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string? DeletedBy { get; set; }

        void MarkAsDeleted(string deletedBy);
        void Restore();
    }
}
