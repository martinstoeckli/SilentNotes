using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentNotes.ViewModels
{
    public class ListItemViewModel
    {
        public object Value { get; set; }

        public string Text { get; set; }

        public string IconName { get; set; }

        public bool IsDivider { get; set; }
    }
}
