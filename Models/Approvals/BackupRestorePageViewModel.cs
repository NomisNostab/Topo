using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topo.Models.Approvals
{
    public class BackupRestorePageViewModel
    {
        public string SelectedUnitId { get; set; } = string.Empty;
        public string SelectedUnitName { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? Units { get; set; }
    }
}
