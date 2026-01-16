using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string? UpdatedBy { get; set; }

        void MarkAsCreated(string createdBy);
        void MarkAsUpdated(string updatedBy);
    }
}
